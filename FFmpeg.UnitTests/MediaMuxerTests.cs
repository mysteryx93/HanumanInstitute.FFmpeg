// ReSharper disable AssignNullToNotNullAttribute
// ReSharper disable PossibleMultipleEnumeration
namespace HanumanInstitute.FFmpeg.UnitTests;

public class MediaMuxerTests
{
    private const string FFmpeg = "ffmpeg", AudioCodec = "-acodec", VideoCodec = "-vcodec", FixAac = "aac_adtstoasc", FixPcm = "pcm_s16le";
    private FakeProcessService _factory;
    private readonly ITestOutputHelper _output;

    public MediaMuxerTests(ITestOutputHelper output)
    {
        _output = output;
    }


    /// <summary>
    /// Creates and initializes the MediaMuxer class for testing.
    /// </summary>
    protected IMediaMuxer SetupMuxer()
    {
        _factory = new FakeProcessService();
        var fileSystemStub = new FakeFileSystemService();
        return new MediaMuxer(_factory, fileSystemStub, new FakeMediaInfoReader());
    }

    /// <summary>
    /// Tests an IFFmpegManager to make sure it contains valid arguments.
    /// </summary>
    /// <param name="hasVideo">Whether the command should include a video source.</param>
    /// <param name="hasAudio">Whether the command should include an audio source.</param>
    /// <param name="instanceIndex">The index of the instance created by the IFFmpegProcessManagerFactory to test, or -1 (default) to use the last.</param>
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


    public static IEnumerable<object[]> GenerateStreamLists_Valid()
    {
        yield return new object[] {
            new List<MediaStream>() {
                new("video", 0, "mpeg2", FFmpegStreamType.Video),
            },
            "dest", true, false
        };
        yield return new object[] {
            new List<MediaStream>() {
                new("audio", 1, "mp3", FFmpegStreamType.Audio)
            },
            "dest", false, true
        };
        yield return new object[] {
            new List<MediaStream>() {
                new("video", 0, "mpeg2", FFmpegStreamType.Video),
                new("audio", 1, "mp3", FFmpegStreamType.Audio)
            },
            "dest", true, true
        };
        yield return new object[] {
            new List<MediaStream>() {
                new("video.264", 0, null, FFmpegStreamType.Video),
                new("audio.m4a", 1, "m4a", FFmpegStreamType.Audio)
            },
            "dest.mp4", true, true
        };
        yield return new object[] {
            new List<MediaStream>() {
                new("Audio.AAC", 1, "aac", FFmpegStreamType.Audio),
                new("Video.265", 0, "mpeg2", FFmpegStreamType.Video),
                new("audio.m4a", 1, "m4a", FFmpegStreamType.Audio)
            },
            "Dest.MP4", true, true
        };
        yield return new object[] {
            new List<MediaStream>() {
                new("Video.MKV", 0, "mpeg2", FFmpegStreamType.Video),
                new("Audio.AAC", 1, "aac", FFmpegStreamType.Audio),
                new("audio.mp4", 1, "", FFmpegStreamType.Video)
            },
            "Dest.MKV", true, true
        };
        yield return new object[] {
            new List<MediaStream>() {
                new("\0\"\0\"\0\n", -5, "\0\0\0\n", FFmpegStreamType.Video),
                new("读写汉字", -10, "读写汉字", FFmpegStreamType.None),
                new("读写汉字", -15, null, FFmpegStreamType.Video),
            },
            "学中文", true, false
        };
    }

    public static IEnumerable<object[]> GenerateStreamLists_EmptyArgs()
    {
        yield return new object[] {
            null,
            "dest",
            typeof(ArgumentNullException)
        };
        yield return new object[] {
            new List<MediaStream>(),
            "dest",
            typeof(ArgumentException)
        };
        yield return new object[] {
            new List<MediaStream>() {
                new("video", 0, "", FFmpegStreamType.None)
            },
            "dest",
            typeof(ArgumentException)
        };
        yield return new object[] {
            new List<MediaStream>() {
                new("video", 0, "", FFmpegStreamType.Video)
            },
            null,
            typeof(ArgumentNullException)
        };
        yield return new object[] {
            new List<MediaStream>() {
                new("video", 0, "", FFmpegStreamType.Video)
            },
            "",
            typeof(ArgumentException)
        };
        yield return new object[] {
            new List<MediaStream>() {
                new(null, 0, "", FFmpegStreamType.Video)
            },
            "dest",
            typeof(ArgumentException)
        };
        yield return new object[] {
            new List<MediaStream>() {
                new("", 0, "", FFmpegStreamType.Video)
            },
            "dest",
            typeof(ArgumentException)
        };
    }

    public static IEnumerable<object[]> GenerateStreamLists_FixAac()
    {
        yield return new object[] {
            new List<MediaStream>() {
                new("audio.aac", 0, "aac", FFmpegStreamType.Audio),
                new("video.mp4", 0, "mp4", FFmpegStreamType.Video),
            },
            "dest", true, true
        };
        yield return new object[] {
            new List<MediaStream>() {
                new("video.Mp4", 0, "mp4", FFmpegStreamType.Video),
                new(".AAC", 0, "aac", FFmpegStreamType.Audio),
            },
            "dest", true, true
        };
    }

    public static IEnumerable<object[]> GenerateStreamLists_FixPcm()
    {
        yield return new object[] {
            new List<MediaStream>() {
                new("audio.wav", 0, "pcm_dvd", FFmpegStreamType.Audio),
                new("video.WAV", 0, null, FFmpegStreamType.Video),
            },
            "dest", true, true
        };
        yield return new object[] {
            new List<MediaStream>() {
                new("video.Mp4", 0, "mp4", FFmpegStreamType.Audio),
                new(".AAC", 0, "pcm_dvd", FFmpegStreamType.Audio),
            },
            "dest", false, true
        };
        yield return new object[] {
            new List<MediaStream>() {
                new(".AAC", 0, "pcm_dvd", FFmpegStreamType.Audio)
            },
            "dest", false, true
        };
    }

    public static IEnumerable<object[]> GenerateStreamLists_DoNotFixAac()
    {
        yield return new object[] {
            new List<MediaStream>() {
                new("audio.aac", 0, "aac", FFmpegStreamType.Video),
                new("video.mp4", 0, "mp4", FFmpegStreamType.Audio),
            },
            "dest", true, true
        };
        yield return new object[] {
            new List<MediaStream>() {
                new("Audio.aac", 0, "aac", FFmpegStreamType.Audio),
                new(".AAC", 0, "aac", FFmpegStreamType.Audio),
            },
            "dest", false, true
        };
    }

    public static IEnumerable<object[]> GenerateStreamLists_DoNotFixPcm()
    {
        yield return new object[] {
            new List<MediaStream>() {
                new("audio.wav", 0, "pcm_dvd", FFmpegStreamType.Video),
                new("video.WAV", 0, null, FFmpegStreamType.Audio),
            },
            "dest", true, true
        };
        yield return new object[] {
            new List<MediaStream>() {
                new("video.aac", 0, "aac", FFmpegStreamType.Audio),
                new(".WAV", 0, "pcm_dvd", FFmpegStreamType.None),
            },
            "dest", false, true
        };
    }

    public static IEnumerable<object[]> GenerateStreamLists_SingleValid()
    {
        yield return new object[] {
            new List<MediaStream>() {
                new("video", 0, "mpeg2", FFmpegStreamType.Video),
                new("audio", 1, "mp3", FFmpegStreamType.Audio)
            },
            "dest"
        };
    }

    public static IEnumerable<object[]> GenerateStreamLists_H264IntoMkv()
    {
        yield return new object[] {
            new List<MediaStream>() {
                new("video.264", 0, "h264", FFmpegStreamType.Video),
                new("audio.aac", 1, "aac", FFmpegStreamType.Audio)
            },
            "dest.mkv", true, true
        };
        yield return new object[] {
            new List<MediaStream>() {
                new("audio.aac", 1, "aac", FFmpegStreamType.Audio),
                new("video.265", 0, "h265", FFmpegStreamType.Video)
            },
            "dest.mkv", true, true
        };
        yield return new object[] {
            new List<MediaStream>() {
                new(".264", 1, "h264", FFmpegStreamType.Video)
            },
            ".mkv", true, false
        };
    }

    public static IEnumerable<object[]> GenerateConcatenate_Valid()
    {
        yield return new object[] {
            new List<string>() {
                "file1"
            },
            "dest.mkv"
        };
        yield return new object[] {
            new List<string>() {
                "file1",
                "file2"
            },
            "dest.mkv"
        };
        yield return new object[] {
            new List<string>() {
                "file1",
                "file2",
                "file3"
            },
            "dest.mkv"
        };
    }

    public static IEnumerable<object[]> GenerateConcatenate_Empty()
    {
        yield return new object[] {
            null,
            "dest.mkv",
            typeof(ArgumentNullException)
        };
        yield return new object[] {
            new List<string>(),
            "dest.mkv",
            typeof(ArgumentException)
        };
        yield return new object[] {
            new List<string>() {
                "file1"
            },
            null,
            typeof(ArgumentNullException)
        };
        yield return new object[] {
            new List<string>() {
                "file1"
            },
            "",
            typeof(ArgumentException)
        };
    }

    public static IEnumerable<object[]> GenerateConcatenate_Single()
    {
        yield return new object[] {
            new List<string>() {
                "file1",
                "file2"
            },
            "dest.mkv"
        };
    }


    public static IEnumerable<object[]> GenerateTruncate_Valid()
    {
        yield return new object[] {
            "source",
            "dest.mkv",
            TimeSpan.Zero,
            TimeSpan.FromSeconds(10)
        };
    }

    //public static IEnumerable<object[]> GenerateTruncate_Empty()
    //{
    //    yield return new object[] {
    //        null,
    //        "dest.mkv",
    //        TimeSpan.Zero,
    //        TimeSpan.FromSeconds(10)
    //    };
    //    yield return new object[] {
    //        "",
    //        "dest.mkv",
    //        TimeSpan.Zero,
    //        TimeSpan.FromSeconds(10)
    //    };
    //    yield return new object[] {
    //        "source",
    //        null,
    //        TimeSpan.Zero,
    //        TimeSpan.FromSeconds(10)
    //    };
    //    yield return new object[] {
    //        "source",
    //        "",
    //        TimeSpan.Zero,
    //        TimeSpan.FromSeconds(10)
    //    };
    //}

    public static IEnumerable<object[]> GenerateTruncate_Single()
    {
        yield return new object[] {
            "source",
            "dest.mkv",
            TimeSpan.Zero,
            TimeSpan.FromSeconds(10)
        };
    }


    [Fact]
    // ReSharper disable once ObjectCreationAsStatement
    public void Constructor_WithFactory_Success() => new MediaMuxer(new FakeProcessService(), new FakeFileSystemService(), Mock.Of<IMediaInfoReader>());

    [Fact]
    public void Constructor_NullFactory_ThrowsException() => Assert.Throws<ArgumentNullException>(() => new MediaMuxer(null, new FakeFileSystemService(), Mock.Of<IMediaInfoReader>()));

    [Fact]
    public void Constructor_NullDependency_ThrowsException() => Assert.Throws<ArgumentNullException>(() => new MediaMuxer(_factory, null, null));


    // Theory is required to serialize FFmpegStream argument, otherwise we cannot see the list of theory cases.
    [Theory]
    [MemberData(nameof(GenerateStreamLists_Valid))]
    public void Muxe_StreamList_Valid_Success(IEnumerable<MediaStream> fileStreams, string destination, bool hasVideo, bool hasAudio)
    {
        var muxer = SetupMuxer();

        var result = muxer.Muxe(fileStreams, destination);

        Assert.Equal(CompletionStatus.Success, result);
        Assert.Single(_factory.Instances);
        AssertFFmpegManager(hasVideo, hasAudio);
    }

    [Theory]
    [MemberData(nameof(GenerateStreamLists_EmptyArgs))]
    public void Muxe_StreamList_EmptyArgs_ThrowsNullException(IEnumerable<MediaStream> fileStreams, string destination, Type ex)
    {
        var muxer = SetupMuxer();

        void Act() => muxer.Muxe(fileStreams, destination);

        Assert.Throws(ex, Act);
    }

    [Theory]
    [MemberData(nameof(GenerateStreamLists_FixAac))]
    public void Muxe_StreamList_WithAudioAac_AddArgumentFixAac(IEnumerable<MediaStream> fileStreams, string destination, bool hasVideo, bool hasAudio)
    {
        Muxe_StreamList_FixAudio(fileStreams, destination, FixAac, hasVideo, hasAudio);
    }

    [Theory]
    [MemberData(nameof(GenerateStreamLists_FixPcm))]
    public void Muxe_StreamList_WithAudioPcm_AddArgumentFixPcm(IEnumerable<MediaStream> fileStreams, string destination, bool hasVideo, bool hasAudio)
    {
        Muxe_StreamList_FixAudio(fileStreams, destination, FixPcm, hasVideo, hasAudio);
    }

    [Theory]
    [MemberData(nameof(GenerateStreamLists_DoNotFixAac))]
    public void Muxe_StreamList_WithNoAudioAac_DoNotAddArgumentFixAac(IEnumerable<MediaStream> fileStreams, string destination, bool hasVideo, bool hasAudio)
    {
        Muxe_StreamList_FixAudio(fileStreams, destination, FixAac, hasVideo, hasAudio, false);
    }

    [Theory]
    [MemberData(nameof(GenerateStreamLists_DoNotFixPcm))]
    public void Muxe_StreamList_WithNoAudioPcm_DoNotAddArgumentFixPcm(IEnumerable<MediaStream> fileStreams, string destination, bool hasVideo, bool hasAudio)
    {
        Muxe_StreamList_FixAudio(fileStreams, destination, FixPcm, hasVideo, hasAudio, false);
    }

    private void Muxe_StreamList_FixAudio(IEnumerable<MediaStream> fileStreams, string destination, string fixString, bool hasVideo, bool hasAudio, bool fixStringExpected = true)
    {
        var muxer = SetupMuxer();

        var result = muxer.Muxe(fileStreams, destination);

        Assert.Equal(CompletionStatus.Success, result);
        Assert.Single(_factory.Instances);
        var manager = AssertFFmpegManager(hasVideo, hasAudio);
        manager.CommandWithArgs.ContainsOrNot(fixString, fixStringExpected);
    }

    [Theory]
    [MemberData(nameof(GenerateStreamLists_H264IntoMkv))]
    public void Muxe_StreamsList_H264IntoMkv_MuxeIntoMp4First(IEnumerable<MediaStream> fileStreams, string destination, bool hasVideo, bool hasAudio)
    {
        var muxer = SetupMuxer();

        var result = muxer.Muxe(fileStreams, destination);

        Assert.Equal(CompletionStatus.Success, result);
        Assert.True(_factory.Instances.Count > 1);
        foreach (var manager in _factory.Instances)
        {
            _output.WriteLine(manager.CommandWithArgs);
        }
        AssertFFmpegManager(hasVideo, hasAudio);
    }

    [Theory]
    [MemberData(nameof(GenerateStreamLists_SingleValid))]
    public void Muxe_StreamsList_ParamOptions_ReturnsSame(IEnumerable<MediaStream> fileStreams, string destination)
    {
        var muxer = SetupMuxer();
        var options = new ProcessOptionsEncoder();

        muxer.Muxe(fileStreams, destination, options);

        Assert.Same(options, _factory.Instances[0].Options);
    }

    [Theory]
    [MemberData(nameof(GenerateStreamLists_SingleValid))]
    public void Muxe_StreamsList_ParamCallback_CallbackCalled(IEnumerable<MediaStream> fileStreams, string destination)
    {
        var muxer = SetupMuxer();
        var callbackCalled = 0;

        muxer.Muxe(fileStreams, destination, null, (_, _) => callbackCalled++);

        Assert.Equal(1, callbackCalled);
    }


    [Theory]
    [InlineData("video", "audio", "dest")]
    [InlineData("video.mp4", "audio.m4a", "dest.mp4")]
    public void Muxe_Simple_AudioVideo_Success(string videoFile, string audioFile, string destination)
    {
        var muxer = SetupMuxer();

        var result = muxer.Muxe(videoFile, audioFile, destination);

        Assert.Equal(CompletionStatus.Success, result);
        Assert.NotEmpty(_factory.Instances);
        AssertFFmpegManager(true, true);
    }

    [Theory]
    [InlineData("audio", "dest")]
    [InlineData("audio.m4a", "dest.mp4")]
    [InlineData("audio.AAC", "Dest.MKV")]
    [InlineData("\0\"\0\"\0\n", "\0\0\0\n")]
    [InlineData("读写汉字", "学中文")]
    public void Muxe_Simple_AudioOnly_Success(string audioFile, string destination)
    {
        var muxer = SetupMuxer();

        var result = muxer.Muxe(null, audioFile, destination);

        Assert.Equal(CompletionStatus.Success, result);
        Assert.NotEmpty(_factory.Instances);
        AssertFFmpegManager(false, true);
    }

    [Theory]
    [InlineData("video", "dest")]
    [InlineData("Video.aac", "dest.mp4")]
    [InlineData("Video.MKV", "Dest.MKV")]
    [InlineData("\0\"\0\"\0\n", "\0\0\0\n")]
    [InlineData("读写汉字", "学中文")]
    public void Muxe_Simple_VideoOnly_Success(string videoFile, string destination)
    {
        var muxer = SetupMuxer();

        var result = muxer.Muxe(videoFile, null, destination);

        Assert.Equal(CompletionStatus.Success, result);
        Assert.NotEmpty(_factory.Instances);
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

        void Act() => muxer.Muxe(videoFile, audioFile, destination);

        Assert.Throws(ex, Act);
    }

    [Theory]
    [InlineData("video", "audio", "dest")]
    public void Muxe_Simple_ParamOptions_ReturnsSame(string videoFile, string audioFile, string destination)
    {
        var muxer = SetupMuxer();
        var options = new ProcessOptionsEncoder();

        muxer.Muxe(videoFile, audioFile, destination, options);

        Assert.Same(options, _factory.Instances[0].Options);
    }

    [Theory]
    [InlineData("video", "audio", "dest")]
    public void Muxe_Simple_ParamCallback_CallbackCalled(string videoFile, string audioFile, string destination)
    {
        var muxer = SetupMuxer();
        var callbackCalled = 0;

        muxer.Muxe(videoFile, audioFile, destination, null, (_, _) => callbackCalled++);

        Assert.Equal(1, callbackCalled);
    }


    [Theory]
    [InlineData("source", "dest")]
    public void ExtractAudio_Valid_Success(string source, string destination)
    {
        var muxer = SetupMuxer();

        var result = muxer.ExtractAudio(source, destination);

        Assert.Equal(CompletionStatus.Success, result);
        Assert.Single(_factory.Instances);
        AssertFFmpegManager(false, true);
    }

    [Theory]
    [MemberData(nameof(TestDataSource.NullAndEmptyStrings), 2, MemberType = typeof(TestDataSource))]
    public void ExtractAudio_EmptyArgs_ThrowsException(string source, string destination, Type ex)
    {
        var muxer = SetupMuxer();

        void Act() => muxer.ExtractAudio(source, destination);

        Assert.Throws(ex, Act);
    }

    [Theory]
    [InlineData("source", "dest")]
    public void ExtractAudio_ParamOptions_ReturnsSame(string source, string destination)
    {
        var muxer = SetupMuxer();
        var options = new ProcessOptionsEncoder();

        muxer.ExtractAudio(source, destination, options);

        Assert.Same(options, _factory.Instances[0].Options);
    }

    [Theory]
    [InlineData("source", "dest")]
    public void ExtractAudio_ParamCallback_CallbackCalled(string source, string destination)
    {
        var muxer = SetupMuxer();
        var callbackCalled = 0;

        muxer.ExtractAudio(source, destination, null, (_, _) => callbackCalled++);

        Assert.Equal(1, callbackCalled);
    }


    [Theory]
    [InlineData("source", "dest")]
    public void ExtractVideo_Valid_Success(string source, string destination)
    {
        var muxer = SetupMuxer();

        var result = muxer.ExtractVideo(source, destination);

        Assert.Equal(CompletionStatus.Success, result);
        Assert.Single(_factory.Instances);
        AssertFFmpegManager(true, false);
    }

    [Theory]
    [MemberData(nameof(TestDataSource.NullAndEmptyStrings), 2, MemberType = typeof(TestDataSource))]
    public void ExtractVideo_EmptyArgs_ThrowsException(string source, string destination, Type ex)
    {
        var muxer = SetupMuxer();

        void Act() => muxer.ExtractVideo(source, destination);

        Assert.Throws(ex, Act);
    }

    [Theory]
    [InlineData("source", "dest")]
    public void ExtractVideo_ParamOptions_ReturnsSame(string source, string destination)
    {
        var muxer = SetupMuxer();
        var options = new ProcessOptionsEncoder();

        muxer.ExtractVideo(source, destination, options);

        Assert.Same(options, _factory.Instances[0].Options);
    }

    [Theory]
    [InlineData("source", "dest")]
    public void ExtractVideo_ParamCallback_CallbackCalled(string source, string destination)
    {
        var muxer = SetupMuxer();
        var callbackCalled = 0;

        muxer.ExtractVideo(source, destination, null, (_, _) => callbackCalled++);

        Assert.Equal(1, callbackCalled);
    }


    [Theory]
    [MemberData(nameof(GenerateConcatenate_Valid))]
    public void Concatenate_Valid_Success(IEnumerable<string> files, string destination)
    {
        var muxer = SetupMuxer();

        var result = muxer.Concatenate(files, destination);

        _output.WriteLine(_factory.Instances.FirstOrDefault()?.CommandWithArgs);
        Assert.Equal(CompletionStatus.Success, result);
        Assert.Single(_factory.Instances);
    }

    [Theory]
    [MemberData(nameof(GenerateConcatenate_Empty))]
    public void Concatenate_EmptyArgs_ThrowsException(IEnumerable<string> files, string destination, Type ex)
    {
        var muxer = SetupMuxer();

        void Act() => muxer.Concatenate(files, destination);

        Assert.Throws(ex, Act);
    }

    [Theory]
    [MemberData(nameof(GenerateConcatenate_Single))]
    public void Concatenate_ParamOptions_ReturnsSame(IEnumerable<string> files, string destination)
    {
        var muxer = SetupMuxer();
        var options = new ProcessOptionsEncoder();

        muxer.Concatenate(files, destination, options);

        Assert.Same(options, _factory.Instances[0].Options);
    }

    [Theory]
    [MemberData(nameof(GenerateConcatenate_Single))]
    public void Concatenate_ParamCallback_CallbackCalled(IEnumerable<string> files, string destination)
    {
        var muxer = SetupMuxer();
        var callbackCalled = 0;

        muxer.Concatenate(files, destination, null, (_, _) => callbackCalled++);

        Assert.Equal(1, callbackCalled);
    }


    [Theory]
    [MemberData(nameof(GenerateTruncate_Valid))]
    public void Truncate_Valid_Success(string source, string destination, TimeSpan? startPos, TimeSpan? duration)
    {
        var muxer = SetupMuxer();

        var result = muxer.Truncate(source, destination, startPos, duration);

        _output.WriteLine(_factory.Instances.FirstOrDefault()?.CommandWithArgs);
        Assert.Equal(CompletionStatus.Success, result);
        Assert.Single(_factory.Instances);
    }

    [Theory]
    [MemberData(nameof(TestDataSource.NullAndEmptyStrings), 2, MemberType = typeof(TestDataSource))]
    public void Truncate_EmptyArgs_ThrowsException(string source, string destination, Type ex)
    {
        var muxer = SetupMuxer();

        void Act() => muxer.Truncate(source, destination, TimeSpan.Zero, TimeSpan.FromSeconds(10));

        Assert.Throws(ex, Act);
    }

    [Theory]
    [MemberData(nameof(GenerateTruncate_Single))]
    public void Truncate_ParamOptions_ReturnsSame(string source, string destination, TimeSpan? startPos, TimeSpan? duration)
    {
        var muxer = SetupMuxer();
        var options = new ProcessOptionsEncoder();

        muxer.Truncate(source, destination, startPos, duration, options);

        Assert.Same(options, _factory.Instances[0].Options);
    }

    [Theory]
    [MemberData(nameof(GenerateTruncate_Single))]
    public void Truncate_ParamCallback_CallbackCalled(string source, string destination, TimeSpan? startPos, TimeSpan? duration)
    {
        var muxer = SetupMuxer();
        var callbackCalled = 0;

        muxer.Truncate(source, destination, startPos, duration, null, (_, _) => callbackCalled++);

        Assert.Equal(1, callbackCalled);
    }
}
