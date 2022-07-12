namespace HanumanInstitute.FFmpeg;

/// <summary>
/// Represents the method that will handle the ProcessCompleted event.
/// </summary>
public delegate void ProcessCompletedEventHandler(object sender, ProcessCompletedEventArgs e);

/// <summary>
/// Provides progress information for the ProcessCompleted event.
/// </summary>
public class ProcessCompletedEventArgs : EventArgs
{
    /// <summary>
    /// The reason the process ended.
    /// </summary>
    public CompletionStatus Status { get; set; }

    /// <summary>
    /// Initializes a new instance of the ProcessCompletedEventArgs class.
    /// </summary>
    public ProcessCompletedEventArgs() { }

    /// <summary>
    /// Initializes a new instance of the ProcessCompletedEventArgs class with specified status.
    /// </summary>
    /// <param name="status">The reason the process ended.</param>
    public ProcessCompletedEventArgs(CompletionStatus status)
    {
        Status = status;
    }
}
