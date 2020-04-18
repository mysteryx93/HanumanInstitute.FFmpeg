using System;
using System.Collections.Generic;

namespace HanumanInstitute.FFmpeg
{
    /// <summary>
    /// Returns file information parsed from FFmpeg.
    /// </summary>
    public interface IFileInfoFFmpeg
    {
        /// <summary>
        /// Returns the estimated frame count of input file.
        /// </summary>
        long FrameCount { get; }
        /// <summary>
        /// Returns the duration of input file.
        /// </summary>
        TimeSpan FileDuration { get; }
        /// <summary>
        /// Returns information about input streams.
        /// </summary>
        List<MediaStreamInfo> FileStreams { get; }
        /// <summary>
        /// Gets the first video stream from FileStreams.
        /// </summary>
        /// <returns>A FFmpegVideoStreamInfo object.</returns>
        MediaVideoStreamInfo VideoStream { get; }
        /// <summary>
        /// Gets the first audio stream from FileStreams.
        /// </summary>
        /// <returns>A FFmpegAudioStreamInfo object.</returns>
        MediaAudioStreamInfo AudioStream { get; }
    }
}
