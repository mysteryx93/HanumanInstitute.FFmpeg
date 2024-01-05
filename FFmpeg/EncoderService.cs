using HanumanInstitute.FFmpeg.Services;

namespace HanumanInstitute.FFmpeg;

/// <inheritdoc />
public class EncoderService : IEncoderService
{
    /// <inheritdoc />
    public IProcessManager Processes { get; set; }
    /// <inheritdoc />
    public IUserInterfaceManager? UiManager { get; set; }
    private readonly IFileInfoParserFactory _parserFactory;
    private readonly IProcessFactory _processFactory;
    private readonly IFileSystemService _fileSystemService;

    /// <summary>
    /// Initializes a new instance of the EncoderService class.
    /// </summary>
    /// <param name="processManager">Provides a custom IProcessManager that allows providing paths for additional applications,
    /// and altering soft process close behavior. Altering soft close behavior is required for console applications.</param>
    /// <param name="uiManager">A custom class that will receive notifications for display.</param>
    /// <param name="parserFactory">A custom factory to parse output from additional applications.</param>
    public EncoderService(IProcessManager? processManager = null, IUserInterfaceManager? uiManager = null,
        IFileInfoParserFactory? parserFactory = null) :
        this(processManager ?? new ProcessManager(), uiManager, parserFactory ?? new FileInfoParserFactory(), new ProcessFactory(), new FileSystemService())
    {
    }

    internal EncoderService(IProcessManager config, IUserInterfaceManager? uiManager, IFileInfoParserFactory parserFactory,
        IProcessFactory processFactory, IFileSystemService fileSystemService)
    {
        Processes = config ?? throw new ArgumentNullException(nameof(config));
        UiManager = uiManager;
        _parserFactory = parserFactory ?? throw new ArgumentNullException(nameof(parserFactory));
        _processFactory = processFactory ?? throw new ArgumentNullException(nameof(processFactory));
        _fileSystemService = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
    }

    /// <inheritdoc />
    public virtual IProcessWorker CreateProcess(object? owner, ProcessOptions? options = null, ProcessStartedEventHandler? callback = null)
    {
        options ??= new ProcessOptions();
        var worker = new ProcessWorker(Processes, _processFactory, options);
        if (callback != null)
        {
            worker.ProcessStarted += callback;
        }
        UiManager?.AttachProcessWorker(owner, worker, options);

        return worker;
    }

    /// <inheritdoc />
    public virtual IProcessWorkerEncoder CreateEncoder(object? owner = null, ProcessOptionsEncoder? options = null,
        ProcessStartedEventHandler? callback = null)
    {
        options ??= new ProcessOptionsEncoder();
        var worker = new ProcessWorkerEncoder(Processes, _processFactory, _fileSystemService, _parserFactory, options);
        if (callback != null)
        {
            worker.ProcessStarted += callback;
        }
        UiManager?.AttachProcessWorker(owner, worker, options);

        return worker;
    }

    /// <summary>
    /// Creates an instance of <see cref="IMediaEncoder"/>  class using this factory.
    /// </summary>
    /// <param name="owner">The owner to send to the <see cref="IUserInterfaceManager"/>.</param>
    /// <returns>The new <see cref="IMediaEncoder"/>.</returns>
    public virtual IMediaEncoder GetMediaEncoder(object owner) => new MediaEncoder(this) { Owner = owner };

    /// <summary>
    /// Creates an instance of <see cref="IMediaInfoReader"/>  class using this factory.
    /// </summary>
    /// <param name="owner">The owner to send to the <see cref="IUserInterfaceManager"/>.</param>
    /// <returns>The new <see cref="IMediaInfoReader"/>.</returns>
    public virtual IMediaInfoReader GetMediaInfoReader(object owner) => new MediaInfoReader(this) { Owner = owner };

    /// <summary>
    /// Creates an instance of <see cref="IMediaMuxer"/>  class using this factory.
    /// </summary>
    /// <param name="owner">The owner to send to the <see cref="IUserInterfaceManager"/>.</param>
    /// <returns>The new <see cref="IMediaMuxer"/>.</returns>
    public virtual IMediaMuxer GetMediaMuxer(object owner) => new MediaMuxer(this) { Owner = owner };

    /// <summary>
    /// Creates an instance of <see cref="IMediaScript"/>  class using this factory.
    /// </summary>
    /// <param name="owner">The owner to send to the <see cref="IUserInterfaceManager"/>.</param>
    /// <returns>The new <see cref="IMediaScript"/>.</returns>
    public virtual IMediaScript GetMediaScript(object owner) => new MediaScript(this) { Owner = owner };
}
