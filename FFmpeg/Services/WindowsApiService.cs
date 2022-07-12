using System.Runtime.InteropServices;

namespace HanumanInstitute.FFmpeg.Services;

/// <summary>
/// Provides Windows API functions to manage processes.
/// </summary>
internal class WindowsApiService : IWindowsApiService
{
    public bool GenerateConsoleCtrlEvent() => NativeMethods.GenerateConsoleCtrlEvent(NativeMethods.CtrlCEvent, 0);

    public bool AttachConsole(uint processId) => NativeMethods.AttachConsole(processId);

    public bool FreeConsole() => NativeMethods.FreeConsole();

    public bool SetConsoleCtrlHandler(ConsoleCtrlDelegate? handlerRoutine, bool add) => NativeMethods.SetConsoleCtrlHandler(handlerRoutine, add);

    private static class NativeMethods
    {
        internal const int CtrlCEvent = 0;
        [DllImport("kernel32.dll")]
        internal static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool AttachConsole(uint dwProcessId);
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern bool FreeConsole();
        [DllImport("kernel32.dll")]
        internal static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate? handlerRoutine, bool add);
    }
}

/// <summary>
/// Delegate used for SetConsoleCtrlHandler Win API call.
/// </summary>
public delegate bool ConsoleCtrlDelegate(uint ctrlType);
