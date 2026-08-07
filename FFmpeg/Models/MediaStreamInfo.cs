namespace HanumanInstitute.FFmpeg;

/// <summary>
/// Base class for a media stream reported by FFmpeg (video, audio, subtitle, data, attachment).
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
    /// Gets or sets the stream language code when present (e.g. eng, und).
    /// </summary>
    public string? Language { get; set; }
    /// <summary>
    /// Stream metadata key/value pairs (e.g. title, handler_name).
    /// Separate from <see cref="Disposition"/> flags.
    /// </summary>
    public IDictionary<string, string> Metadata { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    /// <summary>
    /// Stream disposition flags (default, forced, etc.). Separate from <see cref="Metadata"/>.
    /// </summary>
    public StreamDisposition Disposition { get; set; } = new();

    /// <summary>
    /// Returns the stream type based on the derived class type.
    /// </summary>
    public FFmpegStreamType StreamType => this switch
    {
        MediaVideoStreamInfo => FFmpegStreamType.Video,
        MediaAudioStreamInfo => FFmpegStreamType.Audio,
        MediaSubtitleStreamInfo => FFmpegStreamType.Subtitle,
        MediaDataStreamInfo => FFmpegStreamType.Data,
        MediaAttachmentStreamInfo => FFmpegStreamType.Attachment,
        _ => FFmpegStreamType.None
    };
}
