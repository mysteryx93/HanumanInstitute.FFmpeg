// ReSharper disable PossibleMultipleEnumeration

using System.Diagnostics.CodeAnalysis;
// ReSharper disable StringLiteralTypo

namespace HanumanInstitute.FFmpeg.IntegrationTests;

[SuppressMessage("Usage", "xUnit1026:Theory methods should use all of their parameters")]
public class MediaMuxerTests
{
    private readonly OutputFeeder _feed;
    private IEncoderService _factory;

    public MediaMuxerTests(ITestOutputHelper output)
    {
        _feed = new OutputFeeder(output);
    }

    private IMediaMuxer SetupMuxer()
    {
        _factory = FactoryConfig.CreateWithConfig();
        return new MediaMuxer(_factory, new FileSystemService(), new MediaInfoReader(_factory));
    }

    private FileInfoFFmpeg GetFileInfo(string path)
    {
        var info = new MediaInfoReader(_factory);
        return info.GetFileInfo(path);
    }

    public static IEnumerable<object[]> GenerateMuxeLists_Valid()
    {
        yield return
        [
            new List<MediaStream> {
                new(AppPaths.Mpeg4, 2, "h264", FFmpegStreamType.Video),
            },
            ".mp4", 1
        ];
        // zelda.flv audio stream (adpcm_swf). Dest must accept that codec with -c copy;
        // modern FFmpeg rejects adpcm_swf in MKV ("can only be written to WAVE…").
        yield return
        [
            new List<MediaStream> {
                new(AppPaths.Flv, 1, "flv", FFmpegStreamType.Audio)
            },
            ".flv", 1
        ];
        yield return
        [
            new List<MediaStream> {
                new(AppPaths.StreamAac, 0, "aac", FFmpegStreamType.Audio),
                new(AppPaths.StreamH264, 0, "h264", FFmpegStreamType.Video),
                new(AppPaths.StreamVp9, 0, "vp9", FFmpegStreamType.Video)
            },
            ".mkv", 3
        ];
        yield return
        [
            new List<MediaStream> {
                new(AppPaths.StreamAac, 0, "aac", FFmpegStreamType.Audio),
                new(AppPaths.StreamH264, 0, "h264", FFmpegStreamType.Video),
                new(AppPaths.StreamVp9, 0, "vp9", FFmpegStreamType.Video),
                new(AppPaths.StreamOpus, 0, "opus", FFmpegStreamType.Audio)
            },
            ".mkv", 4
        ];
    }

    public static IEnumerable<object[]> GenerateMuxeLists_Invalid()
    {
        yield return
        [
            new List<MediaStream> {
                new("invalidfile", 0, "", FFmpegStreamType.Video),
            },
            ".mp4", 1
        ];
    }

    public static IEnumerable<object[]> GenerateConcatenate_Valid()
    {
        yield return
        [
            new List<string> {
                AppPaths.Part1
            },
            ".mp4"
        ];
        yield return
        [
            new List<string> {
                AppPaths.Part1, AppPaths.Part2, AppPaths.Part3
            },
            ".mp4"
        ];
    }

    public static IEnumerable<object[]> GenerateConcatenate_Invalid()
    {
        yield return
        [
            new List<string> {
                "invalidfile"
            },
            ".mp4"
        ];
    }

    public static IEnumerable<object[]> GenerateTruncate_Valid()
    {
        yield return
        [
            AppPaths.StreamVp9,
            ".webm",
            null,
            TimeSpan.FromSeconds(5)
        ];
        yield return
        [
            AppPaths.Mpeg4WithAudio,
            ".mp4",
            TimeSpan.FromSeconds(4),
            TimeSpan.FromSeconds(3)
        ];
        yield return
        [
            AppPaths.StreamOpus,
            ".ogg",
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(8)
        ];
    }

    public static IEnumerable<object[]> GenerateTruncate_Invalid()
    {
        yield return
        [
            "invalidfile",
            ".webm",
            null,
            TimeSpan.FromSeconds(5)
        ];
    }


    [Theory]
    [InlineData(AppPaths.StreamH264, AppPaths.StreamAac, ".mp4", 2)]
    [InlineData(AppPaths.StreamVp9, AppPaths.StreamOpus, ".webm", 2)]
    [InlineData(AppPaths.StreamH264, AppPaths.StreamOpus, ".mkv", 2)]
    // Mpeg2 video + zelda.flv audio (adpcm_swf). .mkv rejects adpcm_swf with -c copy; .mov accepts both.
    [InlineData(AppPaths.Mpeg2, AppPaths.Flv, ".mov", 2)]
    [InlineData(AppPaths.Flv, AppPaths.StreamOpus, ".mkv", 2)]
    [InlineData(AppPaths.StreamH264, null, ".mp4", 1)]
    [InlineData("", AppPaths.StreamOpus, ".webm", 1)]
    public void Muxe_Simple_Valid_Success(string videoFile, string audioFile, string destExt, int streamCount)
    {
        var srcVideo = AppPaths.GetInputFile(videoFile);
        var srcAudio = AppPaths.GetInputFile(audioFile);
        var dest = AppPaths.PrepareDestPath("Muxe", videoFile, destExt);
        var muxer = SetupMuxer();

        var result = muxer.Muxe(srcVideo, srcAudio, dest, null, _feed.RunCallback);

        Assert.Equal(CompletionStatus.Success, result);
        Assert.True(File.Exists(dest));
        var fileInfo = GetFileInfo(dest);
        Assert.Equal(streamCount, fileInfo.FileStreams.Count);
    }

    [Theory]
    [MemberData(nameof(GenerateMuxeLists_Valid))]
    public void Muxe_List_Valid_Success(IEnumerable<MediaStream> fileStreams, string destExt, int streamCount)
    {
        var streams = fileStreams
            .Select(s => new MediaStream(AppPaths.GetInputFile(s.Path), s.Index, s.Format, s.Type))
            .ToList();
        var dest = AppPaths.PrepareDestPath("MuxeList", streams[0].Path, destExt);
        var muxer = SetupMuxer();

        var result = muxer.Muxe(streams, dest, null, null, _feed.RunCallback);

        Assert.Equal(CompletionStatus.Success, result);
        Assert.True(File.Exists(dest));
        var fileInfo = GetFileInfo(dest);
        Assert.Equal(streamCount, fileInfo.FileStreams.Count);
    }

    [Theory]
    [MemberData(nameof(GenerateMuxeLists_Invalid))]
    public void Muxe_List_Invalid_ReturnsStatusFailed(IEnumerable<MediaStream> fileStreams, string destExt, int _)
    {
        var streams = fileStreams
            .Select(s => new MediaStream(AppPaths.GetInputFile(s.Path), s.Index, s.Format, s.Type))
            .ToList();
        var dest = AppPaths.PrepareDestPath("MuxeFailed", streams[0].Path, destExt);
        var muxer = SetupMuxer();

        var result = muxer.Muxe(streams, dest, callback: _feed.RunCallback);

        Assert.Equal(CompletionStatus.Failed, result);
    }

    [Theory]
    [InlineData(AppPaths.StreamOpus, ".ogg")]
    [InlineData(AppPaths.Mpeg4WithAudio, ".mkv")]
    // zelda.flv audio is adpcm_swf; -c copy into .mkv fails on modern FFmpeg — keep FLV source, use .flv dest.
    [InlineData(AppPaths.Flv, ".flv")]
    public void ExtractAudio_Valid_Success(string source, string destExt)
    {
        var src = AppPaths.GetInputFile(source);
        var dest = AppPaths.PrepareDestPath("ExtractAudio", source, destExt);
        var muxer = SetupMuxer();

        var result = muxer.ExtractAudio(src, dest, null, _feed.RunCallback);

        Assert.Equal(CompletionStatus.Success, result);
        Assert.True(File.Exists(dest));
    }

    [Theory]
    [InlineData(AppPaths.Mpeg2, ".aaa")]
    public void ExtractAudio_WrongExtension_StatusFailed(string source, string destExt)
    {
        var src = AppPaths.GetInputFile(source);
        var dest = AppPaths.PrepareDestPath("ExtractAudio", source, destExt);
        var muxer = SetupMuxer();

        var result = muxer.ExtractAudio(src, dest, null, _feed.RunCallback);

        Assert.Equal(CompletionStatus.Failed, result);
        Assert.False(File.Exists(dest));
    }


    [Theory]
    [InlineData(AppPaths.Mpeg2, ".mp4")]
    [InlineData(AppPaths.Mpeg4, ".mp4")]
    [InlineData(AppPaths.Flv, ".mkv")]
    public void ExtractVideo_Valid_Success(string source, string destExt)
    {
        var src = AppPaths.GetInputFile(source);
        var dest = AppPaths.PrepareDestPath("ExtractVideo", source, destExt);
        var muxer = SetupMuxer();

        var result = muxer.ExtractVideo(src, dest, null, _feed.RunCallback);

        Assert.Equal(CompletionStatus.Success, result);
        Assert.True(File.Exists(dest));
    }

    [Theory]
    [InlineData(AppPaths.Mpeg4, ".bbb")]
    public void ExtractVideo_WrongExtension_StatusFailed(string source, string destExt)
    {
        var src = AppPaths.GetInputFile(source);
        var dest = AppPaths.PrepareDestPath("ExtractVideo", source, destExt);
        var muxer = SetupMuxer();

        var result = muxer.ExtractVideo(src, dest, null, _feed.RunCallback);

        Assert.Equal(CompletionStatus.Failed, result);
        Assert.False(File.Exists(dest));
    }

    [Theory]
    [MemberData(nameof(GenerateConcatenate_Valid))]
    public void Concatenate_Valid_Success(IEnumerable<string> source, string destExt)
    {
        var src = source.Select(AppPaths.GetInputFile).ToList();
        var dest = AppPaths.PrepareDestPath("Concatenate", source.First(), destExt);
        var muxer = SetupMuxer();

        var result = muxer.Concatenate(src, dest, null, _feed.RunCallback);

        Assert.Equal(CompletionStatus.Success, result);
        Assert.True(File.Exists(dest));
    }

    [Theory]
    [MemberData(nameof(GenerateConcatenate_Invalid))]
    public void Concatenate_Invalid_StatusFailed(IEnumerable<string> source, string destExt)
    {
        var src = source.Select(AppPaths.GetInputFile).ToList();
        var dest = AppPaths.PrepareDestPath("Concatenate", source.First(), destExt);
        var muxer = SetupMuxer();

        var result = muxer.Concatenate(src, dest, null, _feed.RunCallback);

        Assert.Equal(CompletionStatus.Failed, result);
    }

    [Theory]
    [MemberData(nameof(GenerateTruncate_Valid))]
    public void Truncate_Valid_Success(string source, string destExt, TimeSpan? startPos, TimeSpan? duration)
    {
        var src = AppPaths.GetInputFile(source);
        var dest = AppPaths.PrepareDestPath("Truncate", source, destExt);
        var muxer = SetupMuxer();
        void Started(object s, ProcessStartedEventArgs e)
        {
            _feed.RunCallback(s, e);
        }

        var result = muxer.Truncate(src, dest, startPos, duration, null, Started);

        Assert.Equal(CompletionStatus.Success, result);
        Assert.True(File.Exists(dest));
        var fileInfo = GetFileInfo(dest);
        if (duration.HasValue)
        {
            Assert.True(Math.Abs((duration.Value - fileInfo.FileDuration).TotalSeconds) < .1, "Truncate did not produce expected file duration.");
        }
    }

    [Theory]
    [MemberData(nameof(GenerateTruncate_Invalid))]
    public void Truncate_Invalid_StatusFailed(string source, string destExt, TimeSpan? startPos, TimeSpan? duration)
    {
        var src = AppPaths.GetInputFile(source);
        var dest = AppPaths.PrepareDestPath("Truncate", source, destExt);
        var muxer = SetupMuxer();

        var result = muxer.Truncate(src, dest, startPos, duration, null, _feed.RunCallback);

        Assert.Equal(CompletionStatus.Failed, result);
    }

    // ── Usage scenarios (public API shape) ──────────────────────────────────

    private static bool HasFrequencyTag(MediaStreamInfo s) =>
        s.Metadata.ContainsKey("frequency");

    private static string FrequencyOf(MediaStreamInfo s) =>
        s.Metadata.TryGetValue("frequency", out var v) ? v : "";

    private static StreamDisposition DefaultDisposition()
    {
        var d = new StreamDisposition();
        d.Set("default");
        return d;
    }

    private static StreamDisposition ClearDisposition() => new();

    /// <summary>
    /// Streams from separate files (probed at those paths) plus container extras from another file.
    /// </summary>
    [Fact]
    public void Muxe_ListedStreams_PlusFromRest_PullsContainerFromOriginal()
    {
        var original = AppPaths.GetInputFile(AppPaths.TaggedAttachment); // video + font attachment + container title
        var videoFile = AppPaths.GetInputFile(AppPaths.StreamH264);
        var audioFile = AppPaths.GetInputFile(AppPaths.StreamAac);
        var dest = AppPaths.PrepareDestPath("MuxeFromInfoRest", AppPaths.TaggedAttachment, ".mkv");
        var muxer = SetupMuxer();
        var videoInfo = GetFileInfo(videoFile).FileStreams.First(s => s.StreamType == FFmpegStreamType.Video);
        var aacInfo = GetFileInfo(audioFile).FileStreams.First();

        var streams = new List<MediaStream>
        {
            MediaStream.FromStreamInfo(videoFile, videoInfo),
            MediaStream.FromStreamInfo(audioFile, aacInfo)
        };

        var result = muxer.Muxe(streams, dest,
            new MuxOptions().From(original).Subtitles().SideStreams().Container(),
            null, _feed.RunCallback);

        Assert.Equal(CompletionStatus.Success, result);
        var outInfo = GetFileInfo(dest);
        Assert.Contains(outInfo.FileStreams, s => s.StreamType == FFmpegStreamType.Video);
        Assert.Contains(outInfo.FileStreams, s => s.StreamType == FFmpegStreamType.Audio);
        Assert.Contains(outInfo.FileStreams, s => s.StreamType == FFmpegStreamType.Attachment);
        Assert.Equal("With Attachment", outInfo.Metadata["title"]);
    }

    /// <summary>
    /// Keep video; insert new AAC as first audio with frequency=432Hz and default; demote original (no second default).
    /// </summary>
    [Fact]
    public void Muxe_InsertPrimaryAac432Hz_NoDuplicateDefault()
    {
        var src = AppPaths.GetInputFile(AppPaths.Mpeg4WithAudio);
        var newAac = AppPaths.GetInputFile(AppPaths.StreamAac);
        var dest = AppPaths.PrepareDestPath("MuxePrimary432", AppPaths.Mpeg4WithAudio, ".mkv");
        var muxer = SetupMuxer();
        var sourceInfo = GetFileInfo(src);
        var aacInfo = GetFileInfo(newAac).FileStreams.First();

        var video = sourceInfo.FileStreams.First(s => s.StreamType == FFmpegStreamType.Video);
        var originalAudio = sourceInfo.FileStreams.First(s => s.StreamType == FFmpegStreamType.Audio);

        var primary = MediaStream.FromStreamInfo(newAac, aacInfo);
        primary.Metadata["frequency"] = "432Hz";
        primary.Disposition = DefaultDisposition();

        var demoted = MediaStream.FromStreamInfo(src, originalAudio);
        demoted.Disposition = ClearDisposition();

        var streams = new List<MediaStream>
        {
            MediaStream.FromStreamInfo(src, video),
            primary,
            demoted
        };

        var result = muxer.Muxe(streams, dest, new MuxOptions().From(src).Container(), null, _feed.RunCallback);

        Assert.Equal(CompletionStatus.Success, result);
        var outInfo = GetFileInfo(dest);
        var audios = outInfo.FileStreams.OfType<MediaAudioStreamInfo>().ToList();
        Assert.Equal(2, audios.Count);
        Assert.Equal("432Hz", FrequencyOf(audios[0]));
        Assert.True(audios[0].Disposition.Has("default"));
        Assert.False(audios[1].Disposition.Has("default"));
        Assert.Equal(1, audios.Count(a => a.Disposition.Has("default")));
    }

    /// <summary>
    /// Three audio tracks: newest first with frequency + default; older ones cleared — only one default.
    /// </summary>
    [Fact]
    public void Muxe_ThreeAudioStreams_NewestDefaultAtPositionZero()
    {
        var src = AppPaths.GetInputFile(AppPaths.Mpeg4WithAudio);
        var aacPath = AppPaths.GetInputFile(AppPaths.StreamAac);
        var opusPath = AppPaths.GetInputFile(AppPaths.StreamOpus);
        var dest = AppPaths.PrepareDestPath("MuxeThreeAudio", AppPaths.Mpeg4WithAudio, ".mkv");
        var muxer = SetupMuxer();
        var sourceInfo = GetFileInfo(src);
        var aacInfo = GetFileInfo(aacPath).FileStreams.First();
        var opusInfo = GetFileInfo(opusPath).FileStreams.First();

        var video = sourceInfo.FileStreams.First(s => s.StreamType == FFmpegStreamType.Video);
        var originalAudio = sourceInfo.FileStreams.First(s => s.StreamType == FFmpegStreamType.Audio);

        var newest = MediaStream.FromStreamInfo(aacPath, aacInfo);
        newest.Metadata["frequency"] = "432Hz";
        newest.Disposition = DefaultDisposition();

        var middle = MediaStream.FromStreamInfo(opusPath, opusInfo);
        middle.Disposition = ClearDisposition();

        var oldest = MediaStream.FromStreamInfo(src, originalAudio);
        oldest.Disposition = ClearDisposition();

        var streams = new List<MediaStream>
        {
            MediaStream.FromStreamInfo(src, video),
            newest,
            middle,
            oldest
        };

        var result = muxer.Muxe(streams, dest, new MuxOptions().From(src).Container(), null, _feed.RunCallback);

        Assert.Equal(CompletionStatus.Success, result);
        var audios = GetFileInfo(dest).FileStreams.OfType<MediaAudioStreamInfo>().ToList();
        Assert.Equal(3, audios.Count);
        Assert.Equal("432Hz", FrequencyOf(audios[0]));
        Assert.True(audios[0].Disposition.Has("default"));
        Assert.False(audios[1].Disposition.Has("default"));
        Assert.False(audios[2].Disposition.Has("default"));
        Assert.Equal(1, audios.Count(a => a.Disposition.Has("default")));
    }

    /// <summary>
    /// Remove every stream that has a frequency tag; preserve video, subs, and container tags.
    /// </summary>
    [Fact]
    public void Muxe_StripFrequencyTaggedStreams_PreservesRest()
    {
        var src = AppPaths.GetInputFile(AppPaths.TaggedMkv); // both audio tracks have frequency
        var dest = AppPaths.PrepareDestPath("MuxeStripFreq", AppPaths.TaggedMkv, ".mkv");
        var muxer = SetupMuxer();
        var info = GetFileInfo(src);

        var keep = info.FileStreams
            .Where(s => !HasFrequencyTag(s))
            .Select(s => MediaStream.FromStreamInfo(src, s))
            .ToList();

        var result = muxer.Muxe(keep, dest, new MuxOptions().From(src).Container(), null, _feed.RunCallback);

        Assert.Equal(CompletionStatus.Success, result);
        var outInfo = GetFileInfo(dest);
        Assert.DoesNotContain(outInfo.FileStreams, HasFrequencyTag);
        Assert.Contains(outInfo.FileStreams, s => s.StreamType == FFmpegStreamType.Video);
        Assert.Contains(outInfo.FileStreams, s => s.StreamType == FFmpegStreamType.Subtitle);
        Assert.Empty(outInfo.FileStreams.OfType<MediaAudioStreamInfo>());
        Assert.Equal("Sample Title", outInfo.Metadata["title"]);
    }

    /// <summary>
    /// Change only which audio is default; same order, no other stream changes.
    /// </summary>
    [Fact]
    public void Muxe_ChangeDefaultOnly_SameStreamOrder()
    {
        var src = AppPaths.GetInputFile(AppPaths.TaggedMkv);
        var dest = AppPaths.PrepareDestPath("MuxeSwapDefault", AppPaths.TaggedMkv, ".mkv");
        var muxer = SetupMuxer();
        var info = GetFileInfo(src);
        var streams = info.FileStreams.Select(s => MediaStream.FromStreamInfo(src, s)).ToList();

        var audios = streams.Where(s => s.Type == FFmpegStreamType.Audio).ToList();
        Assert.True(audios.Count >= 2);
        audios[0].Disposition = ClearDisposition();
        audios[1].Disposition = DefaultDisposition();

        var result = muxer.Muxe(streams, dest, new MuxOptions().From(src).Container(), null, _feed.RunCallback);

        Assert.Equal(CompletionStatus.Success, result);
        var outInfo = GetFileInfo(dest);
        Assert.Equal(info.FileStreams.Count, outInfo.FileStreams.Count);
        var outAudios = outInfo.FileStreams.OfType<MediaAudioStreamInfo>().ToList();
        Assert.False(outAudios[0].Disposition.Has("default"));
        Assert.True(outAudios[1].Disposition.Has("default"));
        Assert.Equal(1, outAudios.Count(a => a.Disposition.Has("default")));
        // Tags still present
        Assert.Equal("440", FrequencyOf(outAudios[0]));
        Assert.Equal("432", FrequencyOf(outAudios[1]));
    }

    [Fact]
    public void Muxe_TaggedMkv_RoundTrip_PreservesStreamsAndTags()
    {
        var src = AppPaths.GetInputFile(AppPaths.TaggedMkv);
        var dest = AppPaths.PrepareDestPath("MuxeTaggedRoundTrip", AppPaths.TaggedMkv, ".mkv");
        var muxer = SetupMuxer();
        var sourceInfo = GetFileInfo(src);
        var streams = sourceInfo.FileStreams.Select(s => MediaStream.FromStreamInfo(src, s)).ToList();

        var result = muxer.Muxe(streams, dest, new MuxOptions().From(src).Container(), null, _feed.RunCallback);

        Assert.Equal(CompletionStatus.Success, result);
        var outInfo = GetFileInfo(dest);
        Assert.Equal(sourceInfo.FileStreams.Count, outInfo.FileStreams.Count);
        Assert.Equal("Sample Title", outInfo.Metadata["title"]);
        Assert.Equal("440", FrequencyOf(outInfo.FileStreams.OfType<MediaAudioStreamInfo>().First()));
        Assert.Contains(outInfo.FileStreams, s => s.StreamType == FFmpegStreamType.Subtitle);
    }
}
