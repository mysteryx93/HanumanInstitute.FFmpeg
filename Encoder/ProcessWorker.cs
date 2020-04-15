using HanumanInstitute.Encoder.Properties;
using HanumanInstitute.Encoder.Services;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace HanumanInstitute.Encoder
{
    /// <summary>
    /// Executes an application and manages its process.
    /// </summary>
    public class ProcessWorker : IProcessWorker, IDisposable
    {
        /// <summary>
        /// Gets or sets the configuration settings.
        /// </summary>
        public IMediaConfig Config { get; set; }
        /// <summary>
        /// Gets or sets the options to control the behaviors of the process.
        /// </summary>
        public ProcessOptions Options { get; set; }
        /// <summary>
        /// Gets or sets the console output to read.
        /// </summary>
        public ProcessOutput OutputType { get; set; }
        /// <summary>
        /// Gets the process currently being executed.
        /// </summary>
        public IProcess WorkProcess { get; private set; }
        /// <summary>
        /// Gets or sets a method to call after the process has been started.
        /// </summary>
        public event ProcessStartedEventHandler ProcessStarted;
        /// <summary>
        /// Occurs when the process writes to its output stream.
        /// </summary>
        public event DataReceivedEventHandler DataReceived;
        /// <summary>
        /// Occurs when the process has terminated its work.
        /// </summary>
        public event ProcessCompletedEventHandler ProcessCompleted;
        /// <summary>
        /// Returns the raw console output.
        /// </summary>
        public string Output => output.ToString();
        /// <summary>
        /// Returns the CompletionStatus of the last operation.
        /// </summary>
        public CompletionStatus LastCompletionStatus { get; private set; }
        /// <summary>
        /// Returns the full command with arguments being run.
        /// </summary>
        public string CommandWithArgs { get; private set; }

        private readonly StringBuilder output = new StringBuilder();
        private CancellationTokenSource cancelWork;
        private readonly IProcessFactory factory;
        protected object LockToken { get; private set; } = new object();

        public ProcessWorker() : this(new MediaConfig(), new ProcessFactory(), null) { }

        public ProcessWorker(IMediaConfig config, IProcessFactory processFactory, ProcessOptions options = null)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            factory = processFactory ?? throw new ArgumentNullException(nameof(processFactory));
            Options = options ?? new ProcessOptions();
        }

        /// <summary>
        /// Runs the command as 'cmd /c', allowing the use of command line features such as piping.
        /// </summary>
        /// <param name="cmd">The full command to be executed with arguments.</param>
        /// <param name="encoder">The application being run, which alters parsing.</param>
        /// <returns>The process completion status.</returns>
        public CompletionStatus RunAsCommand(string cmd)
        {
            ArgHelper.ValidateNotNullOrEmpty(cmd, nameof(cmd));
            return Run("cmd", $@"/c "" {cmd} """);
        }

        /// <summary>
        /// Runs specified process with specified arguments.
        /// </summary>
        /// <param name="fileName">The process to start.</param>
        /// <param name="arguments">The set of arguments to use when starting the process.</param>
        /// <returns>The process completion status.</returns>
        /// <exception cref="System.IO.FileNotFoundException">Occurs when the file to run is not found.</exception>
        /// <exception cref="InvalidOperationException">Occurs when this class instance is already running another process.</exception>
        public virtual CompletionStatus Run(string fileName, string arguments)
        {
            ArgHelper.ValidateNotNullOrEmpty(fileName, nameof(fileName));

            IProcess P;
            lock (LockToken)
            {
                if (WorkProcess != null) { throw new InvalidOperationException(Resources.ProcessWorkerBusy); }
                P = factory.Create();
                WorkProcess = P;
            }
            output.Clear();
            cancelWork = new CancellationTokenSource();
            if (Options == null)
            {
                Options = new ProcessOptions();
            }

            P.StartInfo.FileName = fileName;
            P.StartInfo.Arguments = arguments;
            CommandWithArgs = $@"""{fileName}"" {arguments}".TrimEnd();

            if (OutputType == ProcessOutput.Output)
            {
                P.OutputDataReceived += OnDataReceived;
            }
            else if (OutputType == ProcessOutput.Error)
            {
                P.ErrorDataReceived += OnDataReceived;
            }

            if (Options.DisplayMode != ProcessDisplayMode.Native)
            {
                if (Options.DisplayMode == ProcessDisplayMode.Interface && Config.UserInterfaceManager != null)
                {
                    Config.UserInterfaceManager.Display(this);
                }

                P.StartInfo.CreateNoWindow = true;
                P.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                if (OutputType == ProcessOutput.Output)
                {
                    P.StartInfo.RedirectStandardOutput = true;
                }
                else if (OutputType == ProcessOutput.Error)
                {
                    P.StartInfo.RedirectStandardError = true;
                }

                P.StartInfo.UseShellExecute = false;
            }

            ProcessStarted?.Invoke(this, new ProcessStartedEventArgs(this));

            P.Start();
            try
            {
                if (!P.HasExited)
                {
                    P.PriorityClass = Options.Priority;
                }
            }
            catch (System.ComponentModel.Win32Exception) { }
            catch (InvalidOperationException) { }

            if (Options.DisplayMode != ProcessDisplayMode.Native)
            {
                if (OutputType == ProcessOutput.Output)
                {
                    P.BeginOutputReadLine();
                }
                else if (OutputType == ProcessOutput.Error)
                {
                    P.BeginErrorReadLine();
                }
            }

            bool Timeout = Wait();

            // ExitCode is 0 for normal exit. Different value when closing the console.
            CompletionStatus Result = Timeout ? CompletionStatus.Timeout : cancelWork.IsCancellationRequested ? CompletionStatus.Cancelled : P.ExitCode == 0 ? CompletionStatus.Success : CompletionStatus.Failed;

            cancelWork = null;
            // Allow changing CompletionStatus in ProcessCompleted.
            ProcessCompletedEventArgs CompletedArgs = new ProcessCompletedEventArgs(Result);
            ProcessCompleted?.Invoke(this, CompletedArgs);
            Result = CompletedArgs.Status;
            LastCompletionStatus = Result;
            if ((Result == CompletionStatus.Failed || Result == CompletionStatus.Timeout) && Options.DisplayMode == ProcessDisplayMode.ErrorOnly)
            {
                Config.UserInterfaceManager?.DisplayError(this);
            }

            WorkProcess = null;
            return Result;
        }

        /// <summary>
        /// Waits for the process to terminate while allowing the cancellation token to kill the process.
        /// </summary>
        /// <returns>Whether a timeout occured.</returns>
        private bool Wait()
        {
            DateTime StartTime = DateTime.Now;
            while (!WorkProcess.HasExited)
            {
                if (cancelWork.Token.IsCancellationRequested && !WorkProcess.HasExited)
                {
                    Config.SoftKill(WorkProcess);
                }

                if (Options.Timeout > TimeSpan.Zero && DateTime.Now - StartTime > Options.Timeout)
                {
                    Config.SoftKill(WorkProcess);
                    return true;
                }
                WorkProcess.WaitForExit(500);
            }
            WorkProcess.WaitForExit();
            return false;
        }

        /// <summary>
        /// Cancels the currently running job and terminate its process.
        /// </summary>
        public void Cancel()
        {
            cancelWork?.Cancel();
        }

        /// <summary>
        /// Occurs when data is received from the executing process.
        /// </summary>
        protected virtual void OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            output.AppendLine(e?.Data);
            DataReceived?.Invoke(this, e);
        }

        /// <summary>
        /// Throws an exception if a progress is already running.
        /// </summary>
        protected void EnsureNotRunning()
        {
            lock (LockToken)
            {
                if (WorkProcess != null)
                {
                    throw new InvalidOperationException(Resources.ProcessWorkerBusy);
                }
            }
        }


        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    cancelWork.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
