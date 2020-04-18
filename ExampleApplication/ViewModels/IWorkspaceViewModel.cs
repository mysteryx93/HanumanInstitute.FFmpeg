using System;
using GalaSoft.MvvmLight.CommandWpf;
using MvvmDialogs;

namespace HanumanInstitute.FFmpegExampleApplication.ViewModels
{
    public interface IWorkspaceViewModel : IModalDialogViewModel
    {
        event EventHandler RequestClose;
        RelayCommand CloseCommand { get; }
        string DisplayName { get; set; }
        bool CanClose { get; set; }
        bool OnClosing();
        void OnRequestClose();
    }
}
