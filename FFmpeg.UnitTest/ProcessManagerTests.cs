using System;
using System.Diagnostics;
using Moq;
using Xunit;
using EmergenceGuardian.Encoder.Services;
using System.Reflection;

namespace EmergenceGuardian.Encoder.UnitTests {
    public class ProcessManagerTests {

        #region Utility Functions

        protected const string MissingFileName = "MissingFile";
        protected const string TestFileName = "test";
        protected const string AppCmd = "cmd";
        protected const string ErrorDataStream = "data_error";
        protected const string OutputDataStream = "data_output";
        protected Mock<IMediaConfig> config;

        public IProcessWorker SetupManager() {
            config = new Mock<IMediaConfig>();
            var factory = new FakeProcessFactory();
            var fileSystem = Mock.Of<FakeFileSystemService>(x =>
                x.Exists(It.IsAny<string>()) == true && x.Exists(MissingFileName) == false);
            return new ProcessWorker(config.Object, factory, fileSystem);
        }

        #endregion

        #region Constructors

        [Fact]
        public void Constructor_Empty_Success() => new ProcessWorker();

        [Fact]
        public void Constructor_NullDependencies_ThrowsException() => Assert.Throws<ArgumentNullException>(() => new ProcessWorker(null, null, null, null));

        [Fact]
        public void Constructor_Dependencies_Success() => new ProcessWorker(new MediaConfig(), new ProcessFactory(), new FileSystemService(), null);

        [Fact]
        public void Constructor_OptionGeneric_Success() => new ProcessWorker(new MediaConfig(), new ProcessFactory(), new FileSystemService(), new ProcessOptions());

        [Fact]
        public void Constructor_OptionFFmpeg_Success() => new ProcessWorker(new MediaConfig(), new ProcessFactory(), new FileSystemService(), new ProcessOptionsEncoder());

        [Fact]
        public void Init_OutputType_ReturnsNone() {
            var Manager = SetupManager();

            Assert.Equal(ProcessOutput.None, Manager.OutputType);
        }

        [Fact]
        public void Init_CommandWithArgs_ReturnsNull() {
            var Manager = SetupManager();

            Assert.Null(Manager.CommandWithArgs);
        }

        #endregion

        #region Run

        //[Fact]
        //public async Task Run_TwiceSameTime_ThrowsException() {
        //    var Manager = SetupManager();

        //    Task T1 = Task.Run(() => Manager.Run("test", null));
        //    Task T2 = Task.Run(() => Manager.Run("test", null));

        //    await Assert.ThrowsAsync<InvalidOperationException>(() => Task.WhenAll(new Task[] { T1, T2 }));
        //}

        [Fact]
       public void Run_Valid_ReturnsStatusSuccess() {
            var Manager = SetupManager();

            CompletionStatus Result = Manager.Run(TestFileName, null);

            Assert.Equal(CompletionStatus.Success, Result);
            Assert.Equal(CompletionStatus.Success, Manager.LastCompletionStatus);
        }

        [Fact]
        public void Run_Valid_ProcessStartedCalledWithValidArgs() {
            var Manager = SetupManager();

            int ProcessStartedCalled = 0;
            Manager.ProcessStarted += (s, e) => {
                ProcessStartedCalled++;
                Assert.NotNull(s);
                Assert.NotNull(e.ProcessWorker);
                Assert.Equal(Manager, e.ProcessWorker);
                Assert.NotNull(Manager.WorkProcess);
            };

            CompletionStatus Result = Manager.Run(TestFileName, null);

            Assert.Equal(1, ProcessStartedCalled);
        }

        [Fact]
        public void Run_Valid_CompletedCalledWithValidArgs() {
            var Manager = SetupManager();

            int CompletedCalled = 0;
            Manager.ProcessCompleted += (s, e) => {
                CompletedCalled++;
                Assert.Equal(CompletionStatus.Success, e.Status);
            };

            CompletionStatus Result = Manager.Run(TestFileName, null);

            Assert.Equal(1, CompletedCalled);
        }

        [Fact]
        public void Run_Timeout_CompletedCalledWithStatusTimeout() {
            var Manager = SetupManager();
            Manager.Options.Timeout = TimeSpan.FromMilliseconds(10);
            Manager.ProcessStarted += (s, e) => {
                var PMock = Mock.Get<IProcess>(e.ProcessWorker.WorkProcess);
                PMock.Setup(x => x.WaitForExit(It.IsAny<int>())).Returns(false);
            };

            int CompletedCalled = 0;
            Manager.ProcessCompleted += (s, e) => {
                CompletedCalled++;
                Assert.Equal(CompletionStatus.Timeout, e.Status);
            };

            CompletionStatus Result = Manager.Run(TestFileName, null);

            Assert.Equal(CompletionStatus.Timeout, Result);
            Assert.Equal(1, CompletedCalled);
            Assert.Equal(CompletionStatus.Timeout, Manager.LastCompletionStatus);
        }

        [Fact]
        public void Run_Cancel_CompletedCalledWithStatusCancelled() {
            var Manager = SetupManager();
            Mock<IProcess> PMock;
            Manager.Options.Timeout = TimeSpan.FromSeconds(2);
            Manager.ProcessStarted += (s, e) => {
                PMock = Mock.Get<IProcess>(e.ProcessWorker.WorkProcess);
                PMock.Setup(x => x.WaitForExit(It.IsAny<int>())).Returns(false);
                Manager.Cancel();
                config.Setup(x => x.SoftKill(It.IsAny<IProcess>())).Callback(() => PMock.Setup(x => x.HasExited).Returns(true));
            };

            int CompletedCalled = 0;
            Manager.ProcessCompleted += (s, e) => {
                CompletedCalled++;
                Assert.Equal(CompletionStatus.Cancelled, e.Status);
            };

            CompletionStatus Result = Manager.Run(TestFileName, null);

            Assert.Equal(CompletionStatus.Cancelled, Result);
            Assert.Equal(1, CompletedCalled);
            Assert.Equal(CompletionStatus.Cancelled, Manager.LastCompletionStatus);
        }

        [Theory]
        [InlineData("ffmpeg.exe", null)]
        [InlineData("ffmpeg.exe", "-i abc.avi")]
        public void Run_Valid_CommandContainsFileNameAndArgs(string fileName, string args) {
            var Manager = SetupManager();

            Manager.Run(fileName, args);

            Assert.Contains(fileName, Manager.CommandWithArgs);
            if (!string.IsNullOrEmpty(args))
                Assert.Contains(args, Manager.CommandWithArgs);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Run_EmptyFileName_ThrowsException(string fileName) {
            var Manager = SetupManager();

            Assert.Throws<ArgumentException>(() => Manager.Run(fileName, null));
        }

        [Theory]
        [InlineData(ProcessOutput.Error, ErrorDataStream)]
        [InlineData(ProcessOutput.Output, OutputDataStream)]
        [InlineData(ProcessOutput.None, null)]
        public void Run_FeedOutput_DataReceivedCalled(ProcessOutput outputType, string expectedData) {
            var Manager = SetupManager();
            Manager.OutputType = outputType;
            Manager.ProcessStarted += (s, e) => {
                var PMock = Mock.Get<IProcess>(e.ProcessWorker.WorkProcess);
                PMock.Raise(x => x.OutputDataReceived += null, FakeProcessWorkerFactory.CreateMockDataReceivedEventArgs(OutputDataStream));
                PMock.Raise(x => x.ErrorDataReceived += null, FakeProcessWorkerFactory.CreateMockDataReceivedEventArgs(ErrorDataStream));
            };
            Manager.DataReceived += (s, e) => {
                if (expectedData != null)
                    Assert.Equal(expectedData, e.Data);
            };

            Manager.Run(TestFileName, null);

            if (expectedData != null)
                Assert.Contains(expectedData, Manager.Output);
            else
                Assert.Equal("", Manager.Output);
        }

        #endregion

        #region RunAsCommand

        [Fact]
        public void RunAsCommand_Valid_ReturnsStatusSuccess() {
            var Manager = SetupManager();

            CompletionStatus Result = Manager.RunAsCommand(TestFileName);

            Assert.Equal(CompletionStatus.Success, Result);
            Assert.Equal(CompletionStatus.Success, Manager.LastCompletionStatus);
        }

        [Fact]
        public void RunAsCommand_Valid_ProcessStartedCalledWithValidArgs() {
            var Manager = SetupManager();
            int ProcessStartedCalled = 0;
            Manager.ProcessStarted += (s, e) => {
                ProcessStartedCalled++;
                Assert.NotNull(s);
                Assert.NotNull(e.ProcessWorker);
                Assert.Equal(Manager, e.ProcessWorker);
                Assert.NotNull(Manager.WorkProcess);
            };

            CompletionStatus Result = Manager.RunAsCommand(TestFileName);

            Assert.Equal(1, ProcessStartedCalled);
        }

        [Fact]
        public void RunAsCommand_Valid_ProcessCompletedCalledWithValidArgs() {
            var Manager = SetupManager();
            int CompletedCalled = 0;
            Manager.ProcessCompleted += (s, e) => {
                CompletedCalled++;
                Assert.Equal(CompletionStatus.Success, e.Status);
            };

            CompletionStatus Result = Manager.RunAsCommand(TestFileName);

            Assert.Equal(1, CompletedCalled);
        }

        [Theory]
        [InlineData("test")]
        public void RunAsCommand_Valid_CommandContainsCmd(string cmd) {
            var Manager = SetupManager();

            Manager.RunAsCommand(cmd);

            Assert.Contains(AppCmd, Manager.CommandWithArgs);
            Assert.Contains(cmd, Manager.CommandWithArgs);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void RunAsCommand_EmptyFileName_ThrowsException(string fileName) {
            var Manager = SetupManager();

            Assert.Throws<ArgumentException>(() => Manager.RunAsCommand(fileName));
        }

        #endregion
  
    }
}
