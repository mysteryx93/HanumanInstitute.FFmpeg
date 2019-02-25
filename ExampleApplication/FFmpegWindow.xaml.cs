using System;
using System.Windows;
using System.Windows.Media;
using EmergenceGuardian.Encoder;

namespace EmergenceGuardian.EncoderExampleApplication {
    /// <summary>
    /// Interaction logic for FFmpegWindow.xaml
    /// </summary>
    public partial class FFmpegWindow : Window, IUserInterfaceWindow {
        public static FFmpegWindow Instance(Window parent, string title, bool autoClose) {
            FFmpegWindow F = new FFmpegWindow {
                Owner = parent,
                title = title,
                autoClose = autoClose
            };
            F.Show();
            return F;
        }

        protected IProcessWorker host;
        protected IProcessWorkerEncoder hostFFmpeg;
        protected IProcessWorker task;
        protected bool autoClose;
        protected string title { get; set; }
        protected ITimeLeftCalculator timeCalc;

        public void Stop() => Dispatcher.Invoke(() => this.Close());

        public FFmpegWindow() {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            SetPageTitle(null);
        }

        public void DisplayTask(IProcessWorker taskArg) {
            Dispatcher.Invoke(() => {
                if (taskArg.Options.IsMainTask) {
                    host = taskArg;
                    hostFFmpeg = host as IProcessWorkerEncoder;
                    if (hostFFmpeg != null) {
                        hostFFmpeg.FileInfoUpdated += FFmpeg_InfoUpdated;
                        hostFFmpeg.ProgressReceived += FFmpeg_StatusUpdated;
                    }
                    host.ProcessCompleted += FFmpeg_Completed;
                    PercentText.Text = 0.ToString("p1");
                    SetPageTitle(PercentText.Text);
                } else {
                    task = taskArg;
                    TaskStatusText.Text = task.Options.Title;
                    task.ProcessCompleted += (sender, e) => {
                        ProcessWorker Proc = (ProcessWorker)sender;
                        Dispatcher.Invoke(() => {
                            if (e.Status == CompletionStatus.Failed && !Proc.WorkProcess.StartInfo.FileName.EndsWith("avs2pipemod.exe"))
                                FFmpegErrorWindow.Instance(Owner, Proc);
                            TaskStatusText.Text = "";
                            task = null;
                            if (autoClose)
                                this.Close();
                        });
                    };
                }
            });
        }

        protected long ResumePos => hostFFmpeg.Options?.ResumePos ?? 0;

        private void SetPageTitle(string status) {
            this.Title = string.IsNullOrEmpty(status) ? title : string.Format("{0} ({1})", title, status);
        }

        private void FFmpeg_InfoUpdated(object sender, EventArgs e) {
            Dispatcher.Invoke(() => {
                var FileInfo = hostFFmpeg.FileInfo as IFileInfoFFmpeg;
                WorkProgressBar.Maximum = FileInfo.FrameCount + ResumePos;
                timeCalc = new TimeLeftCalculator(FileInfo.FrameCount + hostFFmpeg?.Options.ResumePos ?? 0);
            });
        }

        private bool EstimatedTimeLeftToggle = false;
        private void FFmpeg_StatusUpdated(object sender, Encoder.ProgressReceivedEventArgs e) {
            Dispatcher.Invoke(() => {
                ProgressStatusFFmpeg Status = e.Progress as ProgressStatusFFmpeg;
                WorkProgressBar.Value = Status.Frame + ResumePos;
                PercentText.Text = (WorkProgressBar.Value / WorkProgressBar.Maximum).ToString("p1");
                SetPageTitle(PercentText.Text);
                FpsText.Text = Status.Fps.ToString();

                // Time left will be updated only 1 out of 2 to prevent changing too quick.
                EstimatedTimeLeftToggle = !EstimatedTimeLeftToggle;
                if (EstimatedTimeLeftToggle) {
                    timeCalc?.Calculate(Status.Frame + ResumePos);
                    TimeSpan TimeLeft = timeCalc.ResultTimeLeft;
                    if (TimeLeft > TimeSpan.Zero)
                        TimeLeftText.Text = TimeLeft.ToString(TimeLeft.TotalHours < 1 ? "m\\:ss" : "h\\:mm\\:ss");
                }
            });
        }

        private void FFmpeg_Completed(object sender, ProcessCompletedEventArgs e) {
            Dispatcher.Invoke(() => {
                ProcessWorker Proc = sender as ProcessWorker;
                if (e.Status == CompletionStatus.Failed && !Proc.WorkProcess.StartInfo.FileName.EndsWith("avs2pipemod.exe"))
                    FFmpegErrorWindow.Instance(Owner, Proc);
                if (autoClose)
                    this.Close();
            });
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            host?.Cancel();
            task?.Cancel();
        }
    }
}
