using System;

namespace HanumanInstitute.Encoder
{
    /// <summary>
    /// Base class to implement a user interface for running processes.
    /// </summary>
    public interface IUserInterfaceManager
    {
        /// <summary>
        /// Gets or sets whether the application has exited.
        /// </summary>
        bool AppExited { get; set; }
        /// <summary>
        /// Starts a user interface that will receive all tasks with the specified jobId.
        /// </summary>
        /// <param name="jobId">The jobId associated with this interface.</param>
        /// <param name="title">The title to display.</param>
        void Start(object jobId, string title);
        /// <summary>
        /// Closes the user interface for specified jobId.
        /// </summary>
        /// <param name="jobId">The jobId to close.</param>
        void Close(object jobId);
        /// <summary>
        /// Displays a process to the user.
        /// </summary>
        /// <param name="host">The process worker to display.</param>
        void Display(IProcessWorker host);
        /// <summary>
        /// When implemented in a derived class, creates the graphical interface window.
        /// </summary>
        /// <param name="title">The title to display.</param>
        /// <param name="autoClose">Whether to automatically close the window after the main task is completed.</param>
        /// <returns>The newly created user interface window.</returns>
        IUserInterfaceWindow CreateUI(string title, bool autoClose);
        /// <summary>
        /// When implemented in a derived class, displays an error window.
        /// </summary>
        /// <param name="host">The task throwing the error.</param>
        void DisplayError(IProcessWorker host);
    }
}
