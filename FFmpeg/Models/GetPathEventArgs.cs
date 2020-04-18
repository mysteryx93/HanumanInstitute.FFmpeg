using System;

namespace HanumanInstitute.FFmpeg
{
    /// <summary>
    /// Represents the method that will handle the GetCustomAppPath event.
    /// </summary>
    public delegate void GetPathEventHandler(object sender, GetPathEventArgs e);

    /// <summary>
    /// Provides progress information for the GetCustomAppPath event.
    /// </summary>
    public class GetPathEventArgs : EventArgs
    {
        public string App { get; set; }
        public string Path { get; set; }

        public GetPathEventArgs() { }

        public GetPathEventArgs(string app)
        {
            App = app;
        }
    }
}
