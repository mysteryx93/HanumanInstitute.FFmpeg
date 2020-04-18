using System;
using HanumanInstitute.FFmpeg;
using HanumanInstitute.FFmpegExampleApplication.ViewModels;

namespace HanumanInstitute.FFmpegExampleApplication.Business
{
    public interface IFFmpegUserInterfaceFactory
    {
        IFFmpegUiViewModel CreateUI(string title, bool autoClose);
        IFFmpegErrorViewModel CreateError(IProcessWorker host);
    }
}
