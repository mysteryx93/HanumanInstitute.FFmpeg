using System;

namespace HanumanInstitute.Encoder
{
    /// <summary>
    /// Represents a video stream with its info.
    /// </summary>
    public class MediaVideoStreamInfo : MediaStreamInfo
    {
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
}
