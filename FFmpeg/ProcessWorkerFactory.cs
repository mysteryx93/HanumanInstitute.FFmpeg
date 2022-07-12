using HanumanInstitute.FFmpeg.Services;

namespace HanumanInstitute.FFmpeg;

/// <summary>
/// Creates new instances of process workers.
/// </summary>
public class ProcessWorkerFactory : IProcessWorkerFactory
{
    /// <summary>
    /// Gets or sets the configuration settings.
    /// </summary>
    public IMediaConfig Config { get; set; }
    /// <summary>
    /// Gets or sets a class deriving from IUserInterfaceManager to manage FFmpeg UI.
    /// </summary>
    public IUserInterfaceManager? UiManager { get; set; }
    private readonly IFileInfoParserFactory _parserFactory;
    private readonly IProcessFactory _processFactory;
    private readonly IFileSystemService _fileSystemService;

    public ProcessWorkerFactory() : this(new MediaConfig(), null, new FileInfoParserFactory(), new ProcessFactory(), new FileSystemService()) { }

    public ProcessWorkerFactory(IMediaConfig config, IUserInterfaceManager? uiManager) : this(config, uiManager, new FileInfoParserFactory(), new ProcessFactory(), new FileSystemService()) { }

    public ProcessWorkerFactory(IMediaConfig config, IUserInterfaceManager? uiManager, IFileInfoParserFactory parserFactory, IProcessFactory processFactory, IFileSystemService fileSystemService)
    {
        Config = config ?? throw new ArgumentNullException(nameof(config));
        UiManager = uiManager;
        _parserFactory = parserFactory ?? throw new ArgumentNullException(nameof(parserFactory));
        _processFactory = processFactory ?? throw new ArgumentNullException(nameof(processFactory));
        _fileSystemService = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
    }

    /// <summary>
    /// Creates a new process worker with specified options.
    /// </summary>
    /// <param name="owner">The owner of the process window that will be passed to IUserInterfaceManagerBase.</param>
    /// <param name="options">The options for starting the process.</param>
    /// <param name="callback">A method that will be called after the process has been started.</param>
    /// <returns>The newly created process manager.</returns>
    public virtual IProcessWorker Create(object? owner, ProcessOptions? options = null, ProcessStartedEventHandler? callback = null)
    {
        options ??= new ProcessOptions();
        var worker = new ProcessWorker(Config, _processFactory, options);
        if (callback != null)
        {
            worker.ProcessStarted += callback;
        }
        UiManager?.AttachProcessWorker(owner, worker, options);

        return worker;
    }

    /// <summary>
    /// Creates a new process worker to run an encoder with specified options.
    /// </summary>
    /// <param name="owner">The owner of the process window that will be passed to IUserInterfaceManagerBase.</param>
    /// <param name="options">The options for starting the process.</param>
    /// <param name="callback">A method that will be called after the process has been started.</param>
    /// <returns>The newly created encoder process manager.</returns>
    public virtual IProcessWorkerEncoder CreateEncoder(object? owner = null, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null)
    {
        options ??= new ProcessOptionsEncoder();
        var worker = new ProcessWorkerEncoder(Config, _processFactory, _fileSystemService, _parserFactory, options);
        if (callback != null)
        {
            worker.ProcessStarted += callback;
        }
        UiManager?.AttachProcessWorker(owner, worker, options);

        return worker;
    }
}