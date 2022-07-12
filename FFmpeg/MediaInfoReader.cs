namespace HanumanInstitute.FFmpeg;

/// <inheritdoc />
public class MediaInfoReader : IMediaInfoReader
{
    private readonly IProcessService _factory;

    /// <summary>
    /// Initializes a new instance of the MediaInfoReader class
    /// </summary>
    /// <param name="processFactory">The Factory responsible for creating processes.</param>
    public MediaInfoReader(IProcessService processFactory) =>
        _factory = processFactory.CheckNotNull(nameof(processFactory));

    /// <inheritdoc />
    public object? Owner { get; set; }

    /// <summary>
    /// Returns the version information from FFmpeg.
    /// </summary>
    /// <param name="options">The options for starting the process.</param>
    /// <param name="callback">A method that will be called after the process has been started.</param>
    /// <returns>A IFFmpegProcess object containing the version information.</returns>
    public string GetVersion(ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null)
    {
        var worker = _factory.CreateEncoder(Owner, options, callback);
        worker.OutputType = ProcessOutput.Output;
        worker.RunEncoder("-version", EncoderApp.FFmpeg);
        return worker.Output;
    }

    /// <summary>
    /// Gets file streams information of specified file via FFmpeg.
    /// </summary>
    /// <param name="source">The file to get information about.</param>
    /// <param name="options">The options for starting the process.</param>
    /// <param name="callback">A method that will be called after the process has been started.</param>
    /// <returns>A IFFmpegProcess object containing the file information.</returns>
    public FileInfoFFmpeg GetFileInfo(string source, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null)
    {
        source.CheckNotNullOrEmpty(nameof(source));

        var worker = _factory.CreateEncoder(Owner, options, callback);
        worker.ProcessCompleted += (s, e) =>
        {
            if (e.Status == CompletionStatus.Failed && (worker.FileInfo as FileInfoFFmpeg)?.FileStreams != null)
            {
                e.Status = CompletionStatus.Success;
            }
        };
        worker.RunEncoder($@"-i ""{source}""", EncoderApp.FFmpeg);
        return (FileInfoFFmpeg)worker.FileInfo;
    }

    /// <summary>
    /// Returns the exact frame count of specified video file.
    /// </summary>
    /// <param name="source">The file to get information about.</param>
    /// <param name="options">The options for starting the process.</param>
    /// <param name="callback">A method that will be called after the process has been started.</param>
    /// <returns>The number of frames in the video.</returns>
    public long GetFrameCount(string source, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null)
    {
        source.CheckNotNullOrEmpty(nameof(source));

        long result = 0;
        var worker = _factory.CreateEncoder(Owner, options, callback);
        worker.ProgressReceived += (sender, e) =>
        {
            // Read all status lines and keep the last one.
            result = ((ProgressStatusFFmpeg)e.Progress).Frame;
        };
        worker.RunEncoder($@"-i ""{source}"" -f null /dev/null", EncoderApp.FFmpeg);
        return result;
    }
}
