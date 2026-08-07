namespace HanumanInstitute.FFmpeg;

/// <summary>
/// Stream disposition flags (e.g. default, forced). Separate from metadata tags.
/// Use <see cref="Has"/> / <see cref="Set"/> by name.
/// </summary>
public class StreamDisposition
{
    // Parse allow-list: stream lines mix disposition with other parentheses, e.g.
    // Stream #0:0(und): Video: h264 (High) (avc1 / 0x31637661), yuv420p(tv, progressive), ... (default)
    // → only "default" (not und, High, avc1, tv, progressive)
    private static readonly HashSet<string> KnownParseNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "default", "dub", "original", "comment", "lyrics", "karaoke", "forced",
        "hearing_impaired", "visual_impaired", "clean_effects", "attached_pic",
        "timed_thumbnails", "captions", "descriptions", "metadata", "dependent", "still_image"
    };

    private readonly HashSet<string> _flags = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the set disposition flag names (e.g. default, forced).
    /// </summary>
    public IReadOnlyCollection<string> Flags => _flags;

    /// <summary>
    /// Returns whether any disposition flag is set.
    /// </summary>
    public bool Any => _flags.Any();

    /// <summary>
    /// Returns whether the named disposition flag is set.
    /// </summary>
    public bool Has(string name)
    {
        name.CheckNotNullOrEmpty();
        return _flags.Contains(name.ToLowerInvariant());
    }

    /// <summary>
    /// Enables or disables a disposition flag by name.
    /// </summary>
    public void Set(string name, bool enabled = true)
    {
        name = name.CheckNotNullOrEmpty().ToLowerInvariant();
        if (enabled)
        {
            _flags.Add(name);
        }
        else
        {
            _flags.Remove(name);
        }
    }

    /// <summary>
    /// Removes all disposition flags.
    /// </summary>
    public void Clear() => _flags.Clear();

    /// <summary>
    /// Returns flags joined for display (e.g. <c>default+forced</c>).
    /// </summary>
    public override string ToString() => string.Join("+", _flags);

    /// <summary>Parses disposition flags from a stream line.</summary>
    public static StreamDisposition FromStreamLine(string streamLine)
    {
        // (default) (forced)
        // (default, forced)
        // not: (und)  (High)  (LC)  yuv420p(tv, progressive)
        var d = new StreamDisposition();
        if (string.IsNullOrEmpty(streamLine)) { return d; }

        var start = -1;
        for (var i = 0; i < streamLine.Length; i++)
        {
            if (streamLine[i] == '(')
            {
                start = i + 1;
            }
            else if (streamLine[i] == ')' && start >= 0)
            {
                var inner = streamLine.Substring(start, i - start);
                foreach (var part in inner.Split([',', ' ', ';'], StringSplitOptions.RemoveEmptyEntries))
                {
                    if (KnownParseNames.Contains(part))
                    {
                        d._flags.Add(part.ToLowerInvariant());
                    }
                }
                start = -1;
            }
        }
        return d;
    }
}
