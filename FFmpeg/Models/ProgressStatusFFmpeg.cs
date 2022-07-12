namespace HanumanInstitute.FFmpeg;

/// <summary>
/// Contains progress information returned from FFmpeg's output.
/// </summary>
public class ProgressStatusFFmpeg
{
    /// <summary>
    /// The position of the last frame that was processed.
    /// </summary>
    public long Frame { get; set; }
    /// <summary>
    /// The encoding speed in frames per second.
    /// </summary>
    public float Fps { get; set; }
    /// <summary>
    /// The encoding quantizer.
    /// </summary>
    public float Quantizer { get; set; }
    /// <summary>
    /// The size of the encoded file so far.
    /// </summary>
    public string Size { get; set; } = "";
    /// <summary>
    /// The time elapsed.
    /// </summary>
    public TimeSpan Time { get; set; }
    /// <summary>
    /// The rate of data being read from source.
    /// </summary>
    public string Bitrate { get; set; } = "";
    /// <summary>
    /// The encoding speed compared to normal playback.
    /// </summary>
    public float Speed { get; set; }
}
