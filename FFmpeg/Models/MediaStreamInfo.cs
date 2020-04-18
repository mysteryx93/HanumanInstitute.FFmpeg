using System;

namespace HanumanInstitute.FFmpeg
{
    /// <summary>
    /// Base class for MediaVideoStream and MediaAudioStream representing a file stream.
    /// </summary>
    public abstract class MediaStreamInfo
    {
        public string RawText { get; set; }
        public int Index { get; set; }
        public string Format { get; set; } = "";

        /// <summary>
        /// Returns the stream type based on the derived class type.
        /// </summary>
        public FFmpegStreamType StreamType => GetType() == typeof(MediaVideoStreamInfo) ? FFmpegStreamType.Video : GetType() == typeof(MediaAudioStreamInfo) ? FFmpegStreamType.Audio : FFmpegStreamType.None;
    }
}
