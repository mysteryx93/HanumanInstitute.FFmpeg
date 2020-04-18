using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;


namespace HanumanInstitute.FFmpegExampleApplication.ViewModels
{
    public class WorkspaceViewModel : ViewModelBase, IWorkspaceViewModel
    {
        public WorkspaceViewModel() { }

        public WorkspaceViewModel(string displayName, bool canClose)
        {
            DisplayName = displayName;
            CanClose = canClose;
        }

        public event EventHandler RequestClose;

        private RelayCommand _closeCommand;
        public RelayCommand CloseCommand => this.InitCommand(ref _closeCommand, OnRequestClose, () => CanClose, true);

        public string DisplayName { get; set; }

        public bool CanClose { get; set; } = true;

        public bool? DialogResult { get; set; }

        public virtual bool OnClosing() => CanClose;

        public void OnRequestClose()
        {
            if (OnClosing())
            {
                RequestClose?.Invoke(this, new EventArgs());
            }
        }
    }
}
