using HanumanInstitute.FFmpeg.Services;

namespace HanumanInstitute.FFmpeg;

/// <summary>
/// Contains the configuration settings of FFmpeg.NET.
/// </summary>
public interface IMediaConfig
{
    /// <summary>
    /// Gets or sets the path to FFmpeg.exe
    /// </summary>
    string FFmpegPath { get; set; }
    /// <summary>
    /// Gets or sets the path to X264.exe
    /// </summary>
    string X264Path { get; set; }
    /// <summary>
    /// Gets or sets the path to X265.exe
    /// </summary>
    string X265Path { get; set; }
    /// <summary>
    /// Gets or sets the path to avs2pipemod.exe to use AviSynth in a separate process.
    /// </summary>
    string Avs2PipeMod { get; set; }
    /// <summary>
    /// Gets or sets the path to vspipe.exe to use VapourSynth in a separate process.
    /// </summary>
    string VsPipePath { get; set; }
    /// <summary>
    /// Occurs when a process needs to be closed. This needs to be managed manually for Console applications.
    /// See http://stackoverflow.com/a/29274238/3960200
    /// </summary>
    event CloseProcessEventHandler? CloseProcess;
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
    /// Occurs when running a custom application name to get the path of the application.
    /// </summary>
    event GetPathEventHandler? GetCustomAppPath;

    /// <summary>
    /// Returns the absolute path of FFmpeg as defined in settings.
    /// </summary>
    //string FFmpegPathAbsolute { get; }
    /// <summary>
    /// Returns the absolute path of Avs2PipeMod as defined in settings.
    /// </summary>
    //string Avs2PipeModPathAbsolute { get; }
    /// <summary>
    /// Returns all FFmpeg running processes.
    /// </summary>
    /// <returns>A list of FFmpeg processes.</returns>
    IProcess[] GetFFmpegProcesses();
    /// <summary>
    /// Soft closes a process. This will work on WinForms and WPF, but needs to be managed differently for Console applications.
    /// See http://stackoverflow.com/a/29274238/3960200
    /// </summary>
    /// <param name="process">The process to close.</param>
    /// <returns>Whether the process was closed.</returns>
    bool SoftKill(IProcess process);
}