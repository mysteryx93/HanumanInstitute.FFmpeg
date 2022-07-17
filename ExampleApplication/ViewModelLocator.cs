using HanumanInstitute.MvvmDialogs.Avalonia;
using Microsoft.Extensions.Logging;
using Splat;

namespace HanumanInstitute.FFmpegExampleApplication;

/// <summary>
/// This class contains static references to all the view models in the
/// application and provides an entry point for the bindings.
/// </summary>
public static class ViewModelLocator
{
    /// <summary>
    /// Initializes a new instance of the ViewModelLocator class.
    /// </summary>
    static ViewModelLocator()
    {
        var container = Locator.CurrentMutable;
            
        // Services
        var loggerFactory = LoggerFactory.Create(builder => builder.AddFilter(logLevel => true).AddDebug());

        // DialogService
        container.RegisterLazySingleton<IDialogService>(() => new DialogService(new DialogManager(
                viewLocator: new ViewLocator(),
                dialogFactory: new DialogFactory().AddMessageBox(),
                logger: loggerFactory.CreateLogger<DialogManager>()),
            viewModelFactory: t => Locator.Current.GetService(t)));

        // FFmpeg
        SplatRegistrations.RegisterLazySingleton<IUserInterfaceManager, FFmpegUserInterfaceManager>();
        SplatRegistrations.RegisterLazySingleton<IProcessManager, ProcessManager>();
        container.RegisterLazySingleton<IEncoderService>(() =>
            new EncoderService(Locator.Current.GetService<IProcessManager>(), Locator.Current.GetService<IUserInterfaceManager>()));
            
        // ViewModels
        SplatRegistrations.Register<MainViewModel>();
        SplatRegistrations.Register<FFmpegErrorViewModel>();
        SplatRegistrations.Register<FFmpegUiViewModel>();
            
        SplatRegistrations.SetupIOC();
    }

    public static MainViewModel Main => Locator.Current.GetService<MainViewModel>()!;
    public static FFmpegErrorViewModel FFmpegError => Locator.Current.GetService<FFmpegErrorViewModel>()!;
    public static FFmpegUiViewModel FFmpegUi => Locator.Current.GetService<FFmpegUiViewModel>()!;

    public static void Cleanup()
    {
    }
}
