using System;
using System.Collections.Generic;

namespace HanumanInstitute.FFmpeg
{
    /// <summary>
    /// Provides functions to manage audio and video streams.
    /// </summary>
    public interface IMediaMuxer
    {
        /// <summary>
        /// Sets the owner of the process windows.
        /// </summary>
        IMediaMuxer SetOwner(object owner);
        /// <summary>
        /// Merges specified audio and video files.
        /// </summary>
        /// <param name="videoFile">The file containing the video.</param>
        /// <param name="audioFile">The file containing the audio.</param>
        /// <param name="destination">The destination file.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The process completion status.</returns>
        CompletionStatus Muxe(string videoFile, string audioFile, string destination, ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null);
        /// <summary>
        /// Merges the specified list of file streams.
        /// </summary>
        /// <param name="fileStreams">The list of file streams to include in the output.</param>
        /// <param name="destination">The destination file.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The process completion status.</returns>
        CompletionStatus Muxe(IEnumerable<MediaStream> fileStreams, string destination, ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null);
        /// <summary>
        /// Extracts the video stream from specified file.
        /// </summary>
        /// <param name="source">The media file to extract from.</param>
        /// <param name="destination">The destination file.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The process completion status.</returns>
        CompletionStatus ExtractVideo(string source, string destination, ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null);
        /// <summary>
        /// Extracts the audio stream from specified file.
        /// </summary>
        /// <param name="source">The media file to extract from.</param>
        /// <param name="destination">The destination file.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The process completion status.</returns>
        CompletionStatus ExtractAudio(string source, string destination, ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null);
        /// <summary>
        /// Concatenates (merges) all specified files.
        /// </summary>
        /// <param name="files">The files to merge.</param>
        /// <param name="destination">The destination file.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The process completion status.</returns>
        CompletionStatus Concatenate(IEnumerable<string> files, string destination, ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null);
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
        CompletionStatus Truncate(string source, string destination, TimeSpan? startPos, TimeSpan? duration = null, ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null);
    }
}
