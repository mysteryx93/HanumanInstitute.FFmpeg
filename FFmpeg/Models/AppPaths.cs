// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo
namespace HanumanInstitute.FFmpeg;

/// <summary>
/// Configures the path where to find each application.
/// </summary>
public class AppPaths
{
    /// <summary>
    /// Gets or sets the path to FFmpeg
    /// </summary>
    public string FFmpeg { get; set; } = "ffmpeg";
    /// <summary>
    /// Gets or sets the path to X264
    /// </summary>
    public string X264 { get; set; } = "x264";
    /// <summary>
    /// Gets or sets the path to X265
    /// </summary>
    public string X265 { get; set; } = "x265";
    /// <summary>
    /// Gets or sets the path to avs2yuv to use Avisynth in a separate process.
    /// </summary>
    public string Avs2Yuv { get; set; } = "avs2yuv";
    /// <summary>
    /// Gets or sets the path to vspipe to use VapourSynth in a separate process.
    /// </summary>
    public string VsPipe { get; set; } = "vspipe";
}
