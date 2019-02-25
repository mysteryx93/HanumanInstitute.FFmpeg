using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EmergenceGuardian.Encoder.Services;

namespace EmergenceGuardian.Encoder {

    #region Interface

    /// <summary>
    /// Executes commands through a media encoder process.
    /// </summary>
    public interface IProcessWorkerEncoder : IProcessWorker {
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
        object FileInfo { get; }
        /// <summary>
        /// Returns the last progress status data received from DataReceived event.
        /// </summary>
        object LastProgressReceived { get; }
        /// <summary>
        /// Occurs after stream info is read from the output.
        /// </summary>
        event EventHandler FileInfoUpdated;
        /// <summary>
        /// Occurs when progress status update is received through the output stream.
        /// </summary>
        event ProgressReceivedEventHandler ProgressReceived;
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

    #endregion

    /// <summary>
    /// Executes commands through a media encoder process
    /// </summary>
    public class ProcessWorkerEncoder : ProcessWorker, IProcessWorkerEncoder {

        #region Declarations / Constructors

        /// <summary>
        /// Gets the application being used for encoding.
        /// </summary>
        public string EncoderApp { get; set; }
        /// <summary>
        /// Gets the class parsing and storing file information.
        /// </summary>
        protected IFileInfoParser Parser { get; private set; }
        /// <summary>
        /// Gets the file information.
        /// </summary>
        public object FileInfo => Parser;
        /// <summary>
        /// Returns the last progress status data received from DataReceived event.
        /// </summary>
        public object LastProgressReceived { get; private set; }
        /// <summary>
        /// Occurs after stream info is read from the output.
        /// </summary>
        public event EventHandler FileInfoUpdated;
        /// <summary>
        /// Occurs when progress status update is received through the output stream.
        /// </summary>
        public event ProgressReceivedEventHandler ProgressReceived;
        protected readonly IFileInfoParserFactory parserFactory;

        public ProcessWorkerEncoder() : this(new MediaConfig(), new ProcessFactory(), new FileSystemService(), new FileInfoParserFactory(), new ProcessOptionsEncoder()) { }

        public ProcessWorkerEncoder(IMediaConfig config, IProcessFactory processFactory, IFileSystemService fileSystemService, IFileInfoParserFactory parserFactory, ProcessOptionsEncoder options = null)
            : base(config, processFactory, fileSystemService, options ?? new ProcessOptionsEncoder()) {
            this.parserFactory = parserFactory ?? throw new ArgumentNullException(nameof(parserFactory));
            OutputType = ProcessOutput.Error;
        }

        /// <summary>
        /// Gets or sets the options to control the behaviors of the encoding process.
        /// </summary>
        public new ProcessOptionsEncoder Options {
            get => base.Options as ProcessOptionsEncoder;
            set => base.Options = value;
        }

        #endregion

        #region Run Methods

        /// <summary>
        /// Runs an encoder process with specified arguments.
        /// </summary>
        /// <param name="arguments">The startup arguments.</param>
        /// <param name="encoderApp">The encoder application to run.</param>
        /// <returns>The process completion status.</returns>
        public CompletionStatus RunEncoder(string arguments, EncoderApp encoderApp) => RunEncoder(arguments, encoderApp.ToString());

        /// <summary>
        /// Runs an encoder process with specified arguments.
        /// </summary>
        /// <param name="arguments">The startup arguments.</param>
        /// <param name="encoderApp">A custom application name to run.</param>
        /// <returns>The process completion status.</returns>
        public CompletionStatus RunEncoder(string arguments, string encoderApp) {
            string AppPath = Config.GetAppPath(encoderApp);
            if (!fileSystem.Exists(AppPath))
                throw new System.IO.FileNotFoundException($@"The file ""{AppPath}"" for the encoding application {encoderApp} configured in MediaConfig was not found.", AppPath);
            EnsureNotRunning();
            this.EncoderApp = encoderApp;
            return Run(AppPath, arguments);
        }

        /// <summary>
        /// Runs an Avisynth script and encodes it in an encoder process with specified arguments.
        /// </summary>
        /// <param name="source">The path of the source Avisynth script file.</param>
        /// <param name="arguments">The encoder startup arguments.</param>
        /// <param name="encoderApp">The encoder application to run.</param>
        /// <returns>The process completion status.</returns>
        public CompletionStatus RunAvisynthToEncoder(string source, string arguments, EncoderApp encoderApp) => RunAvisynthToEncoder(source, arguments, encoderApp.ToString());

        /// <summary>
        /// Runs an Avisynth script and encodes it in an encoder process with specified arguments.
        /// </summary>
        /// <param name="source">The path of the source Avisynth script file.</param>
        /// <param name="arguments">The encoder startup arguments.</param>
        /// <param name="encoderApp">A custom application name to run.</param>
        /// <returns>The process completion status.</returns>
        public CompletionStatus RunAvisynthToEncoder(string source, string arguments, string encoderApp) {
            if (string.IsNullOrEmpty(source))
                throw new ArgumentException("Source cannot be null or empty.", nameof(source));
            if (!fileSystem.Exists(Config.Avs2PipeMod))
                throw new System.IO.FileNotFoundException($@"File ""{Config.Avs2PipeMod}"" specified by Config.Avs2PipeModPath is not found.");
            EnsureNotRunning();
            this.EncoderApp = encoderApp;
            String Query = string.Format(@"""{0}"" -y4mp ""{1}"" | ""{2}"" {3}", Config.Avs2PipeMod, source, Config.GetAppPath(encoderApp), arguments);
            return RunAsCommand(Query);
        }

        /// <summary>
        /// Runs a VapourSynth script and encodes it in an encoder process with specified arguments.
        /// </summary>
        /// <param name="source">The path of the source VapourSynth script file.</param>
        /// <param name="arguments">The encoder startup arguments.</param>
        /// <param name="encoderApp">The encoder application to run.</param>
        /// <returns>The process completion status.</returns>
        public CompletionStatus RunVapourSynthToEncoder(string source, string arguments, EncoderApp encoderApp) => RunVapourSynthToEncoder(source, arguments, encoderApp.ToString());

        /// <summary>
        /// Runs a VapourSynth script and encodes it in an encoder process with specified arguments.
        /// </summary>
        /// <param name="source">The path of the source VapourSynth script file.</param>
        /// <param name="arguments">The encoder startup arguments.</param>
        /// <param name="encoderApp">A custom application name to run.</param>
        /// <returns>The process completion status.</returns>
        public CompletionStatus RunVapourSynthToEncoder(string source, string arguments, string encoderApp) {
            if (string.IsNullOrEmpty(source))
                throw new ArgumentException("Source cannot be null or empty.", nameof(source));
            if (!fileSystem.Exists(Config.VsPipePath))
                throw new System.IO.FileNotFoundException($@"File ""{Config.VsPipePath}"" specified by Config.VsPipePath is not found.");
            EnsureNotRunning();
            this.EncoderApp = encoderApp;
            String Query = string.Format(@"""{0}"" --y4m ""{1}"" - | ""{2}"" {3}", Config.VsPipePath, source, Config.GetAppPath(encoderApp), arguments);
            return RunAsCommand(Query);
        }

        /// <summary>
        /// Runs specified process with specified arguments.
        /// </summary>
        /// <param name="fileName">The application to start.</param>
        /// <param name="arguments">The set of arguments to use when starting the application.</param>
        /// <returns>The process completion status.</returns>
        /// <exception cref="System.IO.FileNotFoundException">Occurs when the file to run is not found.</exception>
        /// <exception cref="InvalidOperationException">Occurs when this class instance is already running another process.</exception>
        public override CompletionStatus Run(string fileName, string arguments) {
            EnsureNotRunning();
            this.Parser = parserFactory.Create(EncoderApp);
            return base.Run(fileName, arguments);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Occurs when data is received from the executing application.
        /// </summary>
        protected override void OnDataReceived(object sender, DataReceivedEventArgs e) {
            if (e.Data == null) {
                if (!Parser.IsParsed)
                    ParseFileInfo();
                return;
            }

            base.OnDataReceived(sender, e);

            object ProgressInfo = null;
            if (!Parser.IsParsed && Parser.HasFileInfo(e.Data))
                ParseFileInfo();
            if (Parser.IsParsed && Parser.IsLineProgressUpdate(e.Data))
                ProgressInfo = Parser.ParseProgress(e.Data);

            if (ProgressInfo != null) {
                LastProgressReceived = ProgressInfo;
                ProgressReceived?.Invoke(this, new ProgressReceivedEventArgs(ProgressInfo));
            }
        }

        /// <summary>
        /// Parses file information from output.
        /// </summary>
        private void ParseFileInfo() {
            Parser.ParseFileInfo(this.Output, this.Options);
            FileInfoUpdated?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Throws an exception if a progress is already running.
        /// </summary>
        private void EnsureNotRunning() {
            lock (lockToken) {
                if (WorkProcess != null)
                    throw new InvalidOperationException("This instance of ProcessWorkerEncoder is busy. You can run concurrent commands by creating other class instances.");
            }
        }

        #endregion

    }
}
