using System;
using System.IO;
using System.Windows;
using System.Threading.Tasks;
using EmergenceGuardian.Encoder;

namespace EmergenceGuardian.EncoderExampleApplication {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private IUserInterfaceManager ffmpegManagerUI;
        private IProcessWorkerFactory factory;
        private IMediaEncoder encoder;
        private IMediaMuxer muxer;

        public MainWindow() {
            InitializeComponent();
            ffmpegManagerUI = new FFmpegUserInterfaceManager(this);
            factory = new ProcessWorkerFactory(new MediaConfig() {
                FFmpegPath = Properties.Settings.Default.FFmpegPath,
                UserInterfaceManager = ffmpegManagerUI
            });
            encoder = new MediaEncoder(factory);
            muxer = new MediaMuxer(factory);
        }

        private void BrowseSource_Click(object sender, RoutedEventArgs e) {
            string Result = ShowFileDialog(SourceDirectory, null);
            if (Result != null)
                SourceTextBox.Text = Result;
        }

        private void BrowseDestination_Click(object sender, RoutedEventArgs e) {
            string Result = ShowSaveFileDialog(DestinationDirectory, "MP4 Files|*.mp4");
            if (Result != null)
                DestinationTextBox.Text = Result;
        }

        private string SourceDirectory {
            get {
                try {
                    return Path.GetDirectoryName(SourceTextBox.Text);
                } catch {
                    return null;
                }
            }
        }

        private string DestinationDirectory {
            get {
                try {
                    return Path.GetDirectoryName(DestinationTextBox.Text);
                } catch {
                    return null;
                }
            }
        }

        public string ShowFileDialog(string defaultPath, string filter) {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            try {
                if (!string.IsNullOrEmpty(defaultPath))
                    dlg.InitialDirectory = Path.GetDirectoryName(defaultPath);
            } catch { }
            dlg.Filter = filter;
            if (dlg.ShowDialog().Value == true) {
                return dlg.FileName;
            } else
                return null;
        }

        public static string ShowSaveFileDialog(string defaultPath, string filter) {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            try {
                if (!string.IsNullOrEmpty(defaultPath))
                    dlg.InitialDirectory = Path.GetDirectoryName(defaultPath);
            } catch { }
            dlg.Filter = filter;
            if (dlg.ShowDialog().Value == true) {
                return dlg.FileName;
            } else
                return null;
        }

        private bool Validate() {
            return !string.IsNullOrEmpty(SourceTextBox.Text) && File.Exists(SourceTextBox.Text) && !string.IsNullOrEmpty(DestinationTextBox.Text) && SourceDirectory != null && DestinationDirectory != null;
        }

        private async void RunSimpleButton_Click(object sender, RoutedEventArgs e) {
            if (Validate()) {
                ProcessOptionsEncoder Options = new ProcessOptionsEncoder(ProcessDisplayMode.Interface, "Encoding to H264/AAC (Simple)");
                string Src = SourceTextBox.Text;
                string Dst = DestinationTextBox.Text;
                await Task.Run(() => {
                    encoder.EncodeFFmpeg(Src, Dst, "h264", "aac", null, Options);
                });
            }
        }

        private static int jobId = 0;
        private async void RunComplexButton_Click(object sender, RoutedEventArgs e) {
            if (Validate()) {
                string Src = SourceTextBox.Text;
                string Dst = DestinationTextBox.Text;
                CompletionStatus Result = await Task.Run(() => ExecuteComplex(Src, Dst));
                MessageBox.Show(Result.ToString(), "Encoding Result");
            }
        }

        private CompletionStatus ExecuteComplex(string src, string dst) {
            string DstEncode = GetPathWithoutExtension(dst) + "_.mp4";
            string DstExtract = GetPathWithoutExtension(dst) + "_.mkv";
            string DstAac = GetPathWithoutExtension(dst) + "_.aac";
            jobId++;
            CompletionStatus Result;

            ffmpegManagerUI.Start(jobId, "Encoding to H264/AAC (Complex)");

            ProcessOptionsEncoder OptionsMain = new ProcessOptionsEncoder(jobId, "", true);
            IProcessWorker ProcessMain = null;
            Task<CompletionStatus> TaskMain = Task.Run(() => encoder.EncodeFFmpeg(src, DstEncode, "h264", null, "", OptionsMain));

            ProcessOptionsEncoder Options = new ProcessOptionsEncoder(jobId, "Extracting Audio", false);
            Result = muxer.ExtractAudio(src, DstExtract, Options);
            if (Result == CompletionStatus.Success) {
                Options.Title = "Encoding Audio";
                Result = encoder.EncodeFFmpeg(DstExtract, DstAac, null, "aac", null, Options,
                    (s, p) => ProcessMain = p.ProcessWorker);
            }

            if (Result != CompletionStatus.Success)
                ProcessMain?.Cancel();

            TaskMain.Wait();
            CompletionStatus Result2 = TaskMain.Result;

            if (Result == CompletionStatus.Success && Result2 == CompletionStatus.Success) {
                Options.Title = "Muxing Audio and Video";
                Result = muxer.Muxe(DstEncode, DstAac, dst, Options);
            }

            File.Delete(DstEncode);
            File.Delete(DstExtract);
            File.Delete(DstAac);
            ffmpegManagerUI.Stop(jobId);
            return Result;
        }

        public static string GetPathWithoutExtension(string path) {
            int Pos = path.LastIndexOf('.');
            return Pos == -1 ? path : path.Substring(0, Pos);
        }
    }
}
