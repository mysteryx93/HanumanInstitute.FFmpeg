using System;

namespace HanumanInstitute.Encoder.Services
{
    /// <summary>
    /// Provides Windows API functions to manage processes.
    /// </summary>
    public interface IWindowsApiService
    {
        bool GenerateConsoleCtrlEvent();
        bool AttachConsole(uint processId);
        bool FreeConsole();
        bool SetConsoleCtrlHandler(ConsoleCtrlDelegate handlerRoutine, bool add);
    }
}
