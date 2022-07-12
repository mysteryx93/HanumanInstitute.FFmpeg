using System.Diagnostics;

namespace HanumanInstitute.FFmpeg;

/// <summary>
/// Contains options to control the behaviors of a process.
/// </summary>
public class ProcessOptions
{
    /// <summary>
    /// Gets or sets the display mode when running a process.
    /// </summary>
    public ProcessDisplayMode DisplayMode { get; set; } = ProcessDisplayMode.None;
    /// <summary>
    /// Gets or sets the title to display.
    /// </summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets an identifier for the job.
    /// </summary>
    public object? JobId { get; set; }
    /// <summary>
    /// If displaying several tasks in the same UI, gets whether this is the main task being performed.
    /// </summary>
    public bool IsMainTask { get; set; } = true;
    /// <summary>
    /// Gets or sets the overall priority category for the associated process.
    /// </summary>
    public ProcessPriorityClass Priority { get; set; } = ProcessPriorityClass.Normal;
    /// <summary>
    /// Gets or sets a timeout after which the process will be stopped.
    /// </summary>
    public TimeSpan Timeout { get; set; }

    /// <summary>
    /// Initializes a new instance of the ProcessOptions class.
    /// </summary>
    public ProcessOptions() { }

    /// <summary>
    /// Initializes a new instance of the ProcessOptions class.
    /// </summary>
    /// <param name="displayMode">Gets or sets the display mode when running a process.</param>
    public ProcessOptions(ProcessDisplayMode displayMode)
    {
        DisplayMode = displayMode;
    }

    /// <summary>
    /// Initializes a new instance of the ProcessOptions class.
    /// </summary>
    /// <param name="displayMode">Gets or sets the display mode when running a process.</param>
    /// <param name="title">The title to display.</param>
    public ProcessOptions(ProcessDisplayMode displayMode, string title)
    {
        DisplayMode = displayMode;
        Title = title;
    }

    /// <summary>
    /// Initializes a new instance of the ProcessOptions class.
    /// </summary>
    /// <param name="displayMode">The display mode when running a process.</param>
    /// <param name="title">The title to display..</param>
    /// <param name="priority">The overall priority category for the associated process.</param>
    public ProcessOptions(ProcessDisplayMode displayMode, string title, ProcessPriorityClass priority)
    {
        DisplayMode = displayMode;
        Title = title;
        Priority = priority;
    }

    /// <summary>
    /// Initializes a new instance of the ProcessOptions class to display several jobs in the same UI.
    /// </summary>
    /// <param name="jobId">An identifier for the job. Can be used to link a set of jobs to the same UI.</param>
    /// <param name="title">The title to display.</param>
    /// <param name="isMainTask">When displaying several tasks in the same UI, whether this is the main task.</param>
    public ProcessOptions(object jobId, string title, bool isMainTask)
    {
        DisplayMode = ProcessDisplayMode.Interface;
        JobId = jobId;
        Title = title;
        IsMainTask = isMainTask;
    }
}
