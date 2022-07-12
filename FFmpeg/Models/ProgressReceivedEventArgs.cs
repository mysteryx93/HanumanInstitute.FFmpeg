namespace HanumanInstitute.FFmpeg;

/// <summary>
/// Represents the method that will handle the StatusUpdated event.
/// </summary>
public delegate void ProgressReceivedEventHandler(object sender, ProgressReceivedEventArgs e);

/// <summary>
/// Provides progress information for the ProgressReceived event.
/// </summary>
public class ProgressReceivedEventArgs : EventArgs
{
    /// <summary>
    /// Gets progress data. The data type depends on the application that was started.
    /// </summary>
    public object Progress { get; set; }

    /// <summary>
    /// Initializes a new instance of the ProgressReceivedEventArgs class with specified progress data.
    /// </summary>
    /// <param name="progress">Gets progress data. The data type depends on the application that was started.</param>
    public ProgressReceivedEventArgs(object progress)
    {
        Progress = progress;
    }
}
