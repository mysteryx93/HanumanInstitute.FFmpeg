using System;
using System.Diagnostics;
using HanumanInstitute.FFmpeg.Services;

namespace HanumanInstitute.FFmpeg
{
    /// <summary>
    /// Executes an application and manages its process.
    /// </summary>
    public interface IProcessWorker
    {
        /// <summary>
        /// Gets or sets the configuration settings.
        /// </summary>
        IMediaConfig Config { get; set; }
        /// <summary>
        /// Gets or sets the options to control the behaviors of the process.
        /// </summary>
        ProcessOptions Options { get; set; }
        /// <summary>
        /// Gets or sets the console output to read.
        /// </summary>
        ProcessOutput OutputType { get; set; }
        /// <summary>
        /// Gets the process currently being executed.
        /// </summary>
        IProcess? WorkProcess { get; }
        /// <summary>
        /// Gets or sets a method to call after the process has been started.
        /// </summary>
        event ProcessStartedEventHandler? ProcessStarted;
        /// <summary>
        /// Occurs when the process writes to its output stream.
        /// </summary>
        event DataReceivedEventHandler? DataReceived;
        /// <summary>
        /// Occurs when the process has terminated its work.
        /// </summary>
        event ProcessCompletedEventHandler? ProcessCompleted;
        /// <summary>
        /// Returns the raw console output.
        /// </summary>
        string Output { get; }

        /// <summary>
        /// Returns the CompletionStatus of the last operation.
        /// </summary>
        CompletionStatus? LastCompletionStatus { get; }
        /// <summary>
        /// Runs the command as 'cmd /c', allowing the use of command line features such as piping.
        /// </summary>
        /// <param name="cmd">The full command to be executed with arguments.</param>
        /// <returns>The process completion status.</returns>
        CompletionStatus RunAsCommand(string cmd);
        /// <summary>
        /// Runs specified process with specified arguments.
        /// </summary>
        /// <param name="fileName">The process to start.</param>
        /// <param name="arguments">The set of arguments to use when starting the process.</param>
        /// <returns>The process completion status.</returns>
        /// <exception cref="System.IO.FileNotFoundException">Occurs when the file to run is not found.</exception>
        /// <exception cref="InvalidOperationException">Occurs when this class instance is already running another process.</exception>
        CompletionStatus Run(string fileName, string arguments);
        /// <summary>
        /// Cancels the currently running job and terminate its process.
        /// </summary>
        void Cancel();
        /// <summary>
        /// Returns the full command with arguments being run.
        /// </summary>
        string CommandWithArgs { get; }
    }
}
