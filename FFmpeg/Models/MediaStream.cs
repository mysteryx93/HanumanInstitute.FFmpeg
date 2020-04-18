using System;

namespace HanumanInstitute.FFmpeg
{
    /// <summary>
    /// Represents a file stream.
    /// </summary>
    public class MediaStream
    {
        public string Path { get; set; }
        public int Index { get; set; }
        public string Format { get; set; }
        public FFmpegStreamType Type { get; set; }

        public MediaStream() { }

        public MediaStream(string path, int index, string format, FFmpegStreamType type)
        {
            Path = path;
            Index = index;
            Format = format;
            Type = type;
        }
    }
}
