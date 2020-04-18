using System;
using HanumanInstitute.FFmpeg;
using HanumanInstitute.FFmpegExampleApplication.ViewModels;

namespace HanumanInstitute.FFmpegExampleApplication.Business
{
    public class FFmpegUserInterfaceFactory : IFFmpegUserInterfaceFactory
    {
        public FFmpegUserInterfaceFactory()
        { }

        public IFFmpegUiViewModel CreateUI(string title, bool autoClose)
        {
            return new FFmpegUiViewModel()
            {
                Title = title,
                AutoClose = autoClose
            };
        }

        public IFFmpegErrorViewModel CreateError(IProcessWorker host)
        {
            return new FFmpegErrorViewModel(host);
        }
    }
}
