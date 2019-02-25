using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using EmergenceGuardian.Encoder.Services;

namespace EmergenceGuardian.Encoder {

    #region Interface

    /// <summary>
    /// Executes an application and manages its process.
    /// </summary>
    public interface IProcessWorker {
        /// <summary>
        /// Gets or sets the configuration settings.
        /// </summary>
        IMediaConfig Config { get; set; }
        /// <summary>
        /// Gets or sets the options to control the behaviors of the process.
        /// </summary>
        ProcessOptions Options { get; set; }
        /// <summary>
        /// Gets or sets the console output to read.
        /// </summary>
        ProcessOutput OutputType { get; set; }
        /// <summary>
        /// Gets the process currently being executed.
        /// </summary>
        IProcess WorkProcess { get; }
        /// <summary>
        /// Gets or sets a method to call after the process has been started.
        /// </summary>
        event ProcessStartedEventHandler ProcessStarted;
        /// <summary>
        /// Occurs when the process writes to its output stream.
        /// </summary>
        event DataReceivedEventHandler DataReceived;
        /// <summary>
        /// Occurs when the process has terminated its work.
        /// </summary>
        event ProcessCompletedEventHandler ProcessCompleted;
        /// <summary>
        /// Returns the raw console output.
        /// </summary>
        string Output { get; }

        /// <summary>
        /// Returns the CompletionStatus of the last operation.
        /// </summary>
        CompletionStatus LastCompletionStatus { get; }
        /// <summary>
        /// Runs the command as 'cmd /c', allowing the use of command line features such as piping.
        /// </summary>
        /// <param name="cmd">The full command to be executed with arguments.</param>
        /// <returns>The process completion status.</returns>
        CompletionStatus RunAsCommand(string cmd);
        /// <summary>
        /// Runs specified process with specified arguments.
        /// </summary>
        /// <param name="fileName">The process to start.</param>
        /// <param name="arguments">The set of arguments to use when starting the process.</param>
        /// <returns>The process completion status.</returns>
        /// <exception cref="System.IO.FileNotFoundException">Occurs when the file to run is not found.</exception>
        /// <exception cref="InvalidOperationException">Occurs when this class instance is already running another process.</exception>
        CompletionStatus Run(string fileName, string arguments);
        /// <summary>
        /// Cancels the currently running job and terminate its process.
        /// </summary>
        void Cancel();
        /// <summary>
        /// Returns the full command with arguments being run.
        /// </summary>
        string CommandWithArgs { get; }
    }

    #endregion

    /// <summary>
    /// Executes an application and manages its process.
    /// </summary>
    public class ProcessWorker : IProcessWorker {

        #region Declarations / Constructors

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

        protected StringBuilder output = new StringBuilder();
        protected CancellationTokenSource cancelWork;
        protected readonly IProcessFactory factory;
        protected readonly IFileSystemService fileSystem;
        protected readonly object lockToken = new object();

        public ProcessWorker() : this(new MediaConfig(), new ProcessFactory(), new FileSystemService(), null) { }

        public ProcessWorker(IMediaConfig config, IProcessFactory processFactory, IFileSystemService fileSystemService, ProcessOptions options = null) {
            this.Config = config ?? throw new ArgumentNullException(nameof(config));
            this.factory = processFactory ?? throw new ArgumentNullException(nameof(processFactory));
            this.fileSystem = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystem));
            this.Options = options ?? new ProcessOptions();
        }

        #endregion

        /// <summary>
        /// Runs the command as 'cmd /c', allowing the use of command line features such as piping.
        /// </summary>
        /// <param name="cmd">The full command to be executed with arguments.</param>
        /// <param name="encoder">The application being run, which alters parsing.</param>
        /// <returns>The process completion status.</returns>
        public CompletionStatus RunAsCommand(string cmd) {
            if (string.IsNullOrEmpty(cmd))
                throw new ArgumentException("Cmd cannot be null or empty.", nameof(cmd));
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
        public virtual CompletionStatus Run(string fileName, string arguments) {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("Filename cannot be null or empty.", nameof(fileName));
            IProcess P;
            lock (lockToken) {
                if (WorkProcess != null)
                    throw new InvalidOperationException("This instance of ProcessWorker is busy. You can run concurrent commands by creating other class instances.");
                P = factory.Create();
                WorkProcess = P;
            }
            output.Clear();
            cancelWork = new CancellationTokenSource();
            if (Options == null)
                Options = new ProcessOptions();

            P.StartInfo.FileName = fileName;
            P.StartInfo.Arguments = arguments;
            CommandWithArgs = $@"""{fileName}"" {arguments}".TrimEnd();

            if (OutputType == ProcessOutput.Output)
                P.OutputDataReceived += OnDataReceived;
            else if (OutputType == ProcessOutput.Error)
                P.ErrorDataReceived += OnDataReceived;

            if (Options.DisplayMode != ProcessDisplayMode.Native) {
                if (Options.DisplayMode == ProcessDisplayMode.Interface && Config.UserInterfaceManager != null)
                    Config.UserInterfaceManager.Display(this);
                P.StartInfo.CreateNoWindow = true;
                P.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                if (OutputType == ProcessOutput.Output)
                    P.StartInfo.RedirectStandardOutput = true;
                else if (OutputType == ProcessOutput.Error)
                    P.StartInfo.RedirectStandardError = true;
                P.StartInfo.UseShellExecute = false;
            }

            ProcessStarted?.Invoke(this, new ProcessStartedEventArgs(this));

            P.Start();
            try {
                if (!P.HasExited)
                    P.PriorityClass = Options.Priority;
            } catch { }
            if (Options.DisplayMode != ProcessDisplayMode.Native) {
                if (OutputType == ProcessOutput.Output)
                    P.BeginOutputReadLine();
                else if (OutputType == ProcessOutput.Error)
                    P.BeginErrorReadLine();
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
                Config.UserInterfaceManager?.DisplayError(this);

            WorkProcess = null;
            return Result;
        }

        /// <summary>
        /// Waits for the process to terminate while allowing the cancellation token to kill the process.
        /// </summary>
        /// <returns>Whether a timeout occured.</returns>
        private bool Wait() {
            DateTime StartTime = DateTime.Now;
            while (!WorkProcess.HasExited) {
                if (cancelWork.Token.IsCancellationRequested && !WorkProcess.HasExited)
                    Config.SoftKill(WorkProcess);
                if (Options.Timeout > TimeSpan.Zero && DateTime.Now - StartTime > Options.Timeout) {
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
        public void Cancel() => cancelWork?.Cancel();

        /// <summary>
        /// Occurs when data is received from the executing process.
        /// </summary>
        protected virtual void OnDataReceived(object sender, DataReceivedEventArgs e) {
            output.AppendLine(e.Data);
            DataReceived?.Invoke(this, e);
        }
    }
}
