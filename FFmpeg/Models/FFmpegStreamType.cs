namespace HanumanInstitute.FFmpeg;

/// <summary>
/// Media stream type as reported by FFmpeg on the stream line.
/// </summary>
public enum FFmpegStreamType
{
    /// <summary>Unknown or unspecified.</summary>
    None,
    /// <summary>Video stream.</summary>
    Video,
    /// <summary>Audio stream.</summary>
    Audio,
    /// <summary>Subtitle / captions stream (e.g. subrip, ass, mov_text).</summary>
    Subtitle,
    /// <summary>Data stream (e.g. timed ID3, bin_data).</summary>
    Data,
    /// <summary>Attachment (e.g. fonts, cover art in Matroska).</summary>
    Attachment
}
