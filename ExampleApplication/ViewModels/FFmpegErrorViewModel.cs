using System;
using System.ComponentModel;
using HanumanInstitute.FFmpeg;
using MvvmDialogs;

namespace HanumanInstitute.FFmpegExampleApplication.ViewModels
{
    public interface IFFmpegErrorViewModel : IWorkspaceViewModel
    {
        string Title { get; }
        string OutputText { get; }
    }

    public class FFmpegErrorViewModel : WorkspaceViewModel, IFFmpegErrorViewModel
    {
        public FFmpegErrorViewModel(IProcessWorker host)
        {
            if (host == null) { throw new ArgumentNullException(nameof(host)); }

            Title = (host.LastCompletionStatus == CompletionStatus.Timeout ? "Timeout: " : "Failed: ") + host.Options.Title;
            OutputText = host.CommandWithArgs + Environment.NewLine + Environment.NewLine + host.Output;
        }

        public string Title { get; private set; }

        public string OutputText { get; private set; }
    }
}
