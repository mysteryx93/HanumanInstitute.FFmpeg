using System;

namespace HanumanInstitute.FFmpeg.Services
{
    /// <summary>
    /// Provides information about the application, environment and operating system.
    /// </summary>
    public interface IEnvironmentService
    {
        /// <summary>
        /// Gets the current date and time expressed as local time.
        /// </summary>
        DateTime Now { get; }
        /// <summary>
        /// Gets the current date and time expressed as Universal Standard Time.
        /// </summary>
        DateTime UtcNow { get; }
        /// <summary>
        /// Gets the newline string defined for this environment.
        /// </summary>
        string NewLine { get; }
    }
}
