using System;
using HanumanInstitute.FFmpeg.Services;

namespace HanumanInstitute.FFmpeg.UnitTests;

public class FakeMediaConfig : IProcessManager
{
    public virtual string FFmpegPath { get; set; } = "ffmpeg.exe";
    public virtual string X264Path { get; set; } = "x264.exe";
    public virtual string X265Path { get; set; } = "x265.exe";
    public virtual string Avs2PipeMod { get; set; } = "avs2pipemod.exe";
    public virtual string VsPipePath { get; set; } = "vspipe.exe";
    public virtual IUserInterfaceManager UserInterfaceManager { get; set; }

    public virtual string ApplicationPath => "\\";

    public event CloseProcessEventHandler CloseProcess;

    public virtual string GetAppPath(string encoderApp)
    {
        if (encoderApp == EncoderApp.FFmpeg.ToString())
        {
            return FFmpegPath;
        }
        else if (encoderApp == EncoderApp.x264.ToString())
        {
            return X264Path;
        }
        else if (encoderApp == EncoderApp.x265.ToString())
        {
            return X265Path;
        }
        else
        {
            // Allow specifying custom application paths by handling this event.
            var args = new GetPathEventArgs(encoderApp);
            GetCustomAppPath?.Invoke(this, args);
            return args.Path;
        }
    }

    public event GetPathEventHandler GetCustomAppPath;

    public virtual IProcess[] GetFFmpegProcesses()
    {
        return null;
    }

    public virtual bool SoftKill(IProcess process)
    {
        CloseProcess?.Invoke(this, new CloseProcessEventArgs(process));
        return false;
    }
}