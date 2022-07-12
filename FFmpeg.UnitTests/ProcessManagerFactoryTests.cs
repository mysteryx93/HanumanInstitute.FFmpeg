using System;
using HanumanInstitute.FFmpeg.Services;
using Moq;
using Xunit;

namespace HanumanInstitute.FFmpeg.UnitTests;

public class ProcessManagerFactoryTests
{
    private FakeMediaConfig _config;

    protected IProcessService SetupFactory()
    {
        var moq = new MockRepository(MockBehavior.Strict);
        _config = new FakeMediaConfig();
        var parserFactory = new FileInfoParserFactory();
        var processFactory = moq.Create<IProcessFactory>();
        var fileSystem = moq.Create<IFileSystemService>();
        return new ProcessService(_config, null, parserFactory, processFactory.Object, fileSystem.Object);
    }

    [Fact]
    public void Constructor_NoParam_Success() => new ProcessService().CreateProcess(null);

    [Fact]
    public void Constructor_Config_Success() => new ProcessService(new ProcessManager(), null).CreateProcess(null);

    [Fact]
    public void Constructor_InjectDependencies_Success() => new ProcessService(new ProcessManager(), null, new FileInfoParserFactory(), new ProcessFactory(), new FileSystemService()).CreateProcess(null);

    [Fact]
    public void Constructor_NullDependencies_ThrowsException() => Assert.Throws<ArgumentNullException>(() => new ProcessService(null, null, null, null, null));

    [Fact]
    public void Constructor_InjectOneDependency_ThrowsException() => Assert.Throws<ArgumentNullException>(() => new ProcessService(new ProcessManager(), null, null, null, null));

    [Fact]
    public void Create_NoParam_ReturnsProcessManager()
    {
        var factory = SetupFactory();

        var result = factory.CreateProcess(null);

        Assert.NotNull(result);
        Assert.IsType<ProcessWorker>(result);
        Assert.Same(_config, result.Processes);
    }

    [Fact]
    public void Create_ParamOptions_ReturnsSameOptions()
    {
        var factory = SetupFactory();
        var options = new ProcessOptions();

        var result = factory.CreateProcess(null, options);

        Assert.Same(options, result.Options);
    }

    [Fact]
    public void CreateEncoder_NoParam_ReturnsProcessManager()
    {
        var factory = SetupFactory();

        var result = factory.CreateEncoder(null);

        Assert.NotNull(result);
        Assert.IsType<ProcessWorkerEncoder>(result);
        Assert.Same(_config, result.Processes);
    }

    [Fact]
    public void CreateEncoder_ParamOptions_ReturnsSameOptions()
    {
        var factory = SetupFactory();
        var options = new ProcessOptionsEncoder();

        var result = factory.CreateEncoder(null, options);

        Assert.Same(options, result.Options);
    }
}