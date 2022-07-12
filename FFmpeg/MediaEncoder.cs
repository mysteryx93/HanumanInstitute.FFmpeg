using System.IO;
using HanumanInstitute.FFmpeg.Properties;
using static System.FormattableString;

namespace HanumanInstitute.FFmpeg;

/// <summary>
/// Provides functions to encode media files.
/// </summary>
public class MediaEncoder : IMediaEncoder
{
    private readonly IProcessWorkerFactory _factory;

    public MediaEncoder(IProcessWorkerFactory processFactory)
    {
        _factory = processFactory ?? throw new ArgumentNullException(nameof(processFactory));
    }

    private object? _owner;
    /// <summary>
    /// Sets the owner of the process windows.
    /// </summary>
    public IMediaEncoder SetOwner(object owner)
    {
        _owner = owner;
        return this;
    }

    /// <summary>
    /// Converts specified file into AVI UT Video format.
    /// </summary>
    /// <param name="source">The file to convert.</param>
    /// <param name="destination">The destination file, ending with .AVI</param>
    /// <param name="audio">Whether to encode audio.</param>
    /// <param name="options">The options for starting the process.</param>
    /// <param name="callback">A method that will be called after the process has been started.</param>
    /// <returns>The process completion status.</returns>
    public CompletionStatus ConvertToAviUtVideo(string source, string destination, bool audio, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null)
    {
        // -vcodec huffyuv or utvideo, -acodec pcm_s16le
        return EncodeFFmpeg(source, destination, "utvideo", audio ? "pcm_s16le" : null, null, options, callback);
    }

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
    public CompletionStatus EncodeFFmpeg(string source, string destination, string? videoCodec, string? audioCodec, string? encodeArgs, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null)
    {
        return EncodeFFmpegInternal(SourceType.Direct, source, destination, videoCodec, audioCodec, encodeArgs, options, callback);
    }

    /// <summary>
    /// Encodes a media file using FFmpeg with specified arguments.
    /// </summary>
    /// <param name="source">The file to convert.</param>
    /// <param name="destination">The destination file.</param>
    /// <param name="videoCodec">The codec(s) to use to encode the video stream(s).</param>
    /// <param name="audioCodec">The codec(s) to use to encode the audio stream(s).</param>
    /// <param name="encodeArgs">Additional arguments to pass to FFmpeg.</param>
    /// <param name="options">The options for starting the process.</param>
    /// <param name="callback">A method that will be called after the process has been started.</param>
    /// <returns>The process completion status.</returns>
    //public CompletionStatus EncodeFFmpeg(string source, string destination, string[] videoCodec, string[] audioCodec, string[] encodeArgs, ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null) {
    //    return EncodeFFmpegInternal(SourceType.Direct, source, destination, videoCodec, audioCodec, encodeArgs, options, callback);
    //}

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
    public CompletionStatus EncodeAvisynthToFFmpeg(string source, string destination, string? videoCodec, string? audioCodec, string? encodeArgs, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null)
    {
        return EncodeFFmpegInternal(SourceType.Avisynth, source, destination, videoCodec, audioCodec, encodeArgs, options, callback);
    }

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
    public CompletionStatus EncodeVapourSynthToFFmpeg(string source, string destination, string? videoCodec, string? audioCodec, string? encodeArgs, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null)
    {
        return EncodeFFmpegInternal(SourceType.VapourSynth, source, destination, videoCodec, audioCodec, encodeArgs, options, callback);
    }

    /// <summary>
    /// Encodes a media file using X264 with specified arguments.
    /// </summary>
    /// <param name="source">The file to convert.</param>
    /// <param name="destination">The destination file.</param>
    /// <param name="encodeArgs">Additional arguments to pass to X264.</param>
    /// <param name="options">The options for starting the process.</param>
    /// <param name="callback">A method that will be called after the process has been started.</param>
    /// <returns>The process completion status.</returns>
    public CompletionStatus EncodeX264(string source, string destination, string? encodeArgs, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null)
    {
        return EncodeX264Internal(SourceType.Direct, EncoderApp.x264, source, destination, encodeArgs, options, callback);
    }

    /// <summary>
    /// Encodes an Avisynth script file using X264 with specified arguments.
    /// </summary>
    /// <param name="source">The file to convert.</param>
    /// <param name="destination">The destination file.</param>
    /// <param name="encodeArgs">Additional arguments to pass to X264.</param>
    /// <param name="options">The options for starting the process.</param>
    /// <param name="callback">A method that will be called after the process has been started.</param>
    /// <returns>The process completion status.</returns>
    public CompletionStatus EncodeAvisynthToX264(string source, string destination, string? encodeArgs, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null)
    {
        return EncodeX264Internal(SourceType.Avisynth, EncoderApp.x264, source, destination, encodeArgs, options, callback);
    }

    /// <summary>
    /// Encodes a VapourSynth script file using X264 with specified arguments.
    /// </summary>
    /// <param name="source">The file to convert.</param>
    /// <param name="destination">The destination file.</param>
    /// <param name="encodeArgs">Additional arguments to pass to X264.</param>
    /// <param name="options">The options for starting the process.</param>
    /// <param name="callback">A method that will be called after the process has been started.</param>
    /// <returns>The process completion status.</returns>
    public CompletionStatus EncodeVapourSynthToX264(string source, string destination, string? encodeArgs, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null)
    {
        return EncodeX264Internal(SourceType.VapourSynth, EncoderApp.x264, source, destination, encodeArgs, options, callback);
    }

    /// X265 only supports YUV and Y4M sources.
    /// <summary>
    /// Encodes a media file using X265 with specified arguments.
    /// </summary>
    /// <param name="source">The file to convert.</param>
    /// <param name="destination">The destination file.</param>
    /// <param name="encodeArgs">Additional arguments to pass to X265.</param>
    /// <param name="options">The options for starting the process.</param>
    /// <param name="callback">A method that will be called after the process has been started.</param>
    /// <returns>The process completion status.</returns>
    //public CompletionStatus EncodeX265(string source, string destination, string encodeArgs, ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null) {
    //    return EncodeX264Internal(SourceType.Direct, EncoderApp.x265, source, destination, encodeArgs, options, callback);
    //}

    /// <summary>
    /// Encodes an Avisynth script file using X265 with specified arguments.
    /// </summary>
    /// <param name="source">The file to convert.</param>
    /// <param name="destination">The destination file.</param>
    /// <param name="encodeArgs">Additional arguments to pass to X265.</param>
    /// <param name="options">The options for starting the process.</param>
    /// <param name="callback">A method that will be called after the process has been started.</param>
    /// <returns>The process completion status.</returns>
    public CompletionStatus EncodeAvisynthToX265(string source, string destination, string? encodeArgs, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null)
    {
        return EncodeX264Internal(SourceType.Avisynth, EncoderApp.x265, source, destination, encodeArgs, options, callback);
    }

    /// <summary>
    /// Encodes a VapourSynth script file using X265 with specified arguments.
    /// </summary>
    /// <param name="source">The file to convert.</param>
    /// <param name="destination">The destination file.</param>
    /// <param name="encodeArgs">Additional arguments to pass to X265.</param>
    /// <param name="options">The options for starting the process.</param>
    /// <param name="callback">A method that will be called after the process has been started.</param>
    /// <returns>The process completion status.</returns>
    public CompletionStatus EncodeVapourSynthToX265(string source, string destination, string? encodeArgs, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null)
    {
        return EncodeX264Internal(SourceType.VapourSynth, EncoderApp.x265, source, destination, encodeArgs, options, callback);
    }



    //private string[] StringToList(string value)
    //{
    //    return string.IsNullOrEmpty(value) ? null : new string[] { value };
    //}

    private CompletionStatus EncodeFFmpegInternal(SourceType sourceType, string source, string destination, string? videoCodec, string? audioCodec, string? encodeArgs, ProcessOptionsEncoder? options, ProcessStartedEventHandler? callback)
    {
        source.CheckNotNullOrEmpty(nameof(source));
        destination.CheckNotNullOrEmpty(nameof(destination));
        if (string.IsNullOrEmpty(videoCodec) && string.IsNullOrEmpty(audioCodec)) { throw new ArgumentException(Resources.CodecNullOrEmpty); }

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
        if (string.IsNullOrEmpty(videoCodec))
        {
            query.Append(" -vn");
        }
        else
        {
            query.Append(Invariant($" -vcodec {videoCodec}"));
        }
        // Add audio codec.
        if (string.IsNullOrEmpty(audioCodec))
        {
            query.Append(" -an");
        }
        else
        {
            query.Append(Invariant($" -acodec {audioCodec}"));
        }

        if (!string.IsNullOrEmpty(encodeArgs))
        {
            query.Append(" ");
            query.Append(encodeArgs);
        }

        query.Append(" \"");
        query.Append(destination);
        query.Append("\"");

        // Run FFmpeg with query.
        var worker = _factory.CreateEncoder(_owner, options, callback);
        var result = RunEncoderInternal(source, query.ToString(), worker, sourceType, EncoderApp.FFmpeg);
        return result;
    }

    private CompletionStatus EncodeX264Internal(SourceType sourceType, EncoderApp encoderApp, string source, string destination, string? encodeArgs, ProcessOptionsEncoder? options, ProcessStartedEventHandler? callback)
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
        if (sourceType == SourceType.Direct)
        {
            query.Append(Invariant($@"""{source}"""));
        }
        else
        {
            query.Append("-");
        }

        // Run X264 or X265 with query.
        var worker = _factory.CreateEncoder(_owner, options, callback);
        var result = RunEncoderInternal(source, query.ToString(), worker, sourceType, encoderApp);
        return result;
    }

    private static CompletionStatus RunEncoderInternal(string source, string arguments, IProcessWorkerEncoder worker, SourceType sourceType, EncoderApp encoderApp)
    {
        if (sourceType == SourceType.Direct)
        {
            return worker.RunEncoder(arguments, encoderApp);
        }
        else if (sourceType == SourceType.Avisynth)
        {
            return worker.RunAvisynthToEncoder(source, arguments, encoderApp);
        }
        else if (sourceType == SourceType.VapourSynth)
        {
            return worker.RunVapourSynthToEncoder(source, arguments, encoderApp);
        }
        else
        {
            return CompletionStatus.Failed;
        }
    }

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
        /// Source is an Avisynth script opened with avs2pipemod.exe
        /// </summary>
        Avisynth,
        /// <summary>
        /// Source is a VapourSynth script opened with vspipe.exe
        /// </summary>
        VapourSynth
    }
}