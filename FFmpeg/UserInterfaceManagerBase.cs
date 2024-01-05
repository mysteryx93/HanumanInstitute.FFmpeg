namespace HanumanInstitute.FFmpeg;

/// <inheritdoc />
public abstract class UserInterfaceManagerBase : IUserInterfaceManager
{
    private readonly List<UiItem> _uiList = new();

    /// <inheritdoc />
    public bool AppExited { get; set; } = false;

    /// <inheritdoc />
    public void AttachProcessWorker(object? owner, IProcessWorker worker, ProcessOptions options)
    {
        if (worker == null) { throw new ArgumentNullException(nameof(worker)); }

        worker.ProcessStarted += (_, _) =>
        {
            if (options.DisplayMode == ProcessDisplayMode.Interface)
            {
                Display(owner, worker);
            }
        };
        worker.ProcessCompleted += (_, e) =>
        {
            if ((e.Status == CompletionStatus.Failed || e.Status == CompletionStatus.Timeout) && 
                (options.DisplayMode == ProcessDisplayMode.ErrorOnly || options.DisplayMode == ProcessDisplayMode.Interface))
            {
                DisplayError(owner, worker);
            }
        };
    }

    /// <inheritdoc />
    public void Start(object? owner, object jobId, string title)
    {
        jobId.CheckNotNull(nameof(jobId));

        if (!AppExited)
        {
            if (!_uiList.Any(u => u.JobId.Equals(jobId)))
            {
                _uiList.Add(new UiItem(jobId, CreateUI(owner, title, false)));
            }
        }
    }

    /// <inheritdoc />
    public void Close(object jobId)
    {
        foreach (var item in _uiList.Where(u => u.JobId.Equals(jobId)).ToArray())
        {
            _uiList.Remove(item);
            item.Value.Close();
        }
    }

    /// <inheritdoc />
    public void Display(object? owner, IProcessWorker host)
    {
        host.CheckNotNull(nameof(host));
        if (!AppExited)
        {
            UiItem? ui = null;
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

    /// <inheritdoc />
    public abstract IUserInterfaceWindow CreateUI(object? owner, string title, bool autoClose);
    /// <inheritdoc />
    public abstract void DisplayError(object? owner, IProcessWorker host);

    private class UiItem
    {
        public object JobId { get; set; }
        public IUserInterfaceWindow Value { get; set; }

        //public UIItem() { }

        public UiItem(object jobId, IUserInterfaceWindow ui)
        {
            JobId = jobId;
            Value = ui;
        }
    }
}
