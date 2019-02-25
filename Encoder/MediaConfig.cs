using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using EmergenceGuardian.Encoder.Services;

namespace EmergenceGuardian.Encoder {

    #region Interface

    /// <summary>
    /// Contains the configuration settings of FFmpeg.NET.
    /// </summary>
    public interface IMediaConfig {
        /// <summary>
        /// Gets or sets the path to FFmpeg.exe
        /// </summary>
        string FFmpegPath { get; set; }
        /// <summary>
        /// Gets or sets the path to X264.exe
        /// </summary>
        string X264Path { get; set; }
        /// <summary>
        /// Gets or sets the path to X265.exe
        /// </summary>
        string X265Path { get; set; }
        /// <summary>
        /// Gets or sets the path to avs2pipemod.exe to use AviSynth in a separate process.
        /// </summary>
        string Avs2PipeMod { get; set; }
        /// <summary>
        /// Gets or sets the path to vspipe.exe to use VapourSynth in a separate process.
        /// </summary>
        string VsPipePath { get; set; }
        /// <summary>
        /// Gets or sets a class that will manage graphical interface instances when DisplayMode = Interface
        /// </summary>
        IUserInterfaceManager UserInterfaceManager { get; set; }
        /// <summary>
        /// Occurs when a process needs to be closed. This needs to be managed manually for Console applications.
        /// See http://stackoverflow.com/a/29274238/3960200
        /// </summary>
        event CloseProcessEventHandler CloseProcess;
        /// <summary>
        /// Gets the path of the executing assembly.
        /// </summary>
        string ApplicationPath { get; }
        /// <summary>
        /// Returns the configured path for specified encoder application.
        /// </summary>
        /// <param name="encoderApp">The encoder to get the configured path for.</param>
        /// <returns>A file path string.</returns>
        string GetAppPath(string encoderApp);
        /// <summary>
        /// Occurs when running a custom application name to get the path of the application.
        /// </summary>
        event GetPathEventHandler GetCustomAppPath;

        /// <summary>
        /// Returns the absolute path of FFmpeg as defined in settings.
        /// </summary>
        //string FFmpegPathAbsolute { get; }
        /// <summary>
        /// Returns the absolute path of Avs2PipeMod as defined in settings.
        /// </summary>
        //string Avs2PipeModPathAbsolute { get; }
        /// <summary>
        /// Returns all FFmpeg running processes.
        /// </summary>
        /// <returns>A list of FFmpeg processes.</returns>
        IProcess[] GetFFmpegProcesses();
        /// <summary>
        /// Soft closes a process. This will work on WinForms and WPF, but needs to be managed differently for Console applications.
        /// See http://stackoverflow.com/a/29274238/3960200
        /// </summary>
        /// <param name="process">The process to close.</param>
        /// <returns>Whether the process was closed.</returns>
        bool SoftKill(IProcess process);
    }

    #endregion

    /// <summary>
    /// Contains the configuration settings of EmergenceGuardian.Encoder.
    /// </summary>
    public class MediaConfig : IMediaConfig {

        #region Declarations / Constructors

        protected readonly IWindowsApiService api;
        protected readonly IFileSystemService fileSystem;

        public MediaConfig() : this(new WindowsApiService(), new FileSystemService()) { }

        public MediaConfig(IWindowsApiService winApi, IFileSystemService fileSystemService) {
            this.api = winApi ?? throw new ArgumentNullException(nameof(winApi));
            this.fileSystem = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
        }

        #endregion

        /// <summary>
        /// Gets or sets the path to FFmpeg.exe
        /// </summary>
        public string FFmpegPath { get; set; } = "ffmpeg.exe";
        /// <summary>
        /// Gets or sets the path to X264.exe
        /// </summary>
        public string X264Path { get; set; } = "x264.exe";
        /// <summary>
        /// Gets or sets the path to X265.exe
        /// </summary>
        public string X265Path { get; set; } = "x265.exe";
        /// <summary>
        /// Gets or sets the path to avs2pipemod.exe to use Avisynth in a separate process.
        /// </summary>
        public string Avs2PipeMod { get; set; } = "avs2pipemod.exe";
        /// <summary>
        /// Gets or sets the path to vspipe.exe to use VapourSynth in a separate process.
        /// </summary>
        public string VsPipePath { get; set; } = "vspipe.exe";
        /// <summary>
        /// Gets or sets a class that will manage graphical interface instances when DisplayMode = Interface
        /// </summary>
        public IUserInterfaceManager UserInterfaceManager { get; set; }
        /// <summary>
        /// Occurs when a process needs to be closed. This needs to be managed manually for Console applications.
        /// See http://stackoverflow.com/a/29274238/3960200
        /// </summary>
        public event CloseProcessEventHandler CloseProcess;
        /// <summary>
        /// Gets the path of the executing assembly.
        /// </summary>
        public string ApplicationPath => fileSystem.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        /// <summary>
        /// Returns the configured path for specified encoder application.
        /// </summary>
        /// <param name="encoderApp">The encoder to get the configured path for.</param>
        /// <returns>A file path string.</returns>
        public string GetAppPath(string encoderApp) {
            if (encoderApp == EncoderApp.FFmpeg.ToString())
                return FFmpegPath;
            else if (encoderApp == EncoderApp.x264.ToString())
                return X264Path;
            else if (encoderApp == EncoderApp.x265.ToString())
                return X265Path;
            else {
                // Allow specifying custom application paths by handling this event.
                GetPathEventArgs Args = new GetPathEventArgs(encoderApp);
                GetCustomAppPath?.Invoke(this, Args);
                return Args.Path;
            }
        }

        /// <summary>
        /// Occurs when running a custom application name to get the path of the application.
        /// </summary>
        public event GetPathEventHandler GetCustomAppPath;

        #region Processes

        /// <summary>
        /// Returns all FFmpeg running processes.
        /// </summary>
        /// <returns>A list of FFmpeg processes.</returns>
        public IProcess[] GetFFmpegProcesses() {
            string ProcessName = fileSystem.GetFileNameWithoutExtension(FFmpegPath);
            return Process.GetProcessesByName(ProcessName).Select(p => new ProcessWrapper(p)).ToArray();
        }

        /// <summary>
        /// Soft closes a process. This will work on WinForms and WPF, but needs to be managed differently for Console applications.
        /// See http://stackoverflow.com/a/29274238/3960200
        /// </summary>
        /// <param name="process">The process to close.</param>
        /// <returns>Whether the process was closed.</returns>
        public bool SoftKill(IProcess process) {
            CloseProcessEventArgs Args = new CloseProcessEventArgs(process);
            CloseProcess?.Invoke(null, Args);
            if (!Args.Handled)
                SoftKillWinApp(process);
            return process.HasExited;
        }

        /// <summary>
        /// Soft closes from a WinForms or WPF process.
        /// </summary>
        /// <param name="process">The process to close.</param>
        public void SoftKillWinApp(IProcess process) {
            if (api.AttachConsole((uint)process.Id)) {
                api.SetConsoleCtrlHandler(null, true);
                try {
                    if (!api.GenerateConsoleCtrlEvent())
                        return;
                    process.WaitForExit();
                } finally {
                    api.FreeConsole();
                    api.SetConsoleCtrlHandler(null, false);
                }
            }
        }

        #endregion

    }
}
