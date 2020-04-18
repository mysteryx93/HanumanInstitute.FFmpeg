using System;
using System.ComponentModel;
using System.Windows;
using MvvmDialogs;
using HanumanInstitute.FFmpeg;
using GalaSoft.MvvmLight.Threading;

namespace HanumanInstitute.FFmpegExampleApplication.Business
{
    public class FFmpegUserInterfaceManager : UserInterfaceManagerBase
    {
        private IDialogService _dialogService;
        private IFFmpegUserInterfaceFactory _uiFactory;

        public FFmpegUserInterfaceManager(IFFmpegUserInterfaceFactory uiFactory, IDialogService dialogService)
        {
            _dialogService = dialogService;
            _uiFactory = uiFactory;
        }

        public override IUserInterfaceWindow CreateUI(object owner, string title, bool autoClose)
        {
            var ui = _uiFactory.CreateUI(title, autoClose);
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                _dialogService.ShowDialog(owner as INotifyPropertyChanged, ui);
            });
            return ui;
        }

        public override void DisplayError(object owner, IProcessWorker host)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                _dialogService.ShowDialog(owner as INotifyPropertyChanged, _uiFactory.CreateError(host));
            });
        }
    }
}
