using System;

namespace HanumanInstitute.Encoder
{
    /// <summary>
    /// Provides functions to get information on media files.
    /// </summary>
    public interface IMediaInfoReader
    {
        /// <summary>
        /// Returns the version information from FFmpeg.
        /// </summary>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The FFmpeg output containing version information.</returns>
        string GetVersion(ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null);
        /// <summary>
        /// Gets file streams information of specified file via FFmpeg.
        /// </summary>
        /// <param name="source">The file to get information about.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>A IFileInfoParserFFmpeg object containing the file information.</returns>
        IFileInfoFFmpeg GetFileInfo(string source, ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null);
        /// <summary>
        /// Returns the exact frame count of specified video file.
        /// </summary>
        /// <param name="source">The file to get information about.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The number of frames in the video.</returns>
        long GetFrameCount(string source, ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null);
    }
}
