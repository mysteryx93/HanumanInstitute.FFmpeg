namespace HanumanInstitute.FFmpeg.Services;

/// <inheritdoc />
internal class EnvironmentService : IEnvironmentService
{
    /// <inheritdoc />
    public DateTime Now => DateTime.Now;

    /// <inheritdoc />
    public DateTime UtcNow => DateTime.UtcNow;

    /// <inheritdoc />
    public string NewLine => Environment.NewLine;
}
