using HanumanInstitute.MvvmDialogs.FrameworkDialogs;

namespace HanumanInstitute.FFmpegExampleApplication.Business;

/// <summary>
/// A global error handler that displays a message box when unhandled errors occur.
/// Registered via ReactiveUIBuilder.WithExceptionHandler in Program.Main.
/// </summary>
public class GlobalErrorHandler : IObserver<Exception>
{
    private IDialogService? _dialogService;

    public static GlobalErrorHandler Instance => _instance ??= new GlobalErrorHandler();
    private static GlobalErrorHandler? _instance;

    /// <summary>
    /// Sets the dependencies for handling and displaying errors.
    /// </summary>
    /// <param name="dialogService">The service used to display dialogs.</param>
    /// <param name="ownerVm">The ViewModel of the View in which to display error messages.</param>
    public static void EndInit(IDialogService dialogService, INotifyPropertyChanged? ownerVm)
    {
        Instance._dialogService = dialogService;
        Instance.OwnerVm = ownerVm;
    }

    /// <summary>
    /// Gets or sets the ViewModel of the View in which to display error messages.
    /// </summary>
    public INotifyPropertyChanged? OwnerVm { get; set; }
    
    /// <inheritdoc />
    public void OnNext(Exception error) => ShowError(error);

    /// <inheritdoc />
    public void OnError(Exception error)
    {
    }

    /// <inheritdoc />
    public void OnCompleted()
    {
    }

    private async void ShowError(Exception error)
    {
        if (_dialogService != null && OwnerVm != null)
        {
            await _dialogService.ShowMessageBoxAsync(OwnerVm!, error.ToString(), "Application Error", MessageBoxButton.Ok, MessageBoxImage.Error);
        }
    }
}
