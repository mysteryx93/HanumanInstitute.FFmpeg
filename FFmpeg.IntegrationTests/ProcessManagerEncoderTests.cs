using System;
using System.Globalization;
using System.IO;
using Xunit;
using Xunit.Abstractions;
using static System.FormattableString;

namespace HanumanInstitute.FFmpeg.IntegrationTests;

public class ProcessManagerEncoderTests
{
    private readonly ITestOutputHelper _output;
    private readonly OutputFeeder _feed;
    private IProcessService _factory;

    public ProcessManagerEncoderTests(ITestOutputHelper output)
    {
        _output = output;
        _feed = new OutputFeeder(output);
    }

    protected IProcessWorkerEncoder SetupManager()
    {
        _factory = FactoryConfig.CreateWithConfig();
        return _factory.CreateEncoder(null, null, _feed.RunCallback);
    }

    [Theory]
    [InlineData(AppPaths.StreamH264, ".mp4")]
    public void RunEncoder_AppX264_Success(string videoFile, string destExt)
    {
        var srcVideo = AppPaths.GetInputFile(videoFile);
        var dest = AppPaths.PrepareDestPath("RunEncoderX264", videoFile, destExt);
        var args = Invariant($@"--preset ultrafast --output ""{dest}"" ""{srcVideo}""");
        var manager = SetupManager();

        var result = manager.RunEncoder(args, EncoderApp.x264);

        Assert.Equal(CompletionStatus.Success, result);
        Assert.True(File.Exists(dest));
    }

    [Theory]
    [InlineData(AppPaths.Avisynth, ".mp4")]
    public void RunAvisynthToEncoder_AppX264_Success(string videoFile, string destExt)
    {
        var srcVideo = AppPaths.GetInputFile(videoFile);
        var dest = AppPaths.PrepareDestPath("RunAvisynthToX264", videoFile, destExt);
        var args = Invariant($@"--demuxer y4m --preset ultrafast -o ""{dest}"" -");
        var manager = SetupManager();

        var result = manager.RunAvisynthToEncoder(srcVideo, args, EncoderApp.x264);

        Assert.Equal(CompletionStatus.Success, result);
        Assert.True(File.Exists(dest));
    }

    [Theory]
    [InlineData(AppPaths.VapourSynth, ".mp4")]
    public void RunVapourSynthToEncoder_AppX264_Success(string videoFile, string destExt)
    {
        var srcVideo = AppPaths.GetInputFile(videoFile);
        var dest = AppPaths.PrepareDestPath("RunVapourSynthToX264", videoFile, destExt);
        var args = Invariant($@"--demuxer y4m --preset ultrafast  -o ""{dest}"" -");
        var manager = SetupManager();

        var result = manager.RunVapourSynthToEncoder(srcVideo, args, EncoderApp.x264);

        Assert.Equal(CompletionStatus.Success, result);
        Assert.True(File.Exists(dest));
    }
}