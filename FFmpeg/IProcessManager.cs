using HanumanInstitute.FFmpeg.Services;

namespace HanumanInstitute.FFmpeg;

/// <summary>
/// Contains the configuration settings of FFmpeg.NET.
/// </summary>
public interface IProcessManager
{
    /// <summary>
    /// Gets the configured paths to standard encoding application.
    /// </summary>
    public AppPaths Paths { get; }

    /// <summary>
    /// Gets the path of the executing assembly.
    /// </summary>
    string ApplicationPath { get; }
    
    /// <summary>
    /// Returns the configured path for specified encoder application.
    /// </summary>
    /// <param name="encoderApp">The encoder to get the configured path for.</param>
    /// <returns>A file path string.</returns>
    string GetAppPath(string encoderApp);

    /// <summary>
    /// Returns all FFmpeg running processes.
    /// </summary>
    /// <returns>A list of FFmpeg processes.</returns>
    IReadOnlyList<IProcess> GetFFmpegProcesses();
    
    /// <summary>
    /// Soft closes a process. This will work on WinForms and WPF, but needs to be managed differently for Console applications.
    /// See http://stackoverflow.com/a/29274238/3960200
    /// </summary>
    /// <param name="process">The process to close.</param>
    /// <returns>Whether the process was closed.</returns>
    bool SoftKill(IProcess process);
}
