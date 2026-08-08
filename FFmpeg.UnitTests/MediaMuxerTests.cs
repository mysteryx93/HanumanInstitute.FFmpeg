// ReSharper disable AssignNullToNotNullAttribute
// ReSharper disable PossibleMultipleEnumeration
namespace HanumanInstitute.FFmpeg.UnitTests;

/// <summary>
/// Command-building and model tests for mux.
/// Usage-oriented command tests plus lean mechanics (validation, AAC/PCM, elementary, From flags).
/// Internal (path,index,format,type) ctor stands in for a resolved probe in command tests.
/// </summary>
public class MediaMuxerTests
{
    private const string FFmpeg = "ffmpeg";
    private const string AudioCodec = "-acodec";
    private const string VideoCodec = "-vcodec";
    private const string FixAac = "aac_adtstoasc";
    private const string FixPcm = "pcm_s16le";

    private FakeEncoderService _factory = null!;
    private readonly ITestOutputHelper _output;

    public MediaMuxerTests(ITestOutputHelper output) => _output = output;

    protected IMediaMuxer SetupMuxer()
    {
        _factory = new FakeEncoderService();
        return new MediaMuxer(_factory, new FakeFileSystemService(), new FakeMediaInfoReader());
    }

    /// <summary>
    /// Fully resolved mux selection (simulates probe result). Default: do not re-probe tags.
    /// </summary>
    private static MediaStream S(string path, int index, string format, FFmpegStreamType type,
        Action<MediaStream> configure = null)
    {
        var stream = new MediaStream(path, index, format, type);
        configure?.Invoke(stream);
        return stream;
    }

    private static StreamDisposition DefaultDisposition()
    {
        var d = new StreamDisposition();
        d.Set("default");
        return d;
    }

    private static StreamDisposition ClearDisposition() => new StreamDisposition();

    private string LastCmd
    {
        get
        {
            var cmd = _factory.Instances.Last().CommandWithArgs;
            _output.WriteLine(cmd);
            return cmd;
        }
    }

    private IProcessWorker AssertFFmpegManager(bool hasVideo, bool hasAudio, int instanceIndex = -1)
    {
        var manager = instanceIndex < 0 ? _factory.Instances.Last() : _factory.Instances[instanceIndex];
        Assert.NotNull(manager);
        _output.WriteLine(manager.CommandWithArgs);
        Assert.Contains(FFmpeg, manager.CommandWithArgs, StringComparison.InvariantCulture);
        manager.CommandWithArgs.ContainsOrNot(VideoCodec, hasVideo);
        manager.CommandWithArgs.ContainsOrNot(AudioCodec, hasAudio);
        return manager;
    }

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }
        return count;
    }

    // ── Construction ────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_WithFactory_Success() =>
        _ = new MediaMuxer(new FakeEncoderService(), new FakeFileSystemService(), Mock.Of<IMediaInfoReader>());

    [Fact]
    public void Constructor_NullFactory_ThrowsException() =>
        Assert.Throws<ArgumentNullException>(() =>
            new MediaMuxer(null!, new FakeFileSystemService(), Mock.Of<IMediaInfoReader>()));

    [Fact]
    public void Constructor_NullFileSystem_ThrowsException() =>
        Assert.Throws<ArgumentNullException>(() =>
            new MediaMuxer(new FakeEncoderService(), null!, Mock.Of<IMediaInfoReader>()));

    [Fact]
    public void Constructor_NullInfoReader_ThrowsException() =>
        Assert.Throws<ArgumentNullException>(() =>
            new MediaMuxer(new FakeEncoderService(), new FakeFileSystemService(), null!));

    // ── MediaStream model (public API) ──────────────────────────────────────

    [Fact]
    public void MediaStream_PublicCtor_PathIndexOnly_TypeNoneUntilProbe()
    {
        var stream = new MediaStream("clip.mkv", 1);

        Assert.Equal("clip.mkv", stream.Path);
        Assert.Equal(1, stream.Index);
        Assert.Equal(FFmpegStreamType.None, stream.Type);
        Assert.Equal(string.Empty, stream.Format);
        Assert.Null(stream.Language);
        Assert.Empty(stream.Metadata);
        Assert.Null(stream.Disposition);
    }

    [Fact]
    public void MediaStream_FromStreamInfo_CopiesProbeIntoSelectionAndWriteOptions()
    {
        var info = new MediaAudioStreamInfo
        {
            Index = 1,
            Format = "aac",
            Language = "eng"
        };
        info.Metadata["frequency"] = "440";
        info.Disposition.Set("default");

        Assert.Equal("440", info.Metadata["frequency"]);

        var stream = MediaStream.FromStreamInfo("/path/file.mkv", info);

        Assert.Equal("/path/file.mkv", stream.Path);
        Assert.Equal(1, stream.Index);
        Assert.Equal("aac", stream.Format);
        Assert.Equal(FFmpegStreamType.Audio, stream.Type);
        Assert.Equal("eng", stream.Language);
        Assert.Equal("440", stream.Metadata["frequency"]);
        Assert.NotNull(stream.Disposition);
        Assert.True(stream.Disposition.Has("default"));
    }

    [Fact]
    public void MediaStream_PathIndex_CopyTagsFrom_AppliesWriteTags()
    {
        var original = new MediaAudioStreamInfo
        {
            Index = 1,
            Format = "aac",
            Language = "jpn"
        };
        original.Metadata["title"] = "Commentary";
        original.Disposition.Set("comment");

        // Different path/index than the probe: selection by ctor, tags via CopyTagsFrom.
        var stream = new MediaStream("/tmp/out.m4a", 0, original.Format, original.StreamType);
        stream.CopyTagsFrom(original);

        Assert.Equal("/tmp/out.m4a", stream.Path);
        Assert.Equal(0, stream.Index);
        Assert.Equal(FFmpegStreamType.Audio, stream.Type);
        Assert.Equal("aac", stream.Format);
        Assert.Equal("jpn", stream.Language);
        Assert.Equal("Commentary", stream.Metadata["title"]);
        Assert.NotNull(stream.Disposition);
        Assert.True(stream.Disposition.Has("comment"));
    }


    [Fact]
    public void MediaStream_CopyTagsFrom_ReplacesLanguageAndMetadata()
    {
        var src = new MediaAudioStreamInfo { Language = "fra" };
        src.Metadata["frequency"] = "432";

        var stream = new MediaStream("a.m4a", 0, "aac", FFmpegStreamType.Audio) { Language = "eng" };
        stream.Metadata["title"] = "old";
        stream.CopyTagsFrom(src);

        Assert.Equal("fra", stream.Language);
        Assert.Equal("432", stream.Metadata["frequency"]);
        Assert.False(stream.Metadata.ContainsKey("title"));
    }

    // ── Stream-list mux (command building) ──────────────────────────────────

    [Theory]
    [InlineData(true, false)]  // video only
    [InlineData(false, true)]  // audio only
    [InlineData(true, true)]   // both
    public void Muxe_StreamList_Valid_Success(bool hasVideo, bool hasAudio)
    {
        var muxer = SetupMuxer();
        var streams = new List<MediaStream>();
        if (hasVideo)
        {
            streams.Add(S("video.mp4", 0, "h264", FFmpegStreamType.Video));
        }
        if (hasAudio)
        {
            streams.Add(S("audio.m4a", 0, "aac", FFmpegStreamType.Audio));
        }

        var result = muxer.Muxe(streams, "dest.mp4");

        Assert.Equal(CompletionStatus.Success, result);
        Assert.Single(_factory.Instances);
        AssertFFmpegManager(hasVideo, hasAudio);
        Assert.Contains("copy", _factory.Instances[0].CommandWithArgs, StringComparison.Ordinal);
    }

    [Fact]
    public void Muxe_StreamList_Null_Throws()
    {
        var muxer = SetupMuxer();
        Assert.Throws<ArgumentNullException>(() => muxer.Muxe((IEnumerable<MediaStream>)null!, "dest"));
    }

    [Fact]
    public void Muxe_StreamList_EmptyWithoutFromMedia_Throws()
    {
        var muxer = SetupMuxer();
        Assert.Throws<ArgumentException>(() => muxer.Muxe(new List<MediaStream>(), "dest"));
    }

    [Fact]
    public void Muxe_StreamList_EmptyPath_Throws()
    {
        var muxer = SetupMuxer();
        Assert.Throws<ArgumentException>(() =>
            muxer.Muxe(new[] { S("", 0, "h264", FFmpegStreamType.Video) }, "dest"));
    }

    [Theory]
    [InlineData(null, typeof(ArgumentNullException))]
    [InlineData("", typeof(ArgumentException))]
    public void Muxe_StreamList_BadDestination_Throws(string destination, Type ex)
    {
        var muxer = SetupMuxer();
        Assert.Throws(ex, () =>
            muxer.Muxe(new[] { S("v.mp4", 0, "h264", FFmpegStreamType.Video) }, destination!));
    }

    [Fact]
    public void Muxe_SamePathMultipleStreams_DedupesInputsAndMapsIndices()
    {
        var muxer = SetupMuxer();
        var streams = new List<MediaStream>
        {
            S("source.mkv", 0, "h264", FFmpegStreamType.Video),
            S("source.mkv", 1, "aac", FFmpegStreamType.Audio),
            S("source.mkv", 2, "subrip", FFmpegStreamType.Subtitle)
        };

        muxer.Muxe(streams, "dest.mkv", new MuxOptions());

        var cmd = LastCmd;
        Assert.Equal(1, CountOccurrences(cmd, "-i \"source.mkv\""));
        Assert.Contains("-map 0:0", cmd, StringComparison.Ordinal);
        Assert.Contains("-map 0:1", cmd, StringComparison.Ordinal);
        Assert.Contains("-map 0:2", cmd, StringComparison.Ordinal);
        Assert.Contains("-c copy", cmd, StringComparison.Ordinal);
    }

    [Fact]
    public void Muxe_PreservesCallerStreamOrder()
    {
        var muxer = SetupMuxer();
        var streams = new List<MediaStream>
        {
            S("audio.m4a", 0, "aac", FFmpegStreamType.Audio),
            S("video.mp4", 0, "h264", FFmpegStreamType.Video)
        };

        muxer.Muxe(streams, "dest.mp4", new MuxOptions());

        var cmd = LastCmd;
        var mapAudio = cmd.IndexOf("-map 0:0", StringComparison.Ordinal);
        var mapVideo = cmd.IndexOf("-map 1:0", StringComparison.Ordinal);
        Assert.True(mapAudio >= 0 && mapVideo > mapAudio);
    }

    [Fact]
    public void Muxe_ParamOptions_ReturnsSame()
    {
        var muxer = SetupMuxer();
        var options = new ProcessOptionsEncoder();

        muxer.Muxe(new[] { S("v.mp4", 0, "h264", FFmpegStreamType.Video) }, "dest", null, options);

        Assert.Same(options, _factory.Instances[0].Options);
    }

    [Fact]
    public void Muxe_ParamCallback_CallbackCalled()
    {
        var muxer = SetupMuxer();
        var callbackCalled = 0;

        muxer.Muxe([S("v.mp4", 0, "h264", FFmpegStreamType.Video)], "dest", callback: (_, _) => callbackCalled++);

        Assert.Equal(1, callbackCalled);
    }

    // ── Path+index only → probe fills Type (FakeMediaInfoReader: 0=Video, 1=Audio) ──

    [Fact]
    public void Muxe_PathIndexOnly_ResolvesTypeFromProbe()
    {
        var muxer = SetupMuxer();
        // Public API: selection only; Type/Format filled at mux time from FakeMediaInfoReader.
        var streams = new List<MediaStream>
        {
            new("source.mkv", 0),
            new("source.mkv", 1)
        };

        var result = muxer.Muxe(streams, "dest.mkv");

        Assert.Equal(CompletionStatus.Success, result);
        Assert.Equal(FFmpegStreamType.Video, streams[0].Type);
        Assert.Equal(FFmpegStreamType.Audio, streams[1].Type);
        AssertFFmpegManager(true, true);
    }


    // ── Usage scenarios (command shape) ─────────────────────────────────────

    [Fact]
    public void Muxe_ListedStreams_PlusFromRest_MapsContainerAndListedFiles()
    {
        var videoInfo = new MediaVideoStreamInfo { Index = 0, Format = "h264" };
        var audioInfo = new MediaAudioStreamInfo { Index = 0, Format = "aac", Language = "eng" };
        audioInfo.Metadata["title"] = "Original Audio";

        var streams = new List<MediaStream>
        {
            MediaStream.FromStreamInfo("video.mp4", videoInfo),
            MediaStream.FromStreamInfo("audio.m4a", audioInfo)
        };

        SetupMuxer().Muxe(streams, "out.mkv",
            new MuxOptions().From("original.mkv").Subtitles().SideStreams().Container());

        var cmd = LastCmd;
        Assert.Contains("-i \"original.mkv\"", cmd, StringComparison.Ordinal);
        Assert.Contains("-i \"video.mp4\"", cmd, StringComparison.Ordinal);
        Assert.Contains("-i \"audio.m4a\"", cmd, StringComparison.Ordinal);
        Assert.Contains("-map_metadata 0", cmd, StringComparison.Ordinal);
        Assert.Contains("-map_chapters 0", cmd, StringComparison.Ordinal);
        Assert.Contains("language=eng", cmd, StringComparison.Ordinal);
        Assert.Contains("-map 1:0", cmd, StringComparison.Ordinal);
        Assert.Contains("-map 2:0", cmd, StringComparison.Ordinal);
    }

    [Fact]
    public void Muxe_InsertPrimaryAac432Hz_DemotesOriginalDefault()
    {
        // Keep video; new AAC is first audio with frequency=432Hz and default; original audio cleared.
        var streams = new List<MediaStream>
        {
            S("source.mp4", 0, "h264", FFmpegStreamType.Video),
            S("new.aac", 0, "aac", FFmpegStreamType.Audio, s =>
            {
                s.Metadata["frequency"] = "432Hz";
                s.Disposition = DefaultDisposition();
            }),
            S("source.mp4", 1, "aac", FFmpegStreamType.Audio,
                s => s.Disposition = ClearDisposition())
        };

        SetupMuxer().Muxe(streams, "dest.mkv", new MuxOptions());

        var cmd = LastCmd;
        Assert.Contains("-map 0:0", cmd, StringComparison.Ordinal); // video
        Assert.Contains("-map 1:0", cmd, StringComparison.Ordinal); // new primary
        Assert.Contains("-map 0:1", cmd, StringComparison.Ordinal); // demoted
        Assert.Contains("frequency=432Hz", cmd, StringComparison.Ordinal);
        Assert.Contains("-disposition:1 default", cmd, StringComparison.Ordinal);
        Assert.Contains("-disposition:2 0", cmd, StringComparison.Ordinal);
    }

    [Fact]
    public void Muxe_ThreeAudioStreams_LatestIsDefaultAtFirstAudioSlot()
    {
        // video + 3 audio: newest first (default), then middle, then original — only one default.
        var streams = new List<MediaStream>
        {
            S("source.mkv", 0, "h264", FFmpegStreamType.Video),
            S("new2.aac", 0, "aac", FFmpegStreamType.Audio, s =>
            {
                s.Metadata["frequency"] = "432Hz";
                s.Disposition = DefaultDisposition();
            }),
            S("new1.aac", 0, "aac", FFmpegStreamType.Audio,
                s => s.Disposition = ClearDisposition()),
            S("source.mkv", 1, "aac", FFmpegStreamType.Audio,
                s => s.Disposition = ClearDisposition())
        };

        SetupMuxer().Muxe(streams, "dest.mkv", new MuxOptions());

        var cmd = LastCmd;
        Assert.Contains("-map 0:0", cmd, StringComparison.Ordinal);
        Assert.Contains("-map 1:0", cmd, StringComparison.Ordinal); // newest
        Assert.Contains("-map 2:0", cmd, StringComparison.Ordinal);
        Assert.Contains("-map 0:1", cmd, StringComparison.Ordinal);
        Assert.Contains("-disposition:1 default", cmd, StringComparison.Ordinal);
        Assert.Contains("-disposition:2 0", cmd, StringComparison.Ordinal);
        Assert.Contains("-disposition:3 0", cmd, StringComparison.Ordinal);
        Assert.Equal(1, CountOccurrences(cmd, "-disposition:1 default"));
    }

    [Fact]
    public void Muxe_StripFrequencyTaggedStreams_MapsOnlyKeptTracks()
    {
        // After probing, drop any stream that carries a frequency tag; remux the rest.
        var video = S("src.mkv", 0, "h264", FFmpegStreamType.Video);
        var a440 = S("src.mkv", 1, "aac", FFmpegStreamType.Audio, s => s.Metadata["frequency"] = "440");
        var a432 = S("src.mkv", 2, "aac", FFmpegStreamType.Audio, s => s.Metadata["frequency"] = "432");
        var sub = S("src.mkv", 3, "subrip", FFmpegStreamType.Subtitle);
        var keep = new[] { video, a440, a432, sub }
            .Where(s => !s.Metadata.ContainsKey("frequency"))
            .ToList();

        SetupMuxer().Muxe(keep, "dest.mkv", new MuxOptions().From("src.mkv").Container());

        var cmd = LastCmd;
        Assert.Contains("-map 0:0", cmd, StringComparison.Ordinal);
        Assert.Contains("-map 0:3", cmd, StringComparison.Ordinal);
        Assert.DoesNotContain("-map 0:1", cmd, StringComparison.Ordinal);
        Assert.DoesNotContain("-map 0:2", cmd, StringComparison.Ordinal);
        Assert.Contains("-map_metadata 0", cmd, StringComparison.Ordinal);
    }

    [Fact]
    public void Muxe_ChangeDefaultOnly_SameOrderSameMaps()
    {
        // Same file, same stream order: clear old default, set new default — no reorder.
        var streams = new List<MediaStream>
        {
            S("src.mkv", 0, "h264", FFmpegStreamType.Video),
            S("src.mkv", 1, "aac", FFmpegStreamType.Audio, s => s.Disposition = ClearDisposition()),
            S("src.mkv", 2, "aac", FFmpegStreamType.Audio, s => s.Disposition = DefaultDisposition())
        };

        SetupMuxer().Muxe(streams, "dest.mkv", new MuxOptions());

        var cmd = LastCmd;
        Assert.Equal(1, CountOccurrences(cmd, "-i \"src.mkv\""));
        Assert.Contains("-map 0:0", cmd, StringComparison.Ordinal);
        Assert.Contains("-map 0:1", cmd, StringComparison.Ordinal);
        Assert.Contains("-map 0:2", cmd, StringComparison.Ordinal);
        Assert.Contains("-disposition:1 0", cmd, StringComparison.Ordinal);
        Assert.Contains("-disposition:2 default", cmd, StringComparison.Ordinal);
    }

    [Fact]
    public void Muxe_Stream_MetadataLanguageDisposition_Emitted()
    {
        var audio = S("audio.aac", 0, "aac", FFmpegStreamType.Audio, s =>
        {
            s.Language = "eng";
            s.Metadata["title"] = "Pitched Audio";
            s.Metadata["frequency"] = "432";
            s.Disposition = DefaultDisposition();
        });

        SetupMuxer().Muxe(new[] { audio }, "dest.m4a", new MuxOptions());

        var cmd = LastCmd;
        Assert.Contains("-metadata:s:0 language=eng", cmd, StringComparison.Ordinal);
        Assert.Contains("-metadata:s:0 frequency=432", cmd, StringComparison.Ordinal);
        Assert.Contains("-disposition:0 default", cmd, StringComparison.Ordinal);
        Assert.DoesNotContain("-disposition:s:", cmd, StringComparison.Ordinal);
    }

    [Fact]
    public void Muxe_EmptyDisposition_EmitsClear_NullOmits()
    {
        SetupMuxer().Muxe(
            new[] { S("a.aac", 0, "aac", FFmpegStreamType.Audio, s => s.Disposition = ClearDisposition()) },
            "dest.m4a");
        Assert.Contains("-disposition:0 0", LastCmd, StringComparison.Ordinal);

        _factory.Instances.Clear();
        SetupMuxer().Muxe(
            new[] { S("a.aac", 0, "aac", FFmpegStreamType.Audio) },
            "dest.m4a");
        Assert.DoesNotContain("-disposition:", LastCmd, StringComparison.Ordinal);
    }

    // ── AAC / PCM remux helpers ─────────────────────────────────────────────
    // ── AAC / PCM remux helpers (format + type driven, not filename tricks) ──

    [Fact]
    public void Muxe_AacAudioWithVideo_AddsBitstreamFilter()
    {
        var muxer = SetupMuxer();
        var streams = new List<MediaStream>
        {
            S("video.mp4", 0, "h264", FFmpegStreamType.Video),
            S("audio.aac", 0, "aac", FFmpegStreamType.Audio)
        };

        muxer.Muxe(streams, "dest.mp4");

        var cmd = LastCmd;
        Assert.Contains(FixAac, cmd, StringComparison.Ordinal);
        AssertFFmpegManager(true, true);
    }

    [Fact]
    public void Muxe_AacAudioAlone_DoesNotAddBitstreamFilter()
    {
        var muxer = SetupMuxer();
        muxer.Muxe(new[] { S("audio.aac", 0, "aac", FFmpegStreamType.Audio) }, "dest.m4a");

        Assert.DoesNotContain(FixAac, LastCmd, StringComparison.Ordinal);
        AssertFFmpegManager(false, true);
    }

    [Fact]
    public void Muxe_PcmDvdAudio_UsesPcmS16le()
    {
        var muxer = SetupMuxer();
        muxer.Muxe(new[] { S("audio.wav", 0, "pcm_dvd", FFmpegStreamType.Audio) }, "dest.mkv");

        Assert.Contains(FixPcm, LastCmd, StringComparison.Ordinal);
        AssertFFmpegManager(false, true);
    }

    [Fact]
    public void Muxe_NonPcmAudio_DoesNotUsePcmS16le()
    {
        var muxer = SetupMuxer();
        muxer.Muxe(new[] { S("audio.m4a", 0, "aac", FFmpegStreamType.Audio) }, "dest.m4a");

        Assert.DoesNotContain(FixPcm, LastCmd, StringComparison.Ordinal);
    }

    // ── Elementary video → MKV workaround ───────────────────────────────────

    [Theory]
    [InlineData("video.264", "h264")]
    [InlineData("video.hevc", "hevc")]
    [InlineData("video.h265", "h265")]
    [InlineData("video.vvc", "vvc")]
    [InlineData("video.h266", "h266")]
    [InlineData("clip.266", "vvc")]
    public void Muxe_ElementaryVideoIntoMkv_RewritesPathToTempMp4(string videoPath, string format)
    {
        var muxer = SetupMuxer();
        var streams = new List<MediaStream>
        {
            S(videoPath, 0, format, FFmpegStreamType.Video),
            S("audio.aac", 0, "aac", FFmpegStreamType.Audio)
        };

        muxer.Muxe(streams, "dest.mkv", new MuxOptions());

        Assert.True(_factory.Instances.Count > 1);
        var finalCmd = LastCmd;
        Assert.Contains("temp.mp4", finalCmd, StringComparison.Ordinal);
        Assert.DoesNotContain(videoPath, finalCmd, StringComparison.Ordinal);
    }

    [Fact]
    public void Muxe_H264InMp4Container_IntoMkv_DoesNotUseElementaryWorkaround()
    {
        var muxer = SetupMuxer();
        var streams = new List<MediaStream>
        {
            S("movie.mp4", 0, "h264", FFmpegStreamType.Video),
            S("movie.mp4", 1, "aac", FFmpegStreamType.Audio)
        };

        muxer.Muxe(streams, "dest.mkv", new MuxOptions());

        Assert.Single(_factory.Instances);
        var cmd = LastCmd;
        Assert.Contains("movie.mp4", cmd, StringComparison.Ordinal);
        Assert.DoesNotContain("temp.mp4", cmd, StringComparison.Ordinal);
    }

    // ── MuxOptions fluent From API ──────────────────────────────────────────

    [Fact]
    public void Muxe_FromInt_IsInputFileIndexNotStreamListIndex()
    {
        var muxer = SetupMuxer();
        // Three stream entries, two unique files → -i 0 = fileA, -i 1 = fileB.
        var streams = new List<MediaStream>
        {
            S("fileA.mkv", 0, "h264", FFmpegStreamType.Video),
            S("fileA.mkv", 1, "aac", FFmpegStreamType.Audio),
            S("fileB.aac", 0, "aac", FFmpegStreamType.Audio)
        };

        muxer.Muxe(streams, "dest.mkv",
            new MuxOptions().From(0).ContainerTags().From(1).ContainerTags());

        var cmd = LastCmd;
        Assert.Equal(1, CountOccurrences(cmd, "-i \"fileA.mkv\""));
        Assert.Equal(1, CountOccurrences(cmd, "-i \"fileB.aac\""));
        Assert.Contains("-map_metadata 0", cmd, StringComparison.Ordinal);
        Assert.Contains("-map_metadata 1", cmd, StringComparison.Ordinal);
    }

    [Fact]
    public void Muxe_FromPath_OpensFileAndMapsContainerTags()
    {
        var muxer = SetupMuxer();
        var streams = new List<MediaStream>
        {
            S("source.mkv", 0, "h264", FFmpegStreamType.Video)
        };

        muxer.Muxe(streams, "dest.mkv", new MuxOptions().From("source.mkv").Metadata());

        Assert.Contains("-map_metadata 0", LastCmd, StringComparison.Ordinal);
    }

    [Fact]
    public void Muxe_FromPath_NotInStreamList_AddsInput()
    {
        var muxer = SetupMuxer();
        // Explicit stream from a.mkv; pull container tags from b.mkv (extra -i).
        var streams = new List<MediaStream>
        {
            S("a.mkv", 0, "h264", FFmpegStreamType.Video)
        };

        muxer.Muxe(streams, "dest.mkv",
            new MuxOptions().From("b.mkv").ContainerTags());

        var cmd = LastCmd;
        Assert.Contains("-i \"b.mkv\"", cmd, StringComparison.Ordinal);
        Assert.Contains("-i \"a.mkv\"", cmd, StringComparison.Ordinal);
        // From(path) is listed first, then stream-list paths.
        var iB = cmd.IndexOf("-i \"b.mkv\"", StringComparison.Ordinal);
        var iA = cmd.IndexOf("-i \"a.mkv\"", StringComparison.Ordinal);
        Assert.True(iB >= 0 && iA > iB);
        Assert.Contains("-map_metadata 0", cmd, StringComparison.Ordinal);
    }

    [Fact]
    public void Muxe_EmptyStreamList_WithFromVideo_Succeeds()
    {
        var muxer = SetupMuxer();
        // FakeMediaInfoReader: stream 0 = video → optional map only.

        var result = muxer.Muxe(
            Array.Empty<MediaStream>(),
            "dest.mkv",
            new MuxOptions().From("source.mkv").Video());

        Assert.Equal(CompletionStatus.Success, result);
        var cmd = LastCmd;
        Assert.Contains("-i \"source.mkv\"", cmd, StringComparison.Ordinal);
        Assert.Contains("-map 0:0", cmd, StringComparison.Ordinal);
        Assert.Contains(VideoCodec, cmd, StringComparison.Ordinal);
    }


    [Fact]
    public void Muxe_FromMedia_MapsVideoAudioFromProbe_DedupesListed()
    {
        var muxer = SetupMuxer();
        // Stream list already maps video 0; From().Media() also wants video+audio (+subs) — audio@1 added once.
        var streams = new List<MediaStream>
        {
            S("source.mkv", 0, "h264", FFmpegStreamType.Video)
        };

        muxer.Muxe(streams, "dest.mkv", new MuxOptions().From("source.mkv").Media());

        var cmd = LastCmd;
        Assert.Contains("-map 0:0", cmd, StringComparison.Ordinal);
        Assert.Contains("-map 0:1", cmd, StringComparison.Ordinal);
        Assert.Equal(1, CountOccurrences(cmd, "-map 0:0"));
    }

    [Fact]
    public void Muxe_AllThenAudioFalse_ExcludesAudioMaps()
    {
        // Reverse-style: everything except audio (listed video only + From rest without audio).
        var streams = new List<MediaStream>
        {
            S("source.mkv", 0, "h264", FFmpegStreamType.Video)
        };

        MuxOptions opts = new MuxOptions().From("source.mkv").All().Audio(false);
        Assert.True(opts.FromInputs[0].Video);
        Assert.False(opts.FromInputs[0].Audio);
        Assert.True(opts.FromInputs[0].Subtitles);
        Assert.True(opts.FromInputs[0].ContainerTags);

        SetupMuxer().Muxe(streams, "dest.mkv", opts);

        var cmd = LastCmd;
        // Fake probe has audio@1 — must not be auto-mapped.
        Assert.DoesNotContain("-map 0:1", cmd, StringComparison.Ordinal);
        Assert.Contains("-map 0:0", cmd, StringComparison.Ordinal);
        Assert.Contains("-map_metadata 0", cmd, StringComparison.Ordinal);
    }


    [Fact]
    public void Muxe_FluentChain_ComposesOnOneMuxOptions()
    {
        var opts = new MuxOptions()
            .From("a.mkv").ContainerTags()
            .From("b.mkv").Chapters()
            .Done()
            .WithAdditionalArguments("-movflags +faststart");
        opts.Metadata["title"] = "Album";

        Assert.Equal(2, opts.FromInputs.Count);
        Assert.True(opts.FromInputs[0].ContainerTags);
        Assert.True(opts.FromInputs[1].Chapters);
        Assert.Equal("-movflags +faststart", opts.AdditionalArguments);
        Assert.Equal("Album", opts.Metadata["title"]);

        var muxer = SetupMuxer();
        muxer.Muxe(new[] { S("a.mkv", 0, "h264", FFmpegStreamType.Video) }, "dest.mkv", opts);

        var cmd = LastCmd;
        Assert.Contains("-map_metadata 0", cmd, StringComparison.Ordinal);
        Assert.Contains("-map_chapters 1", cmd, StringComparison.Ordinal);
        Assert.Contains("-movflags +faststart", cmd, StringComparison.Ordinal);
        Assert.Contains("-metadata title=Album", cmd, StringComparison.Ordinal);
    }

    [Fact]
    public void Muxe_MuxOptionsImplicitFromBuilder()
    {
        MuxOptions opts = new MuxOptions().From(0).ContainerTags();
        Assert.Single(opts.FromInputs);
        Assert.Equal(0, opts.FromInputs[0].InputIndex);
    }

    // ── Simple Muxe(videoFile, audioFile) ───────────────────────────────────
    // FakeMediaInfoReader always exposes video@0 and audio@1 — path names only select which file is probed.

    [Fact]
    public void Muxe_Simple_AudioVideo_Success()
    {
        var muxer = SetupMuxer();
        Assert.Equal(CompletionStatus.Success, muxer.Muxe("video.mp4", "audio.m4a", "dest.mp4"));
        AssertFFmpegManager(true, true);
    }


    [Fact]
    public void Muxe_Simple_AudioOnly_Success()
    {
        var muxer = SetupMuxer();
        Assert.Equal(CompletionStatus.Success, muxer.Muxe(null, "audio.m4a", "dest.mp4"));
        AssertFFmpegManager(false, true);
    }

    [Fact]
    public void Muxe_Simple_VideoOnly_Success()
    {
        var muxer = SetupMuxer();
        Assert.Equal(CompletionStatus.Success, muxer.Muxe("video.mp4", null, "dest.mkv"));
        AssertFFmpegManager(true, false);
    }

    [Theory]
    [InlineData(null, null, null, typeof(ArgumentNullException))]
    [InlineData("", "", "", typeof(ArgumentException))]
    [InlineData("video.mp4", "", "", typeof(ArgumentException))]
    [InlineData("", "audio.aac", "", typeof(ArgumentException))]
    [InlineData("", "", "dest.mp4", typeof(ArgumentException))]
    [InlineData("video.mp4", "audio.aac", null, typeof(ArgumentNullException))]
    public void Muxe_Simple_EmptyArgs_ThrowsException(string videoFile, string audioFile, string destination, Type ex)
    {
        var muxer = SetupMuxer();
        Assert.Throws(ex, () => muxer.Muxe(videoFile, audioFile, destination));
    }

    [Fact]
    public void Muxe_Simple_ParamOptions_ReturnsSame()
    {
        var muxer = SetupMuxer();
        var options = new ProcessOptionsEncoder();
        muxer.Muxe("video", "audio", "dest", options);
        Assert.Same(options, _factory.Instances[0].Options);
    }

    [Fact]
    public void Muxe_Simple_ParamCallback_CallbackCalled()
    {
        var muxer = SetupMuxer();
        var callbackCalled = 0;
        muxer.Muxe("video", "audio", "dest", null, (_, _) => callbackCalled++);
        Assert.Equal(1, callbackCalled);
    }

    // ── Extract / Concatenate / Truncate ────────────────────────────────────

    [Fact]
    public void ExtractAudio_Valid_Success()
    {
        var muxer = SetupMuxer();
        Assert.Equal(CompletionStatus.Success, muxer.ExtractAudio("source", "dest"));
        Assert.Single(_factory.Instances);
        AssertFFmpegManager(false, true);
    }

    [Theory]
    [MemberData(nameof(TestDataSource.NullAndEmptyStrings), 2, MemberType = typeof(TestDataSource))]
    public void ExtractAudio_EmptyArgs_ThrowsException(string source, string destination, Type ex)
    {
        var muxer = SetupMuxer();
        Assert.Throws(ex, () => muxer.ExtractAudio(source, destination));
    }

    [Fact]
    public void ExtractAudio_ParamOptions_ReturnsSame()
    {
        var muxer = SetupMuxer();
        var options = new ProcessOptionsEncoder();
        muxer.ExtractAudio("source", "dest", options);
        Assert.Same(options, _factory.Instances[0].Options);
    }

    [Fact]
    public void ExtractAudio_ParamCallback_CallbackCalled()
    {
        var muxer = SetupMuxer();
        var n = 0;
        muxer.ExtractAudio("source", "dest", null, (_, _) => n++);
        Assert.Equal(1, n);
    }

    [Fact]
    public void ExtractVideo_Valid_Success()
    {
        var muxer = SetupMuxer();
        Assert.Equal(CompletionStatus.Success, muxer.ExtractVideo("source", "dest"));
        Assert.Single(_factory.Instances);
        AssertFFmpegManager(true, false);
    }

    [Theory]
    [MemberData(nameof(TestDataSource.NullAndEmptyStrings), 2, MemberType = typeof(TestDataSource))]
    public void ExtractVideo_EmptyArgs_ThrowsException(string source, string destination, Type ex)
    {
        var muxer = SetupMuxer();
        Assert.Throws(ex, () => muxer.ExtractVideo(source, destination));
    }

    [Fact]
    public void ExtractVideo_ParamOptions_ReturnsSame()
    {
        var muxer = SetupMuxer();
        var options = new ProcessOptionsEncoder();
        muxer.ExtractVideo("source", "dest", options);
        Assert.Same(options, _factory.Instances[0].Options);
    }

    [Fact]
    public void ExtractVideo_ParamCallback_CallbackCalled()
    {
        var muxer = SetupMuxer();
        var n = 0;
        muxer.ExtractVideo("source", "dest", null, (_, _) => n++);
        Assert.Equal(1, n);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Concatenate_Valid_Success(int fileCount)
    {
        var muxer = SetupMuxer();
        var files = Enumerable.Range(1, fileCount).Select(i => "file" + i).ToList();

        var result = muxer.Concatenate(files, "dest.mkv");

        Assert.Equal(CompletionStatus.Success, result);
        Assert.Single(_factory.Instances);
        _output.WriteLine(_factory.Instances[0].CommandWithArgs);
    }

    [Theory]
    [MemberData(nameof(GenerateConcatenate_Empty))]
    public void Concatenate_EmptyArgs_ThrowsException(IEnumerable<string> files, string destination, Type ex)
    {
        var muxer = SetupMuxer();
        Assert.Throws(ex, () => muxer.Concatenate(files, destination));
    }

    public static IEnumerable<object[]> GenerateConcatenate_Empty()
    {
        yield return new object[] { null, "dest.mkv", typeof(ArgumentNullException) };
        yield return new object[] { new List<string>(), "dest.mkv", typeof(ArgumentException) };
        yield return new object[] { new List<string> { "file1" }, null, typeof(ArgumentNullException) };
        yield return new object[] { new List<string> { "file1" }, "", typeof(ArgumentException) };
    }

    [Fact]
    public void Concatenate_ParamOptions_ReturnsSame()
    {
        var muxer = SetupMuxer();
        var options = new ProcessOptionsEncoder();
        muxer.Concatenate(new[] { "file1", "file2" }, "dest.mkv", options);
        Assert.Same(options, _factory.Instances[0].Options);
    }

    [Fact]
    public void Concatenate_ParamCallback_CallbackCalled()
    {
        var muxer = SetupMuxer();
        var n = 0;
        muxer.Concatenate(new[] { "file1", "file2" }, "dest.mkv", null, (_, _) => n++);
        Assert.Equal(1, n);
    }

    [Fact]
    public void Truncate_Valid_Success()
    {
        var muxer = SetupMuxer();
        var result = muxer.Truncate("source", "dest.mkv", TimeSpan.Zero, TimeSpan.FromSeconds(10));
        Assert.Equal(CompletionStatus.Success, result);
        Assert.Single(_factory.Instances);
        _output.WriteLine(_factory.Instances[0].CommandWithArgs);
    }

    [Theory]
    [MemberData(nameof(TestDataSource.NullAndEmptyStrings), 2, MemberType = typeof(TestDataSource))]
    public void Truncate_EmptyArgs_ThrowsException(string source, string destination, Type ex)
    {
        var muxer = SetupMuxer();
        Assert.Throws(ex, () => muxer.Truncate(source, destination, TimeSpan.Zero, TimeSpan.FromSeconds(10)));
    }

    [Fact]
    public void Truncate_ParamOptions_ReturnsSame()
    {
        var muxer = SetupMuxer();
        var options = new ProcessOptionsEncoder();
        muxer.Truncate("source", "dest.mkv", TimeSpan.Zero, TimeSpan.FromSeconds(10), options);
        Assert.Same(options, _factory.Instances[0].Options);
    }

    [Fact]
    public void Truncate_ParamCallback_CallbackCalled()
    {
        var muxer = SetupMuxer();
        var n = 0;
        muxer.Truncate("source", "dest.mkv", TimeSpan.Zero, TimeSpan.FromSeconds(10), null, (_, _) => n++);
        Assert.Equal(1, n);
    }
}
