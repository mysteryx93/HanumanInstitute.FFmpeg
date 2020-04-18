using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using GalaSoft.MvvmLight.Threading;

namespace HanumanInstitute.FFmpegExampleApplication
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        static App()
        {
            DispatcherHelper.Initialize();
        }
    }
}
