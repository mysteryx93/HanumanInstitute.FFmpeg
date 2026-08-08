namespace HanumanInstitute.FFmpeg;

/// <summary>
/// Options for a mux operation beyond the explicit stream list: container tags,
/// chapters, and which track types to pull from input containers.
/// </summary>
public class MuxOptions
{
    private readonly List<MuxFromInput> _from = [];

    /// <summary>
    /// Per-input include rules, in call order.
    /// </summary>
    public IReadOnlyList<MuxFromInput> FromInputs => _from;

    /// <summary>
    /// Output container tags (<c>-metadata key=value</c>).
    /// </summary>
    public IDictionary<string, string> Metadata { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Raw FFmpeg args inserted before the output path.
    /// </summary>
    public string? AdditionalArguments { get; set; }

    /// <summary>
    /// Include selected content from this file (adds an <c>-i</c> if that path is not already open).
    /// </summary>
    public MuxFromBuilder From(string path)
    {
        path.CheckNotNullOrEmpty();
        var rule = new MuxFromInput(path);
        _from.Add(rule);
        return new MuxFromBuilder(this, rule);
    }

    /// <summary>
    /// Include selected content from an open FFmpeg input file index (<c>-i</c> number), not a stream index.
    /// Distinct paths become inputs in order: paths from <see cref="From(string)"/> first, then
    /// remaining paths from the stream list (each path once).
    /// </summary>
    public MuxFromBuilder From(int inputIndex)
    {
        var rule = new MuxFromInput(inputIndex);
        _from.Add(rule);
        return new MuxFromBuilder(this, rule);
    }

    /// <summary>
    /// Sets <see cref="AdditionalArguments"/> and returns this instance.
    /// </summary>
    public MuxOptions WithAdditionalArguments(string args)
    {
        AdditionalArguments = args;
        return this;
    }
}
