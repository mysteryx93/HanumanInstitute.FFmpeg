namespace HanumanInstitute.FFmpeg;

/// <summary>
/// Represents an audio stream with its info.
/// </summary>
public class MediaAudioStreamInfo : MediaStreamInfo
{
    /// <summary>
    /// The sample rate of the audio.
    /// </summary>
    public int SampleRate { get; set; }
    /// <summary>
    /// The channels layout of the audio, such as 'mono' or 'stereo'. 
    /// </summary>
    public string Channels { get; set; } = "";
    /// <summary>
    /// The audio bit-depth, such as 's16p'. 
    /// </summary>
    public string BitDepth { get; set; } = "";
    /// <summary>
    /// The audio bitrate in kb/s.
    /// </summary>
    public int Bitrate { get; set; }
}
