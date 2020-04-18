using System;
using System.Diagnostics;
using System.Globalization;
using HanumanInstitute.FFmpeg.Properties;
using HanumanInstitute.FFmpeg.Services;

namespace HanumanInstitute.FFmpeg
{
    /// <summary>
    /// Executes commands through a media encoder process
    /// </summary>
    public class ProcessWorkerEncoder : ProcessWorker, IProcessWorkerEncoder
    {
        private readonly IFileSystemService _fileSystem;
        private readonly IFileInfoParserFactory _parserFactory;

        public ProcessWorkerEncoder(IMediaConfig config, IProcessFactory processFactory, IFileSystemService fileSystemService, IFileInfoParserFactory parserFactory, ProcessOptionsEncoder options)
            : base(config, processFactory, options ?? new ProcessOptionsEncoder())
        {
            _fileSystem = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
            _parserFactory = parserFactory ?? throw new ArgumentNullException(nameof(parserFactory));
            OutputType = ProcessOutput.Error;
        }

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

        /// <summary>
        /// Gets or sets the options to control the behaviors of the encoding process.
        /// </summary>
        public new ProcessOptionsEncoder Options
        {
            get => base.Options as ProcessOptionsEncoder;
            set => base.Options = value;
        }

        /// <summary>
        /// Runs an encoder process with specified arguments.
        /// </summary>
        /// <param name="arguments">The startup arguments.</param>
        /// <param name="encoderApp">The encoder application to run.</param>
        /// <returns>The process completion status.</returns>
        public CompletionStatus RunEncoder(string arguments, EncoderApp encoderApp)
        {
            return RunEncoder(arguments, encoderApp.ToString());
        }

        /// <summary>
        /// Runs an encoder process with specified arguments.
        /// </summary>
        /// <param name="arguments">The startup arguments.</param>
        /// <param name="encoderApp">A custom application name to run.</param>
        /// <returns>The process completion status.</returns>
        public CompletionStatus RunEncoder(string arguments, string encoderApp)
        {
            var appPath = Config.GetAppPath(encoderApp);
            if (!_fileSystem.Exists(appPath))
            {
                throw new System.IO.FileNotFoundException($@"The file ""{appPath}"" for the encoding application {encoderApp} configured in MediaConfig was not found.", appPath);
            }

            EnsureNotRunning();
            EncoderApp = encoderApp;
            return Run(appPath, arguments);
        }

        /// <summary>
        /// Runs an Avisynth script and encodes it in an encoder process with specified arguments.
        /// </summary>
        /// <param name="source">The path of the source Avisynth script file.</param>
        /// <param name="arguments">The encoder startup arguments.</param>
        /// <param name="encoderApp">The encoder application to run.</param>
        /// <returns>The process completion status.</returns>
        public CompletionStatus RunAvisynthToEncoder(string source, string arguments, EncoderApp encoderApp)
        {
            return RunAvisynthToEncoder(source, arguments, encoderApp.ToString());
        }

        /// <summary>
        /// Runs an Avisynth script and encodes it in an encoder process with specified arguments.
        /// </summary>
        /// <param name="source">The path of the source Avisynth script file.</param>
        /// <param name="arguments">The encoder startup arguments.</param>
        /// <param name="encoderApp">A custom application name to run.</param>
        /// <returns>The process completion status.</returns>
        public CompletionStatus RunAvisynthToEncoder(string source, string arguments, string encoderApp)
        {
            ArgHelper.ValidateNotNullOrEmpty(source, nameof(source));
            if (!_fileSystem.Exists(Config.Avs2PipeMod)) { throw new System.IO.FileNotFoundException(string.Format(CultureInfo.InvariantCulture, Resources.Avs2PipeModPathNotFound, Config.Avs2PipeMod)); }
            EnsureNotRunning();
            EncoderApp = encoderApp;
            var query = string.Format(CultureInfo.InvariantCulture, @"""{0}"" -y4mp ""{1}"" | ""{2}"" {3}", Config.Avs2PipeMod, source, Config.GetAppPath(encoderApp), arguments);
            return RunAsCommand(query);
        }

        /// <summary>
        /// Runs a VapourSynth script and encodes it in an encoder process with specified arguments.
        /// </summary>
        /// <param name="source">The path of the source VapourSynth script file.</param>
        /// <param name="arguments">The encoder startup arguments.</param>
        /// <param name="encoderApp">The encoder application to run.</param>
        /// <returns>The process completion status.</returns>
        public CompletionStatus RunVapourSynthToEncoder(string source, string arguments, EncoderApp encoderApp)
        {
            return RunVapourSynthToEncoder(source, arguments, encoderApp.ToString());
        }

        /// <summary>
        /// Runs a VapourSynth script and encodes it in an encoder process with specified arguments.
        /// </summary>
        /// <param name="source">The path of the source VapourSynth script file.</param>
        /// <param name="arguments">The encoder startup arguments.</param>
        /// <param name="encoderApp">A custom application name to run.</param>
        /// <returns>The process completion status.</returns>
        public CompletionStatus RunVapourSynthToEncoder(string source, string arguments, string encoderApp)
        {
            ArgHelper.ValidateNotNullOrEmpty(source, nameof(source));
            if (!_fileSystem.Exists(Config.VsPipePath)) { throw new System.IO.FileNotFoundException(string.Format(CultureInfo.InvariantCulture, Resources.VsPipePathNotFound, Config.VsPipePath)); }

            EnsureNotRunning();
            EncoderApp = encoderApp;
            var query = string.Format(CultureInfo.InvariantCulture, @"""{0}"" --y4m ""{1}"" - | ""{2}"" {3}", Config.VsPipePath, source, Config.GetAppPath(encoderApp), arguments);
            return RunAsCommand(query);
        }

        /// <summary>
        /// Runs specified process with specified arguments.
        /// </summary>
        /// <param name="fileName">The application to start.</param>
        /// <param name="arguments">The set of arguments to use when starting the application.</param>
        /// <returns>The process completion status.</returns>
        /// <exception cref="System.IO.FileNotFoundException">Occurs when the file to run is not found.</exception>
        /// <exception cref="InvalidOperationException">Occurs when this class instance is already running another process.</exception>
        public override CompletionStatus Run(string fileName, string arguments)
        {
            EnsureNotRunning();
            Parser = _parserFactory.Create(EncoderApp);
            return base.Run(fileName, arguments);
        }

        /// <summary>
        /// Occurs when data is received from the executing application.
        /// </summary>
        protected override void OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e == null || e.Data == null)
            {
                if (!Parser.IsParsed)
                {
                    ParseFileInfo();
                }
                return;
            }

            base.OnDataReceived(sender, e);

            object progressInfo = null;
            if (!Parser.IsParsed && Parser.HasFileInfo(e.Data))
            {
                ParseFileInfo();
            }

            if (Parser.IsParsed && Parser.IsLineProgressUpdate(e.Data))
            {
                progressInfo = Parser.ParseProgress(e.Data);
            }

            if (progressInfo != null)
            {
                LastProgressReceived = progressInfo;
                ProgressReceived?.Invoke(this, new ProgressReceivedEventArgs(progressInfo));
            }
        }

        /// <summary>
        /// Parses file information from output.
        /// </summary>
        private void ParseFileInfo()
        {
            Parser.ParseFileInfo(Output, Options);
            FileInfoUpdated?.Invoke(this, new EventArgs());
        }
    }
}
