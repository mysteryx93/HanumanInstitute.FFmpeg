using System;

namespace HanumanInstitute.Encoder
{
    /// <summary>
    /// Represents an audio stream with its info.
    /// </summary>
    public class MediaAudioStreamInfo : MediaStreamInfo
    {
        public int SampleRate { get; set; }
        public string Channels { get; set; } = "";
        public string BitDepth { get; set; } = "";
        public int Bitrate { get; set; }
    }
}
