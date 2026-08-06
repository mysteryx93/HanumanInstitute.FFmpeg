using Avalonia;
using ReactiveUI.Avalonia;
using ReactiveUI.Builder;

namespace HanumanInstitute.FFmpegExampleApplication;

internal static class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        RxAppBuilder.CreateReactiveUIBuilder()
            .WithAvalonia()
            .WithCoreServices()
            .BuildApp();

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI(x =>
            {
                x.WithExceptionHandler(Business.GlobalErrorHandler.Instance);
            });
}
