using System;
using System.ComponentModel;
using System.Globalization;
using HanumanInstitute.FFmpeg;
using static System.FormattableString;

namespace HanumanInstitute.FFmpegExampleApplication.ViewModels
{
    public class FFmpegUiViewModel : WorkspaceViewModel, IFFmpegUiViewModel
    {
        public FFmpegUiViewModel() // IUserInterfaceManager uiManager
        {
        }

        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                SetTitleWithStatus();
            }
        }

        private string _status;
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                SetTitleWithStatus();
            }
        }

        public string TitleWithStatus { get; private set; }

        public bool AutoClose { get; set; }

        public long ProgressBarMax { get; private set; } = 100;

        public long ProgressBarValue { get; private set; } = 0;

        public string TaskName { get; private set; }

        public string Fps { get; private set; }

        public string TimeLeft { get; private set; }

        public string CancelText { get; private set; } = "_Cancel";

        private IProcessWorker _host;
        private IProcessWorkerEncoder _hostFFmpeg;
        private IProcessWorker _task;
        private ITimeLeftCalculator _timeCalc;

        public void DisplayTask(IProcessWorker taskArg)
        {
            if (taskArg == null) { throw new ArgumentNullException(nameof(taskArg)); }

            if (taskArg.Options.IsMainTask)
            {
                _host = taskArg;
                _hostFFmpeg = _host as IProcessWorkerEncoder;
                if (_hostFFmpeg != null)
                {
                    _hostFFmpeg.FileInfoUpdated += FFmpeg_InfoUpdated;
                    _hostFFmpeg.ProgressReceived += FFmpeg_StatusUpdated;
                }
                _host.ProcessCompleted += FFmpeg_Completed;
                Status = 0.ToString("p1", CultureInfo.InvariantCulture);
            }
            else
            {
                _task = taskArg;
                TaskName = _task.Options.Title;
                _task.ProcessCompleted += (sender, e) =>
                {
                    TaskName = "";
                    _task = null;
                    WorkCompleted();
                };
            }
        }

        protected long ResumePos => _hostFFmpeg.Options?.ResumePos ?? 0;

        private void SetTitleWithStatus()
        {
            TitleWithStatus = string.IsNullOrEmpty(Status) ? Title : Invariant($"{Title} ({Status})");
        }

        private void FFmpeg_InfoUpdated(object sender, EventArgs e)
        {
            var fileInfo = _hostFFmpeg.FileInfo as FileInfoFFmpeg;
            ProgressBarMax = fileInfo.FrameCount + ResumePos;
            _timeCalc = new TimeLeftCalculator(fileInfo.FrameCount + _hostFFmpeg?.Options.ResumePos ?? 0);
        }

        private bool _estimatedTimeLeftToggle = false;
        private void FFmpeg_StatusUpdated(object sender, ProgressReceivedEventArgs e)
        {
            var progress = e.Progress as ProgressStatusFFmpeg;
            ProgressBarValue = progress.Frame + ResumePos;
            Status = ((double)ProgressBarValue / ProgressBarMax).ToString("p1", CultureInfo.CurrentCulture);
            Fps = progress.Fps.ToString(CultureInfo.CurrentCulture);

            // Time left will be updated only 1 out of 2 to prevent changing too quick.
            _estimatedTimeLeftToggle = !_estimatedTimeLeftToggle;
            if (_estimatedTimeLeftToggle)
            {
                _timeCalc?.Calculate(progress.Frame + ResumePos);
                var resultTimeLeft = _timeCalc.ResultTimeLeft;
                if (resultTimeLeft > TimeSpan.Zero)
                {
                    TimeLeft = resultTimeLeft.ToString(resultTimeLeft.TotalHours < 1 ? "m\\:ss" : "h\\:mm\\:ss", CultureInfo.InvariantCulture);
                }
            }
        }

        private void FFmpeg_Completed(object sender, ProcessCompletedEventArgs e)
        {
            WorkCompleted();
        }

        private void WorkCompleted()
        {
            CancelText = "_Close";
            if (AutoClose)
            {
                Close();
            }
        }

        public void Close()
        {
            if (CloseCommand.CanExecute(null))
            {
                CloseCommand.Execute(null);
            }
        }

        public override bool OnClosing()
        {
            _host?.Cancel();
            _task?.Cancel();
            return base.OnClosing();
        }
    }
}
