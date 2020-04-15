using System;
using Moq;
using Xunit;
using EmergenceGuardian.Encoder.Services;
using System.IO;

namespace EmergenceGuardian.Encoder.UnitTests {
    public class ProcessManagerEncoderTests {

        #region Utility Functions

        protected const string AppFFmpeg = "ffmpeg.exe";
        protected const string AppCmd = "cmd";
        protected const string AvisynthApp = "avs2pipemod.exe";
        protected const string VapourSynthApp = "vspipe.exe";
        protected const string MissingFileName = "MissingFile";
        protected const string TestSource = "source";
        protected Mock<FakeMediaConfig> config;

        protected IProcessWorkerEncoder SetupManager() {
            config = new Mock<FakeMediaConfig>() { CallBase = true };
            var parserFactory = new FileInfoParserFactory();
            var factory = new FakeProcessFactory();
            var fileSystem = Mock.Of<FakeFileSystemService>(x =>
                x.Exists(It.IsAny<string>()) == true && x.Exists(MissingFileName) == false);
            return new ProcessWorkerEncoder(config.Object, factory, fileSystem, parserFactory);
        }

        #endregion

        #region Constructors

        [Fact]
        public void Constructor_Empty_Success() => new ProcessWorkerEncoder();

        [Fact]
        public void Constructor_NullDependencies_ThrowsException() => Assert.Throws<ArgumentNullException>(() => new ProcessWorkerEncoder(null, null, null, null, null));

        [Fact]
        public void Constructor_Dependencies_Success() => new ProcessWorkerEncoder(new MediaConfig(), new ProcessFactory(), new FileSystemService(), new FileInfoParserFactory(), null);

        [Fact]
        public void Constructor_OptionFFmpeg_Success() => new ProcessWorkerEncoder(new MediaConfig(), new ProcessFactory(), new FileSystemService(), new FileInfoParserFactory(), new ProcessOptionsEncoder());

        [Fact]
        public void Init_OutputType_ReturnError() {
            var Manager = SetupManager();

            Assert.Equal(ProcessOutput.Error, Manager.OutputType);
        }

        #endregion

        #region Options

        [Fact]
        public void Options_SetOptionsFFmpeg_ReturnsSame() {
            var Manager = SetupManager();
            var Options = new ProcessOptionsEncoder();

            Manager.Options = Options;

            Assert.Equal(Options, Manager.Options);
        }

        [Fact]
        public void Options_SetOptionsBase_ReturnsNullBaseReturnsSame() {
            var Manager = SetupManager();
            var ManagerBase = Manager as ProcessWorker;
            var Options = new ProcessOptions();

            ManagerBase.Options = Options;

            Assert.Null(Manager.Options);
            Assert.Equal(ManagerBase.Options, Options);
        }

        #endregion

        #region RunEncoder

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("args")]
        public void RunEncoder_Valid_CommandContainAppAndArgs(string args) {
            var Manager = SetupManager();

            var Result = Manager.RunEncoder(args, EncoderApp.FFmpeg);

            Assert.Equal(CompletionStatus.Success, Result);
            Assert.Contains(AppFFmpeg, Manager.CommandWithArgs);
            if (!string.IsNullOrEmpty(args))
                Assert.Contains(args, Manager.CommandWithArgs);
        }

        [Theory]
        [InlineData(EncoderApp.FFmpeg)]
        [InlineData(EncoderApp.x264)]
        [InlineData(EncoderApp.x265)]
        public void RunEncoder_Valid_EncodeAppReturnsSpecifiedApp(EncoderApp encoderApp) {
            var Manager = SetupManager();

            Manager.RunEncoder(null, encoderApp);

            Assert.Equal(encoderApp.ToString(), Manager.EncoderApp);
        }

        [Fact]
        public void RunEncoder_AppNotFound_ThrowsFileNotFoundException() {
            var Manager = SetupManager();
            config.Setup(x => x.FFmpegPath).Returns(MissingFileName);

            Assert.Throws<FileNotFoundException>(() => Manager.RunEncoder(null, EncoderApp.FFmpeg));
        }

        [Fact]
        public void RunEncoder_InvalidEncoderApp_ThrowsException() {
            var Manager = SetupManager();

            Assert.Throws<ArgumentException>(() => Manager.RunEncoder(null, null));
        }

        [Theory]
        [InlineData(OutputSamples.FFmpegEncode1, 100)]
        public void RunEncoder_OptionFrameCount_ReturnsSpecifiedFrameCount(string output, int frameCount) {
            var Manager = SetupManager();
            Manager.Options.FrameCount = frameCount;
            Manager.ProcessStarted += (s, e) => FakeProcessWorkerFactory.FeedOutputToProcess(Manager, output);

            Manager.RunEncoder(null, EncoderApp.FFmpeg);

            var Info = Manager.FileInfo as FileInfoFFmpeg;
            Assert.Equal(frameCount, Info.FrameCount);
        }

        #endregion

        #region RunAvisynthToEncoder

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("args")]
        public void RunAvisynthToEncoder_Valid_CommandContainsCmdAndSourceAndArgs(string args) {
            var Manager = SetupManager();

            var R = Manager.RunAvisynthToEncoder(TestSource, args, EncoderApp.FFmpeg);

            Assert.Equal(CompletionStatus.Success, R);
            Assert.Contains(AppCmd, Manager.CommandWithArgs);
            Assert.Contains(TestSource, Manager.CommandWithArgs);
            if (!string.IsNullOrEmpty(args))
                Assert.Contains(args, Manager.CommandWithArgs);
        }

        [Fact]
        public void RunVapourSynthToEncoder_Valid_CommandContainsAvisynth() {
            var Manager = SetupManager();

            Manager.RunAvisynthToEncoder(TestSource, null, EncoderApp.FFmpeg);

            Assert.Contains(AvisynthApp, Manager.CommandWithArgs);
        }

        [Theory]
        [InlineData(EncoderApp.FFmpeg)]
        [InlineData(EncoderApp.x264)]
        [InlineData(EncoderApp.x265)]
        public void RunAvisynthToEncoder_Valid_EncodeAppReturnsSpecifiedApp(EncoderApp encoderApp) {
            var Manager = SetupManager();

            Manager.RunAvisynthToEncoder(TestSource, null, encoderApp);

            Assert.Equal(encoderApp.ToString(), Manager.EncoderApp);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void RunAvisynthToEncoder_EmptyArg_ThrowsException(string source) {
            var Manager = SetupManager();

            Assert.Throws<ArgumentException>(() => Manager.RunAvisynthToEncoder(source, null, EncoderApp.FFmpeg));
        }

        [Fact]
        public void RunAvisynthToEncoder_InvalidEncoderApp_ThrowsException() {
            var Manager = SetupManager();

            Assert.Throws<ArgumentException>(() => Manager.RunAvisynthToEncoder(TestSource, null, null));
        }


        #endregion

        #region RunVapourSynthToEncoder

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("args")]
        public void RunVapourSynthToEncoder_Valid_CommandContainsCmdAndSourceAndArgs(string args) {
            var Manager = SetupManager();

            var R = Manager.RunVapourSynthToEncoder(TestSource, args, EncoderApp.FFmpeg);

            Assert.Equal(CompletionStatus.Success, R);
            Assert.Contains(AppCmd, Manager.CommandWithArgs);
            Assert.Contains(TestSource, Manager.CommandWithArgs);
            if (!string.IsNullOrEmpty(args))
                Assert.Contains(args, Manager.CommandWithArgs);
        }

        [Fact]
        public void RunVapourSynthToEncoder_Valid_CommandContainsVapourSynth() {
            var Manager = SetupManager();

            Manager.RunVapourSynthToEncoder(TestSource, null, EncoderApp.FFmpeg);

            Assert.Contains(VapourSynthApp, Manager.CommandWithArgs);
        }

        [Theory]
        [InlineData(EncoderApp.FFmpeg)]
        [InlineData(EncoderApp.x264)]
        [InlineData(EncoderApp.x265)]
        public void RunVapourSynthToEncoder_Valid_EncodeAppReturnsSpecifiedApp(EncoderApp encoderApp) {
            var Manager = SetupManager();

            Manager.RunVapourSynthToEncoder(TestSource, null, encoderApp);

            Assert.Equal(encoderApp.ToString(), Manager.EncoderApp);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void RunVapourSynthToEncoder_EmptyArg_ThrowsException(string source) {
            var Manager = SetupManager();

            Assert.Throws<ArgumentException>(() => Manager.RunVapourSynthToEncoder(source, null, EncoderApp.FFmpeg));
        }

        [Fact]
        public void RunVapourSynthToEncoder_InvalidEncoderApp_ThrowsException() {
            var Manager = SetupManager();

            Assert.Throws<ArgumentException>(() => Manager.RunVapourSynthToEncoder(TestSource, null, null));
        }

        #endregion

        #region RunEncoder InjectFFmpeg

        [Theory]
        [InlineData(OutputSamples.FFmpegEncode1)]
        [InlineData(OutputSamples.FFmpegEncode2)]
        [InlineData(OutputSamples.FFmpegEncode3)]
        public void RunEncoder_InjectFFmpeg_StatusSuccess(string output) {
            var Manager = SetupManager();
            Manager.ProcessStarted += (s, e) => FakeProcessWorkerFactory.FeedOutputToProcess(Manager, output);

            var Result = Manager.RunEncoder(null, EncoderApp.FFmpeg);

            Assert.Equal(CompletionStatus.Success, Result);
        }

        [Theory]
        [InlineData(OutputSamples.FFmpegEncode1, 7163)]
        [InlineData(OutputSamples.FFmpegEncode2, 7164)]
        public void RunEncoder_InjectFFmpeg_ExpectedFrameCount(string output, int expectedFrameCount) {
            var Manager = SetupManager();
            Manager.ProcessStarted += (s, e) => FakeProcessWorkerFactory.FeedOutputToProcess(Manager, output);

            var Result = Manager.RunEncoder(null, EncoderApp.FFmpeg);

            var Info = Manager.FileInfo as FileInfoFFmpeg;
            Assert.Equal(expectedFrameCount, Info.FrameCount);
            Assert.True(Info.FileDuration > TimeSpan.Zero);
        }

        [Theory]
        [InlineData(OutputSamples.FFmpegEncode1, 29)]
        [InlineData(OutputSamples.FFmpegEncode2, 27)]
        [InlineData(OutputSamples.FFmpegEncode3, 10)]
        public void RunEncoder_InjectFFmpeg_EventsTriggered(string output, int statusLines) {
            var Manager = SetupManager();
            int DataReceivedCalled = 0;
            Manager.DataReceived += (s, e) => DataReceivedCalled++;
            int InfoUpdatedCalled = 0;
            Manager.FileInfoUpdated += (s, e) => InfoUpdatedCalled++;
            int StatusUpdatedCalled = 0;
            Manager.ProgressReceived += (s, e) => StatusUpdatedCalled++;
            Manager.ProcessStarted += (s, e) => FakeProcessWorkerFactory.FeedOutputToProcess(Manager, output);

            var Result = Manager.RunEncoder(null, EncoderApp.FFmpeg);

            Assert.True(DataReceivedCalled > 0);
            Assert.Equal(1, InfoUpdatedCalled);
            Assert.Equal(statusLines, StatusUpdatedCalled);
        }

        [Theory]
        [InlineData(OutputSamples.FFmpegEncode1, "mpeg1video", "mp2")]
        public void RunEncoder_InjectFFmpeg_ExpectedStreams(string output, string videoFormat, string audioFormat) {
            var Manager = SetupManager();
            Manager.ProcessStarted += (s, e) => FakeProcessWorkerFactory.FeedOutputToProcess(Manager, output);

            var Result = Manager.RunEncoder(null, EncoderApp.FFmpeg);

            var Info = Manager.FileInfo as FileInfoFFmpeg;
            if (videoFormat != null) {
                Assert.NotNull(Info.VideoStream);
                Assert.Equal(videoFormat, Info.VideoStream.Format);
            } else
                Assert.Null(Info.VideoStream);
            if (audioFormat != null) {
                Assert.NotNull(Info.AudioStream);
                Assert.Equal(audioFormat, Info.AudioStream.Format);
            } else
                Assert.Null(Info.AudioStream);
        }

        #endregion

        #region RunEncoder InjectX264

        [Theory]
        [InlineData(OutputSamples.X264Encode1)]
        [InlineData(OutputSamples.X264Encode2)]
        [InlineData(OutputSamples.X264Encode3)]
        public void RunEncoder_InjectX264_StatusSuccess(string output) {
            var Manager = SetupManager();
            Manager.ProcessStarted += (s, e) => FakeProcessWorkerFactory.FeedOutputToProcess(Manager, output);

            var Result = Manager.RunEncoder(null, EncoderApp.x264);

            Assert.Equal(CompletionStatus.Success, Result);
        }


        [Theory]
        [InlineData(OutputSamples.X264Encode1, 438)]
        [InlineData(OutputSamples.X264Encode2, 0)]
        [InlineData(OutputSamples.X264Encode3, 1000000)]
        public void RunEncoder_InjectX264_ExpectedFrameCount(string output, int expectedFrameCount) {
            var Manager = SetupManager();
            Manager.ProcessStarted += (s, e) => FakeProcessWorkerFactory.FeedOutputToProcess(Manager, output);

            var Result = Manager.RunEncoder(null, EncoderApp.x264);

            var Info = Manager.FileInfo as FileInfoX264;
            Assert.Equal(expectedFrameCount, Info.FrameCount);
        }

        [Theory]
        [InlineData(OutputSamples.X264Encode1, 6)]
        [InlineData(OutputSamples.X264Encode2, 10)]
        [InlineData(OutputSamples.X264Encode3, 10)]
        public void RunEncoder_InjectX264_EventsTriggered(string output, int statusLines) {
            var Manager = SetupManager();
            int DataReceivedCalled = 0;
            Manager.DataReceived += (s, e) => DataReceivedCalled++;
            int InfoUpdatedCalled = 0;
            Manager.FileInfoUpdated += (s, e) => InfoUpdatedCalled++;
            int StatusUpdatedCalled = 0;
            Manager.ProgressReceived += (s, e) => StatusUpdatedCalled++;
            Manager.ProcessStarted += (s, e) => FakeProcessWorkerFactory.FeedOutputToProcess(Manager, output);

            var Result = Manager.RunEncoder(null, EncoderApp.x264);

            Assert.True(DataReceivedCalled > 0);
            Assert.Equal(1, InfoUpdatedCalled);
            Assert.Equal(statusLines, StatusUpdatedCalled);
        }

        #endregion

    }
}