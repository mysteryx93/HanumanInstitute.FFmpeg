namespace HanumanInstitute.FFmpegExampleApplication.ViewModels;

public interface IWorkspaceViewModel : IModalDialogViewModel, ICloseable
{
    bool CanClose { get; set; }
    ICommand CloseCommand { get; }
    string DisplayName { get; set; }
    bool OnClosing();
}
