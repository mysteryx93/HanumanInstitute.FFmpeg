namespace HanumanInstitute.FFmpeg.IntegrationTests;

public class MediaInfoTests
{
    private readonly ITestOutputHelper _output;
    private readonly OutputFeeder _feed;

    public MediaInfoTests(ITestOutputHelper output)
    {
        _output = output;
        _feed = new OutputFeeder(output);
    }

    private IMediaInfoReader SetupInfo()
    {
        var factory = FactoryConfig.CreateWithConfig();
        return new MediaInfoReader(factory);
    }

    private void WriteOutput(FileInfoFFmpeg fileInfo)
    {
        foreach (var item in fileInfo.FileStreams)
        {
            _output.WriteLine(item.RawText);
        }
    }

    [Fact]
    public void GetVersion_Valid_ReturnsVersionInfo()
    {
        var info = SetupInfo();

        var output = info.GetVersion(null, _feed.RunCallback);

        Assert.NotEmpty(output);
        Assert.Contains("version", output, StringComparison.InvariantCulture);
    }

    [Theory]
    [InlineData(AppPaths.Mpeg4, 3)] // 2x Data + Video (legacy sample)
    [InlineData(AppPaths.Mpeg2, 1)]
    [InlineData(AppPaths.Flv, 2)]
    [InlineData(AppPaths.StreamAac, 1)]
    [InlineData(AppPaths.StreamH264, 1)]
    [InlineData(AppPaths.StreamOpus, 1)]
    [InlineData(AppPaths.StreamVp9, 1)]
    public void GetFileInfo_Valid_ReturnsWorkerWithStreams(string source, int streamCount)
    {
        var info = SetupInfo();
        var src = AppPaths.GetInputFile(source);

        var fileInfo = info.GetFileInfo(src, null, _feed.RunCallback);
        
        WriteOutput(fileInfo);
        Assert.NotNull(fileInfo.FileStreams);
        Assert.Equal(streamCount, fileInfo.FileStreams.Count);
    }

    [Theory]
    [InlineData(AppPaths.InvalidFile)]
    public void GetFileInfo_InvalidFile_ReturnsWorkerWithEmptyStreamList(string source)
    {
        var info = SetupInfo();
        var src = AppPaths.GetInputFile(source);

        var fileInfo = info.GetFileInfo(src, null, _feed.RunCallback);
        
        Assert.NotNull(fileInfo.FileStreams);
        Assert.Empty(fileInfo.FileStreams);
    }

    [Theory]
    [InlineData(AppPaths.Mpeg2)]
    [InlineData(AppPaths.Flv)]
    [InlineData(AppPaths.StreamH264)]
    public void GetFrameCount_Valid_ReturnsFrameCount(string source)
    {
        var info = SetupInfo();
        var src = AppPaths.GetInputFile(source);

        var count = info.GetFrameCount(src, null, _feed.RunCallback);

        Assert.True(count > 0, "Frame count should be a positive number.");
    }

    [Theory]
    [InlineData(AppPaths.InvalidFile)]
    public void GetFrameCount_InvalidFile_ReturnsZero(string source)
    {
        var info = SetupInfo();
        var src = AppPaths.GetInputFile(source);

        var count = info.GetFrameCount(src, null, _feed.RunCallback);

        Assert.Equal(0, count);
    }

    [Theory]
    [InlineData(AppPaths.TaggedMkv, "matroska")]
    [InlineData(AppPaths.TaggedMp3, "mp3")]
    [InlineData(AppPaths.TaggedM4a, "mp4")]
    public void GetFileInfo_TaggedMedia_ParsesMetadata(string source, string formatContains)
    {
        var info = SetupInfo();
        var src = AppPaths.GetInputFile(source);

        var fileInfo = info.GetFileInfo(src, null, _feed.RunCallback);

        WriteOutput(fileInfo);
        foreach (var kv in fileInfo.Metadata)
        {
            _output.WriteLine($"format[{kv.Key}]={kv.Value}");
        }

        Assert.Contains(formatContains, fileInfo.FormatName ?? "", StringComparison.OrdinalIgnoreCase);
        Assert.Equal("Sample Title", fileInfo.Metadata["title"]);
        Assert.Equal("Sample Artist", fileInfo.Metadata["artist"]);
    }

    [Fact]
    public void GetFileInfo_TaggedMkv_ParsesVideoAndAudioStreams()
    {
        var info = SetupInfo();
        var src = AppPaths.GetInputFile(AppPaths.TaggedMkv);

        var fileInfo = info.GetFileInfo(src, null, _feed.RunCallback);
        WriteOutput(fileInfo);

        Assert.Equal(1, fileInfo.FileStreams.Count(s => s.StreamType == FFmpegStreamType.Video));
        Assert.Equal(2, fileInfo.FileStreams.Count(s => s.StreamType == FFmpegStreamType.Audio));

        var video = Assert.IsType<MediaVideoStreamInfo>(fileInfo.FileStreams[0]);
        Assert.Equal("Main Video", video.Metadata["title"]);

        var originalAudio = Assert.IsType<MediaAudioStreamInfo>(fileInfo.FileStreams[1]);
        Assert.Equal("Original Audio", originalAudio.Metadata["title"]);
        Assert.Equal("440", originalAudio.Metadata["frequency"]);
        Assert.True(originalAudio.Disposition.Has("default"));

        var pitchedAudio = Assert.IsType<MediaAudioStreamInfo>(fileInfo.FileStreams[2]);
        Assert.Equal("Pitched Audio", pitchedAudio.Metadata["title"]);
        Assert.Equal("432", pitchedAudio.Metadata["frequency"]);
        Assert.False(pitchedAudio.Disposition.Has("default"));
    }

    [Fact]
    public void GetFileInfo_Subtitle_ParsesCorrectly()
    {
        var info = SetupInfo();
        var src = AppPaths.GetInputFile(AppPaths.TaggedMkv);

        var fileInfo = info.GetFileInfo(src, null, _feed.RunCallback);
        WriteOutput(fileInfo);

        Assert.Equal(1, fileInfo.FileStreams.Count(s => s.StreamType == FFmpegStreamType.Subtitle));

        var sub = Assert.IsType<MediaSubtitleStreamInfo>(fileInfo.FileStreams[3]);
        Assert.Equal(FFmpegStreamType.Subtitle, sub.StreamType);
        Assert.Equal("subrip", sub.Format);
        Assert.Equal("eng", sub.Language);
        Assert.Equal("English Captions", sub.Metadata["title"]);
        Assert.Same(sub, fileInfo.SubtitleStream);
    }

    [Fact]
    public void GetFileInfo_TaggedMp3_AudioOnlyWithMetadata()
    {
        var info = SetupInfo();
        var src = AppPaths.GetInputFile(AppPaths.TaggedMp3);

        var fileInfo = info.GetFileInfo(src, null, _feed.RunCallback);

        Assert.Single(fileInfo.FileStreams);
        Assert.IsType<MediaAudioStreamInfo>(fileInfo.FileStreams[0]);
        Assert.Equal("Sample Album", fileInfo.Metadata["album"]);
    }

    [Fact]
    public void GetFileInfo_TaggedM4a_ParsesStreamHandlerName()
    {
        var info = SetupInfo();
        var src = AppPaths.GetInputFile(AppPaths.TaggedM4a);

        var fileInfo = info.GetFileInfo(src, null, _feed.RunCallback);

        Assert.Single(fileInfo.FileStreams);
        var audio = Assert.IsType<MediaAudioStreamInfo>(fileInfo.FileStreams[0]);
        Assert.Equal("SoundHandler", audio.Metadata["handler_name"]);
        Assert.True(audio.Disposition.Has("default"));
    }

    [Fact]
    public void GetFileInfo_TaggedMkv_StreamMetadataKeysAreCaseInsensitive()
    {
        var info = SetupInfo();
        var src = AppPaths.GetInputFile(AppPaths.TaggedMkv);

        var fileInfo = info.GetFileInfo(src, null, _feed.RunCallback);
        var audio = fileInfo.AudioStream;

        Assert.NotNull(audio);
        Assert.Equal("440", audio.Metadata["FREQUENCY"]);
        Assert.Equal("440", audio.Metadata["frequency"]);
    }

    [Fact]
    public void GetFileInfo_Data_ParsesCorrectly()
    {
        var info = SetupInfo();
        var src = AppPaths.GetInputFile(AppPaths.Mpeg4);

        var fileInfo = info.GetFileInfo(src, null, _feed.RunCallback);
        WriteOutput(fileInfo);

        Assert.Equal(2, fileInfo.FileStreams.Count(s => s.StreamType == FFmpegStreamType.Data));

        var data0 = Assert.IsType<MediaDataStreamInfo>(fileInfo.FileStreams[0]);
        Assert.Equal(FFmpegStreamType.Data, data0.StreamType);
        Assert.Equal("none", data0.Format);
        Assert.Equal("und", data0.Language);
        Assert.True(data0.Disposition.Has("default"));
        Assert.True(data0.Metadata.ContainsKey("handler_name"));

        var data1 = Assert.IsType<MediaDataStreamInfo>(fileInfo.FileStreams[1]);
        Assert.True(data1.Metadata.ContainsKey("handler_name"));
    }

    [Fact]
    public void GetFileInfo_Attachment_ParsesCorrectly()
    {
        var info = SetupInfo();
        var src = AppPaths.GetInputFile(AppPaths.TaggedAttachment);

        var fileInfo = info.GetFileInfo(src, null, _feed.RunCallback);
        WriteOutput(fileInfo);

        Assert.Equal(1, fileInfo.FileStreams.Count(s => s.StreamType == FFmpegStreamType.Attachment));

        var attach = Assert.IsType<MediaAttachmentStreamInfo>(fileInfo.FileStreams[1]);
        Assert.Equal(FFmpegStreamType.Attachment, attach.StreamType);
        Assert.Equal("ttf", attach.Format);
        Assert.Equal("test.ttf", attach.Metadata["filename"]);
        Assert.Equal("application/x-truetype-font", attach.Metadata["mimetype"]);
    }
}
