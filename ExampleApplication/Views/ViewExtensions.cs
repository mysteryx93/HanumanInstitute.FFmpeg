using System;
using System.Windows;
using GalaSoft.MvvmLight.Threading;
using HanumanInstitute.FFmpegExampleApplication.ViewModels;

namespace HanumanInstitute.FFmpegExampleApplication.Views
{
    public static class ViewExtensions
    {
        /// <summary>
        /// Listens to the IWorkspaceViewModel.RequestClose event to close the window.
        /// </summary>
        /// <param name="window">The window to close on demand.</param>
        public static void HandleRequestClose(this Window window)
        {
            if (window == null) { throw new ArgumentNullException(nameof(window)); }

            window.SourceInitialized += (s1, e1) =>
            {
                if (window?.DataContext is IWorkspaceViewModel viewModel)
                {
                    viewModel.RequestClose += (s2, e2) => DispatcherHelper.CheckBeginInvokeOnUI(() => window.Close());
                    window.Closing += (s3, e3) => viewModel.OnClosing();
                }
                else
                {
                    throw new InvalidOperationException("Window.DataContext must be of type IWorkspaceViewModel to use HandleRequestClose.");
                }
            };
        }
    }
}
