using System;
using Xunit;

namespace HanumanInstitute.FFmpeg.UnitTests
{
    public class FileInfoParserFFmpegTests
    {
        protected static IFileInfoParser SetupParser() => new FileInfoFFmpeg();

        [Theory]
        [InlineData("This is some invalid data: Stream #0", 0)]
        [InlineData(OutputSamples.FFmpegInfo1, 1)]
        [InlineData(OutputSamples.FFmpegInfo2, 2)]
        [InlineData(OutputSamples.FFmpegEncode1, 2)]
        [InlineData(@"  Duration: 00:00:44.00, start: 0.373378, bitrate: 1402 kb/s
    Stream #0:0[0x1e0]: Video: mpeg1video, yuv420p(tv), 352x288 [SAR 178:163 DAR 1958:1467], 1150 kb/s, 25 fps, 25 tbr, 90k tbn, 25 tbc
   aStream #0:1[0x1c0]: Audio: mp2, 44100 Hz, stereo, s16p, 224 kb/s
", 1)]
        public void ParseFileInfo_Valid_ReturnsExpectedStreamCount(string outputText, int streamCount)
        {
            var parser = SetupParser();

            parser.ParseFileInfo(outputText, null);

            var info = parser as IFileInfoFFmpeg;
            Assert.NotNull(info?.FileStreams);
            Assert.Equal(streamCount, info.FileStreams.Count);
        }

        [Theory]
        [InlineData("    Stream #0:1[0x1c0]: Audio: mp2, 44100 Hz, stereo, s16p, 224 kb/s", 1, "mp2", 44100, "stereo", "s16p", 224)]
        [InlineData("    Stream #0:0: Audio: mp3, 44100 Hz, stereo, s16p, 192 kb/s", 0, "mp3", 44100, "stereo", "s16p", 192)]
        [InlineData("    Stream #0:1(und): Audio: aac (LC) (mp4a / 0x6134706D), 44100 Hz, stereo, fltp, 132 kb/s (default)", 1, "aac", 44100, "stereo", "fltp", 132)]
        public void ParseAudioStreamInfo_Valid_ReturnsExpectedData(string text, int index, string format, int sampleRate, string channels, string bitDepth, int bitrate)
        {
            var result = FileInfoFFmpeg.ParseStreamInfo(text);

            var info = result as MediaAudioStreamInfo;
            Assert.NotNull(info);
            Assert.Equal(text, info.RawText);
            Assert.Equal(FFmpegStreamType.Audio, info.StreamType);
            Assert.Equal(index, info.Index);
            Assert.Equal(format, info.Format);
            Assert.Equal(sampleRate, info.SampleRate);
            Assert.Equal(channels, info.Channels);
            Assert.Equal(bitDepth, info.BitDepth);
            Assert.Equal(bitrate, info.Bitrate);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("This is invalid data")]
        [InlineData("   Stream #0:0: Audio: mp3, 44100 Hz, stereo, s16p, 192 kb/s")]
        public void ParseAudioStreamInfo_Invalid_ReturnsNull(string text)
        {
            var result = FileInfoFFmpeg.ParseStreamInfo(text);

            Assert.Null(result);
        }

        [Theory]
        [InlineData("    Stream #0:1: Video: this, , , is; invalid data", 1, "this", "", "", "", 0, 0, 1, 1, 1, 1, 0, 8, 0)]
        [InlineData("    Stream #0:0[0x1e0]: Video: mpeg1video, yuv420p(tv), 352x288 [SAR 178:163 DAR 1958:1467], 1150 kb/s, 25 fps, 25 tbr, 90k tbn, 25 tbc", 0, "mpeg1video", "yuv420p", "tv", "", 352, 288, 178, 163, 1958, 1467, 25, 8, 1150)]
        [InlineData("    Stream #0:1: Video: mjpeg, yuvj420p(pc, bt470bg/unknown/unknown), 1000x1000 [SAR 1:1 DAR 1:1], 90k tbr, 90k tbn, 90k tbc", 1, "mjpeg", "yuvj420p", "pc", "bt470bg/unknown/unknown", 1000, 1000, 1, 1, 1, 1, 0, 8, 0)]
        [InlineData("    Stream #0:0(und): Video: h264 (High) (avc1 / 0x31637661), yuv420p, 352x288 [SAR 178:163 DAR 1958:1467], 228 kb/s, 25 fps, 25 tbr, 12800 tbn, 50 tbc (default)", 0, "h264", "yuv420p", "", "", 352, 288, 178, 163, 1958, 1467, 25, 8, 228)]
        public void ParseVideoStreamInfo_Valid_ReturnsExpectedData(string text, int index, string format, string colorSpace, string colorRange, string colorMatrix, int width, int height, int sar1, int sar2, int dar1, int dar2, double frameRate, int bitDepth, int bitrate)
        {
            var result = FileInfoFFmpeg.ParseStreamInfo(text);

            var info = result as MediaVideoStreamInfo;
            Assert.NotNull(info);
            Assert.Equal(text, info.RawText);
            Assert.Equal(FFmpegStreamType.Video, info.StreamType);
            Assert.Equal(index, info.Index);
            Assert.Equal(format, info.Format);
            Assert.Equal(colorSpace, info.ColorSpace);
            Assert.Equal(colorRange, info.ColorRange);
            Assert.Equal(colorMatrix, info.ColorMatrix);
            Assert.Equal(width, info.Width);
            Assert.Equal(height, info.Height);
            Assert.Equal(sar1, info.SAR1);
            Assert.Equal(sar2, info.SAR2);
            Assert.Equal(dar1, info.DAR1);
            Assert.Equal(dar2, info.DAR2);
            Assert.Equal(frameRate, info.FrameRate);
            Assert.Equal(bitDepth, info.BitDepth);
            Assert.Equal(bitrate, info.Bitrate);
        }

        [Theory]
        [InlineData("", 0, 0, 0, "", 0, "", 0)]
        [InlineData(null, 0, 0, 0, "", 0, "", 0)]
        [InlineData("This is invalid data.", 0, 0, 0, "", 0, "", 0)]
        [InlineData("frame=  929 fps=0.0 q=-0.0 size=   68483kB time=00:00:37.00 bitrate=15162.6kbits/s speed=  74x    ", 929, 0, 0, "68483kB", 37, "15162.6kbits/s", 74)]
        [InlineData("frame=100000 fps=1531 q=-1.0 Lsize=    1828kB time=00:00:26.00 bitrate=   3.6kbits/s speed=63.8x    ", 100000, 1531, -1, "1828kB", 26, "3.6kbits/s", 63.8)]
        public void ParseFFmpegProgress_Any_ReturnsExpectedData(string text, long frame, float fps, float quantizer, string size, double timeSeconds, string bitrate, float speed)
        {
            var parser = SetupParser();

            var result = parser.ParseProgress(text) as ProgressStatusFFmpeg;

            Assert.NotNull(result);
            Assert.Equal(frame, result.Frame);
            Assert.Equal(fps, result.Fps);
            Assert.Equal(quantizer, result.Quantizer);
            Assert.Equal(size, result.Size);
            Assert.Equal(TimeSpan.FromSeconds(timeSeconds), result.Time);
            Assert.Equal(bitrate, result.Bitrate);
            Assert.Equal(speed, result.Speed);
        }

        [Theory]
        [InlineData(null, null, null)]
        [InlineData("", "", null)]
        [InlineData(" ", "     ", null)]
        [InlineData("   =   ", " ", "")]
        [InlineData("mode=1 key=MyKey key=value2 ", "key", "MyKey")]
        [InlineData("mode=1 key2=MyKey key=value2", "key", "value2")]
        [InlineData("mode=1 key2=MyKey key=value2   ", "key", "value2")]
        [InlineData("mode=1 key2=MyKey key3=value2", "key", null)]
        public void ParseAttribute_Any_ReturnsExpectedValue(string text, string key, string expected)
        {
            var result = FileInfoFFmpeg.ParseAttribute(text, key);

            Assert.Equal(expected, result);
        }
    }
}
