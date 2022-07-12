using System.Diagnostics;

namespace HanumanInstitute.FFmpeg.UnitTests;

public class ProcessFactoryTests
{
    private static IProcessFactory SetupFactory()
    {
        return new ProcessFactory();
    }

    [Fact]
    public void Create_NoArg_ProcessWrapper()
    {
        var factory = SetupFactory();

        var result = factory.Create();

        Assert.NotNull(result);
        Assert.IsType<ProcessWrapper>(result);
        Assert.NotNull(result.StartInfo);
    }

    [Fact]
    public void CreateWrapper_ProcessArg_WrapperAroundProcess()
    {
        var factory = SetupFactory();
        var p = new Process();

        var result = factory.Create(p);

        Assert.NotNull(result);
        Assert.IsType<ProcessWrapper>(result);
        Assert.Equal(p.StartInfo, result.StartInfo);
        p.Dispose();
    }

    [Fact]
    public void CreateWrapper_NullArg_ProcessWrapper()
    {
        var factory = SetupFactory();

        var result = factory.Create(null);

        Assert.NotNull(result);
        Assert.IsType<ProcessWrapper>(result);
        Assert.NotNull(result.StartInfo);
    }
}