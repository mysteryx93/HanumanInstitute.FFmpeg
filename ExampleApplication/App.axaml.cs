using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using HanumanInstitute.FFmpegExampleApplication.Views;
using Splat;

namespace HanumanInstitute.FFmpegExampleApplication;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            GlobalErrorHandler.BeginInit(); // Must be set before any ICommand is created.
            
            desktop.MainWindow = new MainView
            {
                DataContext = ViewModelLocator.Main
            };
            
            GlobalErrorHandler.EndInit(Locator.Current.GetService<IDialogService>()!, desktop?.MainWindow.DataContext as INotifyPropertyChanged);
        }

        base.OnFrameworkInitializationCompleted();
    }
}
