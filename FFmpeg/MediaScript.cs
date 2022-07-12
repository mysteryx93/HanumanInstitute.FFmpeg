using HanumanInstitute.FFmpeg.Properties;
using HanumanInstitute.FFmpeg.Services;
using static System.FormattableString;

namespace HanumanInstitute.FFmpeg;

/// <summary>
/// Provides methods to execute Avisynth or VapourSynth media script files.
/// </summary>
public class MediaScript : IMediaScript
{
    private readonly IProcessWorkerFactory _factory;
    private readonly IFileSystemService _fileSystem;

    public MediaScript(IProcessWorkerFactory processFactory) : this(processFactory, new FileSystemService()) { }

    internal MediaScript(IProcessWorkerFactory processFactory, IFileSystemService fileSystemService)
    {
        _factory = processFactory.CheckNotNull(nameof(processFactory));
        _fileSystem = fileSystemService.CheckNotNull(nameof(fileSystemService));
    }

    private object? _owner;
    /// <summary>
    /// Sets the owner of the process windows.
    /// </summary>
    public IMediaScript SetOwner(object owner)
    {
        _owner = owner;
        return this;
    }

    /// <summary>
    /// Runs avs2pipemod with specified source file. The output will be discarded.
    /// </summary>
    /// <param name="path">The path to the script to run.</param>
    /// <param name="options">The options for starting the process.</param>
    /// <param name="callback">A method that will be called after the process has been started.</param>
    /// <returns>The process completion status.</returns>
    public CompletionStatus RunAvisynth(string path, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null)
    {
        path.CheckNotNullOrEmpty(nameof(path));

        // if (!_fileSystem.Exists(_factory.Config.Avs2PipeMod))
        // {
        //     throw new System.IO.FileNotFoundException(string.Format(CultureInfo.InvariantCulture, Resources.Avs2PipeModPathNotFound, _factory.Config.Avs2PipeMod));
        // }

        var args = Invariant($@"""{path}"" -rawvideo > NUL");
        var worker = _factory.Create(_owner, options, callback);
        worker.OutputType = ProcessOutput.Error;
        var cmd = Invariant($@"""{_factory.Processes.Paths.Avs2PipeMod}"" {args}");
        var result = worker.RunAsCommand(cmd);
        return result;
    }

    /// <summary>
    /// Runs vspipe with specified source file. The output will be discarded.
    /// </summary>
    /// <param name="path">The path to the script to run.</param>
    /// <param name="options">The options for starting the process.</param>
    /// <param name="callback">A method that will be called after the process has been started.</param>
    /// <returns>The process completion status.</returns>
    public CompletionStatus RunVapourSynth(string path, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null)
    {
        path.CheckNotNullOrEmpty(nameof(path));

        // if (!_fileSystem.Exists(_factory.Config.Avs2PipeMod))
        // {
        //     throw new System.IO.FileNotFoundException(string.Format(CultureInfo.InvariantCulture, Resources.Avs2PipeModPathNotFound, _factory.Config.Avs2PipeMod));
        // }

        var args = Invariant($@"""{path}"" .");
        var worker = _factory.Create(_owner, options, callback);
        var result = worker.Run(_factory.Processes.Paths.VsPipePath, args);
        return result;
    }
}
