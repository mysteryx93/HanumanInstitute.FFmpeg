using System;
using System.Linq;
using System.Collections.Generic;

namespace HanumanInstitute.Encoder
{
    /// <summary>
    /// Base class to implement a user interface for running processes.
    /// </summary>
    public abstract class UserInterfaceManagerBase : IUserInterfaceManager
    {
        private readonly List<UIItem> UIList = new List<UIItem>();

        /// <summary>
        /// Gets or sets whether the application has exited.
        /// </summary>
        public bool AppExited { get; set; } = false;

        /// <summary>
        /// Starts a user interface that will receive all tasks with the specified jobId.
        /// </summary>
        /// <param name="jobId">The jobId associated with this interface.</param>
        /// <param name="title">The title to display.</param>
        public void Start(object jobId, string title)
        {
            ArgHelper.ValidateNotNull(jobId, nameof(jobId));

            if (!AppExited)
            {
                if (!UIList.Any(u => u.JobId.Equals(jobId)))
                    UIList.Add(new UIItem(jobId, CreateUI(title, false)));
            }
        }

        /// <summary>
        /// Closes the user interface for specified jobId.
        /// </summary>
        /// <param name="jobId">The jobId to close.</param>
        public void Close(object jobId)
        {
            foreach (var item in UIList.Where(u => u.JobId.Equals(jobId)).ToArray())
            {
                UIList.Remove(item);
                item.Value.Close();
            }
        }

        /// <summary>
        /// Displays a process to the user.
        /// </summary>
        /// <param name="host">The process worker to display.</param>
        public void Display(IProcessWorker host)
        {
            ArgHelper.ValidateNotNull(host, nameof(host));
            if (!AppExited)
            {
                UIItem UI = null;
                if (host.Options.JobId != null)
                    UI = UIList.FirstOrDefault(u => u.JobId.Equals(host.Options.JobId));
                if (UI != null)
                    UI.Value.DisplayTask(host);
                else
                {
                    string Title = !string.IsNullOrEmpty(host.Options.Title) ? host.Options.Title : "Process Running";
                    CreateUI(Title, true).DisplayTask(host);
                }
            }
        }

        /// <summary>
        /// When implemented in a derived class, creates the graphical interface window.
        /// </summary>
        /// <param name="title">The title to display.</param>
        /// <param name="autoClose">Whether to automatically close the window after the main task is completed.</param>
        /// <returns>The newly created user interface window.</returns>
        public abstract IUserInterfaceWindow CreateUI(string title, bool autoClose);
        /// <summary>
        /// When implemented in a derived class, displays an error window.
        /// </summary>
        /// <param name="host">The task throwing the error.</param>
        public abstract void DisplayError(IProcessWorker host);

        private class UIItem
        {
            public object JobId { get; set; }
            public IUserInterfaceWindow Value { get; set; }

            public UIItem() { }

            public UIItem(object jobId, IUserInterfaceWindow ui)
            {
                this.JobId = jobId;
                this.Value = ui;
            }
        }
    }
}
