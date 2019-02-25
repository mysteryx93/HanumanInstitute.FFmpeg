using System;
using EmergenceGuardian.Encoder.Services;

namespace EmergenceGuardian.Encoder {

    #region Interface

    /// <summary>
    /// Creates new instances of process workers.
    /// </summary>
    public interface IProcessWorkerFactory {
        /// <summary>
        /// Gets or sets the configuration settings.
        /// </summary>
        IMediaConfig Config { get; set; }
        /// <summary>
        /// Creates a new process manager with specified options.
        /// </summary>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The newly created process manager.</returns>
        IProcessWorker Create(ProcessOptions options = null, ProcessStartedEventHandler callback = null);
        /// <summary>
        /// Creates a new process worker to run an encoder with specified options.
        /// </summary>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The newly created encoder process manager.</returns>
        IProcessWorkerEncoder CreateEncoder(ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null);
    }

    #endregion

    /// <summary>
    /// Creates new instances of process workers.
    /// </summary>
    public class ProcessWorkerFactory : IProcessWorkerFactory {

        #region Declarations / Constructors

        /// <summary>
        /// Gets or sets the configuration settings.
        /// </summary>
        public IMediaConfig Config { get; set; }
        public IFileInfoParserFactory ParserFactory { get; set; }
        public IProcessFactory ProcessFactory { get; set; }
        public IFileSystemService FileSystemService { get; set; }

        public ProcessWorkerFactory() : this(new MediaConfig(), new FileInfoParserFactory(), new ProcessFactory(), new FileSystemService()) { }

        public ProcessWorkerFactory(IMediaConfig config) : this(config, new FileInfoParserFactory(), new ProcessFactory(), new FileSystemService()) { }

        public ProcessWorkerFactory(IMediaConfig config, IFileInfoParserFactory parserFactory, IProcessFactory processFactory, IFileSystemService fileSystemService) {
            this.Config = config ?? throw new ArgumentNullException(nameof(config));
            this.ParserFactory = parserFactory ?? throw new ArgumentNullException(nameof(parserFactory));
            this.ProcessFactory = processFactory ?? throw new ArgumentNullException(nameof(processFactory));
            this.FileSystemService = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
        }

        #endregion

        /// <summary>
        /// Creates a new process worker with specified options.
        /// </summary>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The newly created process manager.</returns>
        public virtual IProcessWorker Create(ProcessOptions options = null, ProcessStartedEventHandler callback = null) {
            var Result = new ProcessWorker(Config, ProcessFactory, FileSystemService, options);
            if (callback != null)
                Result.ProcessStarted += callback;
            return Result;
        }

        /// <summary>
        /// Creates a new process worker to run an encoder with specified options.
        /// </summary>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The newly created encoder process manager.</returns>
        public virtual IProcessWorkerEncoder CreateEncoder(ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null) {
            var Result = new ProcessWorkerEncoder(Config, ProcessFactory, FileSystemService, ParserFactory, options);
            if (callback != null)
                Result.ProcessStarted += callback;
            return Result;
        }
    }
}
