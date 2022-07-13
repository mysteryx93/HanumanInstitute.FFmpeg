
namespace HanumanInstitute.FFmpeg;

/// <summary>
/// Allows the implementing class to parse encoder console outputs.
/// </summary>
public interface IFileInfoParser {
    /// <summary>
    /// Returns whether ParseFileInfo has been called.
    /// </summary>
    bool IsParsed { get; }
    /// <summary>
    /// Returns whether enough information has been received to parse file information.
    /// </summary>
    /// <param name="data">The last line of output received.</param>
    /// <returns>Whether enough information was received to call ParseFileInfo.</returns>
    bool HasFileInfo(string data);
    /// <summary>
    /// Parses the output to read file information.
    /// </summary>
    /// <param name="outputText">The encoder process output.</param>
    /// <param name="options">Options that were specified when calling the encoder.</param>
    void ParseFileInfo(string outputText, ProcessOptionsEncoder? options);
    /// <summary>
    /// Returns whether specified line of output is a progress update.
    /// </summary>
    /// <param name="data">A line of output.</param>
    /// <returns>Whether the output line is a progress update.</returns>
    bool IsLineProgressUpdate(string data);
    /// <summary>
    /// Parses a progress update line of output.
    /// </summary>
    /// <param name="data">A line of output.</param>
    /// <returns>An object with parsed data.</returns>
    object ParseProgress(string data);
}
