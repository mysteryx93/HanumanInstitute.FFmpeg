using System.IO;

// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo

namespace HanumanInstitute.FFmpeg;

/// <inheritdoc />
public class MediaEncoder : IMediaEncoder
{
    private readonly IProcessService _factory;

    /// <summary>
    /// Initializes a new instance of the MediaEncoder class
    /// </summary>
    /// <param name="processFactory">The Factory responsible for creating processes.</param>
    public MediaEncoder(IProcessService processFactory) =>
        _factory = processFactory.CheckNotNull(nameof(processFactory));

    /// <summary>
    /// Sets the owner of the process windows.
    /// </summary>
    public object? Owner { get; set; }

    /// <inheritdoc />
    public CompletionStatus ConvertToAviUtVideo(string source, string destination, bool audio, ProcessOptionsEncoder? options = null,
        ProcessStartedEventHandler? callback = null) =>
        // -vcodec huffyuv or utvideo, -acodec pcm_s16le
        EncodeFFmpeg(source, destination, "utvideo", audio ? "pcm_s16le" : null, null, options, callback);

    /// <inheritdoc />
    public CompletionStatus EncodeFFmpeg(string source, string destination, string? videoCodec, string? audioCodec, string? encodeArgs,
        ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null) =>
        EncodeFFmpegInternal(SourceType.Direct, source, destination, videoCodec, audioCodec, encodeArgs, options, callback);

    // /// <summary>
    // /// Encodes a media file using FFmpeg with specified arguments.
    // /// </summary>
    // /// <param name="source">The file to convert.</param>
    // /// <param name="destination">The destination file.</param>
    // /// <param name="videoCodec">The codec(s) to use to encode the video stream(s).</param>
    // /// <param name="audioCodec">The codec(s) to use to encode the audio stream(s).</param>
    // /// <param name="encodeArgs">Additional arguments to pass to FFmpeg.</param>
    // /// <param name="options">The options for starting the process.</param>
    // /// <param name="callback">A method that will be called after the process has been started.</param>
    // /// <returns>The process completion status.</returns>
    //public CompletionStatus EncodeFFmpeg(string source, string destination, string[] videoCodec, string[] audioCodec, string[] encodeArgs, ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null) {
    //    return EncodeFFmpegInternal(SourceType.Direct, source, destination, videoCodec, audioCodec, encodeArgs, options, callback);
    //}

    /// <inheritdoc />
    public CompletionStatus EncodeAvisynthToFFmpeg(string source, string destination, string? videoCodec, string? audioCodec,
        string? encodeArgs, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null) =>
        EncodeFFmpegInternal(SourceType.Direct, source, destination, videoCodec, audioCodec, encodeArgs, options, callback);

    /// <inheritdoc />
    public CompletionStatus EncodeVapourSynthToFFmpeg(string source, string destination, string? videoCodec, string? audioCodec,
        string? encodeArgs, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null) =>
        EncodeFFmpegInternal(SourceType.VapourSynth, source, destination, videoCodec, audioCodec, encodeArgs, options, callback);

    /// <inheritdoc />
    public CompletionStatus EncodeX264(string source, string destination, string? encodeArgs, ProcessOptionsEncoder? options = null,
        ProcessStartedEventHandler? callback = null) =>
        EncodeX264Internal(SourceType.Direct, EncoderApp.x264, source, destination, encodeArgs, options, callback);

    /// <inheritdoc />
    public CompletionStatus EncodeAvisynthToX264(string source, string destination, string? encodeArgs,
        ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null) =>
        EncodeX264Internal(SourceType.Avisynth, EncoderApp.x264, source, destination, encodeArgs, options, callback);

    /// <inheritdoc />
    public CompletionStatus EncodeVapourSynthToX264(string source, string destination, string? encodeArgs,
        ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null) =>
        EncodeX264Internal(SourceType.VapourSynth, EncoderApp.x264, source, destination, encodeArgs, options, callback);

    // /// X265 only supports YUV and Y4M sources.
    // /// <summary>
    // /// Encodes a media file using X265 with specified arguments.
    // /// </summary>
    // /// <param name="source">The file to convert.</param>
    // /// <param name="destination">The destination file.</param>
    // /// <param name="encodeArgs">Additional arguments to pass to X265.</param>
    // /// <param name="options">The options for starting the process.</param>
    // /// <param name="callback">A method that will be called after the process has been started.</param>
    // /// <returns>The process completion status.</returns>
    //public CompletionStatus EncodeX265(string source, string destination, string encodeArgs, ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null) {
    //    return EncodeX264Internal(SourceType.Direct, EncoderApp.x265, source, destination, encodeArgs, options, callback);
    //}

    /// <inheritdoc />
    public CompletionStatus EncodeAvisynthToX265(string source, string destination, string? encodeArgs,
        ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null) =>
        EncodeX264Internal(SourceType.Avisynth, EncoderApp.x265, source, destination, encodeArgs, options, callback);

    /// <inheritdoc />
    public CompletionStatus EncodeVapourSynthToX265(string source, string destination, string? encodeArgs,
        ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null) =>
        EncodeX264Internal(SourceType.VapourSynth, EncoderApp.x265, source, destination, encodeArgs, options, callback);

    private CompletionStatus EncodeFFmpegInternal(SourceType sourceType, string source, string destination, string? videoCodec,
        string? audioCodec, string? encodeArgs, ProcessOptionsEncoder? options, ProcessStartedEventHandler? callback)
    {
        source.CheckNotNullOrEmpty(nameof(source));
        destination.CheckNotNullOrEmpty(nameof(destination));
        if (string.IsNullOrEmpty(videoCodec) && string.IsNullOrEmpty(audioCodec))
        {
            throw new ArgumentException(Resources.CodecNullOrEmpty);
        }

        File.Delete(destination);
        var query = new StringBuilder();
        query.Append("-y -i ");
        if (sourceType == SourceType.Direct)
        {
            query.Append("\"");
            query.Append(source);
            query.Append("\"");
        }
        else
        {
            query.Append("-"); // Pipe source
        }

        // Add video codec.
        query.Append(videoCodec.HasValue() ?
            Invariant($" -vcodec {videoCodec}") :
            " -vn");

        // Add audio codec.
        query.Append(audioCodec.HasValue() ?
            Invariant($" -acodec {audioCodec}") :
            " -an");

        if (!string.IsNullOrEmpty(encodeArgs))
        {
            query.Append(" ");
            query.Append(encodeArgs);
        }

        query.Append(" \"");
        query.Append(destination);
        query.Append("\"");

        // Run FFmpeg with query.
        var worker = _factory.CreateEncoder(Owner, options, callback);
        var result = RunEncoderInternal(source, query.ToString(), worker, sourceType, EncoderApp.FFmpeg);
        return result;
    }

    private CompletionStatus EncodeX264Internal(SourceType sourceType, EncoderApp encoderApp, string source, string destination,
        string? encodeArgs, ProcessOptionsEncoder? options, ProcessStartedEventHandler? callback)
    {
        source.CheckNotNullOrEmpty(nameof(source));
        destination.CheckNotNullOrEmpty(nameof(destination));
        File.Delete(destination);

        var query = new StringBuilder();
        if (sourceType != SourceType.Direct)
        {
            query.AppendFormat(CultureInfo.InvariantCulture, "--{0}y4m ", encoderApp == EncoderApp.x264 ? "demuxer " : "");
        }

        if (!string.IsNullOrEmpty(encodeArgs))
        {
            query.Append(Invariant($"{encodeArgs} "));
        }

        query.Append(Invariant($@"-o ""{destination}"" "));
        query.Append(sourceType == SourceType.Direct ?
            Invariant($@"""{source}""") :
            "-");

        // Run X264 or X265 with query.
        var worker = _factory.CreateEncoder(Owner, options, callback);
        var result = RunEncoderInternal(source, query.ToString(), worker, sourceType, encoderApp);
        return result;
    }

    private static CompletionStatus RunEncoderInternal(string source, string arguments, IProcessWorkerEncoder worker, SourceType sourceType,
        EncoderApp encoderApp) =>
        sourceType switch
        {
            SourceType.Direct => worker.RunEncoder(arguments, encoderApp),
            SourceType.Avisynth => worker.RunAvisynthToEncoder(source, arguments, encoderApp),
            SourceType.VapourSynth => worker.RunVapourSynthToEncoder(source, arguments, encoderApp),
            _ => CompletionStatus.Failed
        };

    /// <summary>
    /// Represents the type of media source.
    /// </summary>
    private enum SourceType
    {
        /// <summary>
        /// Source is directly opened by the encoder.
        /// </summary>
        Direct,
        /// <summary>
        /// Source is an Avisynth script opened with avs2pipemod.
        /// </summary>
        Avisynth,
        /// <summary>
        /// Source is a VapourSynth script opened with vspipe.
        /// </summary>
        VapourSynth
    }
}
