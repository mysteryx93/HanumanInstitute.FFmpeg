using EmergenceGuardian.Encoder.Services;
using System;

namespace EmergenceGuardian.Encoder {

    #region Interface

    /// <summary>
    /// Provides methods to execute Avisynth or VapourSynth media script files.
    /// </summary>
    public interface IMediaScript {
        /// <summary>
        /// Runs avs2pipemod with specified source file. The output will be discarded.
        /// </summary>
        /// <param name="path">The path to the script to run.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The process completion status.</returns>
        CompletionStatus RunAvisynth(string path, ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null);
        /// <summary>
        /// Runs vspipe with specified source file. The output will be discarded.
        /// </summary>
        /// <param name="path">The path to the script to run.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The process completion status.</returns>
        CompletionStatus RunVapourSynth(string path, ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null);
    }

    #endregion

    /// <summary>
    /// Provides methods to execute Avisynth or VapourSynth media script files.
    /// </summary>
    public class MediaScript : IMediaScript {

        #region Declarations / Constructors

        protected readonly IProcessWorkerFactory factory;
        protected readonly IFileSystemService fileSystem;

        public MediaScript(IProcessWorkerFactory processFactory) : this(processFactory, new FileSystemService()) { }

        public MediaScript(IProcessWorkerFactory processFactory, IFileSystemService fileSystemService) {
            this.factory = processFactory ?? throw new ArgumentNullException(nameof(processFactory));
            this.fileSystem = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
        }

        #endregion

        /// <summary>
        /// Runs avs2pipemod with specified source file. The output will be discarded.
        /// </summary>
        /// <param name="path">The path to the script to run.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The process completion status.</returns>
        public CompletionStatus RunAvisynth(string path, ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null) {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Path cannot be null or empty.", nameof(path));
            if (!fileSystem.Exists(factory.Config.Avs2PipeMod))
                throw new System.IO.FileNotFoundException($@"File ""{factory.Config.Avs2PipeMod}"" specified by Config.Avs2PipeModPath is not found.");
            string Args = $@"""{path}"" -rawvideo > NUL";
            IProcessWorker Manager = factory.Create(options, callback);
            Manager.OutputType = ProcessOutput.Error;
            string Cmd = $@"""{factory.Config.Avs2PipeMod}"" {Args}";
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
        public CompletionStatus RunVapourSynth(string path, ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null) {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Path cannot be null or empty.", nameof(path));
            if (!fileSystem.Exists(factory.Config.VsPipePath))
                throw new System.IO.FileNotFoundException($@"File ""{factory.Config.VsPipePath}"" specified by Config.VsPipePath is not found.");
            string Args = $@"""{path}"" .";
            IProcessWorker Manager = factory.Create(options, callback);
            CompletionStatus Result = Manager.Run(factory.Config.VsPipePath, Args);
            return Result;
        }
    }
}
