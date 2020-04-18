using System;
using Xunit;

namespace HanumanInstitute.FFmpeg.UnitTests
{
    public class FileInfoParserX264Tests
    {
        protected static IFileInfoParser SetupParser() => new FileInfoX264();

        [Theory]
        [InlineData("", false)]
        [InlineData(null, false)]
        [InlineData("     1   0.10  10985.28    0:00:10    22.35 KB  ", true)]
        [InlineData("[ 65.8%]    288/438    336.84   345.13   0:00:00   0:00:05  404.45 KB  615.10 KB  ", true)]
        [InlineData("[ 65.8%]    288/438    ", false)]
        [InlineData(" 10000  843.74      2.83    0:00:11   144.06 KB  ", true)]
        [InlineData("[  1.0%]  10000/1000000 850.99     2.83   0:00:11   0:19:23  144.06 KB   14.07 MB  ", true)]
        public void X264LineIsStatus_Any_ReturnsExpectedData(string text, bool expectedResult)
        {
            var parser = SetupParser();

            var result = parser.IsLineProgressUpdate(text);

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData("", 0)]
        [InlineData(null, 0)]
        [InlineData("     1   0.10  10985.28    0:00:10    22.35 KB  ", 0)]
        [InlineData("[ 65.8%]    288/438    336.84   345.13   0:00:00   0:00:05  404.45 KB  615.10 KB  ", 438)]
        public void ParseX264FrameCount_Any_ReturnsExpectedData(string text, int expectedFrameCount)
        {
            var result = FileInfoX264.ParseFrameCount(text);

            Assert.Equal(expectedFrameCount, result);
        }

        [Theory]
        [InlineData("", 0, 0, 0, 0, "")]
        [InlineData(null, 0, 0, 0, 0, "")]
        [InlineData("     1   0.10  10985.28    0:00:10    22.35 KB  ", 1, 0.1, 10985.28, 0, "22.35 KB")]
        [InlineData("[ 65.8%]    288/438    336.84   345.13   0:00:00   0:00:05  404.45 KB  615.10 KB  ", 288, 336.84, 345.13, 5, "404.45 KB")]
        [InlineData(" 10000  843.74      2.83    0:00:11   144.06 KB  ", 10000, 843.74, 2.83, 0, "144.06 KB")]
        [InlineData("[  1.0%]  10000/1000000 850.99     2.83   0:00:11   0:00:23  144.06 KB   14.07 MB  ", 10000, 850.99, 2.83, 23, "144.06 KB")]
        public void ParseX264Progress_Any_ReturnsExpectedData(string text, int frame, float fps, float bitrate, int timeSeconds, string size)
        {
            var parser = SetupParser();

            var result = parser.ParseProgress(text) as ProgressStatusX264;

            Assert.NotNull(result);
            Assert.Equal(frame, result.Frame);
            Assert.Equal(fps, result.Fps);
            Assert.Equal(bitrate, result.Bitrate);
            Assert.Equal(TimeSpan.FromSeconds(timeSeconds), result.Time);
            Assert.Equal(size, result.Size);
        }
    }
}
