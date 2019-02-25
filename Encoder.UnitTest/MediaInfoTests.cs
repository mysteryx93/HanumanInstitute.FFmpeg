using System;
using System.Linq;
using EmergenceGuardian.Encoder.Services;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace EmergenceGuardian.Encoder.UnitTests {
    public class MediaInfoTests {

        #region Declarations

        private FakeProcessWorkerFactory factory;
        private readonly ITestOutputHelper output;
        private const string TestFile = "test";

        public MediaInfoTests(ITestOutputHelper output) {
            this.output = output;
        }

        #endregion

        #region Utility Functions

        protected IMediaInfoReader SetupInfo() {
            factory = new FakeProcessWorkerFactory();
            return new MediaInfoReader(factory);
        }

        protected void AssertSingleInstance() {
            string ResultCommand = factory.Instances.FirstOrDefault()?.CommandWithArgs;
            output.WriteLine(ResultCommand);
            Assert.Single(factory.Instances);
            Assert.NotNull(ResultCommand);
        }

        #endregion

        #region Constructors

        [Fact]
        public void Constructor_WithFactory_Success() => new MediaInfoReader(new FakeProcessWorkerFactory());

        [Fact]
        public void Constructor_NullFactory_ThrowsException() => Assert.Throws<ArgumentNullException>(() => new MediaInfoReader(null));

        #endregion

        #region GetVersion

        [Fact]
        public void GetVersion_Valid_ReturnsOutput() {
            var Info = SetupInfo();

            string Result = Info.GetVersion();

            AssertSingleInstance();
            Assert.NotNull(Result);
        }

        [Fact]
        public void GetVersion_ParamOptions_ReturnsSame() {
            var Info = SetupInfo();
            var Options = new ProcessOptionsEncoder();

            Info.GetVersion(Options);

            Assert.Same(Options, factory.Instances[0].Options);
        }

        [Fact]
        public void GetVersion_ParamCallback_CallbackCalled() {
            var Info = SetupInfo();
            int CallbackCalled = 0;

            Info.GetVersion(null, (s, e) => CallbackCalled++);

            Assert.Equal(1, CallbackCalled);
        }

        #endregion

        #region GetFileInfo

        [Theory]
        [InlineData("source")]
        public void GetFileInfo_Valid_ReturnsProcessManager(string source) {
            var Info = SetupInfo();

            Info.GetFileInfo(source);

            AssertSingleInstance();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void GetFileInfo_EmptyArg_ThrowsException(string source) {
            var Info = SetupInfo();

            Assert.Throws<ArgumentException>(() => Info.GetFileInfo(source));
        }

        [Theory]
        [InlineData("source")]
        public void GetFileInfo_ParamOptions_ReturnsSame(string source) {
            var Info = SetupInfo();
            var Options = new ProcessOptionsEncoder();

            Info.GetFileInfo(source, Options);

            Assert.Same(Options, factory.Instances[0].Options);
        }

        [Theory]
        [InlineData("source")]
        public void GetFileInfo_ParamCallback_CallbackCalled(string source) {
            var Info = SetupInfo();
            int CallbackCalled = 0;

            Info.GetFileInfo(source, null, (s, e) => CallbackCalled++);

            Assert.Equal(1, CallbackCalled);
        }

        #endregion

        #region GetFrameCount

        [Theory]
        [InlineData(OutputSamples.FFmpegInfoFrameCount, 438)]
        public void GetFrameCount_Valid_ReturnsFrameCount(string output, int expected) {
            var Info = SetupInfo();
            ProcessStartedEventHandler Callback = (s, e) => FakeProcessWorkerFactory.FeedOutputToProcess(e.ProcessWorker, output);

            var Result = Info.GetFrameCount(TestFile, null, Callback);

            Assert.Equal(expected, Result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void GetFrameCount_EmptyArg_ThrowsException(string source) {
            var Info = SetupInfo();

            Assert.Throws<ArgumentException>(() => Info.GetFrameCount(source));
        }

        [Theory]
        [InlineData("source")]
        public void GetFrameCount_ParamOptions_ReturnsSame(string source) {
            var Info = SetupInfo();
            var Options = new ProcessOptionsEncoder();

            Info.GetFrameCount(source, Options);

            Assert.Same(Options, factory.Instances[0].Options);
        }

        [Theory]
        [InlineData("source")]
        public void GetFrameCount_ParamCallback_CallbackCalled(string source) {
            var Info = SetupInfo();
            int CallbackCalled = 0;

            Info.GetFrameCount(source, null, (s, e) => CallbackCalled++);

            Assert.Equal(1, CallbackCalled);
        }

        #endregion

    }
}