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
    [InlineData(AppPaths.Mpeg4, 1)]
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
}
