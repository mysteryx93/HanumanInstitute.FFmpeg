using System;

namespace HanumanInstitute.Encoder
{
    /// <summary>
    /// Represents the method that will handle the StatusUpdated event.
    /// </summary>
    public delegate void ProgressReceivedEventHandler(object sender, ProgressReceivedEventArgs e);

    /// <summary>
    /// Provides progress information for the ProgressReceived event.
    /// </summary>
    public class ProgressReceivedEventArgs : EventArgs
    {
        public object Progress { get; set; }

        public ProgressReceivedEventArgs() { }

        public ProgressReceivedEventArgs(object progress)
        {
            Progress = progress;
        }
    }
}
