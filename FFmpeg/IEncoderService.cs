namespace HanumanInstitute.FFmpeg;

/// <summary>
/// Creates new instances of process workers. Main entry point to access all process services.
/// </summary>
public interface IEncoderService
{
    /// <summary>
    /// Gets or sets the configuration settings.
    /// </summary>
    IProcessManager Processes { get; set; }
    
    /// <summary>
    /// Gets or sets a class deriving from IUserInterfaceManager to manage FFmpeg UI.
    /// </summary>
    IUserInterfaceManager? UiManager { get; set; }
    
    /// <summary>
    /// Creates a new process manager with specified options.
    /// </summary>
    /// <param name="owner">The owner of the process window that will be passed to IUserInterfaceManagerBase.</param>
    /// <param name="options">The options for starting the process.</param>
    /// <param name="callback">A method that will be called after the process has been started.</param>
    /// <returns>The newly created process manager.</returns>
    IProcessWorker CreateProcess(object? owner = null, ProcessOptions? options = null, ProcessStartedEventHandler? callback = null);
    
    /// <summary>
    /// Creates a new process worker to run an encoder with specified options.
    /// </summary>
    /// <param name="owner">The owner of the process window that will be passed to IUserInterfaceManagerBase.</param>
    /// <param name="options">The options for starting the process.</param>
    /// <param name="callback">A method that will be called after the process has been started.</param>
    /// <returns>The newly created encoder process manager.</returns>
    IProcessWorkerEncoder CreateEncoder(object? owner = null, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null);

    /// <summary>
    /// Creates an instance of <see cref="IMediaEncoder"/>  class using this factory.
    /// </summary>
    /// <param name="owner">The owner to send to the <see cref="IUserInterfaceManager"/>.</param>
    /// <returns>The new <see cref="IMediaEncoder"/>.</returns>
    IMediaEncoder GetMediaEncoder(object owner);

    /// <summary>
    /// Creates an instance of <see cref="IMediaInfoReader"/>  class using this factory.
    /// </summary>
    /// <param name="owner">The owner to send to the <see cref="IUserInterfaceManager"/>.</param>
    /// <returns>The new <see cref="IMediaInfoReader"/>.</returns>
    IMediaInfoReader GetMediaInfoReader(object owner);

    /// <summary>
    /// Creates an instance of <see cref="IMediaMuxer"/>  class using this factory.
    /// </summary>
    /// <param name="owner">The owner to send to the <see cref="IUserInterfaceManager"/>.</param>
    /// <returns>The new <see cref="IMediaMuxer"/>.</returns>
    IMediaMuxer GetMediaMuxer(object owner);

    /// <summary>
    /// Creates an instance of <see cref="IMediaScript"/>  class using this factory.
    /// </summary>
    /// <param name="owner">The owner to send to the <see cref="IUserInterfaceManager"/>.</param>
    /// <returns>The new <see cref="IMediaScript"/>.</returns>
    IMediaScript GetMediaScript(object owner);
}
