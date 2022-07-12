using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using HanumanInstitute.FFmpeg.Services;
using HanumanInstitute.Validators;
using Microsoft.Extensions.Options;
// ReSharper disable ClassWithVirtualMembersNeverInherited.Global

namespace HanumanInstitute.FFmpeg;

/// <summary>
/// Contains the configuration settings of HanumanInstitute.FFmpeg.
/// </summary>
public class ProcessManager : IProcessManager
{
    private readonly IOptions<AppPaths> _appPaths;
    private readonly IWindowsApiService _api;
    private readonly IFileSystemService _fileSystem;

    /// <summary>
    /// Initializes a new instance of the ProcessManager class.
    /// </summary>
    /// <param name="appPaths"></param>
    public ProcessManager(IOptions<AppPaths>? appPaths) : this(appPaths, new WindowsApiService(), new FileSystemService()) { }

    internal ProcessManager(IOptions<AppPaths>? appPaths, IWindowsApiService winApi, IFileSystemService fileSystemService)
    {
        _appPaths = appPaths ?? Options.Create(new AppPaths());
        _api = winApi ?? throw new ArgumentNullException(nameof(winApi));
        _fileSystem = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
    }

    /// <inheritdoc />
    public AppPaths Paths => _appPaths.Value;

    /// <summary>
    /// Gets the path of the executing assembly.
    /// </summary>
    public string ApplicationPath => _fileSystem.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

    /// <summary>
    /// Returns the configured path for specified encoder application.
    /// </summary>
    /// <param name="encoderApp">The encoder to get the configured path for.</param>
    /// <returns>A file path string.</returns>
    public virtual string GetAppPath(string encoderApp)
    {
        if (encoderApp == EncoderApp.FFmpeg.ToString())
        {
            return _appPaths.Value.FFmpegPath;
        }
        else if (encoderApp == EncoderApp.x264.ToString())
        {
            return _appPaths.Value.X264Path;
        }
        else if (encoderApp == EncoderApp.x265.ToString())
        {
            return _appPaths.Value.X265Path;
        }
        return string.Empty;
    }

    /// <summary>
    /// Returns all FFmpeg running processes.
    /// </summary>
    /// <returns>A list of FFmpeg processes.</returns>
    public IReadOnlyList<IProcess> GetFFmpegProcesses()
    {
        var processName = _fileSystem.GetFileNameWithoutExtension(_appPaths.Value.FFmpegPath);
        return Process.GetProcessesByName(processName).Select(p => new ProcessWrapper(p)).ToList<IProcess>();
    }

    /// <summary>
    /// Soft closes a process. This will work on WinForms and WPF, but needs to be managed differently for Console applications.
    /// See http://stackoverflow.com/a/29274238/3960200
    /// </summary>
    /// <param name="process">The process to close.</param>
    /// <returns>Whether the process was closed.</returns>
    public virtual bool SoftKill(IProcess process)
    {
        process.CheckNotNull(nameof(process));

        if (!process.HasExited)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                SoftKillWinApp(process);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // kill pid                
                Process.Start("kill", process.Id.ToStringInvariant());
                process.WaitForExit();
            }
        }

        return process.HasExited;
    }

    /// <summary>
    /// Soft closes from a WinForms or WPF process.
    /// </summary>
    /// <param name="process">The process to close.</param>
    private void SoftKillWinApp(IProcess process)
    {
        if (process == null) { throw new ArgumentNullException(nameof(process)); }

        if (_api.AttachConsole((uint)process.Id))
        {
            _api.SetConsoleCtrlHandler(null, true);
            try
            {
                if (!_api.GenerateConsoleCtrlEvent())
                {
                    return;
                }

                process.WaitForExit();
            }
            finally
            {
                _api.FreeConsole();
                _api.SetConsoleCtrlHandler(null, false);
            }
        }
    }
}
