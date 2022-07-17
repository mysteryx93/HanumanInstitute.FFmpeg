// ReSharper disable AssignNullToNotNullAttribute

using System.Runtime.InteropServices;

namespace HanumanInstitute.FFmpeg.UnitTests;

public class ProcessManagerTests
{
    protected const string MissingFileName = "MissingFile";
    protected const string TestFileName = "test";
    protected const string ErrorDataStream = "data_error";
    protected const string OutputDataStream = "data_output";
    private Mock<IProcessManager> _config;

    public IProcessWorker SetupManager()
    {
        _config = new Mock<IProcessManager>();
        var factory = new FakeProcessFactory();
        // var fileSystem = Mock.Of<FakeFileSystemService>(x =>
        //     x.Exists(It.IsAny<string>()) == true && x.Exists(MissingFileName) == false);
        return new ProcessWorker(_config.Object, factory);
    }

    private string AppCmd => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd" : "/bin/bash";

    [Fact]
    public void Init_OutputType_ReturnsNone()
    {
        var manager = SetupManager();

        Assert.Equal(ProcessOutput.None, manager.OutputType);
    }

    [Fact]
    public void Init_CommandWithArgs_ReturnsEmpty()
    {
        var manager = SetupManager();

        Assert.Empty(manager.CommandWithArgs);
    }


    //[Fact]
    //public async Task Run_TwiceSameTime_ThrowsException() {
    //    var manager = SetupManager();

    //    Task T1 = Task.Run(() => manager.Run("test", null));
    //    Task T2 = Task.Run(() => manager.Run("test", null));

    //    await Assert.ThrowsAsync<InvalidOperationException>(() => Task.WhenAll(new Task[] { T1, T2 }));
    //}

    [Fact]
    public void Run_Valid_ReturnsStatusSuccess()
    {
        var manager = SetupManager();

        var result = manager.Run(TestFileName, null);

        Assert.Equal(CompletionStatus.Success, result);
        Assert.Equal(CompletionStatus.Success, manager.LastCompletionStatus);
    }

    [Fact]
    public void Run_Valid_ProcessStartedCalledWithValidArgs()
    {
        var manager = SetupManager();

        var processStartedCalled = 0;
        manager.ProcessStarted += (s, e) =>
        {
            processStartedCalled++;
            Assert.NotNull(s);
            Assert.NotNull(e.ProcessWorker);
            Assert.Equal(manager, e.ProcessWorker);
            Assert.NotNull(manager.WorkProcess);
        };

        manager.Run(TestFileName, null);

        Assert.Equal(1, processStartedCalled);
    }

    [Fact]
    public void Run_Valid_CompletedCalledWithValidArgs()
    {
        var manager = SetupManager();

        var completedCalled = 0;
        manager.ProcessCompleted += (_, e) =>
        {
            completedCalled++;
            Assert.Equal(CompletionStatus.Success, e.Status);
        };

        manager.Run(TestFileName, null);

        Assert.Equal(1, completedCalled);
    }

    [Fact]
    public void Run_Timeout_CompletedCalledWithStatusTimeout()
    {
        var manager = SetupManager();
        manager.Options.Timeout = TimeSpan.FromMilliseconds(10);
        manager.ProcessStarted += (_, e) =>
        {
            var pMock = Mock.Get(e.ProcessWorker.WorkProcess);
            pMock.Setup(x => x.WaitForExit(It.IsAny<int>())).Returns(false);
        };

        var completedCalled = 0;
        manager.ProcessCompleted += (_, e) =>
        {
            completedCalled++;
            Assert.Equal(CompletionStatus.Timeout, e.Status);
        };

        var result = manager.Run(TestFileName, null);

        Assert.Equal(CompletionStatus.Timeout, result);
        Assert.Equal(1, completedCalled);
        Assert.Equal(CompletionStatus.Timeout, manager.LastCompletionStatus);
    }

    [Fact]
    public void Run_Cancel_CompletedCalledWithStatusCancelled()
    {
        var manager = SetupManager();
        Mock<IProcess> pMock;
        manager.Options.Timeout = TimeSpan.FromSeconds(2);
        manager.ProcessStarted += (_, e) =>
        {
            pMock = Mock.Get(e.ProcessWorker.WorkProcess);
            pMock.Setup(x => x.WaitForExit(It.IsAny<int>())).Returns(false);
            manager.Cancel();
            _config.Setup(x => x.SoftKill(It.IsAny<IProcess>())).Callback(() => pMock.Setup(x => x.HasExited).Returns(true));
        };

        var completedCalled = 0;
        manager.ProcessCompleted += (_, e) =>
        {
            completedCalled++;
            Assert.Equal(CompletionStatus.Cancelled, e.Status);
        };

        var result = manager.Run(TestFileName, null);

        Assert.Equal(CompletionStatus.Cancelled, result);
        Assert.Equal(1, completedCalled);
        Assert.Equal(CompletionStatus.Cancelled, manager.LastCompletionStatus);
    }

    [Theory]
    [InlineData("ffmpeg", null)]
    [InlineData("ffmpeg", "-i abc.avi")]
    public void Run_Valid_CommandContainsFileNameAndArgs(string fileName, string args)
    {
        var manager = SetupManager();

        manager.Run(fileName, args);

        Assert.Contains(fileName, manager.CommandWithArgs, StringComparison.InvariantCulture);
        if (!string.IsNullOrEmpty(args))
        {
            Assert.Contains(args, manager.CommandWithArgs, StringComparison.InvariantCulture);
        }
    }

    [Theory]
    [MemberData(nameof(TestDataSource.NullAndEmptyStrings), 1, MemberType = typeof(TestDataSource))]
    public void Run_EmptyFileName_ThrowsException(string fileName, Type ex)
    {
        var manager = SetupManager();

        void Act() => manager.Run(fileName, null);

        Assert.Throws(ex, Act);
    }

    [Theory]
    [InlineData(ProcessOutput.Error, ErrorDataStream)]
    [InlineData(ProcessOutput.Output, OutputDataStream)]
    [InlineData(ProcessOutput.None, null)]
    public void Run_FeedOutput_DataReceivedCalled(ProcessOutput outputType, string expectedData)
    {
        var manager = SetupManager();
        manager.OutputType = outputType;
        manager.ProcessStarted += (_, e) =>
        {
            var pMock = Mock.Get(e.ProcessWorker.WorkProcess);
            pMock.Raise(x => x.OutputDataReceived += null, FakeEncoderService.CreateMockDataReceivedEventArgs(OutputDataStream));
            pMock.Raise(x => x.ErrorDataReceived += null, FakeEncoderService.CreateMockDataReceivedEventArgs(ErrorDataStream));
        };
        manager.DataReceived += (_, e) =>
        {
            if (expectedData != null)
            {
                Assert.Equal(expectedData, e.Data);
            }
        };

        manager.Run(TestFileName, null);

        if (expectedData != null)
        {
            Assert.Contains(expectedData, manager.Output, StringComparison.InvariantCulture);
        }
        else
        {
            Assert.Equal("", manager.Output);
        }
    }


    [Fact]
    public void RunAsCommand_Valid_ReturnsStatusSuccess()
    {
        var manager = SetupManager();

        var result = manager.RunAsCommand(TestFileName);

        Assert.Equal(CompletionStatus.Success, result);
        Assert.Equal(CompletionStatus.Success, manager.LastCompletionStatus);
    }

    [Fact]
    public void RunAsCommand_Valid_ProcessStartedCalledWithValidArgs()
    {
        var manager = SetupManager();
        var processStartedCalled = 0;
        manager.ProcessStarted += (s, e) =>
        {
            processStartedCalled++;
            Assert.NotNull(s);
            Assert.NotNull(e.ProcessWorker);
            Assert.Equal(manager, e.ProcessWorker);
            Assert.NotNull(manager.WorkProcess);
        };

        manager.RunAsCommand(TestFileName);

        Assert.Equal(1, processStartedCalled);
    }

    [Fact]
    public void RunAsCommand_Valid_ProcessCompletedCalledWithValidArgs()
    {
        var manager = SetupManager();
        var completedCalled = 0;
        manager.ProcessCompleted += (_, e) =>
        {
            completedCalled++;
            Assert.Equal(CompletionStatus.Success, e.Status);
        };

        manager.RunAsCommand(TestFileName);

        Assert.Equal(1, completedCalled);
    }

    [Theory]
    [InlineData("test")]
    public void RunAsCommand_Valid_CommandContainsCmd(string cmd)
    {
        var manager = SetupManager();

        manager.RunAsCommand(cmd);

        Assert.Contains(AppCmd, manager.CommandWithArgs, StringComparison.InvariantCulture);
        Assert.Contains(cmd, manager.CommandWithArgs, StringComparison.InvariantCulture);
    }

    [Theory]
    [MemberData(nameof(TestDataSource.NullAndEmptyStrings), 1, MemberType = typeof(TestDataSource))]
    public void RunAsCommand_EmptyFileName_ThrowsException(string fileName, Type ex)
    {
        var manager = SetupManager();

        void Act() => manager.RunAsCommand(fileName);

        Assert.Throws(ex, Act);
    }
}
