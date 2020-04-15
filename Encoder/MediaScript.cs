using System;
using System.Globalization;
using static System.FormattableString;
using HanumanInstitute.Encoder.Properties;
using HanumanInstitute.Encoder.Services;

namespace HanumanInstitute.Encoder
{
    /// <summary>
    /// Provides methods to execute Avisynth or VapourSynth media script files.
    /// </summary>
    public class MediaScript : IMediaScript
    {
        private readonly IProcessWorkerFactory factory;
        private readonly IFileSystemService fileSystem;

        public MediaScript(IProcessWorkerFactory processFactory) : this(processFactory, new FileSystemService()) { }

        public MediaScript(IProcessWorkerFactory processFactory, IFileSystemService fileSystemService)
        {
            this.factory = processFactory ?? throw new ArgumentNullException(nameof(processFactory));
            this.fileSystem = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
        }

        /// <summary>
        /// Runs avs2pipemod with specified source file. The output will be discarded.
        /// </summary>
        /// <param name="path">The path to the script to run.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The process completion status.</returns>
        public CompletionStatus RunAvisynth(string path, ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null)
        {
            ArgHelper.ValidateNotNull(path, nameof(path));
            if (!fileSystem.Exists(factory.Config.Avs2PipeMod))
            {
                throw new System.IO.FileNotFoundException(string.Format(CultureInfo.InvariantCulture, Resources.Avs2PipeModPathNotFound, factory.Config.Avs2PipeMod));
            }

            string Args = Invariant($@"""{path}"" -rawvideo > NUL");
            IProcessWorker Manager = factory.Create(options, callback);
            Manager.OutputType = ProcessOutput.Error;
            string Cmd = Invariant($@"""{factory.Config.Avs2PipeMod}"" {Args}");
            CompletionStatus Result = Manager.RunAsCommand(Cmd);
            return Result;
        }

        /// <summary>
        /// Runs vspipe with specified source file. The output will be discarded.
        /// </summary>
        /// <param name="path">The path to the script to run.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The process completion status.</returns>
        public CompletionStatus RunVapourSynth(string path, ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null)
        {
            ArgHelper.ValidateNotNull(path, nameof(path));
            if (!fileSystem.Exists(factory.Config.Avs2PipeMod))
            {
                throw new System.IO.FileNotFoundException(string.Format(CultureInfo.InvariantCulture, Resources.Avs2PipeModPathNotFound, factory.Config.Avs2PipeMod));
            }

            string Args = Invariant($@"""{path}"" .");
            IProcessWorker Manager = factory.Create(options, callback);
            CompletionStatus Result = Manager.Run(factory.Config.VsPipePath, Args);
            return Result;
        }
    }
}
