using System.Runtime.InteropServices;
using Microsoft.Extensions.Options;

namespace HanumanInstitute.FFmpeg.UnitTests;

public class FFmpegConfigTests
{
    private Mock<IWindowsApiService> _api;

    public IProcessManager SetupConfig()
    {
        _api = new Mock<IWindowsApiService>(MockBehavior.Strict);
        _api.Setup(x => x.AttachConsole(It.IsAny<uint>())).Returns(false);
        var fileSystem = new FakeFileSystemService();
        return new ProcessManager(null, _api.Object, fileSystem);
    }

    [Theory]
    [InlineData(EncoderApp.FFmpeg, "ffmpeg")]
    [InlineData(EncoderApp.x264, "x264")]
    [InlineData(EncoderApp.x265, "x265")]
    public void GetAppPath_EachValue_MatchesResult(EncoderApp encoderApp, string expectedValue)
    {
        var config = SetupConfig();

        var result = config.GetAppPath(encoderApp.ToString());

        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public void SoftKill_Valid_AttachedConsoleCalled()
    {
        var config = SetupConfig();
        var process = Mock.Of<IProcess>(x => x.HasExited == true && x.Id == 1);

        var result = config.SoftKill(process);

        Assert.True(result);
        // Very AttachConsole was called. The rest of SoftKillWinApp will not be tested.
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _api.Verify(x => x.AttachConsole((uint)process.Id), Times.Once());
        }
    }
}
