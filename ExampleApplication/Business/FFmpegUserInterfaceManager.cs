using Avalonia.Threading;

namespace HanumanInstitute.FFmpegExampleApplication.Business;

public class FFmpegUserInterfaceManager : UserInterfaceManagerBase
{
    private readonly IDialogService _dialogService;

    public FFmpegUserInterfaceManager(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    public override IUserInterfaceWindow CreateUI(object? owner, string title, bool autoClose)
    {
        var ui = _dialogService.CreateViewModel<FFmpegUiViewModel>();
        ui.Title = title;
        ui.AutoClose = autoClose;

        Dispatcher.UIThread.InvokeAsync(() => 
            _dialogService.ShowDialogAsync((INotifyPropertyChanged)owner!, ui));
        return ui;
    }

    public override void DisplayError(object? owner, IProcessWorker host)
    {
        var ui = _dialogService.CreateViewModel<FFmpegErrorViewModel>();
        ui.Process = host;
        
        Dispatcher.UIThread.InvokeAsync(() => 
            _dialogService.ShowDialogAsync((INotifyPropertyChanged)owner!, ui));
    }
}
