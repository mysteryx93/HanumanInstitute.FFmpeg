using System.Diagnostics;

namespace HanumanInstitute.FFmpeg.Services;

/// <inheritdoc />
internal class ProcessFactory : IProcessFactory
{
    /// <inheritdoc />
    public IProcess Create() => new ProcessWrapper();
    /// <inheritdoc />
    public IProcess Create(Process? process) => new ProcessWrapper(process);
}
