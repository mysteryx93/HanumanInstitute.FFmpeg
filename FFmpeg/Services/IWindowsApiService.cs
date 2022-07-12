namespace HanumanInstitute.FFmpeg.Services;

/// <summary>
/// Provides Windows API functions to manage processes.
/// </summary>
internal interface IWindowsApiService
{
    bool GenerateConsoleCtrlEvent();
    bool AttachConsole(uint processId);
    bool FreeConsole();
    bool SetConsoleCtrlHandler(ConsoleCtrlDelegate? handlerRoutine, bool add);
}
