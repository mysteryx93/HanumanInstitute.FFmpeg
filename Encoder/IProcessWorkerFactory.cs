using System;

namespace HanumanInstitute.Encoder
{
    /// <summary>
    /// Creates new instances of process workers.
    /// </summary>
    public interface IProcessWorkerFactory
    {
        /// <summary>
        /// Gets or sets the configuration settings.
        /// </summary>
        IMediaConfig Config { get; set; }
        /// <summary>
        /// Creates a new process manager with specified options.
        /// </summary>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The newly created process manager.</returns>
        IProcessWorker Create(ProcessOptions options = null, ProcessStartedEventHandler callback = null);
        /// <summary>
        /// Creates a new process worker to run an encoder with specified options.
        /// </summary>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The newly created encoder process manager.</returns>
        IProcessWorkerEncoder CreateEncoder(ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null);
    }
}
