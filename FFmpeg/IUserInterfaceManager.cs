using System;

namespace HanumanInstitute.FFmpeg
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

        void AttachProcessWorker(object? owner, IProcessWorker worker, ProcessOptions options);
        /// <summary>
        /// Starts a user interface that will receive all tasks with the specified jobId.
        /// </summary>
        /// <param name="owner">The owner to set for the window.</param>
        /// <param name="jobId">The jobId associated with this interface.</param>
        /// <param name="title">The title to display.</param>
        void Start(object owner, object jobId, string title);
        /// <summary>
        /// Closes the user interface for specified jobId.
        /// </summary>
        /// <param name="jobId">The jobId to close.</param>
        void Close(object jobId);
        /// <summary>
        /// Displays a process to the user.
        /// </summary>
        /// <param name="owner">The owner to set for the window.</param>
        /// <param name="host">The process worker to display.</param>
        void Display(object owner, IProcessWorker host);
        /// <summary>
        /// When implemented in a derived class, creates the graphical interface window.
        /// </summary>
        /// <param name="owner">The owner to set for the window.</param>
        /// <param name="title">The title to display.</param>
        /// <param name="autoClose">Whether to automatically close the window after the main task is completed.</param>
        /// <returns>The newly created user interface window.</returns>
        IUserInterfaceWindow CreateUI(object owner, string title, bool autoClose);
        /// <summary>
        /// When implemented in a derived class, displays an error window.
        /// </summary>
        /// <param name="owner">The owner to set for the window.</param>
        /// <param name="host">The task throwing the error.</param>
        void DisplayError(object owner, IProcessWorker host);
    }
}
