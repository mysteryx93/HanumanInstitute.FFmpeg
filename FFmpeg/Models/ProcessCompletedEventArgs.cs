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
    public CompletionStatus Status { get; set; }

    public ProcessCompletedEventArgs() { }

    public ProcessCompletedEventArgs(CompletionStatus status)
    {
        Status = status;
    }
}
