using System;
using System.Collections.Generic;
using System.Linq;

namespace HanumanInstitute.FFmpeg
{
    /// <summary>
    /// Base class to implement a user interface for running processes.
    /// </summary>
    public abstract class UserInterfaceManagerBase : IUserInterfaceManager
    {
        private readonly List<UIItem> _uiList = new List<UIItem>();

        /// <summary>
        /// Gets or sets whether the application has exited.
        /// </summary>
        public bool AppExited { get; set; } = false;

        public void AttachProcessWorker(object owner, IProcessWorker worker, ProcessOptions options)
        {
            if (worker == null) { throw new ArgumentNullException(nameof(worker)); }

            worker.ProcessStarted += (s, e) =>
            {
                if (options.DisplayMode == ProcessDisplayMode.Interface)
                {
                    Display(owner, worker);
                }
            };
            worker.ProcessCompleted += (s, e) =>
            {
                if ((e.Status == CompletionStatus.Failed || e.Status == CompletionStatus.Timeout) && 
                    (options.DisplayMode == ProcessDisplayMode.ErrorOnly || options.DisplayMode == ProcessDisplayMode.Interface))
                {
                    DisplayError(owner, worker);
                }
            };
        }

        /// <summary>
        /// Starts a user interface that will receive all tasks with the specified jobId.
        /// </summary>
        /// <param name="owner">The owner to set for the window.</param>
        /// <param name="jobId">The jobId associated with this interface.</param>
        /// <param name="title">The title to display.</param>
        public void Start(object owner, object jobId, string title)
        {
            ArgHelper.ValidateNotNull(jobId, nameof(jobId));

            if (!AppExited)
            {
                if (!_uiList.Any(u => u.JobId.Equals(jobId)))
                {
                    _uiList.Add(new UIItem(jobId, CreateUI(owner, title, false)));
                }
            }
        }

        /// <summary>
        /// Closes the user interface for specified jobId.
        /// </summary>
        /// <param name="jobId">The jobId to close.</param>
        public void Close(object jobId)
        {
            foreach (var item in _uiList.Where(u => u.JobId.Equals(jobId)).ToArray())
            {
                _uiList.Remove(item);
                item.Value.Close();
            }
        }

        /// <summary>
        /// Displays a process to the user.
        /// </summary>
        /// <param name="owner">The owner to set for the window.</param>
        /// <param name="host">The process worker to display.</param>
        public void Display(object owner, IProcessWorker host)
        {
            ArgHelper.ValidateNotNull(host, nameof(host));
            if (!AppExited)
            {
                UIItem ui = null;
                if (host.Options.JobId != null)
                {
                    ui = _uiList.FirstOrDefault(u => u.JobId.Equals(host.Options.JobId));
                }

                if (ui != null)
                {
                    ui.Value.DisplayTask(host);
                }
                else
                {
                    var title = !string.IsNullOrEmpty(host.Options.Title) ? host.Options.Title : "Process Running";
                    CreateUI(owner, title, true).DisplayTask(host);
                }
            }
        }

        /// <summary>
        /// When implemented in a derived class, creates the graphical interface window.
        /// </summary>
        /// <param name="owner">The owner to set for the window.</param>
        /// <param name="title">The title to display.</param>
        /// <param name="autoClose">Whether to automatically close the window after the main task is completed.</param>
        /// <returns>The newly created user interface window.</returns>
        public abstract IUserInterfaceWindow CreateUI(object owner, string title, bool autoClose);
        /// <summary>
        /// When implemented in a derived class, displays an error window.
        /// </summary>
        /// <param name="owner">The owner to set for the window.</param>
        /// <param name="host">The task throwing the error.</param>
        public abstract void DisplayError(object owner, IProcessWorker host);

        private class UIItem
        {
            public object JobId { get; set; }
            public IUserInterfaceWindow Value { get; set; }

            public UIItem() { }

            public UIItem(object jobId, IUserInterfaceWindow ui)
            {
                JobId = jobId;
                Value = ui;
            }
        }
    }
}
