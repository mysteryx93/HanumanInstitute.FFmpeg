using System.Diagnostics;

namespace HanumanInstitute.FFmpeg.Services;

/// <summary>
/// Creates instances of process wrapper classes.
/// </summary>
public interface IProcessFactory
{
    /// <summary>
    /// Creates a new instance of IProcess.
    /// </summary>
    IProcess Create();
    /// <summary>
    /// Creates a new instance of IProcess as a wrapper around an existing process.
    /// </summary>
    /// <param name="process">The process to wrap the new class around.</param>
    IProcess Create(Process process);
}