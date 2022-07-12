namespace HanumanInstitute.FFmpeg;

/// <summary>
/// Contains progress information returned from x264's output.
/// </summary>
public class ProgressStatusX264
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
    /// The rate of data being read from source.
    /// </summary>
    public float Bitrate { get; set; }
    /// <summary>
    /// The time elapsed.
    /// </summary>
    public TimeSpan Time { get; set; }
    /// <summary>
    /// The size of the encoded file so far.
    /// </summary>
    public string Size { get; set; } = "";
}
