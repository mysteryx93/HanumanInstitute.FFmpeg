using HanumanInstitute.Encoder.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace HanumanInstitute.Encoder
{

    /// <summary>
    /// Contains the configuration settings of HanumanInstitute.Encoder.
    /// </summary>
    public class MediaConfig : IMediaConfig
    {
        private readonly IWindowsApiService api;
        private readonly IFileSystemService fileSystem;

        public MediaConfig() : this(new WindowsApiService(), new FileSystemService()) { }

        public MediaConfig(IWindowsApiService winApi, IFileSystemService fileSystemService)
        {
            api = winApi ?? throw new ArgumentNullException(nameof(winApi));
            fileSystem = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
        }

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
        public string GetAppPath(string encoderApp)
        {
            if (encoderApp == EncoderApp.FFmpeg.ToString())
            {
                return FFmpegPath;
            }
            else if (encoderApp == EncoderApp.x264.ToString())
            {
                return X264Path;
            }
            else if (encoderApp == EncoderApp.x265.ToString())
            {
                return X265Path;
            }
            else
            {
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

        /// <summary>
        /// Returns all FFmpeg running processes.
        /// </summary>
        /// <returns>A list of FFmpeg processes.</returns>
        public IProcess[] GetFFmpegProcesses()
        {
            string ProcessName = fileSystem.GetFileNameWithoutExtension(FFmpegPath);
            return Process.GetProcessesByName(ProcessName).Select(p => new ProcessWrapper(p)).ToArray();
        }

        /// <summary>
        /// Soft closes a process. This will work on WinForms and WPF, but needs to be managed differently for Console applications.
        /// See http://stackoverflow.com/a/29274238/3960200
        /// </summary>
        /// <param name="process">The process to close.</param>
        /// <returns>Whether the process was closed.</returns>
        public bool SoftKill(IProcess process)
        {
            if (process == null) { throw new ArgumentNullException(nameof(process)); }

            CloseProcessEventArgs Args = new CloseProcessEventArgs(process);
            CloseProcess?.Invoke(null, Args);
            if (!Args.Handled)
            {
                SoftKillWinApp(process);
            }

            return process.HasExited;
        }

        /// <summary>
        /// Soft closes from a WinForms or WPF process.
        /// </summary>
        /// <param name="process">The process to close.</param>
        public void SoftKillWinApp(IProcess process)
        {
            if (process == null) { throw new ArgumentNullException(nameof(process)); }

            if (api.AttachConsole((uint)process.Id))
            {
                api.SetConsoleCtrlHandler(null, true);
                try
                {
                    if (!api.GenerateConsoleCtrlEvent())
                    {
                        return;
                    }

                    process.WaitForExit();
                }
                finally
                {
                    api.FreeConsole();
                    api.SetConsoleCtrlHandler(null, false);
                }
            }
        }
    }
}
