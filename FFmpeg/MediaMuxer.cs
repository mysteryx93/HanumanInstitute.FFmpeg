using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using HanumanInstitute.FFmpeg.Properties;
using HanumanInstitute.FFmpeg.Services;
using static System.FormattableString;

namespace HanumanInstitute.FFmpeg
{
    /// <summary>
    /// Provides functions to manage audio and video streams.
    /// </summary>
    public class MediaMuxer : IMediaMuxer
    {
        private readonly IProcessWorkerFactory _factory;
        private readonly IFileSystemService _fileSystem;
        private readonly IMediaInfoReader _infoReader;

        //public MediaMuxer(IProcessWorkerFactory processFactory, IUserInterfaceManager uiManager) : this(processFactory, uiManager, new FileSystemService(), new MediaInfoReader(processFactory)) { }

        public MediaMuxer(IProcessWorkerFactory processFactory, IFileSystemService fileSystemService, IMediaInfoReader infoReader)
        {
            _factory = processFactory ?? throw new ArgumentNullException(nameof(processFactory));
            _fileSystem = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
            _infoReader = infoReader ?? throw new ArgumentNullException(nameof(infoReader));
        }

        private object? _owner;
        /// <summary>
        /// Sets the owner of the process windows.
        /// </summary>
        public IMediaMuxer SetOwner(object owner)
        {
            _owner = owner;
            return this;
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
        public CompletionStatus Muxe(string? videoFile, string? audioFile, string destination, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null)
        {
            if (string.IsNullOrEmpty(audioFile)) { videoFile.CheckNotNullOrEmpty(nameof(videoFile)); }
            destination.CheckNotNullOrEmpty(nameof(destination));

            var inputStreamList = new List<MediaStream>();
            MediaStream? inputStream;
            if (!string.IsNullOrEmpty(videoFile))
            {
                inputStream = GetStreamInfo(videoFile!, FFmpegStreamType.Video, options);
                if (inputStream != null)
                {
                    inputStreamList.Add(inputStream);
                }
            }
            if (!string.IsNullOrEmpty(audioFile))
            {
                inputStream = GetStreamInfo(audioFile!, FFmpegStreamType.Audio, options);
                if (inputStream != null)
                {
                    inputStreamList.Add(inputStream);
                }
            }

            if (inputStreamList.Any())
            {
                return Muxe(inputStreamList, destination, options, callback);
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
        public CompletionStatus Muxe(IEnumerable<MediaStream> fileStreams, string destination, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null)
        {
            fileStreams.CheckNotNull(nameof(fileStreams));
            fileStreams.CheckNotNullOrEmpty(nameof(fileStreams));
            destination.CheckNotNullOrEmpty(nameof(destination));

            var result = CompletionStatus.Success;
            var tempFiles = new List<string>();
            _fileSystem.Delete(destination);

            // FFMPEG fails to muxe H264 into MKV container. Converting to MP4 and then muxing with the audio, however, works.
            foreach (var item in fileStreams)
            {
                if (string.IsNullOrEmpty(item.Path) && item.Type != FFmpegStreamType.None)
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resources.ArgumentNullOrEmpty, "FFmpegStream.Path"));
                }
                if (item.Type == FFmpegStreamType.Video && (item.Format == "h264" || item.Format == "h265") && destination.EndsWith(".mkv", StringComparison.InvariantCulture))
                {
                    var newFile = item.Path.Substring(0, item.Path.LastIndexOf('.')) + ".mp4";
                    result = Muxe(new List<MediaStream>() { item }, newFile, options);
                    tempFiles.Add(newFile);
                    if (result != CompletionStatus.Success)
                    {
                        break;
                    }
                }
            }

            if (result == CompletionStatus.Success)
            {
                // Join audio and video files.
                var query = new StringBuilder();
                var map = new StringBuilder();
                query.Append("-y ");
                var streamIndex = 0;
                bool hasVideo = false, hasAudio = false, hasPcmDvdAudio = false;
                var aacFix = new StringBuilder();
                var fileStreamsOrdered = fileStreams.OrderBy(f => f.Type);
                foreach (var item in fileStreamsOrdered)
                {
                    if (item.Type == FFmpegStreamType.Video)
                    {
                        hasVideo = true;
                    }

                    if (item.Type == FFmpegStreamType.Audio)
                    {
                        hasAudio = true;
                        if (item.Format == "aac")
                        {
                            aacFix.AppendFormat(CultureInfo.InvariantCulture, "-bsf:{0} aac_adtstoasc ", streamIndex);
                        }

                        if (item.Format == "pcm_dvd")
                        {
                            hasPcmDvdAudio = true;
                        }
                    }
                    query.Append("-i \"");
                    query.Append(item.Path);
                    query.Append("\" ");
                    map.Append("-map ");
                    map.Append(streamIndex++);
                    map.Append(":");
                    map.Append(item.Index);
                    map.Append(" ");
                }
                if (!hasVideo && !hasAudio)
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resources.ArgumentNullOrEmpty, nameof(fileStreams)), nameof(fileStreams));
                }

                if (hasVideo)
                {
                    query.Append("-vcodec copy ");
                }

                if (hasAudio)
                {
                    query.Append(hasPcmDvdAudio ? "-acodec pcm_s16le " : "-acodec copy ");
                }

                query.Append(map);
                // FFMPEG-encoded AAC streams are invalid and require an extra flag to join.
                if (aacFix.Length > 0 && hasVideo)
                {
                    query.Append(aacFix);
                }

                query.Append("\"");
                query.Append(destination);
                query.Append("\"");
                var worker = _factory.CreateEncoder(_owner, options, callback);

                result = worker.RunEncoder(query.ToString(), EncoderApp.FFmpeg);
            }

            // Delete temp file.
            foreach (var item in tempFiles)
            {
                _fileSystem.Delete(item);
            }
            return result;
        }

        /// <summary>
        /// Returns stream information as FFmpegStream about specified media file that can be used to call a muxing operation.
        /// </summary>
        /// <param name="path">The path of the media file to query.</param>
        /// <param name="streamType">The type of media stream to query.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <returns>A FFmpegStream object.</returns>
        private MediaStream? GetStreamInfo(string path, FFmpegStreamType streamType, ProcessOptionsEncoder? options)
        {
            var fileInfo = _infoReader.GetFileInfo(path, options);
            var streamInfo = fileInfo.FileStreams?.FirstOrDefault(x => x.StreamType == streamType);
            if (streamInfo != null)
            {
                return new MediaStream()
                {
                    Path = path,
                    Index = streamInfo.Index,
                    Format = streamInfo.Format,
                    Type = streamType
                };
            }
            return null;
        }

        /// <summary>
        /// Extracts the video stream from specified file.
        /// </summary>
        /// <param name="source">The media file to extract from.</param>
        /// <param name="destination">The destination file.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The process completion status.</returns>
        public CompletionStatus ExtractVideo(string source, string destination, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null)
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
        public CompletionStatus ExtractAudio(string source, string destination, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null)
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
        private CompletionStatus ExtractStream(string args, string source, string destination, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null)
        {
            source.CheckNotNullOrEmpty(nameof(source));
            destination.CheckNotNullOrEmpty(nameof(destination));

            _fileSystem.Delete(destination);
            var worker = _factory.CreateEncoder(_owner, options, callback);

            return worker.RunEncoder(string.Format(CultureInfo.InvariantCulture, args, source, destination), EncoderApp.FFmpeg);
        }

        /// <summary>
        /// Concatenates (merges) all specified files.
        /// </summary>
        /// <param name="files">The files to merge.</param>
        /// <param name="destination">The destination file.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The process completion status.</returns>
        public CompletionStatus Concatenate(IEnumerable<string> files, string destination, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null)
        {
            files.CheckNotNull(nameof(files));
            files.CheckNotNullOrEmpty(nameof(files));
            destination.CheckNotNullOrEmpty(nameof(destination));

            var result = CompletionStatus.None;

            // Write temp file.
            var tempFile = _fileSystem.GetTempFile();
            var tempContent = new StringBuilder();
            foreach (var item in files)
            {
                tempContent.AppendFormat(CultureInfo.InvariantCulture, "file '{0}'", item).AppendLine();
            }
            _fileSystem.WriteAllText(tempFile, tempContent.ToString());

            var query = Invariant($@"-y -f concat -fflags +genpts -async 1 -safe 0 -i ""{tempFile}"" -c copy ""{destination}""");

            var worker = _factory.CreateEncoder(_owner, options, callback);

            result = worker.RunEncoder(query.ToString(CultureInfo.InvariantCulture), EncoderApp.FFmpeg);

            _fileSystem.Delete(tempFile);
            return result;
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
        public CompletionStatus Truncate(string source, string destination, TimeSpan? startPos, TimeSpan? duration = null, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null)
        {
            source.CheckNotNullOrEmpty(nameof(source));
            destination.CheckNotNullOrEmpty(nameof(destination));

            _fileSystem.Delete(destination);
            var worker = _factory.CreateEncoder(_owner, options, callback);

            var args = string.Format(CultureInfo.InvariantCulture, @"-i ""{0}"" -vcodec copy -acodec copy {1}{2}""{3}""", source,
                startPos.HasValue && startPos > TimeSpan.Zero ? $"-ss {startPos:c} " : "",
                duration.HasValue && duration > TimeSpan.Zero ? $"-t {duration:c} " : "",
                destination);
            return worker.RunEncoder(args, EncoderApp.FFmpeg);
        }
    }
}
