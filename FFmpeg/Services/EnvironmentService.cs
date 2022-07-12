namespace HanumanInstitute.FFmpeg.Services;

/// <inheritdoc />
public class EnvironmentService : IEnvironmentService
{
    /// <inheritdoc />
    public DateTime Now => DateTime.Now;

    /// <inheritdoc />
    public DateTime UtcNow => DateTime.UtcNow;

    /// <inheritdoc />
    public string NewLine => Environment.NewLine;
}
