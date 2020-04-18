using System;

namespace HanumanInstitute.FFmpeg
{
    /// <summary>
    /// Contains progress information returned from x264's output.
    /// </summary>
    public class ProgressStatusX264
    {
        public long Frame { get; set; }
        public float Fps { get; set; }
        public float Bitrate { get; set; }
        public TimeSpan Time { get; set; }
        public string Size { get; set; } = "";
    }
}
