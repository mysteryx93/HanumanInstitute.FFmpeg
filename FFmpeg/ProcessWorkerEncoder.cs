using System.Diagnostics;
using HanumanInstitute.FFmpeg.Services;

namespace HanumanInstitute.FFmpeg;

/// <inheritdoc cref="IProcessWorkerEncoder" />
public class ProcessWorkerEncoder : ProcessWorker, IProcessWorkerEncoder
{
    private readonly IFileSystemService _fileSystem;
    private readonly IFileInfoParserFactory _parserFactory;

    internal ProcessWorkerEncoder(IProcessManager config, IProcessFactory processFactory, IFileSystemService fileSystemService, IFileInfoParserFactory parserFactory, ProcessOptionsEncoder options)
        : base(config, processFactory, options)
    {
        _fileSystem = fileSystemService.CheckNotNull(nameof(fileSystemService));
        _parserFactory = parserFactory.CheckNotNull(nameof(parserFactory));
        OutputType = ProcessOutput.Error;
    }

    /// <inheritdoc />
    public string EncoderApp { get; set; } = string.Empty;
    /// <summary>
    /// Gets the class parsing and storing file information.
    /// </summary>
    protected IFileInfoParser Parser { get; private set; } = default!;
    /// <inheritdoc />
    public object FileInfo => Parser;
    /// <inheritdoc />
    public object? LastProgressReceived { get; private set; }
    /// <inheritdoc />
    public event EventHandler? FileInfoUpdated;
    /// <inheritdoc />
    public event ProgressReceivedEventHandler? ProgressReceived;

    /// <inheritdoc />
    public new ProcessOptionsEncoder Options
    {
        get => (ProcessOptionsEncoder)base.Options;
        set => base.Options = value;
    }

    /// <inheritdoc />
    public CompletionStatus RunEncoder(string arguments, EncoderApp encoderApp) =>
        RunEncoder(arguments, encoderApp.ToString());

    /// <inheritdoc />
    public CompletionStatus RunEncoder(string arguments, string encoderApp)
    {
        var appPath = Processes.GetAppPath(encoderApp);
        // if (!_fileSystem.Exists(appPath))
        // {
        //     throw new System.IO.FileNotFoundException($@"The file ""{appPath}"" for the encoding application {encoderApp} configured in MediaConfig was not found.", appPath);
        // }

        EnsureNotRunning();
        EncoderApp = encoderApp;
        return Run(appPath, arguments);
    }

    /// <inheritdoc />
    public CompletionStatus RunAvisynthToEncoder(string source, string arguments, EncoderApp encoderApp) =>
        RunAvisynthToEncoder(source, arguments, encoderApp.ToString());

    /// <inheritdoc />
    public CompletionStatus RunAvisynthToEncoder(string source, string arguments, string encoderApp)
    {
        source.CheckNotNullOrEmpty(nameof(source));
        //if (!_fileSystem.Exists(Config.Avs2PipeMod)) { throw new System.IO.FileNotFoundException(string.Format(CultureInfo.InvariantCulture, Resources.Avs2PipeModPathNotFound, Config.Avs2PipeMod)); }
        EnsureNotRunning();
        EncoderApp = encoderApp;
        var query = string.Format(CultureInfo.InvariantCulture, @"""{0}"" -y4mp ""{1}"" | ""{2}"" {3}", Processes.Paths.Avs2PipeMod, source, Processes.GetAppPath(encoderApp), arguments);
        return RunAsCommand(query);
    }

    /// <inheritdoc />
    public CompletionStatus RunVapourSynthToEncoder(string source, string arguments, EncoderApp encoderApp) =>
        RunVapourSynthToEncoder(source, arguments, encoderApp.ToString());

    /// <inheritdoc />
    public CompletionStatus RunVapourSynthToEncoder(string source, string arguments, string encoderApp)
    {
        source.CheckNotNullOrEmpty(nameof(source));
        // if (!_fileSystem.Exists(Processes.VsPipePath)) { throw new System.IO.FileNotFoundException(string.Format(CultureInfo.InvariantCulture, Resources.VsPipePathNotFound, Processes.VsPipePath)); }

        EnsureNotRunning();
        EncoderApp = encoderApp;
        var query = string.Format(CultureInfo.InvariantCulture, @"""{0}"" -c y4m ""{1}"" - | ""{2}"" {3}", Processes.Paths.VsPipePath, source, Processes.GetAppPath(encoderApp), arguments);
        return RunAsCommand(query);
    }

    /// <inheritdoc />
    public override CompletionStatus Run(string fileName, string arguments)
    {
        EnsureNotRunning();
        Parser = _parserFactory.Create(EncoderApp);
        return base.Run(fileName, arguments);
    }

    /// <summary>
    /// Occurs when data is received from the executing application.
    /// </summary>
    protected override void OnDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data == null)
        {
            if (!Parser.IsParsed)
            {
                ParseFileInfo();
            }
            return;
        }

        base.OnDataReceived(sender, e);

        object? progressInfo = null;
        if (!Parser.IsParsed && Parser.HasFileInfo(e.Data))
        {
            ParseFileInfo();
        }

        if (Parser.IsParsed && Parser.IsLineProgressUpdate(e.Data))
        {
            progressInfo = Parser.ParseProgress(e.Data);
        }

        if (progressInfo != null)
        {
            LastProgressReceived = progressInfo;
            ProgressReceived?.Invoke(this, new ProgressReceivedEventArgs(progressInfo));
        }
    }

    /// <summary>
    /// Parses file information from output.
    /// </summary>
    private void ParseFileInfo()
    {
        Parser.ParseFileInfo(Output, Options);
        FileInfoUpdated?.Invoke(this, EventArgs.Empty);
    }
}
