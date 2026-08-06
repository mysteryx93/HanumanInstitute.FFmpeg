using System.Globalization;
using HanumanInstitute.Validators;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace HanumanInstitute.FFmpegExampleApplication.ViewModels;

public partial class FFmpegUiViewModel : WorkspaceViewModel, IUserInterfaceWindow
{
    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        set
        {
            this.RaiseAndSetIfChanged(ref _title, value);
            SetTitleWithStatus();
        }
    }

    private string _status = string.Empty;
    public string Status
    {
        get => _status;
        set
        {
            this.RaiseAndSetIfChanged(ref _status, value);
            SetTitleWithStatus();
        }
    }

    [Reactive]
    public partial string TitleWithStatus { get; set; } = string.Empty;

    [Reactive]
    public partial bool AutoClose { get; set; }

    [Reactive]
    public partial long ProgressBarMax { get; set; } = 100;

    [Reactive]
    public partial long ProgressBarValue { get; set; }

    [Reactive]
    public partial string TaskName { get; set; } = string.Empty;

    [Reactive]
    public partial string Fps { get; set; } = string.Empty;

    [Reactive]
    public partial string TimeLeft { get; set; } = string.Empty;

    [Reactive]
    public partial string CancelText { get; set; } = "_Cancel";

    private IProcessWorker? _host;
    private IProcessWorkerEncoder? _hostFFmpeg;
    private IProcessWorker? _task;
    private ITimeLeftCalculator? _timeCalc;

    public void DisplayTask(IProcessWorker taskArg)
    {
        if (taskArg.Options.IsMainTask)
        {
            _host = taskArg;
            _hostFFmpeg = _host as IProcessWorkerEncoder;
            if (_hostFFmpeg != null)
            {
                _hostFFmpeg.FileInfoUpdated += FFmpeg_InfoUpdated;
                _hostFFmpeg.ProgressReceived += FFmpeg_StatusUpdated;
            }
            _host.ProcessCompleted += FFmpeg_Completed;
            Status = 0.ToString("p1", CultureInfo.InvariantCulture);
        }
        else
        {
            _task = taskArg;
            TaskName = _task.Options.Title;
            _task.ProcessCompleted += (_, _) =>
            {
                TaskName = "";
                _task = null;
                WorkCompleted();
            };
        }
    }

    protected long ResumePos => _hostFFmpeg?.Options.ResumePos ?? 0;

    private void SetTitleWithStatus()
    {
        TitleWithStatus = string.IsNullOrEmpty(Status) ? Title : Invariant($"{Title} ({Status})");
    }

    private void FFmpeg_InfoUpdated(object? sender, EventArgs e)
    {
        if (_hostFFmpeg?.FileInfo is FileInfoFFmpeg fileInfo)
        {
            ProgressBarMax = fileInfo.FrameCount + ResumePos;
            _timeCalc = new TimeLeftCalculator(fileInfo.FrameCount + _hostFFmpeg?.Options.ResumePos ?? 0);
        }
    }

    private bool _estimatedTimeLeftToggle;
    private void FFmpeg_StatusUpdated(object? sender, ProgressReceivedEventArgs e)
    {
        var progress = (ProgressStatusFFmpeg)e.Progress;
        ProgressBarValue = progress.Frame + ResumePos;
        Status = ((double)ProgressBarValue / ProgressBarMax).ToString("p1", CultureInfo.CurrentCulture);
        Fps = progress.Fps.ToString(CultureInfo.CurrentCulture);

        // Time left will be updated only 1 out of 2 to prevent changing too quick.
        _estimatedTimeLeftToggle = !_estimatedTimeLeftToggle;
        if (_estimatedTimeLeftToggle && _timeCalc != null)
        {
            _timeCalc.Calculate(progress.Frame + ResumePos);
            var resultTimeLeft = _timeCalc.ResultTimeLeft;
            if (resultTimeLeft > TimeSpan.Zero)
            {
                TimeLeft = resultTimeLeft.ToString(resultTimeLeft.TotalHours < 1 ? "m\\:ss" : "h\\:mm\\:ss", CultureInfo.InvariantCulture);
            }
        }
    }

    private void FFmpeg_Completed(object? sender, ProcessCompletedEventArgs e)
    {
        WorkCompleted();
    }

    private void WorkCompleted()
    {
        CancelText = "_Close";
        if (AutoClose)
        {
            Close();
        }
    }

    public void Close() => base.CloseCommand.ExecuteIfCan();

    public override bool OnClosing()
    {
        _host?.Cancel();
        _task?.Cancel();
        return base.OnClosing();
    }
}
