using HanumanInstitute.FFmpeg.Services;

namespace HanumanInstitute.FFmpeg;

/// <inheritdoc />
public class TimeLeftCalculator : ITimeLeftCalculator
{
    private readonly KeyValuePair<DateTime, long>[] _progressHistory;
    private int _iterator;
    private bool _fullCycle;
    private long _frameCount;
    private int _historyLength;
    /// <inheritdoc />
    public TimeSpan ResultTimeLeft { get; private set; }
    /// <inheritdoc />
    public double ResultFps { get; private set; }

    private readonly IEnvironmentService _environment;

    /// <summary>
    /// Initializes a new instance of the TimeLeftCalculator class.
    /// </summary>
    /// <param name="frameCount">The total number of frames to encode.</param>
    /// <param name="historyLength">The number of status entries to store. The larger the number, the slower the time left will change. Default is 20.</param>
    public TimeLeftCalculator(long frameCount, int historyLength = 20) : this(new EnvironmentService(), frameCount, historyLength) { }

    /// <summary>
    /// Initializes a new instance of the TimeLeftCalculator class.
    /// </summary>
    /// <param name="environmentService">A reference to an IEnvironmentService.</param>
    /// <param name="frameCount">The total number of frames to encode.</param>
    /// <param name="historyLength">The number of status entries to store. The larger the number, the slower the time left will change. Default is 20.</param>
    internal TimeLeftCalculator(IEnvironmentService environmentService, long frameCount, int historyLength = 20)
    {
        _environment = environmentService ?? throw new ArgumentNullException(nameof(environmentService));
        FrameCount = frameCount;
        HistoryLength = historyLength;
        _progressHistory = new KeyValuePair<DateTime, long>[historyLength];
    }

    /// <inheritdoc />
    public long FrameCount
    {
        get => _frameCount;
        set => _frameCount = value >= 0 ? value : throw new ArgumentOutOfRangeException(nameof(FrameCount));
    }

    /// <inheritdoc />
    public int HistoryLength
    {
        get => _historyLength;
        set => _historyLength = value >= 1 ? value : throw new ArgumentOutOfRangeException(nameof(HistoryLength));
    }

    /// <inheritdoc />
    public void Calculate(long pos)
    {
        if (pos < 0) { return; }

        _progressHistory[_iterator] = new KeyValuePair<DateTime, long>(_environment.Now, pos);

        // Calculate SampleWorkTime and SampleWorkFrame for each host
        var sampleWorkTime = TimeSpan.Zero;
        long sampleWorkFrame = 0;
        var posFirst = -1;
        if (_fullCycle)
        {
            posFirst = (_iterator + 1) % HistoryLength;
        }
        else if (_iterator > 0)
        {
            posFirst = 0;
        }

        if (posFirst > -1)
        {
            sampleWorkTime += _progressHistory[_iterator].Key - _progressHistory[posFirst].Key;
            sampleWorkFrame += _progressHistory[_iterator].Value - _progressHistory[posFirst].Value;
        }

        if (sampleWorkTime.TotalSeconds > 0 && sampleWorkFrame >= 0)
        {
            ResultFps = sampleWorkFrame / sampleWorkTime.TotalSeconds;
            var workLeft = FrameCount - pos;
            if (workLeft <= 0)
            {
                ResultTimeLeft = TimeSpan.Zero;
            }
            else if (ResultFps > 0)
            {
                ResultTimeLeft = TimeSpan.FromSeconds(workLeft / ResultFps);
            }
        }

        _iterator = (_iterator + 1) % HistoryLength;
        if (_iterator == 0)
        {
            _fullCycle = true;
        }
    }
}
