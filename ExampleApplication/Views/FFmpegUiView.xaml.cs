using System;
using System.Windows;
using HanumanInstitute.FFmpegExampleApplication.ViewModels;

namespace HanumanInstitute.FFmpegExampleApplication.Views {
    /// <summary>
    /// Interaction logic for FFmpegWindow.xaml
    /// </summary>
    public partial class FFmpegUiView : Window {
        public FFmpegUiView() {
            InitializeComponent();
            this.HandleRequestClose();
        }
    }
}
