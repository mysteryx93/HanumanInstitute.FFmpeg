// ReSharper disable AssignNullToNotNullAttribute
namespace HanumanInstitute.FFmpeg.UnitTests;

public class TimeLeftCalculatorFactoryTests
{
    private static ITimeLeftCalculatorFactory SetupFactory()
    {
        return new TimeLeftCalculatorFactory(new FakeEnvironmentService());
    }

    [Fact]
    public void Constructor_Empty_Success() => new TimeLeftCalculatorFactory(new FakeEnvironmentService()).Create(0);

    [Fact]
    public void Constructor_NullDependency_ThrowsException()
    {
        TimeLeftCalculatorFactory Act() => new(null);
        
        Assert.Throws<ArgumentNullException>(Act);  
    } 

    [Theory]
    [InlineData(100)]
    public void Create_1Param_ValidInstance(int frameCount)
    {
        var factory = SetupFactory();

        var result = factory.Create(frameCount);

        Assert.NotNull(result);
        Assert.IsType<TimeLeftCalculator>(result);
        Assert.Equal(frameCount, result.FrameCount);
    }

    [Theory]
    [InlineData(100, 30)]
    public void Create_2Params_ValidInstance(int frameCount, int historyLength)
    {
        var factory = SetupFactory();

        var result = factory.Create(frameCount, historyLength);

        Assert.NotNull(result);
        Assert.IsType<TimeLeftCalculator>(result);
        Assert.Equal(frameCount, result.FrameCount);
        Assert.Equal(historyLength, result.HistoryLength);
    }
}
