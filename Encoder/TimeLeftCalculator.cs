using HanumanInstitute.Encoder.Services;
using System;
using System.Collections.Generic;

namespace HanumanInstitute.Encoder
{
    /// <summary>
    /// Allows calculating the time left during an encoding process.
    /// </summary>
    public class TimeLeftCalculator : ITimeLeftCalculator
    {
        private readonly KeyValuePair<DateTime, long>[] progressHistory;
        private int iterator;
        private bool fullCycle;
        private long frameCount;
        private int historyLength;
        /// <summary>
        /// After calling Calculate, returns the estimated processing time left.
        /// </summary>
        public TimeSpan ResultTimeLeft { get; private set; }
        /// <summary>
        /// After calling Calculate, returns the estimated processing rate per second.
        /// </summary>
        public double ResultFps { get; private set; }

        private readonly IEnvironmentService environment;

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
        public TimeLeftCalculator(IEnvironmentService environmentService, long frameCount, int historyLength = 20)
        {
            environment = environmentService ?? throw new ArgumentNullException(nameof(environmentService));
            FrameCount = frameCount;
            HistoryLength = historyLength;
            progressHistory = new KeyValuePair<DateTime, long>[historyLength];
        }

        /// <summary>
        /// Gets or sets the total number of frames to encode.
        /// </summary>
        public long FrameCount {
            get => frameCount;
            set => frameCount = value >= 0 ? value : throw new ArgumentOutOfRangeException(nameof(FrameCount));
        }

        /// <summary>
        /// Gets or sets the number of status entries to store. The larger the number, the slower the time left will change.
        /// </summary>
        public int HistoryLength {
            get => historyLength;
            set => historyLength = value >= 1 ? value : throw new ArgumentOutOfRangeException(nameof(HistoryLength));
        }

        /// <summary>
        /// Calculates the time left and fps. Result will be in ResultTimeLeft and ResultFps.
        /// </summary>
        /// <param name="pos">The current frame position.</param>
        public void Calculate(long pos)
        {
            if (pos < 0) { return; }

            progressHistory[iterator] = new KeyValuePair<DateTime, long>(environment.Now, pos);

            // Calculate SampleWorkTime and SampleWorkFrame for each host
            TimeSpan SampleWorkTime = TimeSpan.Zero;
            long SampleWorkFrame = 0;
            int PosFirst = -1;
            if (fullCycle)
            {
                PosFirst = (iterator + 1) % HistoryLength;
            }
            else if (iterator > 0)
            {
                PosFirst = 0;
            }

            if (PosFirst > -1)
            {
                SampleWorkTime += progressHistory[iterator].Key - progressHistory[PosFirst].Key;
                SampleWorkFrame += progressHistory[iterator].Value - progressHistory[PosFirst].Value;
            }

            if (SampleWorkTime.TotalSeconds > 0 && SampleWorkFrame >= 0)
            {
                ResultFps = SampleWorkFrame / SampleWorkTime.TotalSeconds;
                long WorkLeft = FrameCount - pos;
                if (WorkLeft <= 0)
                {
                    ResultTimeLeft = TimeSpan.Zero;
                }
                else if (ResultFps > 0)
                {
                    ResultTimeLeft = TimeSpan.FromSeconds(WorkLeft / ResultFps);
                }
            }

            iterator = (iterator + 1) % HistoryLength;
            if (iterator == 0)
            {
                fullCycle = true;
            }
        }
    }
}
