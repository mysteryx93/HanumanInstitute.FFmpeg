using System;
using System.Windows;
using HanumanInstitute.FFmpegExampleApplication.ViewModels;

namespace HanumanInstitute.FFmpegExampleApplication.Views {
    /// <summary>
    /// Interaction logic for ErrorWindow.xaml
    /// </summary>
    public partial class FFmpegErrorView : Window {

        public FFmpegErrorView() {
            InitializeComponent();
            this.HandleRequestClose();
        }
    }
}
