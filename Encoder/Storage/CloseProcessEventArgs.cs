using System;
using HanumanInstitute.Encoder.Services;

namespace HanumanInstitute.Encoder
{
    /// <summary>
    /// Represents a method that will be called when a process needs to be closed.
    /// </summary>
    public delegate void CloseProcessEventHandler(object sender, CloseProcessEventArgs e);

    /// <summary>
    /// Provides process information for CloseProcess event.
    /// </summary>
    public class CloseProcessEventArgs : EventArgs
    {
        public IProcess Process { get; set; }
        public bool Handled { get; set; } = false;

        public CloseProcessEventArgs()
        {
        }

        public CloseProcessEventArgs(IProcess process)
        {
            Process = process;
        }
    }
}
