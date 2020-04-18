using System;
using System.ComponentModel;
using HanumanInstitute.FFmpeg;

namespace HanumanInstitute.FFmpegExampleApplication.ViewModels
{
    public interface IFFmpegUiViewModel : IUserInterfaceWindow, IWorkspaceViewModel
    {
        bool AutoClose { get; set; }
        string Fps { get; }
        long ProgressBarMax { get; }
        long ProgressBarValue { get; }
        string Status { get; set; }
        string TaskName { get; }
        string TimeLeft { get; }
        string Title { get; set; }
        string TitleWithStatus { get; }
        string CancelText { get; }
    }
}
