using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using static System.FormattableString;
using HanumanInstitute.Encoder.Properties;
using HanumanInstitute.Encoder.Services;

namespace HanumanInstitute.Encoder
{
    /// <summary>
    /// Provides functions to manage audio and video streams.
    /// </summary>
    public class MediaMuxer : IMediaMuxer
    {
        private readonly IProcessWorkerFactory factory;
        private readonly IFileSystemService fileSystem;
        private readonly IMediaInfoReader infoReader;

        public MediaMuxer(IProcessWorkerFactory processFactory) : this(processFactory, new FileSystemService(), new MediaInfoReader(processFactory)) { }

        public MediaMuxer(IProcessWorkerFactory processFactory, IFileSystemService fileSystemService, IMediaInfoReader infoReader)
        {
            factory = processFactory ?? throw new ArgumentNullException(nameof(processFactory));
            fileSystem = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
            this.infoReader = infoReader ?? throw new ArgumentNullException(nameof(infoReader));
        }

        /// <summary>
        /// Merges specified audio and video files.
        /// </summary>
        /// <param name="videoFile">The file containing the video.</param>
        /// <param name="audioFile">The file containing the audio.</param>
        /// <param name="destination">The destination file.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The process completion status.</returns>
        public CompletionStatus Muxe(string videoFile, string audioFile, string destination, ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null)
        {
            if (string.IsNullOrEmpty(audioFile)) { ArgHelper.ValidateNotNullOrEmpty(videoFile, nameof(videoFile)); }
            ArgHelper.ValidateNotNullOrEmpty(destination, nameof(destination));

            List<MediaStream> InputStreamList = new List<MediaStream>();
            MediaStream InputStream;
            if (!string.IsNullOrEmpty(videoFile))
            {
                InputStream = GetStreamInfo(videoFile, FFmpegStreamType.Video, options);
                if (InputStream != null)
                {
                    InputStreamList.Add(InputStream);
                }
            }
            if (!string.IsNullOrEmpty(audioFile))
            {
                InputStream = GetStreamInfo(audioFile, FFmpegStreamType.Audio, options);
                if (InputStream != null)
                {
                    InputStreamList.Add(InputStream);
                }
            }

            if (InputStreamList.Any())
            {
                return Muxe(InputStreamList, destination, options, callback);
            }
            else
            {
                return CompletionStatus.Failed;
            }
        }

        /// <summary>
        /// Merges the specified list of file streams.
        /// </summary>
        /// <param name="fileStreams">The list of file streams to include in the output.</param>
        /// <param name="destination">The destination file.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The process completion status.</returns>
        public CompletionStatus Muxe(IEnumerable<MediaStream> fileStreams, string destination, ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null)
        {
            ArgHelper.ValidateNotNull(fileStreams, nameof(fileStreams));
            ArgHelper.ValidateListNotNullOrEmpty(fileStreams, nameof(fileStreams));
            ArgHelper.ValidateNotNullOrEmpty(destination, nameof(destination));

            CompletionStatus Result = CompletionStatus.Success;
            List<string> TempFiles = new List<string>();
            fileSystem.Delete(destination);

            // FFMPEG fails to muxe H264 into MKV container. Converting to MP4 and then muxing with the audio, however, works.
            foreach (MediaStream item in fileStreams)
            {
                if (string.IsNullOrEmpty(item.Path) && item.Type != FFmpegStreamType.None)
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resources.ArgNullOrEmpty, "FFmpegStream.Path"));
                }
                if (item.Type == FFmpegStreamType.Video && (item.Format == "h264" || item.Format == "h265") && destination.EndsWith(".mkv", StringComparison.InvariantCulture))
                {
                    string NewFile = item.Path.Substring(0, item.Path.LastIndexOf('.')) + ".mp4";
                    Result = Muxe(new List<MediaStream>() { item }, NewFile, options);
                    TempFiles.Add(NewFile);
                    if (Result != CompletionStatus.Success)
                    {
                        break;
                    }
                }
            }

            if (Result == CompletionStatus.Success)
            {
                // Join audio and video files.
                StringBuilder Query = new StringBuilder();
                StringBuilder Map = new StringBuilder();
                Query.Append("-y ");
                int StreamIndex = 0;
                bool HasVideo = false, HasAudio = false, HasPcmDvdAudio = false;
                StringBuilder AacFix = new StringBuilder();
                var FileStreamsOrdered = fileStreams.OrderBy(f => f.Type);
                foreach (MediaStream item in FileStreamsOrdered)
                {
                    if (item.Type == FFmpegStreamType.Video)
                    {
                        HasVideo = true;
                    }

                    if (item.Type == FFmpegStreamType.Audio)
                    {
                        HasAudio = true;
                        if (item.Format == "aac")
                        {
                            AacFix.AppendFormat(CultureInfo.InvariantCulture, "-bsf:{0} aac_adtstoasc ", StreamIndex);
                        }

                        if (item.Format == "pcm_dvd")
                        {
                            HasPcmDvdAudio = true;
                        }
                    }
                    Query.Append("-i \"");
                    Query.Append(item.Path);
                    Query.Append("\" ");
                    Map.Append("-map ");
                    Map.Append(StreamIndex++);
                    Map.Append(":");
                    Map.Append(item.Index);
                    Map.Append(" ");
                }
                if (!HasVideo && !HasAudio)
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resources.ArgNullOrEmpty, nameof(fileStreams)), nameof(fileStreams));
                }

                if (HasVideo)
                {
                    Query.Append("-vcodec copy ");
                }

                if (HasAudio)
                {
                    Query.Append(HasPcmDvdAudio ? "-acodec pcm_s16le " : "-acodec copy ");
                }

                Query.Append(Map);
                // FFMPEG-encoded AAC streams are invalid and require an extra flag to join.
                if (AacFix.Length > 0 && HasVideo)
                {
                    Query.Append(AacFix);
                }

                Query.Append("\"");
                Query.Append(destination);
                Query.Append("\"");
                IProcessWorkerEncoder Worker = factory.CreateEncoder(options, callback);
                Result = Worker.RunEncoder(Query.ToString(), EncoderApp.FFmpeg);
            }

            // Delete temp file.
            foreach (string item in TempFiles)
            {
                fileSystem.Delete(item);
            }
            return Result;
        }

        /// <summary>
        /// Returns stream information as FFmpegStream about specified media file that can be used to call a muxing operation.
        /// </summary>
        /// <param name="path">The path of the media file to query.</param>
        /// <param name="streamType">The type of media stream to query.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <returns>A FFmpegStream object.</returns>
        private MediaStream GetStreamInfo(string path, FFmpegStreamType streamType, ProcessOptionsEncoder options)
        {
            IFileInfoFFmpeg FileInfo = infoReader.GetFileInfo(path, options);
            MediaStreamInfo StreamInfo = FileInfo.FileStreams?.FirstOrDefault(x => x.StreamType == streamType);
            if (StreamInfo != null)
            {
                return new MediaStream()
                {
                    Path = path,
                    Index = StreamInfo.Index,
                    Format = StreamInfo.Format,
                    Type = streamType
                };
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Extracts the video stream from specified file.
        /// </summary>
        /// <param name="source">The media file to extract from.</param>
        /// <param name="destination">The destination file.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The process completion status.</returns>
        public CompletionStatus ExtractVideo(string source, string destination, ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null)
        {
            return ExtractStream(@"-y -i ""{0}"" -vcodec copy -an ""{1}""", source, destination, options, callback);
        }

        /// <summary>
        /// Extracts the audio stream from specified file.
        /// </summary>
        /// <param name="source">The media file to extract from.</param>
        /// <param name="destination">The destination file.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The process completion status.</returns>
        public CompletionStatus ExtractAudio(string source, string destination, ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null)
        {
            return ExtractStream(@"-y -i ""{0}"" -vn -acodec copy ""{1}""", source, destination, options, callback);
        }

        /// <summary>
        /// Extracts an audio or video stream from specified file.
        /// </summary>
        /// <param name="args">The arguments string that will be passed to FFmpeg.</param>
        /// <param name="source">The media file to extract from.</param>
        /// <param name="destination">The destination file.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The process completion status.</returns>
        private CompletionStatus ExtractStream(string args, string source, string destination, ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null)
        {
            ArgHelper.ValidateNotNullOrEmpty(source, nameof(source));
            ArgHelper.ValidateNotNullOrEmpty(destination, nameof(destination));

            fileSystem.Delete(destination);
            IProcessWorkerEncoder Worker = factory.CreateEncoder(options, callback);
            return Worker.RunEncoder(string.Format(CultureInfo.InvariantCulture, args, source, destination), EncoderApp.FFmpeg);
        }

        /// <summary>
        /// Concatenates (merges) all specified files.
        /// </summary>
        /// <param name="files">The files to merge.</param>
        /// <param name="destination">The destination file.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The process completion status.</returns>
        public CompletionStatus Concatenate(IEnumerable<string> files, string destination, ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null)
        {
            ArgHelper.ValidateNotNull(files, nameof(files));
            ArgHelper.ValidateListNotNullOrEmpty(files, nameof(files));
            ArgHelper.ValidateNotNullOrEmpty(destination, nameof(destination));

            CompletionStatus Result = CompletionStatus.None;

            // Write temp file.
            string TempFile = fileSystem.GetTempFile();
            StringBuilder TempContent = new StringBuilder();
            foreach (string item in files)
            {
                TempContent.AppendFormat(CultureInfo.InvariantCulture, "file '{0}'", item).AppendLine();
            }
            fileSystem.WriteAllText(TempFile, TempContent.ToString());

            string Query = Invariant($@"-y -f concat -fflags +genpts -async 1 -safe 0 -i ""{TempFile}"" -c copy ""{destination}""");

            IProcessWorkerEncoder Worker = factory.CreateEncoder(options, callback);
            Result = Worker.RunEncoder(Query.ToString(CultureInfo.InvariantCulture), EncoderApp.FFmpeg);

            fileSystem.Delete(TempFile);
            return Result;
        }

        /// <summary>
        /// Truncates a media file from specified start position with specified duration. This can result in data loss or corruption if not splitting exactly on a framekey.
        /// </summary>
        /// <param name="source">The source file to truncate.</param>
        /// <param name="destination">The output file to write.</param>
        /// <param name="startPos">The position where to start copying. Anything before this position will be ignored. TimeSpan.Zero or null to start from beginning.</param>
        /// <param name="duration">The duration after which to stop copying. Anything after this duration will be ignored. TimeSpan.Zero or null to copy until the end.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The process completion status.</returns>
        public CompletionStatus Truncate(string source, string destination, TimeSpan? startPos, TimeSpan? duration = null, ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null)
        {
            ArgHelper.ValidateNotNullOrEmpty(source, nameof(source));
            ArgHelper.ValidateNotNullOrEmpty(destination, nameof(destination));

            fileSystem.Delete(destination);
            IProcessWorkerEncoder Worker = factory.CreateEncoder(options, callback);
            string Args = string.Format(CultureInfo.InvariantCulture, @"-i ""{0}"" -vcodec copy -acodec copy {1}{2}""{3}""", source,
                startPos.HasValue && startPos > TimeSpan.Zero ? $"-ss {startPos:c} " : "",
                duration.HasValue && duration > TimeSpan.Zero ? $"-t {duration:c} " : "",
                destination);
            return Worker.RunEncoder(Args, EncoderApp.FFmpeg);
        }
    }
}
