using System;

namespace HanumanInstitute.FFmpeg
{
    /// <summary>
    /// Represents the type of media file stream.
    /// </summary>
    public enum FFmpegStreamType
    {
        /// <summary>
        /// No stream type specified.
        /// </summary>
        None,
        /// <summary>
        /// Video stream.
        /// </summary>
        Video,
        /// <summary>
        /// Audio stream.
        /// </summary>
        Audio
    }
}
