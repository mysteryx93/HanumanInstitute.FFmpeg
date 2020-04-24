using System;
using System.Diagnostics.CodeAnalysis;

namespace HanumanInstitute.FFmpeg
{
    /// <summary>
    /// Executes commands through a media encoder process.
    /// </summary>
    public interface IProcessWorkerEncoder : IProcessWorker
    {
        /// <summary>
        /// Gets the application being used for encoding.
        /// </summary>
        string EncoderApp { get; }
        /// <summary>
        /// Gets or sets the options to control the behaviors of the process.
        /// </summary>
        new ProcessOptionsEncoder Options { get; set; }
        /// <summary>
        /// Gets the file information.
        /// </summary>
        [NotNull]
        object? FileInfo { get; }
        /// <summary>
        /// Returns the last progress status data received from DataReceived event.
        /// </summary>
        object? LastProgressReceived { get; }
        /// <summary>
        /// Occurs after stream info is read from the output.
        /// </summary>
        event EventHandler? FileInfoUpdated;
        /// <summary>
        /// Occurs when progress status update is received through the output stream.
        /// </summary>
        event ProgressReceivedEventHandler? ProgressReceived;
        /// <summary>
        /// Runs an encoder process with specified arguments.
        /// </summary>
        /// <param name="arguments">The startup arguments.</param>
        /// <param name="encoderApp">The encoder application to run.</param>
        /// <returns>The process completion status.</returns>
        CompletionStatus RunEncoder(string arguments, EncoderApp encoderApp);
        /// <summary>
        /// Runs an encoder process with specified arguments.
        /// </summary>
        /// <param name="arguments">The startup arguments.</param>
        /// <param name="encoderApp">A custom application name to run.</param>
        /// <returns>The process completion status.</returns>
        CompletionStatus RunEncoder(string arguments, string encoderApp);
        /// <summary>
        /// Runs an Avisynth script and encodes it in an encoder process with specified arguments.
        /// </summary>
        /// <param name="source">The path of the source Avisynth script file.</param>
        /// <param name="arguments">The encoder startup arguments.</param>
        /// <param name="encoderApp">The encoder application to run.</param>
        /// <returns>The process completion status.</returns>
        CompletionStatus RunAvisynthToEncoder(string source, string arguments, EncoderApp encoderApp);
        /// <summary>
        /// Runs an Avisynth script and encodes it in an encoder process with specified arguments.
        /// </summary>
        /// <param name="source">The path of the source Avisynth script file.</param>
        /// <param name="arguments">The encoder startup arguments.</param>
        /// <param name="encoderApp">A custom application name to run.</param>
        /// <returns>The process completion status.</returns>
        CompletionStatus RunAvisynthToEncoder(string source, string arguments, string encoderApp);
        /// <summary>
        /// Runs a VapourSynth script and encodes it in an encoder process with specified arguments.
        /// </summary>
        /// <param name="source">The path of the source VapourSynth script file.</param>
        /// <param name="arguments">The encoder startup arguments.</param>
        /// <param name="encoderApp">The encoder application to run.</param>
        /// <returns>The process completion status.</returns>
        CompletionStatus RunVapourSynthToEncoder(string source, string arguments, EncoderApp encoderApp);
        /// <summary>
        /// Runs a VapourSynth script and encodes it in an encoder process with specified arguments.
        /// </summary>
        /// <param name="source">The path of the source VapourSynth script file.</param>
        /// <param name="arguments">The encoder startup arguments.</param>
        /// <param name="encoderApp">A custom application name to run.</param>
        /// <returns>The process completion status.</returns>
        CompletionStatus RunVapourSynthToEncoder(string source, string arguments, string encoderApp);
    }
}
