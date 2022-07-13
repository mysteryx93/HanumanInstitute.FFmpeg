using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using HanumanInstitute.FFmpeg.Services;

namespace HanumanInstitute.FFmpeg;

/// <inheritdoc cref="IProcessWorker"/>
public class ProcessWorker : IProcessWorker, IDisposable
{
    /// <inheritdoc />
    public IProcessManager Processes { get; set; }
    /// <inheritdoc />
    public ProcessOptions Options { get; set; }
    /// <inheritdoc />
    public ProcessOutput OutputType { get; set; }
    /// <inheritdoc />
    public IProcess? WorkProcess { get; private set; }
    /// <inheritdoc />
    public event ProcessStartedEventHandler? ProcessStarted;
    /// <inheritdoc />
    public event DataReceivedEventHandler? DataReceived;
    /// <inheritdoc />
    public event ProcessCompletedEventHandler? ProcessCompleted;
    /// <inheritdoc />
    public string Output => _output.ToString();
    /// <inheritdoc />
    public CompletionStatus? LastCompletionStatus { get; private set; }
    /// <inheritdoc />
    public string CommandWithArgs { get; private set; } = string.Empty;

    private readonly StringBuilder _output = new StringBuilder();
    private readonly IProcessFactory _factory;
    /// <summary>
    /// A token to lock access to WorkProcess from various threads.
    /// </summary>
    protected object WorkProcessLock { get; private set; } = new object();
    private CancellationTokenSource? _cancelWork;

    //public ProcessWorker() : this(new MediaConfig(), new ProcessFactory(), null) { }

    internal ProcessWorker(IProcessManager config, IProcessFactory processFactory, ProcessOptions? options = null)
    {
        Processes = config ?? throw new ArgumentNullException(nameof(config));
        _factory = processFactory ?? throw new ArgumentNullException(nameof(processFactory));
        Options = options ?? new ProcessOptions();
    }

    /// <inheritdoc />
    public CompletionStatus RunAsCommand(string cmd)
    {
        cmd.CheckNotNullOrEmpty(nameof(cmd));
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Run("cmd", $@"/c "" {cmd} """);
        }
        else // Linux, MacOS
        {
            cmd = cmd.Replace("\"", "\\\"");
            return Run("/bin/bash", $@"-c "" {cmd} """);
        }
    }

    /// <inheritdoc />
    public virtual CompletionStatus Run(string fileName, string arguments)
    {
        fileName.CheckNotNullOrEmpty(nameof(fileName));

        IProcess p;
        lock (WorkProcessLock)
        {
            if (WorkProcess != null) { throw new InvalidOperationException(Resources.ProcessWorkerBusy); }
            p = _factory.Create();
            WorkProcess = p;
        }
        _output.Clear();
        _cancelWork = new CancellationTokenSource();

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

        try
        {
            p.Start();
        }
        catch (Win32Exception ex)
        {
            throw new FileNotFoundException(ex.Message);
        }
        
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
        var result = timeout ? CompletionStatus.Timeout :
            _cancelWork.IsCancellationRequested ? CompletionStatus.Cancelled :
            p.ExitCode == 0 ? CompletionStatus.Success : CompletionStatus.Failed;

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

    /// <inheritdoc />
    public void Cancel() => _cancelWork?.Cancel();

    /// <summary>
    /// Occurs when data is received from the executing process.
    /// </summary>
    protected virtual void OnDataReceived(object sender, DataReceivedEventArgs e)
    {
        _output.AppendLine(e.Data);
        DataReceived?.Invoke(this, e);
    }

    /// <summary>
    /// Throws an exception if a progress is already running.
    /// </summary>
    protected void EnsureNotRunning()
    {
        if (WorkProcess != null)
        {
            lock (WorkProcessLock)
            {
                if (WorkProcess != null)
                {
                    throw new InvalidOperationException(Resources.ProcessWorkerBusy);
                }
            }
        }
    }

    private bool _disposedValue;
    /// <inheritdoc cref="IDisposable" />
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

    /// <inheritdoc cref="IDisposable" />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
