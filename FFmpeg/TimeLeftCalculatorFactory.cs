using System;
using HanumanInstitute.FFmpeg.Services;

namespace HanumanInstitute.FFmpeg
{
    /// <summary>
    /// Allows calculating the time left for a FFmpeg process.
    /// </summary>
    public class TimeLeftCalculatorFactory : ITimeLeftCalculatorFactory
    {
        private readonly IEnvironmentService _environment;

        //public TimeLeftCalculatorFactory() : this(new EnvironmentService()) { }

        public TimeLeftCalculatorFactory(IEnvironmentService environmentService)
        {
            _environment = environmentService ?? throw new ArgumentNullException(nameof(environmentService));
        }

        /// <summary>
        /// Creates a new instance of the TimeLeftCalculator class.
        /// </summary>
        /// <param name="frameCount">The total number of frames to encode.</param>
        /// <returns>The new TimeLeftCalculator instance.</returns>
        public ITimeLeftCalculator Create(long frameCount) => new TimeLeftCalculator(_environment, frameCount);
        /// <summary>
        /// Creates a new instance of the TimeLeftCalculator class.
        /// </summary>
        /// <param name="frameCount">The total number of frames to encode.</param>
        /// <param name="historyLength">The number of status entries to store. The larger the number, the slower the time left will change.</param>
        /// <returns>The new TimeLeftCalculator instance.</returns>
        public ITimeLeftCalculator Create(long frameCount, int historyLength) => new TimeLeftCalculator(_environment, frameCount, historyLength);
    }
}
