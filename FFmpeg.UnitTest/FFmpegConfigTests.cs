using System;
using HanumanInstitute.FFmpeg.Services;
using Moq;
using Xunit;

namespace HanumanInstitute.FFmpeg.UnitTests
{
    public class FFmpegConfigTests
    {
        private Mock<IWindowsApiService> _api;

        public IMediaConfig SetupConfig()
        {
            _api = new Mock<IWindowsApiService>(MockBehavior.Strict);
            _api.Setup(x => x.AttachConsole(It.IsAny<uint>())).Returns(false);
            var fileSystem = new FakeFileSystemService();
            return new MediaConfig(_api.Object, fileSystem);
        }

        [Theory]
        [InlineData(EncoderApp.FFmpeg, "ffmpeg.exe")]
        [InlineData(EncoderApp.x264, "x264.exe")]
        [InlineData(EncoderApp.x265, "x265.exe")]
        public void GetAppPath_EachValue_MatchesResult(EncoderApp encoderApp, string expectedValue)
        {
            var config = SetupConfig();

            var result = config.GetAppPath(encoderApp.ToString());

            Assert.Equal(expectedValue, result);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SoftKill_Valid_AttachedConsoleCalled(bool handled)
        {
            var config = SetupConfig();
            CloseProcessEventArgs calledArgs = null;
            config.CloseProcess += (s, e) =>
            {
                calledArgs = e;
                if (handled)
                {
                    e.Handled = true;
                }
            };
            var process = Mock.Of<IProcess>(x => x.HasExited == true && x.Id == 1);

            var result = config.SoftKill(process);

            Assert.True(result);
            Assert.NotNull(calledArgs);
            Assert.Equal(process, calledArgs.Process);
            // Very AttachConsole was called. The rest of SoftKillWinApp will not be tested.
            _api.Verify(x => x.AttachConsole((uint)process.Id), handled ? Times.Never() : Times.Once());
        }
    }
}
