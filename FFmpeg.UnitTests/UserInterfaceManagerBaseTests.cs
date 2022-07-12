using System;
using HanumanInstitute.FFmpeg.Services;
using Moq;
using Xunit;

namespace HanumanInstitute.FFmpeg.UnitTests;

public class UserInterfaceManagerBaseTests
{
    protected const string TestTitle = "job title";
    protected const string TestFileName = "test";
    protected const int TestJobId = 0;
    private Mock<IProcessManager> _config;

    protected static Mock<FakeUserInterfaceManagerBase> SetupUI()
    {
        return new Mock<FakeUserInterfaceManagerBase>() { CallBase = true };
    }

    public IProcessWorker SetupManager()
    {
        _config = new Mock<IProcessManager>();
        var factory = new FakeProcessFactory();
        return new ProcessWorker(_config.Object, factory, null);
    }

    [Theory]
    [InlineData(0)]
    [InlineData("")]
    public void Start_Valid_CreateUiCalled(object jobId)
    {
        var uiMock = SetupUI();

        uiMock.Object.Start(null, jobId, TestTitle);

        uiMock.Verify(x => x.CreateUI(null, TestTitle, It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public void Start_NullJobId_ThrowsNullException()
    {
        var uiMock = SetupUI();

        Assert.Throws<ArgumentNullException>(() => uiMock.Object.Start(null, null, TestTitle));
    }

    [Fact]
    public void Stop_Valid_StopCalledOnWindow()
    {
        var uiMock = SetupUI();

        uiMock.Object.Start(null, TestJobId, TestTitle);
        uiMock.Object.Close(TestJobId);

        Assert.Single(uiMock.Object.Instances);
        var wMock = Mock.Get<IUserInterfaceWindow>(uiMock.Object.Instances[0]);
        wMock.Verify(x => x.Close(), Times.Once);
    }

    [Fact]
    public void Display_ProcessManagerWithJobId_DisplayTaskCalledOnWindow()
    {
        var uiMock = SetupUI();
        var manager = SetupManager();
        manager.Options.JobId = TestJobId;

        uiMock.Object.Start(null, TestJobId, TestTitle);
        uiMock.Object.Display(null, manager);

        var wMock = Mock.Get<IUserInterfaceWindow>(uiMock.Object.Instances[0]);
        wMock.Verify(x => x.DisplayTask(It.IsAny<IProcessWorker>()), Times.Once);
    }

    [Fact]
    public void Display_ProcessManagerWithTitle_CreateUiCalled()
    {
        var uiMock = SetupUI();
        var manager = SetupManager();
        manager.Options.Title = TestTitle;

        uiMock.Object.Display(null, manager);

        uiMock.Verify(x => x.CreateUI(null, TestTitle, It.IsAny<bool>()), Times.Once);
    }

    //[Fact]
    //public void RunWithOptionDisplayErrorOnly_Timeout_DisplayError()
    //{
    //    var uiMock = SetupUI();
    //    var manager = SetupManager();
    //    // _config.Setup(x => x.UserInterfaceManager).Returns(uiMock.Object);
    //    manager.Options.DisplayMode = ProcessDisplayMode.ErrorOnly;
    //    manager.Options.Timeout = TimeSpan.FromMilliseconds(10);
    //    manager.ProcessStarted += (s, e) =>
    //    {
    //        var pMock = Mock.Get<IProcess>(e.ProcessWorker.WorkProcess);
    //        pMock.Setup(x => x.WaitForExit(It.IsAny<int>())).Returns(false);
    //    };

    //    manager.Run(TestFileName, null);

    //    uiMock.Verify(x => x.DisplayError(null, manager), Times.Once);
    //}
}