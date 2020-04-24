using System;

namespace HanumanInstitute.FFmpeg
{
    /// <summary>
    /// Provides methods to execute Avisynth or VapourSynth media script files.
    /// </summary>
    public interface IMediaScript
    {
        /// <summary>
        /// Sets the owner of the process windows.
        /// </summary>
        IMediaScript SetOwner(object owner);
        /// <summary>
        /// Runs avs2pipemod with specified source file. The output will be discarded.
        /// </summary>
        /// <param name="path">The path to the script to run.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The process completion status.</returns>
        CompletionStatus RunAvisynth(string path, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null);
        /// <summary>
        /// Runs vspipe with specified source file. The output will be discarded.
        /// </summary>
        /// <param name="path">The path to the script to run.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The process completion status.</returns>
        CompletionStatus RunVapourSynth(string path, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null);
    }
}
