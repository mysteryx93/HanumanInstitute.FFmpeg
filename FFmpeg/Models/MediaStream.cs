namespace HanumanInstitute.FFmpeg;

/// <summary>
/// Represents a file stream.
/// </summary>
public class MediaStream
{
    /// <summary>
    /// Gets or sets the path of the file.
    /// </summary>
    public string Path { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the index of the stream in the file.
    /// </summary>
    public int Index { get; set; }
    /// <summary>
    /// Gets or sets the data format of the stream.
    /// </summary>
    public string Format { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the type of stream: audio or video.
    /// </summary>
    public FFmpegStreamType Type { get; set; }

    /// <summary>
    /// Initializes a new instance of MediaStream.
    /// </summary>
    public MediaStream() { }

    /// <summary>
    /// Initializes a new instance of MediaStream.
    /// </summary>
    /// <param name="path">The path of the file.</param>
    /// <param name="index">The index of the stream in the file.</param>
    /// <param name="format">The data format of the stream.</param>
    /// <param name="type">The type of stream: audio or video.</param>
    public MediaStream(string path, int index, string format, FFmpegStreamType type)
    {
        Path = path;
        Index = index;
        Format = format;
        Type = type;
    }
}
