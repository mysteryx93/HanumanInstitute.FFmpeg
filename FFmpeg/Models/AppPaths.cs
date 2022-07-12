// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo
namespace HanumanInstitute.FFmpeg;

/// <summary>
/// Configures the path where to find each application.
/// </summary>
public class AppPaths
{
    /// <summary>
    /// Gets or sets the path to FFmpeg.exe
    /// </summary>
    public string FFmpegPath { get; set; } = "ffmpeg.exe";
    /// <summary>
    /// Gets or sets the path to X264.exe
    /// </summary>
    public string X264Path { get; set; } = "x264.exe";
    /// <summary>
    /// Gets or sets the path to X265.exe
    /// </summary>
    public string X265Path { get; set; } = "x265.exe";
    /// <summary>
    /// Gets or sets the path to avs2pipemod.exe to use Avisynth in a separate process.
    /// </summary>
    public string Avs2PipeMod { get; set; } = "avs2pipemod.exe";
    /// <summary>
    /// Gets or sets the path to vspipe.exe to use VapourSynth in a separate process.
    /// </summary>
    public string VsPipePath { get; set; } = "vspipe.exe";
}
