namespace HanumanInstitute.FFmpeg;

/// <summary>
/// Fluent configuration for one input container on a shared <see cref="MuxOptions"/> instance.
/// </summary>
public sealed class MuxFromBuilder
{
    private readonly MuxOptions _owner;
    private readonly MuxFromInput _rule;

    internal MuxFromBuilder(MuxOptions owner, MuxFromInput rule)
    {
        _owner = owner;
        _rule = rule;
    }

    /// <summary>
    /// Container tags (<c>-map_metadata</c>).
    /// </summary>
    /// <param name="include">true to include (default); false to exclude.</param>
    public MuxFromBuilder ContainerTags(bool include = true)
    {
        _rule.ContainerTags = include;
        return this;
    }

    /// <summary>
    /// Alias for <see cref="ContainerTags"/>.
    /// </summary>
    /// <param name="include">true to include (default); false to exclude.</param>
    public MuxFromBuilder Metadata(bool include = true) => ContainerTags(include);

    /// <summary>
    /// Chapters (<c>-map_chapters</c>).
    /// </summary>
    /// <param name="include">true to include (default); false to exclude.</param>
    public MuxFromBuilder Chapters(bool include = true)
    {
        _rule.Chapters = include;
        return this;
    }

    /// <summary>
    /// Video streams (excludes cover art).
    /// </summary>
    /// <param name="include">true to include (default); false to exclude.</param>
    public MuxFromBuilder Video(bool include = true)
    {
        _rule.Video = include;
        return this;
    }

    /// <summary>
    /// Audio streams.
    /// </summary>
    /// <param name="include">true to include (default); false to exclude.</param>
    public MuxFromBuilder Audio(bool include = true)
    {
        _rule.Audio = include;
        return this;
    }

    /// <summary>
    /// Subtitle / caption streams.
    /// </summary>
    /// <param name="include">true to include (default); false to exclude.</param>
    public MuxFromBuilder Subtitles(bool include = true)
    {
        _rule.Subtitles = include;
        return this;
    }

    /// <summary>
    /// Cover art (<c>attached_pic</c>).
    /// </summary>
    /// <param name="include">true to include (default); false to exclude.</param>
    public MuxFromBuilder Cover(bool include = true)
    {
        _rule.Cover = include;
        return this;
    }

    /// <summary>
    /// Attachment streams (e.g. fonts).
    /// </summary>
    /// <param name="include">true to include (default); false to exclude.</param>
    public MuxFromBuilder Attachments(bool include = true)
    {
        _rule.Attachments = include;
        return this;
    }

    /// <summary>
    /// Data streams.
    /// </summary>
    /// <param name="include">true to include (default); false to exclude.</param>
    public MuxFromBuilder Data(bool include = true)
    {
        _rule.Data = include;
        return this;
    }

    /// <summary>
    /// Video + audio + subtitles.
    /// </summary>
    /// <param name="include">true to include (default); false to exclude.</param>
    public MuxFromBuilder Media(bool include = true) => Video(include).Audio(include).Subtitles(include);

    /// <summary>
    /// Container tags + chapters.
    /// </summary>
    /// <param name="include">true to include (default); false to exclude.</param>
    public MuxFromBuilder Container(bool include = true) => ContainerTags(include).Chapters(include);

    /// <summary>
    /// Cover + attachments + data.
    /// </summary>
    /// <param name="include">true to include (default); false to exclude.</param>
    public MuxFromBuilder SideStreams(bool include = true) => Cover(include).Attachments(include).Data(include);

    /// <summary>
    /// All include flags for this input.
    /// </summary>
    /// <param name="include">true to include (default); false to exclude.</param>
    public MuxFromBuilder All(bool include = true) => Media(include).Container(include).SideStreams(include);

    /// <summary>
    /// Another file path on the same <see cref="MuxOptions"/>.
    /// </summary>
    public MuxFromBuilder From(string path) => _owner.From(path);

    /// <summary>
    /// Another open <c>-i</c> index on the same <see cref="MuxOptions"/>.
    /// </summary>
    public MuxFromBuilder From(int inputIndex) => _owner.From(inputIndex);

    /// <summary>
    /// Returns the owning <see cref="MuxOptions"/> (e.g. to set <see cref="MuxOptions.Metadata"/>).
    /// </summary>
    public MuxOptions Done() => _owner;

    /// <summary>
    /// Allows passing the fluent chain where <see cref="MuxOptions"/> is expected.
    /// </summary>
    public static implicit operator MuxOptions(MuxFromBuilder builder) => builder._owner;
}
