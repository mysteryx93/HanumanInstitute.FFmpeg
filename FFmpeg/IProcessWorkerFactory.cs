using System;

namespace HanumanInstitute.FFmpeg
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
        /// Gets or sets a class deriving from IUserInterfaceManager to manage FFmpeg UI.
        /// </summary>
        IUserInterfaceManager UiManager { get; set; }
        /// <summary>
        /// Creates a new process manager with specified options.
        /// </summary>
        /// <param name="owner">The owner of the process window that will be passed to IUserInterfaceManagerBase.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The newly created process manager.</returns>
        IProcessWorker Create(object owner, ProcessOptions options = null, ProcessStartedEventHandler callback = null);
        /// <summary>
        /// Creates a new process worker to run an encoder with specified options.
        /// </summary>
        /// <param name="owner">The owner of the process window that will be passed to IUserInterfaceManagerBase.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The newly created encoder process manager.</returns>
        IProcessWorkerEncoder CreateEncoder(object owner, ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null);
    }
}
