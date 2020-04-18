using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace HanumanInstitute.FFmpegExampleApplication.ViewModels
{
    public static class CommandHelper
    {
        /// <summary>
        /// Creates a new DelegateCommand the first time it is called and then re-use that command.
        /// </summary>
        /// <param name="cmd">The reference of the command object to initialize.</param>
        /// <param name="execute">The method this command will execute.</param>
        /// <param name="canExecute">The method returning whether command can execute.</param>
        /// <returns>The initialized command object.</returns>
        public static RelayCommand InitCommand(this ViewModelBase vm, ref RelayCommand cmd, Action execute, Func<bool> canExecute, bool keepTargetAlive = false)
        {
            return cmd ?? (cmd = new RelayCommand(execute, canExecute, keepTargetAlive));
        }

        public static RelayCommand<T> InitCommand<T>(this ViewModelBase vm, ref RelayCommand<T> cmd, Action<T> execute, Func<T, bool> canExecute, bool keepTargetAlive = false)
        {
            return cmd ?? (cmd = new RelayCommand<T>(execute, canExecute, keepTargetAlive));
        }
    }
}
