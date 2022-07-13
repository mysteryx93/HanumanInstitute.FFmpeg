// ReSharper disable AssignNullToNotNullAttribute

namespace HanumanInstitute.FFmpeg.UnitTests;

public class MediaScriptTests
{
    protected const string AppAvs2Yuv = "avs2yuv";
    protected const string AppVsPipe = "vspipe";
    protected const string MissingFileName = "MissingFile";
    private Mock<FakeProcessManager> _config;
    private readonly FakeProcessService _factory = new();
    private readonly ITestOutputHelper _output;

    public MediaScriptTests(ITestOutputHelper output)
    {
        _output = output;
    }

    protected IMediaScript SetupScript()
    {
        _config = new Mock<FakeProcessManager>() { CallBase = true };
        _factory.Processes = _config.Object;
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
    // ReSharper disable once ObjectCreationAsStatement
    public void Constructor_WithFactory_Success() => new MediaEncoder(_factory);

    [Fact]
    public void Constructor_NullFactory_ThrowsException()
    {
        MediaScript Act() => new(null);

        Assert.Throws<ArgumentNullException>(Act);
    }

    [Fact]
    public void Constructor_NullDependency_ThrowsException()
    {
        MediaScript Act() => new(_factory, null);

        Assert.Throws<ArgumentNullException>(Act);
    }

    [Theory]
    [InlineData("file")]
    public void RunAvisynth_ValidFile_CommandContainsAvs2PipeMod(string path)
    {
        var script = SetupScript();

        var result = script.RunAvisynth(path);

        Assert.Equal(CompletionStatus.Success, result);
        AssertSingleInstance();
        Assert.Contains(AppAvs2Yuv, _factory.Instances[0].CommandWithArgs, StringComparison.InvariantCulture);
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
}
