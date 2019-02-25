using System;
using EmergenceGuardian.Encoder.Services;

namespace EmergenceGuardian.Encoder {
    /// <summary>
    /// Represents the method that will handle the StatusUpdated event.
    /// </summary>
    public delegate void ProgressReceivedEventHandler(object sender, ProgressReceivedEventArgs e);

    /// <summary>
    /// Provides progress information for the ProgressReceived event.
    /// </summary>
    public class ProgressReceivedEventArgs : EventArgs {
        public object Progress { get; set; }

        public ProgressReceivedEventArgs() { }

        public ProgressReceivedEventArgs(object progress) {
            Progress = progress;
        }
    }

    /// <summary>
    /// Represents the method that will handle the ProcessStarted event.
    /// </summary>
    public delegate void ProcessStartedEventHandler(object sender, ProcessStartedEventArgs e);

    /// <summary>
    /// Provides job information for the ProcessStarted event.
    /// </summary>
    public class ProcessStartedEventArgs : EventArgs {
        public IProcessWorker ProcessWorker { get; set; }

        public ProcessStartedEventArgs() { }

        public ProcessStartedEventArgs(IProcessWorker processWorker) {
            ProcessWorker = processWorker;
        }
    }

    /// <summary>
    /// Represents a method that will be called when a process needs to be closed.
    /// </summary>
    public delegate void CloseProcessEventHandler(object sender, CloseProcessEventArgs e);

    /// <summary>
    /// Provides process information for CloseProcess event.
    /// </summary>
    public class CloseProcessEventArgs : EventArgs {
        public IProcess Process { get; set; }
        public bool Handled { get; set; } = false;

        public CloseProcessEventArgs() {
        }

        public CloseProcessEventArgs(IProcess process) {
            Process = process;
        }
    }

    /// <summary>
    /// Represents the method that will handle the ProcessCompleted event.
    /// </summary>
    public delegate void ProcessCompletedEventHandler(object sender, ProcessCompletedEventArgs e);

    /// <summary>
    /// Provides progress information for the ProcessCompleted event.
    /// </summary>
    public class ProcessCompletedEventArgs : EventArgs {
        public CompletionStatus Status { get; set; }

        public ProcessCompletedEventArgs() { }

        public ProcessCompletedEventArgs(CompletionStatus status) {
            Status = status;
        }
    }

    /// <summary>
    /// Represents the method that will handle the GetCustomAppPath event.
    /// </summary>
    public delegate void GetPathEventHandler(object sender, GetPathEventArgs e);

    /// <summary>
    /// Provides progress information for the GetCustomAppPath event.
    /// </summary>
    public class GetPathEventArgs : EventArgs {
        public string App { get; set; }
        public string Path { get; set; }

        public GetPathEventArgs() { }

        public GetPathEventArgs(string app) {
            this.App = app;
        }
    }

    /// <summary>
    /// Delegate used for SetConsoleCtrlHandler Win API call.
    /// </summary>
    public delegate bool ConsoleCtrlDelegate(uint CtrlType);
}
