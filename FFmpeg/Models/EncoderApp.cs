// ReSharper disable InconsistentNaming
namespace HanumanInstitute.FFmpeg;

/// <summary>
/// Represents the application to use for encoding.
/// </summary>
public enum EncoderApp
{
    /// <summary>
    /// Run FFmpeg encoder.
    /// </summary>
    FFmpeg,
    /// <summary>
    /// Run x264 encoder.
    /// </summary>
    x264,
    /// <summary>
    /// Run x265 encoder.
    /// </summary>
    x265
}
