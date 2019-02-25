using System;

namespace EmergenceGuardian.Encoder {
    /// <summary>
    /// Base class for MediaVideoStream and MediaAudioStream representing a file stream.
    /// </summary>
    public abstract class MediaStreamInfo {
        public string RawText { get; set; }
        public int Index { get; set; }
        public string Format { get; set; } = "";

        /// <summary>
        /// Returns the stream type based on the derived class type.
        /// </summary>
        public FFmpegStreamType StreamType => this.GetType() == typeof(MediaVideoStreamInfo) ? FFmpegStreamType.Video : this.GetType() == typeof(MediaAudioStreamInfo) ? FFmpegStreamType.Audio : FFmpegStreamType.None;
    }

    /// <summary>
    /// Represents a video stream with its info.
    /// </summary>
    public class MediaVideoStreamInfo : MediaStreamInfo {
        public string ColorSpace { get; set; } = "";
        public string ColorRange { get; set; } = "";
        public string ColorMatrix { get; set; } = "";
        public int Width { get; set; }
        public int Height { get; set; }
        public int SAR1 { get; set; } = 1;
        public int SAR2 { get; set; } = 1;
        public int DAR1 { get; set; } = 1;
        public int DAR2 { get; set; } = 1;
        public double PixelAspectRatio { get; set; } = 1;
        public double DisplayAspectRatio { get; set; } = 1;
        public double FrameRate { get; set; }
        public int BitDepth { get; set; } = 8;
        public int Bitrate { get; set; }
    }

    /// <summary>
    /// Represents an audio stream with its info.
    /// </summary>
    public class MediaAudioStreamInfo : MediaStreamInfo {
        public int SampleRate { get; set; }
        public string Channels { get; set; } = "";
        public string BitDepth { get; set; } = "";
        public int Bitrate { get; set; }
    }
}
