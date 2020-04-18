using System;
using System.IO;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Threading;
using HanumanInstitute.FFmpeg;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using MvvmDialogs.FrameworkDialogs.SaveFile;

namespace HanumanInstitute.FFmpegExampleApplication.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly IUserInterfaceManager _uiManager;
        private readonly IMediaEncoder _encoder;
        private readonly IMediaMuxer _muxer;

        public MainViewModel(IDialogService dialogService, IUserInterfaceManager uiManager, IMediaEncoder encoder, IMediaMuxer muxer)
        {
            _dialogService = dialogService;
            _uiManager = uiManager;
            _encoder = encoder?.SetOwner(this);
            _muxer = muxer?.SetOwner(this);
            FFmpegPath = Properties.Settings.Default.FFmpegPath;
        }

        public string FFmpegPath { get; set; }

        public string SourcePath { get; set; } = @"E:\AVSMeter\zzzz.mp4";

        public string DestinationPath { get; set; } = @"E:\AVSMeter\_test.mp4";

        private RelayCommand _showOpenFileCommand;
        public RelayCommand ShowOpenFileCommand => this.InitCommand(ref _showOpenFileCommand, OnShowOpenFileCommand, () => true);

        private void OnShowOpenFileCommand()
        {
            var defaultPath = TryGetDirectory(SourcePath);
            var settings = new OpenFileDialogSettings()
            {
                InitialDirectory = !string.IsNullOrEmpty(defaultPath) ? Path.GetDirectoryName(defaultPath) : null
            };
            var result = _dialogService.ShowOpenFileDialog(this, settings);

            if (result == true)
            {
                SourcePath = settings.FileName;
            }
        }

        private RelayCommand _showSaveFileCommand;
        public RelayCommand ShowSaveFileCommand => this.InitCommand(ref _showSaveFileCommand, OnShowSaveFileCommand, () => true);

        private void OnShowSaveFileCommand()
        {
            var defaultPath = TryGetDirectory(DestinationPath);
            var settings = new SaveFileDialogSettings()
            {
                Filter = "MP4 Files|*.mp4",
                InitialDirectory = !string.IsNullOrEmpty(defaultPath) ? Path.GetDirectoryName(defaultPath) : null
            };
            var result = _dialogService.ShowSaveFileDialog(this, settings);

            if (result == true)
            {
                DestinationPath = settings.FileName;
            }
        }

        private static string TryGetDirectory(string path)
        {
            try
            {
                return Path.GetDirectoryName(path);
            }
            catch (ArgumentException) { }
            catch (PathTooLongException) { }
            return null;
        }

        private bool Validate()
        {
            return !string.IsNullOrEmpty(SourcePath) && File.Exists(SourcePath) && !string.IsNullOrEmpty(DestinationPath) && TryGetDirectory(SourcePath) != null && TryGetDirectory(DestinationPath) != null;
        }

        private RelayCommand _runSimpleTaskCommand;
        public RelayCommand RunSimpleTaskCommand => this.InitCommand(ref _runSimpleTaskCommand, OnRunSimpleTask, () => true);

        private async void OnRunSimpleTask()
        {
            if (Validate())
            {
                var options = new ProcessOptionsEncoder(ProcessDisplayMode.Interface, "Encoding to H264/AAC (Simple)");
                await Task.Run(() =>
                {
                    _encoder.EncodeFFmpeg(SourcePath, DestinationPath, "h264", "aac", null, options);
                }).ConfigureAwait(false);
            }
        }

        private RelayCommand _runComplexTaskCommand;
        public RelayCommand RunComplexTaskCommand => this.InitCommand(ref _runComplexTaskCommand, OnRunComplexTask, () => true);

        private static int s_jobId = 0;
        private async void OnRunComplexTask()
        {
            if (Validate())
            {
                var result = await Task.Run(() => ExecuteComplex(SourcePath, DestinationPath)).ConfigureAwait(false);
                DispatcherHelper.CheckBeginInvokeOnUI(() => _dialogService.ShowMessageBox(this, result.ToString(), "Encoding Result"));
            }
        }

        private CompletionStatus ExecuteComplex(string src, string dst)
        {
            var dstEncode = GetPathWithoutExtension(dst) + "_.mp4";
            var dstExtract = GetPathWithoutExtension(dst) + "_.mkv";
            var dstAac = GetPathWithoutExtension(dst) + "_.aac";
            s_jobId++;
            CompletionStatus result;

            _uiManager.Start(this, s_jobId, "Encoding to H264/AAC (Complex)");

            var optionsMain = new ProcessOptionsEncoder(s_jobId, "", true);
            IProcessWorker processMain = null;
            var taskMain = Task.Run(() => _encoder.EncodeFFmpeg(src, dstEncode, "h264", null, "", optionsMain));

            var options = new ProcessOptionsEncoder(s_jobId, "Extracting Audio", false);
            result = _muxer.ExtractAudio(src, dstExtract, options);
            if (result == CompletionStatus.Success)
            {
                options.Title = "Encoding Audio";
                result = _encoder.EncodeFFmpeg(dstExtract, dstAac, null, "aac", null, options,
                    (s, p) => processMain = p.ProcessWorker);
            }

            if (result != CompletionStatus.Success)
            {
                processMain?.Cancel();
            }

            taskMain.Wait();
            var result2 = taskMain.Result;

            if (result == CompletionStatus.Success && result2 == CompletionStatus.Success)
            {
                options.Title = "Muxing Audio and Video";
                result = _muxer.Muxe(dstEncode, dstAac, dst, options);
            }

            File.Delete(dstEncode);
            File.Delete(dstExtract);
            File.Delete(dstAac);
            _uiManager.Close(s_jobId);

            return GetStatus(result, result2);
        }

        private static CompletionStatus GetStatus(CompletionStatus status1, CompletionStatus status2)
        {
            return HasStatus(CompletionStatus.Timeout) ??
                HasStatus(CompletionStatus.Failed) ??
                HasStatus(CompletionStatus.Cancelled) ??
                HasStatus(CompletionStatus.Success) ??
                CompletionStatus.None;

            CompletionStatus? HasStatus(CompletionStatus result) => status1 == result || status2 == result ? result : (CompletionStatus?)null;
        }

        private static string GetPathWithoutExtension(string path)
        {
            var pos = path.LastIndexOf('.');
            return pos == -1 ? path : path.Substring(0, pos);
        }

    }
}
