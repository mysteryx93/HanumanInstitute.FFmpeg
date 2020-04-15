using System;

namespace HanumanInstitute.Encoder
{
    /// <summary>
    /// Allows calculating the time left during an encoding process.
    /// </summary>
    public interface ITimeLeftCalculator
    {
        /// <summary>
        /// Gets or sets the total number of frames to encode.
        /// </summary>
        long FrameCount { get; set; }
        /// <summary>
        /// Gets or sets the number of status entries to store. The larger the number, the slower the time left will change.
        /// </summary>
        int HistoryLength { get; }
        /// <summary>
        /// After calling Calculate, returns the estimated processing time left.
        /// </summary>
        TimeSpan ResultTimeLeft { get; }
        /// <summary>
        /// After calling Calculate, returns the estimated processing rate per second.
        /// </summary>
        double ResultFps { get; }
        /// <summary>
        /// Calculates the time left and fps. Result will be in ResultTimeLeft and ResultFps.
        /// </summary>
        /// <param name="pos">The current frame position.</param>
        void Calculate(long pos);
    }
}
