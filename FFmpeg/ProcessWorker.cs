using System.Diagnostics;
using HanumanInstitute.FFmpeg.Properties;
using HanumanInstitute.FFmpeg.Services;

namespace HanumanInstitute.FFmpeg;

/// <summary>
/// Executes an application and manages its process.
/// </summary>
public class ProcessWorker : IProcessWorker, IDisposable
{
    /// <summary>
    /// Gets or sets the process manager.
    /// </summary>
    public IProcessManager Processes { get; set; }
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
    public IProcess? WorkProcess { get; private set; }
    /// <summary>
    /// Gets or sets a method to call after the process has been started.
    /// </summary>
    public event ProcessStartedEventHandler? ProcessStarted;
    /// <summary>
    /// Occurs when the process writes to its output stream.
    /// </summary>
    public event DataReceivedEventHandler? DataReceived;
    /// <summary>
    /// Occurs when the process has terminated its work.
    /// </summary>
    public event ProcessCompletedEventHandler? ProcessCompleted;
    /// <summary>
    /// Returns the raw console output.
    /// </summary>
    public string Output => _output.ToString();
    /// <summary>
    /// Returns the CompletionStatus of the last operation.
    /// </summary>
    public CompletionStatus? LastCompletionStatus { get; private set; }
    /// <summary>
    /// Returns the full command with arguments being run.
    /// </summary>
    public string CommandWithArgs { get; private set; } = string.Empty;

    private readonly StringBuilder _output = new StringBuilder();
    private readonly IProcessFactory _factory;
    protected object LockToken { get; private set; } = new object();
    private CancellationTokenSource? _cancelWork;

    //public ProcessWorker() : this(new MediaConfig(), new ProcessFactory(), null) { }

    internal ProcessWorker(IProcessManager config, IProcessFactory processFactory, ProcessOptions? options = null)
    {
        Processes = config ?? throw new ArgumentNullException(nameof(config));
        _factory = processFactory ?? throw new ArgumentNullException(nameof(processFactory));
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
        cmd.CheckNotNullOrEmpty(nameof(cmd));
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
        fileName.CheckNotNullOrEmpty(nameof(fileName));

        IProcess p;
        lock (LockToken)
        {
            if (WorkProcess != null) { throw new InvalidOperationException(Resources.ProcessWorkerBusy); }
            p = _factory.Create();
            WorkProcess = p;
        }
        _output.Clear();
        _cancelWork = new CancellationTokenSource();
        if (Options == null)
        {
            Options = new ProcessOptions();
        }

        p.StartInfo.FileName = fileName;
        p.StartInfo.Arguments = arguments;
        CommandWithArgs = $@"""{fileName}"" {arguments}".TrimEnd();

        if (OutputType == ProcessOutput.Output)
        {
            p.OutputDataReceived += OnDataReceived;
        }
        else if (OutputType == ProcessOutput.Error)
        {
            p.ErrorDataReceived += OnDataReceived;
        }

        if (Options.DisplayMode != ProcessDisplayMode.Native)
        {
            //if (Options.DisplayMode == ProcessDisplayMode.Interface && Config.UserInterfaceManager != null)
            //{
            //    Config.UserInterfaceManager.Display(Owner, this);
            //}

            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            if (OutputType == ProcessOutput.Output)
            {
                p.StartInfo.RedirectStandardOutput = true;
            }
            else if (OutputType == ProcessOutput.Error)
            {
                p.StartInfo.RedirectStandardError = true;
            }

            p.StartInfo.UseShellExecute = false;
        }

        ProcessStarted?.Invoke(this, new ProcessStartedEventArgs(this));

        p.Start();
        try
        {
            if (!p.HasExited)
            {
                p.PriorityClass = Options.Priority;
            }
        }
        catch (System.ComponentModel.Win32Exception) { }
        catch (InvalidOperationException) { }

        if (Options.DisplayMode != ProcessDisplayMode.Native)
        {
            if (OutputType == ProcessOutput.Output)
            {
                p.BeginOutputReadLine();
            }
            else if (OutputType == ProcessOutput.Error)
            {
                p.BeginErrorReadLine();
            }
        }

        var timeout = Wait();

        // ExitCode is 0 for normal exit. Different value when closing the console.
        var result = timeout ? CompletionStatus.Timeout : _cancelWork.IsCancellationRequested ? CompletionStatus.Cancelled : p.ExitCode == 0 ? CompletionStatus.Success : CompletionStatus.Failed;

        _cancelWork = null;
        // Allow changing CompletionStatus in ProcessCompleted.
        var completedArgs = new ProcessCompletedEventArgs(result);
        ProcessCompleted?.Invoke(this, completedArgs);
        result = completedArgs.Status;
        LastCompletionStatus = result;
        //if ((result == CompletionStatus.Failed || result == CompletionStatus.Timeout) && Options.DisplayMode == ProcessDisplayMode.ErrorOnly)
        //{
        //    Config.UserInterfaceManager?.DisplayError(Owner, this);
        //}

        WorkProcess = null;
        return result;
    }

    /// <summary>
    /// Waits for the process to terminate while allowing the cancellation token to kill the process.
    /// </summary>
    /// <returns>Whether a timeout occured.</returns>
    private bool Wait()
    {
        var startTime = DateTime.Now;
        while (!WorkProcess!.HasExited)
        {
            if (_cancelWork!.Token.IsCancellationRequested && !WorkProcess.HasExited)
            {
                Processes.SoftKill(WorkProcess);
            }

            if (Options.Timeout > TimeSpan.Zero && DateTime.Now - startTime > Options.Timeout)
            {
                Processes.SoftKill(WorkProcess);
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
        _cancelWork?.Cancel();
    }

    /// <summary>
    /// Occurs when data is received from the executing process.
    /// </summary>
    protected virtual void OnDataReceived(object sender, DataReceivedEventArgs e)
    {
        _output.AppendLine(e?.Data);
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


    private bool _disposedValue = false;
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _cancelWork?.Dispose();
            }
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
