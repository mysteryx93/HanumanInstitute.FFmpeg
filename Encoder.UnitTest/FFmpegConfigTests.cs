using System;
using Moq;
using Xunit;
using EmergenceGuardian.Encoder.Services;

namespace EmergenceGuardian.Encoder.UnitTests {
    public class FFmpegConfigTests {
        protected Mock<IWindowsApiService> api;

        public IMediaConfig SetupConfig() {
            api = new Mock<IWindowsApiService>(MockBehavior.Strict);
            api.Setup(x => x.AttachConsole(It.IsAny<uint>())).Returns(false);
            var FileSystem = new FakeFileSystemService();
            return new MediaConfig(api.Object, FileSystem);
        }

        [Theory]
        [InlineData(EncoderApp.FFmpeg, "ffmpeg.exe")]
        [InlineData(EncoderApp.x264, "x264.exe")]
        [InlineData(EncoderApp.x265, "x265.exe")]
        public void GetAppPath_EachValue_MatchesResult(EncoderApp encoderApp, string expectedValue) {
            var Config = SetupConfig();

            var Result = Config.GetAppPath(encoderApp.ToString());

            Assert.Equal(expectedValue, Result);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SoftKill_Valid_AttachedConsoleCalled(bool handled) {
            var Config = SetupConfig();
            CloseProcessEventArgs CalledArgs = null;
            Config.CloseProcess += (s, e) => {
                CalledArgs = e;
                if (handled)
                    e.Handled = true;
            };
            var Process = Mock.Of<IProcess>(x => x.HasExited == true && x.Id == 1);

            bool Result = Config.SoftKill(Process);

            Assert.True(Result);
            Assert.NotNull(CalledArgs);
            Assert.Equal(Process, CalledArgs.Process);
            // Very AttachConsole was called. The rest of SoftKillWinApp will not be tested.
            api.Verify(x => x.AttachConsole((uint)Process.Id), handled ? Times.Never() : Times.Once());
        }
    }
}
