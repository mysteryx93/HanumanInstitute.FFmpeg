namespace HanumanInstitute.FFmpeg;

/// <summary>
/// Represents the method that will handle the ProcessStarted event.
/// </summary>
public delegate void ProcessStartedEventHandler(object sender, ProcessStartedEventArgs e);

/// <summary>
/// Provides job information for the ProcessStarted event.
/// </summary>
public class ProcessStartedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the <see cref="IProcessWorker"/> that just started.
    /// </summary>
    public IProcessWorker ProcessWorker { get; set; }

    /// <summary>
    /// Initializes a new instance of the ProcessStartedEventArgs class with specified process worker.
    /// </summary>
    /// <param name="processWorker">The <see cref="IProcessWorker"/> that just started.</param>
    public ProcessStartedEventArgs(IProcessWorker processWorker)
    {
        ProcessWorker = processWorker;
    }
}
