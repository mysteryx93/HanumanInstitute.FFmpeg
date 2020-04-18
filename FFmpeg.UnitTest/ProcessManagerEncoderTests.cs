using System;
using System.IO;
using HanumanInstitute.FFmpeg.Services;
using Moq;
using Xunit;

namespace HanumanInstitute.FFmpeg.UnitTests
{
    public class ProcessManagerEncoderTests
    {
        protected const string AppFFmpeg = "ffmpeg.exe";
        protected const string AppCmd = "cmd";
        protected const string AvisynthApp = "avs2pipemod.exe";
        protected const string VapourSynthApp = "vspipe.exe";
        protected const string MissingFileName = "MissingFile";
        protected const string TestSource = "source";
        private Mock<FakeMediaConfig> _config;

        protected IProcessWorkerEncoder SetupManager()
        {
            _config = new Mock<FakeMediaConfig>() { CallBase = true };
            var parserFactory = new FileInfoParserFactory();
            var factory = new FakeProcessFactory();
            var fileSystem = Mock.Of<FakeFileSystemService>(x =>
                x.Exists(It.IsAny<string>()) == true && x.Exists(MissingFileName) == false);
            return new ProcessWorkerEncoder(_config.Object, factory, fileSystem, parserFactory, null);
        }


        [Fact]
        public void Constructor_NullDependencies_ThrowsException() => Assert.Throws<ArgumentNullException>(() => new ProcessWorkerEncoder(null, null, null, null, null));

        [Fact]
        public void Constructor_Dependencies_Success() => new ProcessWorkerEncoder(new MediaConfig(), new ProcessFactory(), new FileSystemService(), new FileInfoParserFactory(), null).Dispose();

        [Fact]
        public void Constructor_OptionFFmpeg_Success() => new ProcessWorkerEncoder(new MediaConfig(), new ProcessFactory(), new FileSystemService(), new FileInfoParserFactory(), new ProcessOptionsEncoder()).Dispose();

        [Fact]
        public void Init_OutputType_ReturnError()
        {
            var manager = SetupManager();

            Assert.Equal(ProcessOutput.Error, manager.OutputType);
        }


        [Fact]
        public void Options_SetOptionsFFmpeg_ReturnsSame()
        {
            var manager = SetupManager();
            var options = new ProcessOptionsEncoder();

            manager.Options = options;

            Assert.Equal(options, manager.Options);
        }

        [Fact]
        public void Options_SetOptionsBase_ReturnsNullBaseReturnsSame()
        {
            var manager = SetupManager();
            var managerBase = manager as ProcessWorker;
            var options = new ProcessOptions();

            managerBase.Options = options;

            Assert.Null(manager.Options);
            Assert.Equal(managerBase.Options, options);
        }


        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("args")]
        public void RunEncoder_Valid_CommandContainAppAndArgs(string args)
        {
            var manager = SetupManager();

            var result = manager.RunEncoder(args, EncoderApp.FFmpeg);

            Assert.Equal(CompletionStatus.Success, result);
            Assert.Contains(AppFFmpeg, manager.CommandWithArgs, StringComparison.InvariantCulture);
            if (!string.IsNullOrEmpty(args))
            {
                Assert.Contains(args, manager.CommandWithArgs, StringComparison.InvariantCulture);
            }
        }

        [Theory]
        [InlineData(EncoderApp.FFmpeg)]
        [InlineData(EncoderApp.x264)]
        [InlineData(EncoderApp.x265)]
        public void RunEncoder_Valid_EncodeAppReturnsSpecifiedApp(EncoderApp encoderApp)
        {
            var manager = SetupManager();

            manager.RunEncoder(null, encoderApp);

            Assert.Equal(encoderApp.ToString(), manager.EncoderApp);
        }

        [Fact]
        public void RunEncoder_AppNotFound_ThrowsFileNotFoundException()
        {
            var manager = SetupManager();
            _config.Setup(x => x.FFmpegPath).Returns(MissingFileName);

            Assert.Throws<FileNotFoundException>(() => manager.RunEncoder(null, EncoderApp.FFmpeg));
        }

        [Fact]
        public void RunEncoder_InvalidEncoderApp_ThrowsException()
        {
            var manager = SetupManager();

            Assert.Throws<ArgumentException>(() => manager.RunEncoder(null, null));
        }

        [Theory]
        [InlineData(OutputSamples.FFmpegEncode1, 100)]
        public void RunEncoder_OptionFrameCount_ReturnsSpecifiedFrameCount(string output, int frameCount)
        {
            var manager = SetupManager();
            manager.Options.FrameCount = frameCount;
            manager.ProcessStarted += (s, e) => FakeProcessWorkerFactory.FeedOutputToProcess(manager, output);

            manager.RunEncoder(null, EncoderApp.FFmpeg);

            var info = manager.FileInfo as FileInfoFFmpeg;
            Assert.Equal(frameCount, info.FrameCount);
        }


        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("args")]
        public void RunAvisynthToEncoder_Valid_CommandContainsCmdAndSourceAndArgs(string args)
        {
            var manager = SetupManager();

            var r = manager.RunAvisynthToEncoder(TestSource, args, EncoderApp.FFmpeg);

            Assert.Equal(CompletionStatus.Success, r);
            Assert.Contains(AppCmd, manager.CommandWithArgs, StringComparison.InvariantCulture);
            Assert.Contains(TestSource, manager.CommandWithArgs, StringComparison.InvariantCulture);
            if (!string.IsNullOrEmpty(args))
            {
                Assert.Contains(args, manager.CommandWithArgs, StringComparison.InvariantCulture);
            }
        }

        [Fact]
        public void RunVapourSynthToEncoder_Valid_CommandContainsAvisynth()
        {
            var manager = SetupManager();

            manager.RunAvisynthToEncoder(TestSource, null, EncoderApp.FFmpeg);

            Assert.Contains(AvisynthApp, manager.CommandWithArgs, StringComparison.InvariantCulture);
        }

        [Theory]
        [InlineData(EncoderApp.FFmpeg)]
        [InlineData(EncoderApp.x264)]
        [InlineData(EncoderApp.x265)]
        public void RunAvisynthToEncoder_Valid_EncodeAppReturnsSpecifiedApp(EncoderApp encoderApp)
        {
            var manager = SetupManager();

            manager.RunAvisynthToEncoder(TestSource, null, encoderApp);

            Assert.Equal(encoderApp.ToString(), manager.EncoderApp);
        }

        [Theory]
        [MemberData(nameof(TestDataSource.NullAndEmptyStrings), 1, MemberType = typeof(TestDataSource))]
        public void RunAvisynthToEncoder_EmptyArg_ThrowsException(string source, Type ex)
        {
            var manager = SetupManager();

            void Act() => manager.RunAvisynthToEncoder(source, null, EncoderApp.FFmpeg);

            Assert.Throws(ex, Act);
        }

        [Fact]
        public void RunAvisynthToEncoder_InvalidEncoderApp_ThrowsException()
        {
            var manager = SetupManager();

            Assert.Throws<ArgumentException>(() => manager.RunAvisynthToEncoder(TestSource, null, null));
        }


        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("args")]
        public void RunVapourSynthToEncoder_Valid_CommandContainsCmdAndSourceAndArgs(string args)
        {
            var manager = SetupManager();

            var r = manager.RunVapourSynthToEncoder(TestSource, args, EncoderApp.FFmpeg);

            Assert.Equal(CompletionStatus.Success, r);
            Assert.Contains(AppCmd, manager.CommandWithArgs, StringComparison.InvariantCulture);
            Assert.Contains(TestSource, manager.CommandWithArgs, StringComparison.InvariantCulture);
            if (!string.IsNullOrEmpty(args))
            {
                Assert.Contains(args, manager.CommandWithArgs, StringComparison.InvariantCulture);
            }
        }

        [Fact]
        public void RunVapourSynthToEncoder_Valid_CommandContainsVapourSynth()
        {
            var manager = SetupManager();

            manager.RunVapourSynthToEncoder(TestSource, null, EncoderApp.FFmpeg);

            Assert.Contains(VapourSynthApp, manager.CommandWithArgs, StringComparison.InvariantCulture);
        }

        [Theory]
        [InlineData(EncoderApp.FFmpeg)]
        [InlineData(EncoderApp.x264)]
        [InlineData(EncoderApp.x265)]
        public void RunVapourSynthToEncoder_Valid_EncodeAppReturnsSpecifiedApp(EncoderApp encoderApp)
        {
            var manager = SetupManager();

            manager.RunVapourSynthToEncoder(TestSource, null, encoderApp);

            Assert.Equal(encoderApp.ToString(), manager.EncoderApp);
        }

        [Theory]
        [MemberData(nameof(TestDataSource.NullAndEmptyStrings), 1, MemberType = typeof(TestDataSource))]
        public void RunVapourSynthToEncoder_EmptyArg_ThrowsException(string source, Type ex)
        {
            var manager = SetupManager();

            void Act() => manager.RunVapourSynthToEncoder(source, null, EncoderApp.FFmpeg);

            Assert.Throws(ex, Act);
        }

        [Fact]
        public void RunVapourSynthToEncoder_InvalidEncoderApp_ThrowsException()
        {
            var manager = SetupManager();

            Assert.Throws<ArgumentException>(() => manager.RunVapourSynthToEncoder(TestSource, null, null));
        }


        [Theory]
        [InlineData(OutputSamples.FFmpegEncode1)]
        [InlineData(OutputSamples.FFmpegEncode2)]
        [InlineData(OutputSamples.FFmpegEncode3)]
        public void RunEncoder_InjectFFmpeg_StatusSuccess(string output)
        {
            var manager = SetupManager();
            manager.ProcessStarted += (s, e) => FakeProcessWorkerFactory.FeedOutputToProcess(manager, output);

            var result = manager.RunEncoder(null, EncoderApp.FFmpeg);

            Assert.Equal(CompletionStatus.Success, result);
        }

        [Theory]
        [InlineData(OutputSamples.FFmpegEncode1, 7163)]
        [InlineData(OutputSamples.FFmpegEncode2, 7164)]
        public void RunEncoder_InjectFFmpeg_ExpectedFrameCount(string output, int expectedFrameCount)
        {
            var manager = SetupManager();
            manager.ProcessStarted += (s, e) => FakeProcessWorkerFactory.FeedOutputToProcess(manager, output);

            var result = manager.RunEncoder(null, EncoderApp.FFmpeg);

            var info = manager.FileInfo as FileInfoFFmpeg;
            Assert.Equal(expectedFrameCount, info.FrameCount);
            Assert.True(info.FileDuration > TimeSpan.Zero);
        }

        [Theory]
        [InlineData(OutputSamples.FFmpegEncode1, 29)]
        [InlineData(OutputSamples.FFmpegEncode2, 27)]
        [InlineData(OutputSamples.FFmpegEncode3, 10)]
        public void RunEncoder_InjectFFmpeg_EventsTriggered(string output, int statusLines)
        {
            var manager = SetupManager();
            var dataReceivedCalled = 0;
            manager.DataReceived += (s, e) => dataReceivedCalled++;
            var infoUpdatedCalled = 0;
            manager.FileInfoUpdated += (s, e) => infoUpdatedCalled++;
            var statusUpdatedCalled = 0;
            manager.ProgressReceived += (s, e) => statusUpdatedCalled++;
            manager.ProcessStarted += (s, e) => FakeProcessWorkerFactory.FeedOutputToProcess(manager, output);

            var result = manager.RunEncoder(null, EncoderApp.FFmpeg);

            Assert.True(dataReceivedCalled > 0);
            Assert.Equal(1, infoUpdatedCalled);
            Assert.Equal(statusLines, statusUpdatedCalled);
        }

        [Theory]
        [InlineData(OutputSamples.FFmpegEncode1, "mpeg1video", "mp2")]
        public void RunEncoder_InjectFFmpeg_ExpectedStreams(string output, string videoFormat, string audioFormat)
        {
            var manager = SetupManager();
            manager.ProcessStarted += (s, e) => FakeProcessWorkerFactory.FeedOutputToProcess(manager, output);

            var result = manager.RunEncoder(null, EncoderApp.FFmpeg);

            var info = manager.FileInfo as FileInfoFFmpeg;
            if (videoFormat != null)
            {
                Assert.NotNull(info.VideoStream);
                Assert.Equal(videoFormat, info.VideoStream.Format);
            }
            else
            {
                Assert.Null(info.VideoStream);
            }

            if (audioFormat != null)
            {
                Assert.NotNull(info.AudioStream);
                Assert.Equal(audioFormat, info.AudioStream.Format);
            }
            else
            {
                Assert.Null(info.AudioStream);
            }
        }


        [Theory]
        [InlineData(OutputSamples.X264Encode1)]
        [InlineData(OutputSamples.X264Encode2)]
        [InlineData(OutputSamples.X264Encode3)]
        public void RunEncoder_InjectX264_StatusSuccess(string output)
        {
            var manager = SetupManager();
            manager.ProcessStarted += (s, e) => FakeProcessWorkerFactory.FeedOutputToProcess(manager, output);

            var result = manager.RunEncoder(null, EncoderApp.x264);

            Assert.Equal(CompletionStatus.Success, result);
        }


        [Theory]
        [InlineData(OutputSamples.X264Encode1, 438)]
        [InlineData(OutputSamples.X264Encode2, 0)]
        [InlineData(OutputSamples.X264Encode3, 1000000)]
        public void RunEncoder_InjectX264_ExpectedFrameCount(string output, int expectedFrameCount)
        {
            var manager = SetupManager();
            manager.ProcessStarted += (s, e) => FakeProcessWorkerFactory.FeedOutputToProcess(manager, output);

            var result = manager.RunEncoder(null, EncoderApp.x264);

            var info = manager.FileInfo as FileInfoX264;
            Assert.Equal(expectedFrameCount, info.FrameCount);
        }

        [Theory]
        [InlineData(OutputSamples.X264Encode1, 6)]
        [InlineData(OutputSamples.X264Encode2, 10)]
        [InlineData(OutputSamples.X264Encode3, 10)]
        public void RunEncoder_InjectX264_EventsTriggered(string output, int statusLines)
        {
            var manager = SetupManager();
            var dataReceivedCalled = 0;
            manager.DataReceived += (s, e) => dataReceivedCalled++;
            var infoUpdatedCalled = 0;
            manager.FileInfoUpdated += (s, e) => infoUpdatedCalled++;
            var statusUpdatedCalled = 0;
            manager.ProgressReceived += (s, e) => statusUpdatedCalled++;
            manager.ProcessStarted += (s, e) => FakeProcessWorkerFactory.FeedOutputToProcess(manager, output);

            var result = manager.RunEncoder(null, EncoderApp.x264);

            Assert.True(dataReceivedCalled > 0);
            Assert.Equal(1, infoUpdatedCalled);
            Assert.Equal(statusLines, statusUpdatedCalled);
        }
    }
}
