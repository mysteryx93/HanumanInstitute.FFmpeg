namespace HanumanInstitute.FFmpeg;

/// <summary>
/// Represents a video stream with its info.
/// </summary>
public class MediaVideoStreamInfo : MediaStreamInfo
{
    /// <summary>
    /// The video color space, such as 'yuv420p'.
    /// </summary>
    public string ColorSpace { get; set; } = "";
    /// <summary>
    /// The video color range, generally 'pc', 'tv' or empty.
    /// </summary>
    public string ColorRange { get; set; } = "";
    /// <summary>
    /// The video color matrix, such as 'bt709'.
    /// </summary>
    public string ColorMatrix { get; set; } = "";
    /// <summary>
    /// The video width in pixels.
    /// </summary>
    public int Width { get; set; }
    /// <summary>
    /// The video height in pixels.
    /// </summary>
    public int Height { get; set; }
    /// <summary>
    /// The storage aspect ratio numerator.
    /// </summary>
    public int Sar1 { get; set; } = 1;
    /// <summary>
    /// The storage aspect ratio denominator.
    /// </summary>
    public int Sar2 { get; set; } = 1;
    /// <summary>
    /// The display aspect ratio numerator.
    /// </summary>
    public int Dar1 { get; set; } = 1;
    /// <summary>
    /// The display aspect ratio denominator.
    /// </summary>
    public int Dar2 { get; set; } = 1;
    /// <summary>
    /// The pixel aspect ratio as a double.
    /// </summary>
    public double PixelAspectRatio { get; set; } = 1;
    /// <summary>
    /// The display aspect ratio as a double.
    /// </summary>
    public double DisplayAspectRatio { get; set; } = 1;
    /// <summary>
    /// The video frame rate per second.
    /// </summary>
    public double FrameRate { get; set; }
    /// <summary>
    /// The video bit-depth.
    /// </summary>
    public int BitDepth { get; set; } = 8;
    /// <summary>
    /// The video bitrate.
    /// </summary>
    public int Bitrate { get; set; }
}
