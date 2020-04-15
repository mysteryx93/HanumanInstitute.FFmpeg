using System;
using System.Globalization;
using System.IO;
using System.Text;
using static System.FormattableString;
using HanumanInstitute.Encoder.Properties;

namespace HanumanInstitute.Encoder
{
    /// <summary>
    /// Provides functions to encode media files.
    /// </summary>
    public class MediaEncoder : IMediaEncoder
    {
        private readonly IProcessWorkerFactory factory;

        public MediaEncoder(IProcessWorkerFactory processFactory)
        {
            factory = processFactory ?? throw new ArgumentNullException(nameof(processFactory));
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
        public CompletionStatus ConvertToAviUtVideo(string source, string destination, bool audio, ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null)
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
        public CompletionStatus EncodeFFmpeg(string source, string destination, string videoCodec, string audioCodec, string encodeArgs, ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null)
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
        public CompletionStatus EncodeAvisynthToFFmpeg(string source, string destination, string videoCodec, string audioCodec, string encodeArgs, ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null)
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
        public CompletionStatus EncodeVapourSynthToFFmpeg(string source, string destination, string videoCodec, string audioCodec, string encodeArgs, ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null)
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
        public CompletionStatus EncodeX264(string source, string destination, string encodeArgs, ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null)
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
        public CompletionStatus EncodeAvisynthToX264(string source, string destination, string encodeArgs, ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null)
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
        public CompletionStatus EncodeVapourSynthToX264(string source, string destination, string encodeArgs, ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null)
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
        public CompletionStatus EncodeAvisynthToX265(string source, string destination, string encodeArgs, ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null)
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
        public CompletionStatus EncodeVapourSynthToX265(string source, string destination, string encodeArgs, ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null)
        {
            return EncodeX264Internal(SourceType.VapourSynth, EncoderApp.x265, source, destination, encodeArgs, options, callback);
        }



        //private string[] StringToList(string value)
        //{
        //    return string.IsNullOrEmpty(value) ? null : new string[] { value };
        //}

        private CompletionStatus EncodeFFmpegInternal(SourceType sourceType, string source, string destination, string videoCodec, string audioCodec, string encodeArgs, ProcessOptionsEncoder options, ProcessStartedEventHandler callback)
        {
            ArgHelper.ValidateNotNullOrEmpty(source, nameof(source));
            ArgHelper.ValidateNotNullOrEmpty(destination, nameof(destination));
            if (string.IsNullOrEmpty(videoCodec) && string.IsNullOrEmpty(audioCodec)) { throw new ArgumentException(Resources.CodecNullOrEmpty); }

            File.Delete(destination);
            StringBuilder Query = new StringBuilder();
            Query.Append("-y -i ");
            if (sourceType == SourceType.Direct)
            {
                Query.Append("\"");
                Query.Append(source);
                Query.Append("\"");
            }
            else
            {
                Query.Append("-"); // Pipe source
            }

            // Add video codec.
            if (string.IsNullOrEmpty(videoCodec))
            {
                Query.Append(" -vn");
            }
            else
            {
                Query.Append(Invariant($" -vcodec {videoCodec}"));
            }
            // Add audio codec.
            if (string.IsNullOrEmpty(audioCodec))
            {
                Query.Append(" -an");
            }
            else
            {
                Query.Append(Invariant($" -acodec {audioCodec}"));
            }

            if (!string.IsNullOrEmpty(encodeArgs))
            {
                Query.Append(" ");
                Query.Append(encodeArgs);
            }

            Query.Append(" \"");
            Query.Append(destination);
            Query.Append("\"");

            // Run FFmpeg with query.
            IProcessWorkerEncoder Worker = factory.CreateEncoder(options, callback);
            CompletionStatus Result = RunEncoderInternal(source, Query.ToString(), Worker, sourceType, EncoderApp.FFmpeg);
            return Result;
        }

        private CompletionStatus EncodeX264Internal(SourceType sourceType, EncoderApp encoderApp, string source, string destination, string encodeArgs, ProcessOptionsEncoder options, ProcessStartedEventHandler callback)
        {
            ArgHelper.ValidateNotNullOrEmpty(source, nameof(source));
            ArgHelper.ValidateNotNullOrEmpty(destination, nameof(destination));
            File.Delete(destination);

            StringBuilder Query = new StringBuilder();
            if (sourceType != SourceType.Direct)
            {
                Query.AppendFormat(CultureInfo.InvariantCulture, "--{0}y4m ", encoderApp == EncoderApp.x264 ? "demuxer " : "");
            }

            if (!string.IsNullOrEmpty(encodeArgs))
            {
                Query.Append(Invariant($"{encodeArgs} "));
            }

            Query.Append(Invariant($@"-o ""{destination}"" "));
            if (sourceType == SourceType.Direct)
            {
                Query.Append(Invariant($@"""{source}"""));
            }
            else
            {
                Query.Append("-");
            }

            // Run X264 or X265 with query.
            IProcessWorkerEncoder Worker = factory.CreateEncoder(options, callback);
            CompletionStatus Result = RunEncoderInternal(source, Query.ToString(), Worker, sourceType, encoderApp);
            return Result;
        }

        private CompletionStatus RunEncoderInternal(string source, string arguments, IProcessWorkerEncoder worker, SourceType sourceType, EncoderApp encoderApp)
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
}
