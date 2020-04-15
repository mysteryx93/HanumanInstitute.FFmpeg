using System;

namespace HanumanInstitute.Encoder
{
    /// <summary>
    /// Represents the method that will handle the ProcessStarted event.
    /// </summary>
    public delegate void ProcessStartedEventHandler(object sender, ProcessStartedEventArgs e);

    /// <summary>
    /// Provides job information for the ProcessStarted event.
    /// </summary>
    public class ProcessStartedEventArgs : EventArgs
    {
        public IProcessWorker ProcessWorker { get; set; }

        public ProcessStartedEventArgs() { }

        public ProcessStartedEventArgs(IProcessWorker processWorker)
        {
            ProcessWorker = processWorker;
        }
    }
}
