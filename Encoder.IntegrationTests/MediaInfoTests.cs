using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace EmergenceGuardian.Encoder.IntegrationTests {
    public class MediaInfoTests {
        private readonly ITestOutputHelper output;
        private readonly OutputFeeder feed;

        public MediaInfoTests(ITestOutputHelper output) {
            this.output = output;
            feed = new OutputFeeder(output);
        }

        private IMediaInfoReader SetupInfo() {
            IProcessWorkerFactory Factory = FactoryConfig.CreateWithConfig();
            return new MediaInfoReader(Factory);
        }

        [Fact]
        public void GetVersion_Valid_ReturnsVersionInfo() {
            var Info = SetupInfo();

            var Output = Info.GetVersion(null, feed.RunCallback);

            Assert.NotEmpty(Output);
            Assert.Contains("version", Output);
        }

        [Theory]
        [InlineData(AppPaths.Mpeg4, 1)]
        [InlineData(AppPaths.Mpeg2, 1)]
        [InlineData(AppPaths.Flv, 2)]
        [InlineData(AppPaths.StreamAac, 1)]
        [InlineData(AppPaths.StreamH264, 1)]
        [InlineData(AppPaths.StreamOpus, 1)]
        [InlineData(AppPaths.StreamVp9, 1)]
        public void GetFileInfo_Valid_ReturnsWorkerWithStreams(string source, int streamCount) {
            var Info = SetupInfo();
            var Src = AppPaths.GetInputFile(source);

            var FileInfo = Info.GetFileInfo(Src, null, feed.RunCallback);
            Assert.NotNull(FileInfo.FileStreams);
            Assert.Equal(streamCount, FileInfo.FileStreams.Count());
        }

        [Theory]
        [InlineData(AppPaths.InvalidFile)]
        public void GetFileInfo_InvalidFile_ReturnsWorkerWithEmptyStreamList(string source) {
            var Info = SetupInfo();
            var Src = AppPaths.GetInputFile(source);

            var FileInfo = Info.GetFileInfo(Src, null, feed.RunCallback);
            Assert.NotNull(FileInfo.FileStreams);
            Assert.Empty(FileInfo.FileStreams);
        }

        [Theory]
        [InlineData(AppPaths.Mpeg2)]
        [InlineData(AppPaths.Flv)]
        [InlineData(AppPaths.StreamH264)]
        public void GetFrameCount_Valid_ReturnsFrameCount(string source) {
            var Info = SetupInfo();
            var Src = AppPaths.GetInputFile(source);

            var Count = Info.GetFrameCount(Src, null, feed.RunCallback);

            Assert.True(Count > 0, "Frame count should be a positive number.");
        }

        [Theory]
        [InlineData(AppPaths.InvalidFile)]
        public void GetFrameCount_InvalidFile_ReturnsZero(string source) {
            var Info = SetupInfo();
            var Src = AppPaths.GetInputFile(source);

            var Count = Info.GetFrameCount(Src, null, feed.RunCallback);

            Assert.Equal(0, Count);
        }
    }
}
