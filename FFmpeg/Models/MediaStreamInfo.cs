namespace HanumanInstitute.FFmpeg;

/// <summary>
/// Base class for MediaVideoStream and MediaAudioStream representing a file stream.
/// </summary>
public abstract class MediaStreamInfo
{
    /// <summary>
    /// Gets or sets the raw text of the stream info.
    /// </summary>
    public string RawText { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the index of the stream in the file.
    /// </summary>
    public int Index { get; set; }
    /// <summary>
    /// Gets or sets the data format of the stream in the file.
    /// </summary>
    public string Format { get; set; } = string.Empty;

    /// <summary>
    /// Returns the stream type based on the derived class type.
    /// </summary>
    public FFmpegStreamType StreamType => 
        GetType() == typeof(MediaVideoStreamInfo) ? FFmpegStreamType.Video : 
        GetType() == typeof(MediaAudioStreamInfo) ? FFmpegStreamType.Audio : FFmpegStreamType.None;
}
