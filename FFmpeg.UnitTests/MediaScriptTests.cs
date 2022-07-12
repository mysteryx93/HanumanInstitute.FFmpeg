using System;
using System.IO;
using System.Linq;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace HanumanInstitute.FFmpeg.UnitTests;

public class MediaScriptTests
{
    protected const string AppAvs2PipeMod = "avs2pipemod.exe";
    protected const string AppVsPipe = "vspipe.exe";
    protected const string MissingFileName = "MissingFile";
    private Mock<FakeMediaConfig> _config;
    private readonly FakeProcessWorkerFactory _factory = new FakeProcessWorkerFactory();
    private readonly ITestOutputHelper _output;

    public MediaScriptTests(ITestOutputHelper output)
    {
        _output = output;
    }

    protected IMediaScript SetupScript()
    {
        _config = new Mock<FakeMediaConfig>() { CallBase = true };
        _factory.Config = _config.Object;
        var fileSystem = Mock.Of<FakeFileSystemService>(x =>
            x.Exists(It.IsAny<string>()) == true && x.Exists(MissingFileName) == false);
        return new MediaScript(_factory, fileSystem);
    }

    protected void AssertSingleInstance()
    {
        var resultCommand = _factory.Instances.FirstOrDefault()?.CommandWithArgs;
        _output.WriteLine(resultCommand);
        Assert.Single(_factory.Instances);
        Assert.NotNull(resultCommand);
    }


    [Fact]
    public void Constructor_WithFactory_Success() => new MediaEncoder(_factory);

    [Fact]
    public void Constructor_NullFactory_ThrowsException() => Assert.Throws<ArgumentNullException>((Func<object>)(() => new MediaEncoder((IProcessWorkerFactory)null)));

    [Fact]
    public void Constructor_NullDependency_ThrowsException() => Assert.Throws<ArgumentNullException>(() => new MediaScript(_factory, null));


    [Theory]
    [InlineData("file")]
    public void RunAvisynth_ValidFile_CommandContainsAvs2PipeMod(string path)
    {
        var script = SetupScript();

        var result = script.RunAvisynth(path);

        Assert.Equal(CompletionStatus.Success, result);
        AssertSingleInstance();
        Assert.Contains(AppAvs2PipeMod, _factory.Instances[0].CommandWithArgs, StringComparison.InvariantCulture);
    }

    [Theory]
    [MemberData(nameof(TestDataSource.NullAndEmptyStrings), 1, MemberType = typeof(TestDataSource))]
    public void RunAvisynth_NoFile_ThrowsException(string path, Type ex)
    {
        var script = SetupScript();

        void Act() => script.RunAvisynth(path);

        Assert.Throws(ex, Act);
    }

    [Theory]
    [InlineData("file")]
    public void RunAvisynth_AvsNotFound_ThrowsFileNotFoundException(string path)
    {
        var script = SetupScript();
        _config.Setup(x => x.Avs2PipeMod).Returns(MissingFileName);

        Assert.Throws<FileNotFoundException>(() => script.RunAvisynth(path));
    }


    [Theory]
    [InlineData("file")]
    public void RunVapourSynth_ValidFile_CommandContainsVsPipe(string path)
    {
        var script = SetupScript();

        var result = script.RunVapourSynth(path);

        Assert.Equal(CompletionStatus.Success, result);
        AssertSingleInstance();
        Assert.Contains(AppVsPipe, _factory.Instances[0].CommandWithArgs, StringComparison.InvariantCulture);
    }

    [Theory]
    [MemberData(nameof(TestDataSource.NullAndEmptyStrings), 1, MemberType = typeof(TestDataSource))]
    public void RunVapourSynth_NoFile_ThrowsException(string path, Type ex)
    {
        var script = SetupScript();

        void Act() => script.RunVapourSynth(path);

        Assert.Throws(ex, Act);
    }

    [Theory]
    [InlineData("file")]
    public void RunVapourSynth_AvsNotFound_ThrowsFileNotFoundException(string path)
    {
        var script = SetupScript();
        _config.Setup(x => x.Avs2PipeMod).Returns(MissingFileName);

        void Act() => script.RunVapourSynth(path);

        Assert.Throws<FileNotFoundException>(Act);
    }
}