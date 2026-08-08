namespace HanumanInstitute.FFmpeg;

/// <summary>
/// What to include from one input file (container): by path or by FFmpeg <c>-i</c> index.
/// </summary>
public sealed class MuxFromInput
{
    /// <summary>
    /// Targets a file path.
    /// </summary>
    public MuxFromInput(string path)
    {
        Path = path.CheckNotNullOrEmpty();
    }

    /// <summary>
    /// Targets an open input file index (<c>-i N</c>), not a stream index inside a file.
    /// </summary>
    public MuxFromInput(int inputIndex)
    {
        InputIndex = inputIndex;
    }

    /// <summary>
    /// File path, or null if this rule uses <see cref="InputIndex"/>.
    /// </summary>
    public string? Path { get; }

    /// <summary>
    /// FFmpeg <c>-i</c> index, or null if this rule uses <see cref="Path"/>.
    /// </summary>
    public int? InputIndex { get; }

    /// <summary>
    /// Copy container tags (<c>-map_metadata</c>).
    /// </summary>
    public bool ContainerTags { get; set; }

    /// <summary>
    /// Copy chapters (<c>-map_chapters</c>).
    /// </summary>
    public bool Chapters { get; set; }

    /// <summary>
    /// Map video streams (excludes cover art).
    /// </summary>
    public bool Video { get; set; }

    /// <summary>
    /// Map audio.
    /// </summary>
    public bool Audio { get; set; }

    /// <summary>
    /// Map subtitles.
    /// </summary>
    public bool Subtitles { get; set; }

    /// <summary>
    /// Map cover art.
    /// </summary>
    public bool Cover { get; set; }

    /// <summary>
    /// Map attachments.
    /// </summary>
    public bool Attachments { get; set; }

    /// <summary>
    /// Map data streams.
    /// </summary>
    public bool Data { get; set; }

    /// <summary>
    /// True if any include flag is set.
    /// </summary>
    public bool Any =>
        ContainerTags || Chapters || Video || Audio || Subtitles || Cover || Attachments || Data;
}
