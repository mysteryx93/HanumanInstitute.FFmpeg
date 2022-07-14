
namespace HanumanInstitute.FFmpegExampleApplication.ViewModels;

// ReSharper disable once InconsistentNaming
public class FFmpegErrorViewModel : WorkspaceViewModel
{
    public IProcessWorker? Process { get; set; }

    public string Title => Process != null ?
        (Process.LastCompletionStatus == CompletionStatus.Timeout ? "Timeout: " : "Failed: ") + Process.Options.Title :
        string.Empty;

    public string OutputText => Process != null ?
        Process.CommandWithArgs + Environment.NewLine + Environment.NewLine + Process.Output :
        string.Empty;
}
