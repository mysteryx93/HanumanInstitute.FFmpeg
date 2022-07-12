namespace HanumanInstitute.FFmpeg;

/// <summary>
/// Represents which process output to read.
/// </summary>
public enum ProcessOutput
{
    /// <summary>
    /// Don't read process output.
    /// </summary>
    None,
    /// <summary>
    /// Read the process' standard output.
    /// </summary>
    Output,
    /// <summary>
    /// Read the process' error output.
    /// </summary>
    Error
}
