using System;
using System.Diagnostics;

namespace HanumanInstitute.FFmpeg.Services
{
    /// <summary>
    /// Creates instances of process wrapper classes.
    /// </summary>
    public class ProcessFactory : IProcessFactory
    {
        /// <summary>
        /// Creates a new instance of IProcess.
        /// </summary>
        public IProcess Create() => new ProcessWrapper();
        /// <summary>
        /// Creates a new instance of IProcess as a wrapper around an existing process.
        /// </summary>
        /// <param name="process">The process to wrap the new class around.</param>
        public IProcess Create(Process process) => new ProcessWrapper(process);
    }
}
