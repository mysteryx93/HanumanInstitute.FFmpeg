namespace HanumanInstitute.FFmpeg.UnitTests;

public class TimeLeftCalculatorTests
{
    private const int FrameCount = 200;
    private const int HistoryLength = 4;
    private FakeEnvironmentService _environment = new FakeEnvironmentService();

    public ITimeLeftCalculator SetupCalc()
    {
        _environment = new FakeEnvironmentService();
        return new TimeLeftCalculator(_environment, FrameCount, HistoryLength);
    }

    /// <summary>
    /// Calculates a value while ensuring ResultFps and ResultTimeLeft remain valid.
    /// </summary>
    protected void CalcValidate(ITimeLeftCalculator calc, long frame, int seconds)
    {
        _environment.AddSeconds(seconds);
        calc.Calculate(frame);
        Assert.True(calc.ResultFps >= 0);
        Assert.True(calc.ResultTimeLeft >= TimeSpan.Zero);
    }


    [Fact]
    public void Constructor_FrameCount_Success() => new TimeLeftCalculator(FrameCount).Calculate(0);

    [Fact]
    public void Constructor_FrameCountHistoryLength_Success() => new TimeLeftCalculator(FrameCount, HistoryLength).Calculate(0);

    [Fact]
    public void Constructor_WithDependency_Success() => new TimeLeftCalculator(_environment, FrameCount, HistoryLength).Calculate(0);

    [Fact]
    public void Constructor_NullDependency_ThrowsException() => Assert.Throws<ArgumentNullException>(() => new TimeLeftCalculator(null, FrameCount, HistoryLength));

    [Fact]
    public void Constructor_MinValues_Success() => new TimeLeftCalculator(_environment, 0, 1).Calculate(0);

    [Theory]
    [InlineData(-100, 30)]
    [InlineData(100, -30)]
    [InlineData(100, 0)]
    public void Constructor_InvalidValues_ThrowsException(int frameCount, int historyLength) => Assert.Throws<ArgumentOutOfRangeException>(() => new TimeLeftCalculator(_environment, frameCount, historyLength));

    [Fact]
    public void Init_CorrectDefaultValues()
    {
        var calc = SetupCalc();

        Assert.Equal(FrameCount, calc.FrameCount);
        Assert.Equal(HistoryLength, calc.HistoryLength);
        Assert.Equal(0, calc.ResultFps);
        Assert.Equal(TimeSpan.Zero, calc.ResultTimeLeft);
    }


    [Fact]
    public void Calc_RunSimulation_ValidResults()
    {
        var calc = SetupCalc();
        var frame = 0;

        // Test at pace of 5 fps.
        for (var i = 1; i <= 10; i++)
        {
            frame += 5;
            CalcValidate(calc, frame, 1);
            if (i > 1)
            {
                Assert.Equal(5, calc.ResultFps);
                Assert.Equal(40 - i, calc.ResultTimeLeft.TotalSeconds);
            }
            else
            {
                Assert.Equal(0, calc.ResultFps);
                Assert.Equal(0, calc.ResultTimeLeft.TotalSeconds);
            }
        }

        // Test at pace of 10 frame per 2 seconds, result should remain 5 fps.
        for (var i = 1; i <= 5; i++)
        {
            frame += 10;
            CalcValidate(calc, frame, 2);
            Assert.Equal(5, calc.ResultFps);
            Assert.Equal(30 - (i * 2), calc.ResultTimeLeft.TotalSeconds);
        }

        // Test at pace of 10 fps.
        for (var i = 0; i < 10; i++)
        {
            frame += 10;
            CalcValidate(calc, frame, 1);
            Assert.InRange<double>(calc.ResultFps, 5, 10);
            Assert.InRange<double>(calc.ResultTimeLeft.TotalSeconds, 0, 15);
        }

        Assert.Equal(10, calc.ResultFps);
        Assert.Equal(0, calc.ResultTimeLeft.TotalSeconds);
    }

    [Fact]
    public void Calc_Negative_OutputWithinValidRange()
    {
        var calc = SetupCalc();

        CalcValidate(calc, -10, 0);
    }

    [Fact]
    public void Calc_MaxLong_OutputWithinValidRange()
    {
        var calc = SetupCalc();

        CalcValidate(calc, long.MaxValue - 2, 1);
        CalcValidate(calc, long.MaxValue - 1, 1);
        CalcValidate(calc, long.MaxValue, 1);
    }

    [Fact]
    public void Calc_DescendingFrame_OutputWithinValidRange()
    {
        var calc = SetupCalc();

        CalcValidate(calc, 10, 1);
        CalcValidate(calc, 15, 1);
        CalcValidate(calc, 10, 1);
        CalcValidate(calc, 5, 1);
        CalcValidate(calc, 15, 1);
        CalcValidate(calc, 15, 1);
        CalcValidate(calc, 15, 0);
        CalcValidate(calc, 15, 0);
        CalcValidate(calc, 15, 0);
    }
}