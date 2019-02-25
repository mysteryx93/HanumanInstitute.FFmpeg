using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace EmergenceGuardian.Encoder.IntegrationTests {
    public class ProcessManagerEncoderTests {
        private readonly ITestOutputHelper output;
        private readonly OutputFeeder feed;
        private IProcessWorkerFactory factory;

        public ProcessManagerEncoderTests(ITestOutputHelper output) {
            this.output = output;
            feed = new OutputFeeder(output);
        }

        protected IProcessWorkerEncoder SetupManager() {
            factory = FactoryConfig.CreateWithConfig();
            return factory.CreateEncoder(null, feed.RunCallback);
        }

        [Theory]
        [InlineData(AppPaths.StreamH264, ".mp4")]
        public void RunEncoder_AppX264_Success(string videoFile, string destExt) {
            string SrcVideo = AppPaths.GetInputFile(videoFile);
            string Dest = AppPaths.PrepareDestPath("RunEncoderX264", videoFile, destExt);
            string Args = string.Format(@"--preset ultrafast --output ""{1}"" ""{0}""", SrcVideo, Dest);
            var Manager = SetupManager();

            var Result = Manager.RunEncoder(Args, EncoderApp.x264);

            Assert.Equal(CompletionStatus.Success, Result);
            Assert.True(File.Exists(Dest));
        }

        [Theory]
        [InlineData(AppPaths.Avisynth, ".mp4")]
        public void RunAvisynthToEncoder_AppX264_Success(string videoFile, string destExt) {
            string SrcVideo = AppPaths.GetInputFile(videoFile);
            string Dest = AppPaths.PrepareDestPath("RunAvisynthToX264", videoFile, destExt);
            string Args = string.Format(@"--demuxer y4m --preset ultrafast -o ""{0}"" -", Dest);
            var Manager = SetupManager();

            var Result = Manager.RunAvisynthToEncoder(SrcVideo, Args, EncoderApp.x264);

            Assert.Equal(CompletionStatus.Success, Result);
            Assert.True(File.Exists(Dest));
        }

        [Theory]
        [InlineData(AppPaths.VapourSynth, ".mp4")]
        public void RunVapourSynthToEncoder_AppX264_Success(string videoFile, string destExt) {
            string SrcVideo = AppPaths.GetInputFile(videoFile);
            string Dest = AppPaths.PrepareDestPath("RunVapourSynthToX264", videoFile, destExt);
            string Args = string.Format(@"--demuxer y4m --preset ultrafast  -o ""{0}"" -", Dest);
            var Manager = SetupManager();

            var Result = Manager.RunVapourSynthToEncoder(SrcVideo, Args, EncoderApp.x264);

            Assert.Equal(CompletionStatus.Success, Result);
            Assert.True(File.Exists(Dest));
        }
    }
}
