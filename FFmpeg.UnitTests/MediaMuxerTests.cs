namespace HanumanInstitute.FFmpeg.UnitTests;

public class MediaMuxerTests
{
    private FakeEncoderService _factory = null!;
    private readonly ITestOutputHelper _output;

    public MediaMuxerTests(ITestOutputHelper output) => _output = output;

    private IMediaMuxer CreateMuxer()
    {
        _factory = new FakeEncoderService();
        return new MediaMuxer(_factory, new FakeFileSystemService(), new FakeMediaInfoReader());
    }

    private string Command => _factory.Instances.Last().CommandWithArgs;

    private void WriteCommand() => _output.WriteLine(Command);

    private IProcessWorker AssertProcess(bool video, bool audio, int index = -1)
    {
        var process = _factory.Instances[index < 0 ? ^1 : index];
        WriteCommand();
        Assert.Contains("ffmpeg", process.CommandWithArgs, StringComparison.InvariantCulture);
        process.CommandWithArgs.ContainsOrNot("-vcodec", video);
        process.CommandWithArgs.ContainsOrNot("-acodec", audio);
        return process;
    }

    [Fact]
    public void Constructor_NullDependencies_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new MediaMuxer(null!, new FakeFileSystemService(), Mock.Of<IMediaInfoReader>()));
        Assert.Throws<ArgumentNullException>(() => new MediaMuxer(new FakeEncoderService(), null!, Mock.Of<IMediaInfoReader>()));
        Assert.Throws<ArgumentNullException>(() => new MediaMuxer(new FakeEncoderService(), new FakeFileSystemService(), null!));
    }

    [Fact]
    public void MediaStream_Constructor_SetsSelection()
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
    public void MediaStream_FromStreamInfo_CopiesProbeData()
    {
        var info = new MediaAudioStreamInfo
        {
            Index = 1,
            Format = "aac",
            Language = "eng"
        };
        info.Metadata["frequency"] = "440";
        info.Disposition.Set("default");

        var stream = MediaStream.FromStreamInfo("/path/file.mkv", info);

        Assert.Equal("/path/file.mkv", stream.Path);
        Assert.Equal(1, stream.Index);
        Assert.Equal(FFmpegStreamType.Audio, stream.Type);
        Assert.Equal("aac", stream.Format);
        Assert.Equal("eng", stream.Language);
        Assert.Equal("440", stream.Metadata["frequency"]);
        Assert.True(stream.Disposition!.Has("default"));
    }

    [Fact]
    public void MediaStream_CopyTagsFrom_CopiesAndReplacesTags()
    {
        var info = new MediaAudioStreamInfo { Language = "fra" };
        info.Metadata["frequency"] = "432";
        info.Disposition.Set("comment");

        var stream = new MediaStream("a.m4a", 0, "aac", FFmpegStreamType.Audio);
        stream.Language = "eng";
        stream.Metadata["title"] = "old";
        stream.CopyTagsFrom(info);

        Assert.Equal("fra", stream.Language);
        Assert.Equal("432", stream.Metadata["frequency"]);
        Assert.DoesNotContain("title", stream.Metadata.Keys);
        Assert.True(stream.Disposition!.Has("comment"));
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Muxe_StreamList_Valid(bool video, bool audio)
    {
        var muxer = CreateMuxer();
        var streams = new List<MediaStream>();
        if (video) streams.Add(new MediaStream("video.mp4", 0, "h264", FFmpegStreamType.Video));
        if (audio) streams.Add(new MediaStream("audio.m4a", 0, "aac", FFmpegStreamType.Audio));

        Assert.Equal(CompletionStatus.Success, muxer.Muxe(streams, "dest.mp4"));
        Assert.Single(_factory.Instances);
        AssertProcess(video, audio);
        // V/A-only uses -vcodec/-acodec copy; -c copy is for jobs with other stream types.
        Assert.Contains("copy", Command, StringComparison.Ordinal);
    }

    [Fact]
    public void Muxe_StreamListEmpty_Throws()
    {
        var muxer = CreateMuxer();
        Assert.Throws<ArgumentNullException>(() => muxer.Muxe(null!, "dest"));
        Assert.Throws<ArgumentException>(() => muxer.Muxe([], "dest"));
    }

    [Theory]
    [InlineData("", "dest")]
    [InlineData("video.mp4", "")]
    public void Muxe_InvalidPath_Throws(string path, string destination)
    {
        var muxer = CreateMuxer();
        Assert.Throws<ArgumentException>(() => muxer.Muxe([new MediaStream(path, 0, "h264", FFmpegStreamType.Video)], destination));
    }

    [Fact]
    public void Muxe_SameInput_DeduplicatesAndMapsStreams()
    {
        var muxer = CreateMuxer();
        muxer.Muxe([
            new MediaStream("source.mkv", 0, "h264", FFmpegStreamType.Video),
            new MediaStream("source.mkv", 1, "aac", FFmpegStreamType.Audio),
            new MediaStream("source.mkv", 2, "subrip", FFmpegStreamType.Subtitle)
        ], "dest.mkv");

        Assert.Equal(1, Command.Split("-i \"source.mkv\"").Length - 1);
        Assert.Contains("-map 0:0", Command, StringComparison.Ordinal);
        Assert.Contains("-map 0:1", Command, StringComparison.Ordinal);
        Assert.Contains("-map 0:2", Command, StringComparison.Ordinal);
    }

    [Fact]
    public void Muxe_PreservesStreamOrder()
    {
        var muxer = CreateMuxer();
        muxer.Muxe([
            new MediaStream("audio.m4a", 0, "aac", FFmpegStreamType.Audio),
            new MediaStream("video.mp4", 0, "h264", FFmpegStreamType.Video)
        ], "dest.mp4");

        var audio = Command.IndexOf("-map 0:0", StringComparison.Ordinal);
        var video = Command.IndexOf("-map 1:0", StringComparison.Ordinal);
        Assert.True(audio >= 0);
        Assert.True(video > audio);
    }

    [Fact]
    public void Muxe_PathOnlyStreams_ResolveFromProbe()
    {
        var muxer = CreateMuxer();
        var streams = new List<MediaStream>
        {
            new("source.mkv", 0),
            new("source.mkv", 1)
        };

        Assert.Equal(CompletionStatus.Success, muxer.Muxe(streams, "dest.mkv"));
        Assert.Equal(FFmpegStreamType.Video, streams[0].Type);
        Assert.Equal(FFmpegStreamType.Audio, streams[1].Type);
        AssertProcess(true, true);
    }

    [Fact]
    public void Muxe_PassesOptionsAndCallback()
    {
        var muxer = CreateMuxer();
        var options = new ProcessOptionsEncoder();
        var calls = 0;

        muxer.Muxe([new MediaStream("video.mp4", 0, "h264", FFmpegStreamType.Video)], "dest", null, options, (_, _) => calls++);

        Assert.Same(options, _factory.Instances[0].Options);
        Assert.Equal(1, calls);
    }

    [Fact]
    public void Muxe_From_MapsAdditionalInputs()
    {
        var muxer = CreateMuxer();
        muxer.Muxe([
            new MediaStream("video.mp4", 0, "h264", FFmpegStreamType.Video),
            new MediaStream("audio.m4a", 0, "aac", FFmpegStreamType.Audio)
        ], "out.mkv", new MuxOptions().From("original.mkv").Subtitles().SideStreams().Container());

        Assert.Contains("-i \"original.mkv\"", Command, StringComparison.Ordinal);
        Assert.Contains("-i \"video.mp4\"", Command, StringComparison.Ordinal);
        Assert.Contains("-i \"audio.m4a\"", Command, StringComparison.Ordinal);
        Assert.Contains("-map_metadata 0", Command, StringComparison.Ordinal);
        Assert.Contains("-map_chapters 0", Command, StringComparison.Ordinal);
    }

    [Fact]
    public void Muxe_FromIndex_UsesInputIndex()
    {
        var muxer = CreateMuxer();
        muxer.Muxe([
            new MediaStream("fileA.mkv", 0, "h264", FFmpegStreamType.Video),
            new MediaStream("fileA.mkv", 1, "aac", FFmpegStreamType.Audio),
            new MediaStream("fileB.aac", 0, "aac", FFmpegStreamType.Audio)
        ], "dest.mkv", new MuxOptions().From(0).ContainerTags().From(1).ContainerTags());

        Assert.Equal(1, Command.Split("-i \"fileA.mkv\"").Length - 1);
        Assert.Equal(1, Command.Split("-i \"fileB.aac\"").Length - 1);
        Assert.Contains("-map_metadata 0", Command, StringComparison.Ordinal);
        Assert.Contains("-map_metadata 1", Command, StringComparison.Ordinal);
    }

    [Fact]
    public void Muxe_EmptyStreams_WithFrom_Succeeds()
    {
        var muxer = CreateMuxer();
        Assert.Equal(CompletionStatus.Success, muxer.Muxe([], "dest.mkv", new MuxOptions().From("source.mkv").Video()));
        Assert.Contains("-i \"source.mkv\"", Command, StringComparison.Ordinal);
        Assert.Contains("-map 0:0", Command, StringComparison.Ordinal);
    }

    [Fact]
    public void MuxOptions_FluentChain_Composes()
    {
        var options = new MuxOptions()
            .From("a.mkv").ContainerTags()
            .From("b.mkv").Chapters()
            .Done()
            .WithAdditionalArguments("-movflags +faststart");

        options.Metadata["title"] = "Album";

        Assert.Equal(2, options.FromInputs.Count);
        Assert.True(options.FromInputs[0].ContainerTags);
        Assert.True(options.FromInputs[1].Chapters);
        Assert.Equal("-movflags +faststart", options.AdditionalArguments);
        Assert.Equal("Album", options.Metadata["title"]);
    }

    [Fact]
    public void Muxe_MetadataAndDisposition_AreWritten()
    {
        var muxer = CreateMuxer();
        var stream = new MediaStream("audio.aac", 0, "aac", FFmpegStreamType.Audio)
        {
            Language = "eng",
            Disposition = new StreamDisposition().Set("default")
        };
        stream.Metadata["title"] = "Pitched Audio";
        stream.Metadata["frequency"] = "432";

        muxer.Muxe([stream], "dest.m4a");

        Assert.Contains("-metadata:s:0 language=eng", Command, StringComparison.Ordinal);
        Assert.Contains("-metadata:s:0 frequency=432", Command, StringComparison.Ordinal);
        Assert.Contains("-disposition:0 default", Command, StringComparison.Ordinal);
    }

    [Fact]
    public void Muxe_EmptyDisposition_ClearsIt()
    {
        var muxer = CreateMuxer();
        var stream = new MediaStream("audio.aac", 0, "aac", FFmpegStreamType.Audio) { Disposition = new StreamDisposition() };

        muxer.Muxe([stream], "dest.m4a");

        Assert.Contains("-disposition:0 0", Command, StringComparison.Ordinal);
    }

    [Fact]
    public void Muxe_MultipleAudio_OneDefault_AutoDemotesOthers()
    {
        var muxer = CreateMuxer();
        var first = new MediaStream("new.aac", 0, "aac", FFmpegStreamType.Audio)
        {
            Disposition = new StreamDisposition().Set("default")
        };
        // Source-like: second still has default from probe copy — library must demote without caller clearing.
        var second = new MediaStream("old.aac", 0, "aac", FFmpegStreamType.Audio)
        {
            Disposition = new StreamDisposition().Set("default")
        };

        muxer.Muxe([
            new MediaStream("video.mp4", 0, "h264", FFmpegStreamType.Video),
            first,
            second
        ], "dest.mkv");

        Assert.Contains("-disposition:1 default", Command, StringComparison.Ordinal);
        Assert.Contains("-disposition:2 0", Command, StringComparison.Ordinal);
    }

    [Fact]
    public void Muxe_MultipleAudio_OneDefault_ClearsNullDispositionSibling()
    {
        var muxer = CreateMuxer();
        var first = new MediaStream("new.aac", 0, "aac", FFmpegStreamType.Audio)
        {
            Disposition = new StreamDisposition().Set("default")
        };
        // Null would omit -disposition and keep source default — must emit clear.
        var second = new MediaStream("old.aac", 0, "aac", FFmpegStreamType.Audio);

        muxer.Muxe([
            new MediaStream("video.mp4", 0, "h264", FFmpegStreamType.Video),
            first,
            second
        ], "dest.mkv");

        Assert.Contains("-disposition:1 default", Command, StringComparison.Ordinal);
        Assert.Contains("-disposition:2 0", Command, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("aac", true, "aac_adtstoasc")]
    [InlineData("aac", false, null)]
    [InlineData("pcm_dvd", false, "pcm_s16le")]
    [InlineData("pcm_s16le", false, null)]
    public void Muxe_CodecSpecificFilters(string format, bool withVideo, string expected)
    {
        var muxer = CreateMuxer();
        var streams = new List<MediaStream>();
        if (withVideo) streams.Add(new MediaStream("video.mp4", 0, "h264", FFmpegStreamType.Video));
        streams.Add(new MediaStream("audio", 0, format, FFmpegStreamType.Audio));

        muxer.Muxe(streams, withVideo ? "dest.mp4" : "dest.m4a");

        if (expected is null)
        {
            Assert.DoesNotContain("aac_adtstoasc", Command, StringComparison.Ordinal);
            Assert.DoesNotContain("pcm_s16le", Command, StringComparison.Ordinal);
        }
        else
            Assert.Contains(expected, Command, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("video.264", "h264")]
    [InlineData("video.h264", "h264")]
    [InlineData("video.265", "hevc")]
    [InlineData("video.h265", "h265")]
    [InlineData("video.hevc", "hevc")]
    [InlineData("video.vvc", "vvc")]
    [InlineData("video.266", "vvc")]
    [InlineData("video.h266", "h266")]
    public void Muxe_ElementaryVideoIntoMkv_UsesTemporaryMp4(string path, string format)
    {
        var muxer = CreateMuxer();
        muxer.Muxe([
            new MediaStream(path, 0, format, FFmpegStreamType.Video),
            new MediaStream("audio.aac", 0, "aac", FFmpegStreamType.Audio)
        ], "dest.mkv");

        Assert.True(_factory.Instances.Count > 1);
        Assert.Contains("temp.mp4", Command, StringComparison.Ordinal);
        Assert.DoesNotContain(path, Command, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("dest.mp4", "h264", "video.264")]
    [InlineData("dest.mkv", "vp9", "video.vp9")]
    public void Muxe_NonElementaryVideo_DoesNotUseTemporaryMp4(string destination, string format, string path)
    {
        var muxer = CreateMuxer();
        muxer.Muxe([new MediaStream(path, 0, format, FFmpegStreamType.Video)], destination);

        Assert.Single(_factory.Instances);
        Assert.DoesNotContain("temp.mp4", Command, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("video.mp4", "audio.m4a", true, true, "dest.mp4")]
    [InlineData(null, "audio.m4a", false, true, "dest.mp4")]
    [InlineData("video.mp4", null, true, false, "dest.mkv")]
    public void Muxe_Simple_Valid(string video, string audio, bool hasVideo, bool hasAudio, string destination)
    {
        var muxer = CreateMuxer();
        Assert.Equal(CompletionStatus.Success, muxer.Muxe(video, audio, destination));
        AssertProcess(hasVideo, hasAudio);
    }

    [Theory]
    [InlineData(null, null, null, typeof(ArgumentNullException))]
    [InlineData("", "", "", typeof(ArgumentException))]
    [InlineData("video.mp4", "", "", typeof(ArgumentException))]
    [InlineData("", "audio.aac", "", typeof(ArgumentException))]
    [InlineData("", "", "dest.mp4", typeof(ArgumentException))]
    public void Muxe_Simple_InvalidArguments_Throw(string video, string audio, string destination, Type exception)
    {
        var muxer = CreateMuxer();
        Assert.Throws(exception, () => muxer.Muxe(video, audio, destination!));
    }

    [Fact]
    public void Muxe_Simple_PassesOptionsAndCallback()
    {
        var muxer = CreateMuxer();
        var options = new ProcessOptionsEncoder();
        var calls = 0;

        muxer.Muxe("video", "audio", "dest", options, (_, _) => calls++);

        Assert.Same(options, _factory.Instances[0].Options);
        Assert.Equal(1, calls);
    }

    [Theory]
    [InlineData(true, "-vcodec copy -an")]
    [InlineData(false, "-vn -acodec copy")]
    public void Extract_ProducesExpectedCommand(bool video, string expected)
    {
        var muxer = CreateMuxer();
        var result = video ? muxer.ExtractVideo("source", "dest") : muxer.ExtractAudio("source", "dest");

        Assert.Equal(CompletionStatus.Success, result);
        Assert.Contains(expected, Command, StringComparison.Ordinal);
        AssertProcess(video, !video);
    }

    [Fact]
    public void Extract_PassesOptionsAndCallback()
    {
        var muxer = CreateMuxer();
        var options = new ProcessOptionsEncoder();
        var calls = 0;

        muxer.ExtractAudio("source", "dest", options, (_, _) => calls++);

        Assert.Same(options, _factory.Instances[0].Options);
        Assert.Equal(1, calls);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Concatenate_Valid(int count)
    {
        var muxer = CreateMuxer();
        var files = Enumerable.Range(1, count).Select(i => $"file{i}").ToList();

        Assert.Equal(CompletionStatus.Success, muxer.Concatenate(files, "dest.mkv"));
        Assert.Contains("-f concat", Command, StringComparison.Ordinal);
        Assert.Contains("-c copy", Command, StringComparison.Ordinal);
    }

    [Fact]
    public void Concatenate_InvalidArguments_Throw()
    {
        var muxer = CreateMuxer();
        Assert.Throws<ArgumentNullException>(() => muxer.Concatenate(null!, "dest.mkv"));
        Assert.Throws<ArgumentException>(() => muxer.Concatenate([], "dest.mkv"));
        Assert.Throws<ArgumentNullException>(() => muxer.Concatenate(["file"], null!));
        Assert.Throws<ArgumentException>(() => muxer.Concatenate(["file"], ""));
    }

    [Fact]
    public void Concatenate_PassesOptionsAndCallback()
    {
        var muxer = CreateMuxer();
        var options = new ProcessOptionsEncoder();
        var calls = 0;

        muxer.Concatenate(["file1", "file2"], "dest.mkv", options, (_, _) => calls++);

        Assert.Same(options, _factory.Instances[0].Options);
        Assert.Equal(1, calls);
    }

    [Fact]
    public void Truncate_Valid()
    {
        var muxer = CreateMuxer();

        Assert.Equal(CompletionStatus.Success, muxer.Truncate("source", "dest.mkv", TimeSpan.Zero, TimeSpan.FromSeconds(10)));
        Assert.Contains("-vcodec copy", Command, StringComparison.Ordinal);
        Assert.Contains("-acodec copy", Command, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(null, "dest.mkv", typeof(ArgumentNullException))]
    [InlineData("", "dest.mkv", typeof(ArgumentException))]
    [InlineData("source", null, typeof(ArgumentNullException))]
    [InlineData("source", "", typeof(ArgumentException))]
    public void Truncate_InvalidArguments_Throw(string source, string destination, Type exceptionType)
    {
        var muxer = CreateMuxer();
        Assert.Throws(exceptionType, () => muxer.Truncate(source!, destination!, TimeSpan.Zero, TimeSpan.FromSeconds(10)));
    }

    [Fact]
    public void Truncate_PassesOptionsAndCallback()
    {
        var muxer = CreateMuxer();
        var options = new ProcessOptionsEncoder();
        var calls = 0;

        muxer.Truncate("source", "dest.mkv", TimeSpan.Zero, TimeSpan.FromSeconds(10), options, (_, _) => calls++);

        Assert.Same(options, _factory.Instances[0].Options);
        Assert.Equal(1, calls);
    }
}
