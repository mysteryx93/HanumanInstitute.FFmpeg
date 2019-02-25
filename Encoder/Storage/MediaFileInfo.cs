//using System;
//using System.Linq;
//using System.Collections.Generic;

//namespace EmergenceGuardian.FFmpeg {

//    public interface IMediaFileInfo { }

//    /// <summary>
//    /// Contains the file information returned by FFmpeg.
//    /// </summary>
//    public class MediaFileInfoFFmpeg : IMediaFileInfo {
//        public long FrameCount { get; set; }
//        public TimeSpan FileDuration { get; set; }
//        public List<MediaStreamInfo> FileStreams { get; set; }

//        public MediaFileInfoFFmpeg() { }

//        public MediaFileInfoFFmpeg(long frameCount, TimeSpan fileDuration, List<MediaStreamInfo> fileStreams) {
//            this.FrameCount = frameCount;
//            this.FileDuration = fileDuration;
//            this.FileStreams = fileStreams;
//        }

//        /// <summary>
//        /// Gets the first video stream from FileStreams.
//        /// </summary>
//        /// <returns>A FFmpegVideoStreamInfo object.</returns>
//        public MediaVideoStreamInfo VideoStream => GetStream(FFmpegStreamType.Video) as MediaVideoStreamInfo;

//        /// <summary>
//        /// Gets the first audio stream from FileStreams.
//        /// </summary>
//        /// <returns>A FFmpegAudioStreamInfo object.</returns>
//        public MediaAudioStreamInfo AudioStream => GetStream(FFmpegStreamType.Audio) as MediaAudioStreamInfo;

//        /// <summary>
//        /// Returns the first stream of specified type.
//        /// </summary>
//        /// <param name="streamType">The type of stream to search for.</param>
//        /// <returns>A FFmpegStreamInfo object.</returns>
//        private MediaStreamInfo GetStream(FFmpegStreamType streamType) {
//            if (FileStreams != null && FileStreams.Count > 0)
//                return FileStreams.FirstOrDefault(f => f.StreamType == streamType);
//            else
//                return null;
//        }
//    }

//    /// <summary>
//    /// Contains the file information returned by FFmpeg.
//    /// </summary>
//    public class MediaFileInfoX264 : IMediaFileInfo {
//        public long FrameCount { get; set; }

//        public MediaFileInfoX264() { }

//        public MediaFileInfoX264(long frameCount) {
//            this.FrameCount = frameCount;
//        }
//    }
//}
