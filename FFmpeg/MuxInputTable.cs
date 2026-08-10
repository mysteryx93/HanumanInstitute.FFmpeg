namespace HanumanInstitute.FFmpeg;

// Maintains the ordered FFmpeg input table.
internal sealed class MuxInputTable
{
    private readonly Dictionary<string, int> _pathToIndex = new(StringComparer.Ordinal);
    public List<string> Paths { get; } = [];

    // Creates the input table from From rules and explicit streams.
    public static MuxInputTable From(IReadOnlyList<MediaStream> streams, MuxOptions muxOptions)
    {
        var table = new MuxInputTable();
        foreach (var rule in muxOptions.FromInputs)
        {
            if (rule.Path.HasValue())
            {
                table.Add(rule.Path!);
            }
        }

        foreach (var stream in streams)
        {
            table.Add(stream.Path);
        }

        return table;
    }

    // Adds a path if it is not already open as an input.
    private void Add(string path)
    {
        if (_pathToIndex.ContainsKey(path))
        {
            return;
        }

        _pathToIndex[path] = Paths.Count;
        Paths.Add(path);
    }

    // Returns the FFmpeg input index for a path.
    public int IndexOf(string path) => _pathToIndex[path];

    // Resolves a MuxFromInput rule to an FFmpeg input index.
    public bool TryResolve(MuxFromInput rule, out int index)
    {
        if (rule.Path.HasValue())
        {
            return _pathToIndex.TryGetValue(rule.Path!, out index);
        }

        if (rule.InputIndex is { } i && i >= 0 && i < Paths.Count)
        {
            index = i;
            return true;
        }

        index = -1;
        return false;
    }
}
