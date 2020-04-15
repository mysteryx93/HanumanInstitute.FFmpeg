using System;

namespace HanumanInstitute.Encoder {
    /// <summary>
    /// Represents a file stream.
    /// </summary>
    public class MediaStream {
        public string Path { get; set; }
        public int Index { get; set; }
        public string Format { get; set; }
        public FFmpegStreamType Type { get; set; }

        public MediaStream() { }

        public MediaStream(string path, int index, string format, FFmpegStreamType type) {
            this.Path = path;
            this.Index = index;
            this.Format = format;
            this.Type = type;
        }
    }    
}
