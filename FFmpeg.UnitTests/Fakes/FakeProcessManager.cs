using Microsoft.Extensions.Options;

// ReSharper disable ClassWithVirtualMembersNeverInherited.Global

namespace HanumanInstitute.FFmpeg.UnitTests;

public class FakeProcessManager : IProcessManager
{
    private readonly IOptions<AppPaths> _appPaths;

    public FakeProcessManager() : this(null) { }

    public FakeProcessManager(IOptions<AppPaths> appPaths = null)
    {
        _appPaths = appPaths ?? Options.Create(new AppPaths());
    }

    public virtual AppPaths Paths => _appPaths.Value;

    public virtual string ApplicationPath => "\\";

    public virtual string GetAppPath(string encoderApp)
    {
        if (encoderApp == EncoderApp.FFmpeg.ToString())
        {
            return _appPaths.Value.FFmpegPath;
        }
        else if (encoderApp == EncoderApp.x264.ToString())
        {
            return _appPaths.Value.X264Path;
        }
        else if (encoderApp == EncoderApp.x265.ToString())
        {
            return _appPaths.Value.X265Path;
        }
        return string.Empty;
    }

    public virtual IReadOnlyList<IProcess> GetFFmpegProcesses() => Array.Empty<IProcess>();

    public bool SoftKill(IProcess process) => false;
}
