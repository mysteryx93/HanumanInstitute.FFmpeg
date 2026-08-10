namespace HanumanInstitute.FFmpeg;

/// <summary>
/// Muxes, extracts, concatenates, and truncates media streams via FFmpeg (stream-copy).
/// </summary>
public interface IMediaMuxer
{
    /// <summary>
    /// Gets or sets the owner of the process windows.
    /// </summary>
    object? Owner { get; set; }

    /// <summary>
    /// Muxes the first video stream and first audio stream of the given files into destination.
    /// Either videoFile or audioFile may be null.
    /// </summary>
    /// <param name="videoFile">File providing the video stream, or null for audio-only.</param>
    /// <param name="audioFile">File providing the audio stream, or null for video-only.</param>
    /// <param name="destination">Output path.</param>
    /// <param name="options">Process options, or null for defaults.</param>
    /// <param name="callback">Invoked after the process has started, or null.</param>
    /// <returns>The process completion status.</returns>
    CompletionStatus Muxe(string? videoFile, string? audioFile, string destination, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null);

    /// <summary>
    /// Muxes the given streams into destination with optional <see cref="MuxOptions"/>
    /// (container tags, chapters, extra maps via <see cref="MuxOptions.From(string)"/>).
    /// </summary>
    /// <param name="fileStreams">Streams to map, in output order.</param>
    /// <param name="destination">Output path.</param>
    /// <param name="muxOptions">Mux options, or null for none.</param>
    /// <param name="options">Process options, or null for defaults.</param>
    /// <param name="callback">Invoked after the process has started, or null.</param>
    /// <returns>The process completion status.</returns>
    CompletionStatus Muxe(IEnumerable<MediaStream> fileStreams, string destination, MuxOptions? muxOptions = null, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null);

    /// <summary>
    /// Extracts the video stream from source into destination.
    /// </summary>
    /// <param name="source">Input path.</param>
    /// <param name="destination">Output path.</param>
    /// <param name="options">Process options, or null for defaults.</param>
    /// <param name="callback">Invoked after the process has started, or null.</param>
    /// <returns>The process completion status.</returns>
    CompletionStatus ExtractVideo(string source, string destination, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null);

    /// <summary>
    /// Extracts the audio stream from source into destination.
    /// </summary>
    /// <param name="source">Input path.</param>
    /// <param name="destination">Output path.</param>
    /// <param name="options">Process options, or null for defaults.</param>
    /// <param name="callback">Invoked after the process has started, or null.</param>
    /// <returns>The process completion status.</returns>
    CompletionStatus ExtractAudio(string source, string destination, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null);

    /// <summary>
    /// Concatenates the given files into destination.
    /// </summary>
    /// <param name="files">Input paths, in order.</param>
    /// <param name="destination">Output path.</param>
    /// <param name="options">Process options, or null for defaults.</param>
    /// <param name="callback">Invoked after the process has started, or null.</param>
    /// <returns>The process completion status.</returns>
    CompletionStatus Concatenate(IEnumerable<string> files, string destination, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null);

    /// <summary>
    /// Truncates source from startPos with optional duration into destination.
    /// </summary>
    /// <param name="source">Input path.</param>
    /// <param name="destination">Output path.</param>
    /// <param name="startPos">Start position, or null to start at the beginning.</param>
    /// <param name="duration">Duration to keep, or null for the remainder of the file.</param>
    /// <param name="options">Process options, or null for defaults.</param>
    /// <param name="callback">Invoked after the process has started, or null.</param>
    /// <returns>The process completion status.</returns>
    CompletionStatus Truncate(string source, string destination, TimeSpan? startPos, TimeSpan? duration = null, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null);

    /// <summary>
    /// Copies container tags, chapters, and matching stream tags/dispositions from
    /// source onto destination (rewrites destination in place).
    /// Destination media is stream-copied. Streams are matched by type then by order within that type
    /// (e.g. 1st video→1st video, 2nd audio→2nd audio). Destination stream metadata keys already present
    /// are kept; missing keys, language, and disposition flags are merged from the matched source stream.
    /// </summary>
    /// <param name="source">File providing metadata (and chapters).</param>
    /// <param name="destination">File whose media is kept and that receives the metadata.</param>
    /// <param name="options">Process options, or null for defaults.</param>
    /// <param name="callback">Invoked after the process has started, or null.</param>
    /// <returns>The process completion status.</returns>
    CompletionStatus CopyMetadata(string source, string destination, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null);
}
