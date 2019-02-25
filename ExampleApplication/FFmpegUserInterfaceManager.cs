using System;
using System.Windows;
using EmergenceGuardian.Encoder;

namespace EmergenceGuardian.EncoderExampleApplication {
    public class FFmpegUserInterfaceManager : UserInterfaceManagerBase {
        private Window parent;

        public FFmpegUserInterfaceManager(Window parent) {
            this.parent = parent;
        }

        public override IUserInterfaceWindow CreateUI(string title, bool autoClose) {
            return Application.Current.Dispatcher.Invoke(() => FFmpegWindow.Instance(parent, title, autoClose));
        }

        public override void DisplayError(IProcessWorker host) {
            Application.Current.Dispatcher.Invoke(() => FFmpegErrorWindow.Instance(parent, host));
        }
    }
}