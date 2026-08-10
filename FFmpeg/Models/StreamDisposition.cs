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
    // FFmpeg also prints multi-word names with spaces: (attached pic), (hearing impaired).
    private static readonly HashSet<string> s_knownParseNames = new(StringComparer.OrdinalIgnoreCase) {
        "default", "dub", "original", "comment", "lyrics", "karaoke", "forced",
        "hearing_impaired", "visual_impaired", "clean_effects", "attached_pic",
        "timed_thumbnails", "captions", "descriptions", "metadata", "dependent", "still_image"
    };

    private readonly HashSet<string> _flags = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// When true, ToString emits relative FFmpeg updates (e.g. <c>-default</c>)
    /// instead of an absolute flag list. Used to clear default without wiping other source dispositions.
    /// </summary>
    public bool IsRelative { get; private set; }

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
    /// <param name="name">Flag name (e.g. default, forced).</param>
    /// <param name="enabled">true to set; false to clear that flag.</param>
    /// <returns>This instance.</returns>
    public StreamDisposition Set(string name, bool enabled = true)
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
        return this;
    }

    /// <summary>
    /// Removes all disposition flags.
    /// </summary>
    /// <returns>This instance.</returns>
    public StreamDisposition Clear()
    {
        _flags.Clear();
        IsRelative = false;
        return this;
    }

    /// <summary>
    /// Returns flags for FFmpeg: absolute <c>default+forced</c>, or relative <c>-default</c>.
    /// </summary>
    public override string ToString()
    {
        if (IsRelative)
        {
            // First leading '-' selects relative mode; further flags use +/- separators.
            return string.Join("", _flags.Select(f => "-" + f));
        }
        return string.Join("+", _flags);
    }

    /// <summary>Parses disposition flags from a stream line.</summary>
    public static StreamDisposition FromStreamLine(string streamLine)
    {
        // (default) (forced)
        // (default, forced)
        // (attached pic)  → attached_pic
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
                // Split only on comma/semicolon so "attached pic" stays one token, then normalize spaces → underscores.
                foreach (var part in inner.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries))
                {
                    var name = part.Trim().Replace(' ', '_');
                    if (name.Length > 0 && s_knownParseNames.Contains(name))
                    {
                        d._flags.Add(name.ToLowerInvariant());
                    }
                }
                start = -1;
            }
        }
        return d;
    }

    /// <summary>
    /// Relative clear of the default flag only (<c>-disposition:N -default</c>).
    /// Keeps other source dispositions such as <c>attached_pic</c>.
    /// </summary>
    public static StreamDisposition RemoveDefault => CreateRelativeRemove("default");

    private static StreamDisposition CreateRelativeRemove(string flag)
    {
        var d = new StreamDisposition { IsRelative = true };
        d._flags.Add(flag.ToLowerInvariant());
        return d;
    }
}
