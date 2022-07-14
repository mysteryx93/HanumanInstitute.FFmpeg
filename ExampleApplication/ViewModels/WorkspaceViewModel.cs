using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace HanumanInstitute.FFmpegExampleApplication.ViewModels;

public class WorkspaceViewModel : ReactiveObject, IWorkspaceViewModel
{
    public WorkspaceViewModel() : this(string.Empty, true) { }

    [Reactive] public bool CanClose { get; set; }

    public WorkspaceViewModel(string displayName, bool canClose)
    {
        DisplayName = displayName;
        CanClose = canClose;
    }

    public event EventHandler? RequestClose;

    public ICommand CloseCommand => _close ??= ReactiveCommand.Create(CloseImpl, 
        this.WhenAnyValue(x => x.CanClose));
    private ICommand? _close;
    private void CloseImpl()
    {
        if (OnClosing())
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }

    public string DisplayName { get; set; }

    public bool? DialogResult { get; } = true;

    public virtual bool OnClosing() => CanClose;

}
