namespace HanumanInstitute.FFmpeg;

/// <summary>
/// One input stream for a mux operation: path, index, type/format, and write tags.
/// </summary>
public class MediaStream
{
    internal MediaStream(string path, int index, string format, FFmpegStreamType type)
        : this(path, index)
    {
        Format = format ?? string.Empty;
        Type = type;
    }
    
    /// <summary>
    /// Creates a stream selection.
    /// </summary>
    /// <param name="path">Input file path.</param>
    /// <param name="index">Stream index in the input file.</param>
    public MediaStream(string path, int index)
    {
        Path = path.CheckNotNull();
        Index = index;
    }

    /// <summary>
    /// Creates a MediaStream for mux from a MediaStreamInfo.
    /// </summary>
    /// <param name="path">Input file path.</param>
    /// <param name="info">Probe result used for type, format, and tags.</param>
    /// <returns>A stream with type, format, and tags from info.</returns>
    public static MediaStream FromStreamInfo(string path, MediaStreamInfo info)
    {
        path.CheckNotNullOrEmpty();
        info.CheckNotNull();
        return new MediaStream(path, info.Index, info.Format, info.StreamType).CopyTagsFrom(info);
    }

    /// <summary>
    /// Input file path.
    /// </summary>
    public string Path { get; private set; }

    /// <summary>
    /// Stream index in the input file.
    /// </summary>
    public int Index { get; private set; }

    /// <summary>
    /// Codec name (e.g. h264, aac). Empty until resolved.
    /// </summary>
    public string Format { get; private set; } = string.Empty;

    /// <summary>
    /// Stream type. None until resolved.
    /// </summary>
    public FFmpegStreamType Type { get; private set; }

    /// <summary>
    /// Language for <c>-metadata:s:N language=</c>, or null to omit.
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// Tags for <c>-metadata:s:N</c>. Empty to omit.
    /// </summary>
    public IDictionary<string, string> Metadata { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Disposition for <c>-disposition:N</c>:
    /// null = omit;
    /// empty = clear (<c>0</c>);
    /// flags = set those flags.
    /// </summary>
    public StreamDisposition? Disposition { get; set; }

    /// <summary>
    /// Per-stream codec, or null for stream-copy.
    /// </summary>
    public string? Codec { get; set; }

    /// <summary>
    /// Copies language, metadata, and disposition from info.
    /// Disposition is set only when the source has flags; otherwise left null.
    /// </summary>
    /// <param name="info">Probe result to copy from.</param>
    /// <returns>This instance.</returns>
    public MediaStream CopyTagsFrom(MediaStreamInfo info)
    {
        info.CheckNotNull();
        Language = info.Language;
        Metadata.Clear();
        foreach (var pair in info.Metadata)
        {
            Metadata[pair.Key] = pair.Value;
        }
        Disposition = info.Disposition.Any ? CloneDisposition(info.Disposition) : null;
        return this;
    }

    /// <summary>
    /// Copies language, metadata, disposition, and codec from source.
    /// </summary>
    /// <param name="source">Stream to copy write settings from.</param>
    /// <returns>This instance.</returns>
    public MediaStream CopyWriteSettingsFrom(MediaStream source)
    {
        source.CheckNotNull();
        Language = source.Language;
        Codec = source.Codec;
        Disposition = source.Disposition == null ? null : CloneDisposition(source.Disposition);
        Metadata.Clear();
        foreach (var pair in source.Metadata)
        {
            Metadata[pair.Key] = pair.Value;
        }
        return this;
    }

    /// <summary>
    /// Sets type and format from info.
    /// </summary>
    /// <param name="info">Probe result for this stream.</param>
    /// <returns>This instance.</returns>
    internal void SetFileInfo(MediaStreamInfo info)
    {
        info.CheckNotNull();
        Format = info.Format ?? string.Empty;
        Type = info.StreamType;
    }

    private static StreamDisposition CloneDisposition(StreamDisposition? source)
    {
        var d = new StreamDisposition();
        if (source == null)
        {
            return d;
        }
        foreach (var flag in source.Flags)
        {
            d.Set(flag);
        }
        return d;
    }
}
