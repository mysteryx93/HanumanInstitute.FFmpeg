using HanumanInstitute.FFmpeg.Services;
// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo

namespace HanumanInstitute.FFmpeg;

/// <inheritdoc />
public class MediaScript : IMediaScript
{
    private readonly IProcessService _factory;
    private readonly IFileSystemService _fileSystem;

    /// <summary>
    /// Initializes a new instance of the MediaScript class
    /// </summary>
    /// <param name="processFactory">The Factory responsible for creating processes.</param>
    public MediaScript(IProcessService processFactory) : this(processFactory, new FileSystemService()) { }

    internal MediaScript(IProcessService processFactory, IFileSystemService fileSystemService)
    {
        _factory = processFactory.CheckNotNull(nameof(processFactory));
        _fileSystem = fileSystemService.CheckNotNull(nameof(fileSystemService));
    }

    /// <inheritdoc />
    public object? Owner { get; set; }

    /// <inheritdoc />
    public CompletionStatus RunAvisynth(string path, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null)
    {
        path.CheckNotNullOrEmpty(nameof(path));

        // if (!_fileSystem.Exists(_factory.Config.Avs2PipeMod))
        // {
        //     throw new System.IO.FileNotFoundException(string.Format(CultureInfo.InvariantCulture, Resources.Avs2PipeModPathNotFound, _factory.Config.Avs2PipeMod));
        // }

        var args = Invariant($@"""{path}"" -rawvideo > NUL");
        var worker = _factory.CreateProcess(Owner, options, callback);
        worker.OutputType = ProcessOutput.Error;
        var cmd = Invariant($@"""{_factory.Processes.Paths.Avs2Yuv}"" {args}");
        var result = worker.RunAsCommand(cmd);
        return result;
    }

    /// <inheritdoc />
    public CompletionStatus RunVapourSynth(string path, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null)
    {
        path.CheckNotNullOrEmpty(nameof(path));

        // if (!_fileSystem.Exists(_factory.Config.Avs2PipeMod))
        // {
        //     throw new System.IO.FileNotFoundException(string.Format(CultureInfo.InvariantCulture, Resources.Avs2PipeModPathNotFound, _factory.Config.Avs2PipeMod));
        // }

        var args = Invariant($@"""{path}"" .");
        var worker = _factory.CreateProcess(Owner, options, callback);
        var result = worker.Run(_factory.Processes.Paths.VsPipePath, args);
        return result;
    }
}
