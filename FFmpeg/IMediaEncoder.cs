namespace HanumanInstitute.FFmpeg;

/// <summary>
/// Provides functions to encode media files.
/// </summary>
public interface IMediaEncoder
{
    /// <summary>
    /// Gets or sets the owner of the process windows.
    /// </summary>
    object? Owner { get; set; }
    /// <summary>
    /// Converts specified file into AVI UT Video format.
    /// </summary>
    /// <param name="source">The file to convert.</param>
    /// <param name="destination">The destination file, ending with .AVI</param>
    /// <param name="audio">Whether to encode audio.</param>
    /// <param name="options">The options for starting the process.</param>
    /// <param name="callback">A method that will be called after the process has been started.</param>
    /// <returns>The process completion status.</returns>
    CompletionStatus ConvertToAviUtVideo(string source, string destination, bool audio, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null);
    /// <summary>
    /// Encodes a media file using FFmpeg with specified arguments.
    /// </summary>
    /// <param name="source">The file to convert.</param>
    /// <param name="destination">The destination file.</param>
    /// <param name="videoCodec">The codec to use to encode the video stream.</param>
    /// <param name="audioCodec">The codec to use to encode the audio stream.</param>
    /// <param name="encodeArgs">Additional arguments to pass to FFmpeg.</param>
    /// <param name="options">The options for starting the process.</param>
    /// <param name="callback">A method that will be called after the process has been started.</param>
    /// <returns>The process completion status.</returns>
    CompletionStatus EncodeFFmpeg(string source, string destination, string? videoCodec, string? audioCodec, string? encodeArgs = null, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null);
    /// <summary>
    /// Encodes an Avisynth script file using FFmpeg with specified arguments.
    /// </summary>
    /// <param name="source">The file to convert.</param>
    /// <param name="destination">The destination file.</param>
    /// <param name="videoCodec">The codec to use to encode the video stream.</param>
    /// <param name="audioCodec">The codec to use to encode the audio stream.</param>
    /// <param name="encodeArgs">Additional arguments to pass to FFmpeg.</param>
    /// <param name="options">The options for starting the process.</param>
    /// <param name="callback">A method that will be called after the process has been started.</param>
    /// <returns>The process completion status.</returns>
    CompletionStatus EncodeAvisynthToFFmpeg(string source, string destination, string? videoCodec, string? audioCodec, string? encodeArgs, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null);
    /// <summary>
    /// Encodes a VapourSynth script file using FFmpeg with specified arguments.
    /// </summary>
    /// <param name="source">The file to convert.</param>
    /// <param name="destination">The destination file.</param>
    /// <param name="videoCodec">The codec to use to encode the video stream.</param>
    /// <param name="audioCodec">The codec( to use to encode the audio stream.</param>
    /// <param name="encodeArgs">Additional arguments to pass to FFmpeg.</param>
    /// <param name="options">The options for starting the process.</param>
    /// <param name="callback">A method that will be called after the process has been started.</param>
    /// <returns>The process completion status.</returns>
    CompletionStatus EncodeVapourSynthToFFmpeg(string source, string destination, string? videoCodec, string? audioCodec, string? encodeArgs, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null);
    /// <summary>
    /// Encodes an Avisynth script by opening it in FFmpeg with <c>-f avisynth</c>.
    /// The argument string is appended as written. An empty destination writes <c>-f null -</c>.
    /// </summary>
    /// <param name="source">The script file to open.</param>
    /// <param name="destination">The output file. Empty writes nowhere.</param>
    /// <param name="encodeArgs">The arguments between the input and the destination.</param>
    /// <param name="options">The options for starting the process.</param>
    /// <param name="callback">A method that will be called after the process has been started.</param>
    /// <returns>The process completion status.</returns>
    CompletionStatus EncodeFFmpegAvisynth(string source, string? destination, string? encodeArgs, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null);
    /// <summary>
    /// Encodes a VapourSynth script by opening it in FFmpeg with <c>-f vapoursynth</c>.
    /// The argument string is appended as written. An empty destination writes <c>-f null -</c>.
    /// </summary>
    /// <param name="source">The script file to open.</param>
    /// <param name="destination">The output file. Empty writes nowhere.</param>
    /// <param name="encodeArgs">The arguments between the input and the destination.</param>
    /// <param name="options">The options for starting the process.</param>
    /// <param name="callback">A method that will be called after the process has been started.</param>
    /// <returns>The process completion status.</returns>
    CompletionStatus EncodeFFmpegVapourSynth(string source, string? destination, string? encodeArgs, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null);
    /// <summary>
    /// Encodes a media file using X264 with specified arguments.
    /// </summary>
    /// <param name="source">The file to convert.</param>
    /// <param name="destination">The destination file.</param>
    /// <param name="encodeArgs">Additional arguments to pass to X264.</param>
    /// <param name="options">The options for starting the process.</param>
    /// <param name="callback">A method that will be called after the process has been started.</param>
    /// <returns>The process completion status.</returns>
    CompletionStatus EncodeX264(string source, string destination, string? encodeArgs, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null);
    /// <summary>
    /// Encodes an Avisynth script file using X264 with specified arguments.
    /// </summary>
    /// <param name="source">The file to convert.</param>
    /// <param name="destination">The destination file.</param>
    /// <param name="encodeArgs">Additional arguments to pass to X264.</param>
    /// <param name="options">The options for starting the process.</param>
    /// <param name="callback">A method that will be called after the process has been started.</param>
    /// <returns>The process completion status.</returns>
    CompletionStatus EncodeAvisynthToX264(string source, string destination, string? encodeArgs, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null);
    /// <summary>
    /// Encodes a VapourSynth script file using X264 with specified arguments.
    /// </summary>
    /// <param name="source">The file to convert.</param>
    /// <param name="destination">The destination file.</param>
    /// <param name="encodeArgs">Additional arguments to pass to X264.</param>
    /// <param name="options">The options for starting the process.</param>
    /// <param name="callback">A method that will be called after the process has been started.</param>
    /// <returns>The process completion status.</returns>
    CompletionStatus EncodeVapourSynthToX264(string source, string destination, string? encodeArgs, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null);
    /// <summary>
    /// Encodes an Avisynth script file using X265 with specified arguments.
    /// </summary>
    /// <param name="source">The file to convert.</param>
    /// <param name="destination">The destination file.</param>
    /// <param name="encodeArgs">Additional arguments to pass to X265.</param>
    /// <param name="options">The options for starting the process.</param>
    /// <param name="callback">A method that will be called after the process has been started.</param>
    /// <returns>The process completion status.</returns>
    CompletionStatus EncodeAvisynthToX265(string source, string destination, string? encodeArgs, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null);
    /// <summary>
    /// Encodes a VapourSynth script file using X265 with specified arguments.
    /// </summary>
    /// <param name="source">The file to convert.</param>
    /// <param name="destination">The destination file.</param>
    /// <param name="encodeArgs">Additional arguments to pass to X265.</param>
    /// <param name="options">The options for starting the process.</param>
    /// <param name="callback">A method that will be called after the process has been started.</param>
    /// <returns>The process completion status.</returns>
    CompletionStatus EncodeVapourSynthToX265(string source, string destination, string? encodeArgs, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null);
}
