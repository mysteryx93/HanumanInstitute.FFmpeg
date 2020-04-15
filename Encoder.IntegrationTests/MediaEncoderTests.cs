using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;
using HanumanInstitute.Encoder;

namespace EmergenceGuardian.Encoder.IntegrationTests {
    public class MediaEncoderTests {

        private IProcessWorkerFactory factory;
        private readonly ITestOutputHelper output;
        private readonly OutputFeeder feed;
        private const string AviExt = ".avi";
        private const string Mp4Ext = ".mp4";

        public MediaEncoderTests(ITestOutputHelper output) {
            this.output = output;
            feed = new OutputFeeder(output);
        }

        private IMediaEncoder SetupEncoder() {
            factory = FactoryConfig.CreateWithConfig();
            return new MediaEncoder(factory);
        }

        private IFileInfoFFmpeg GetFileInfo(string path) {
            var Info = new MediaInfoReader(factory);
            return Info.GetFileInfo(path);
        }

        private void AssertMedia(string dest, int streamCount) {
            Assert.True(File.Exists(dest));
            var FileInfo = GetFileInfo(dest) as IFileInfoFFmpeg;
            Assert.Equal(streamCount, FileInfo.FileStreams.Count);
        }


        // FFmpeg

        [Theory]
        [InlineData(AppPaths.Mpeg4WithAudio, true, 2)]
        [InlineData(AppPaths.Mpeg4WithAudio, false, 1)]
        [InlineData(AppPaths.StreamVp9, true, 1)]
        [InlineData(AppPaths.StreamVp9, false, 1)]
        [InlineData(AppPaths.StreamOpus, true, 1)]
        public void ConvertToAviUtVideo_Valid_Success(string source, bool audio, int streamCount) {
            string SrcVideo = AppPaths.GetInputFile(source);
            string Dest = AppPaths.PrepareDestPath("ConvertToAvi", source, AviExt);
            var Encoder = SetupEncoder();

            var Result = Encoder.ConvertToAviUtVideo(SrcVideo, Dest, audio, null, feed.RunCallback);

            Assert.Equal(CompletionStatus.Success, Result);
            AssertMedia(Dest, streamCount);
        }

        [Theory]
        [InlineData(AppPaths.InvalidFile, true)]
        [InlineData(AppPaths.StreamOpus, false)]
        public void ConvertToAviUtVideo_Invalid_Failure(string source, bool audio) {
            string SrcVideo = AppPaths.GetInputFile(source);
            string Dest = AppPaths.PrepareDestPath("ConvertToAvi", source, AviExt);
            var Encoder = SetupEncoder();

            var Result = Encoder.ConvertToAviUtVideo(SrcVideo, Dest, audio, null, feed.RunCallback);

            Assert.Equal(CompletionStatus.Failed, Result);
        }

        [Theory]
        [InlineData(AppPaths.Mpeg4WithAudio, ".mp4", "libx264", null, "-preset ultrafast", 1)]
        [InlineData(AppPaths.Mpeg4WithAudio, ".mp4", "libx264", "aac", "-preset ultrafast", 2)]
        [InlineData(AppPaths.Mpeg4WithAudio, ".m4a", null, "aac", "", 1)]
        [InlineData(AppPaths.StreamOpus, ".m4a", "", "aac", "", 1)]
        [InlineData(AppPaths.StreamVp9, ".mp4", "libx264", "", "-preset ultrafast", 1)]
        public void EncodeFFmpeg_Valid_Success(string source, string destExt, string videoCodec, string audioCodec, string encodeArgs, int streamCount) {
            string SrcVideo = AppPaths.GetInputFile(source);
            string Dest = AppPaths.PrepareDestPath("EncodeFFmpeg", source, destExt);
            var Encoder = SetupEncoder();

            var Result = Encoder.EncodeFFmpeg(SrcVideo, Dest, videoCodec, audioCodec, encodeArgs, null, feed.RunCallback);

            Assert.Equal(CompletionStatus.Success, Result);
            AssertMedia(Dest, streamCount);
        }

        [Theory]
        [InlineData(AppPaths.InvalidFile, ".mp4", "libx264", null, null)]
        [InlineData(AppPaths.Mpeg4, ".mp4", null, "aac", null)]
        [InlineData(AppPaths.Mpeg4WithAudio, ".mp4", "libx264", "aac", "invalid")]
        [InlineData(AppPaths.StreamOpus, ".mp4", "", "invalid", "")]
        [InlineData(AppPaths.StreamVp9, ".mp4", "invalid", "", "-preset ultrafast")]
        public void EncodeFFmpeg_Invalid_Failure(string source, string destExt, string videoCodec, string audioCodec, string encodeArgs) {
            string SrcVideo = AppPaths.GetInputFile(source);
            string Dest = AppPaths.PrepareDestPath("EncodeFFmpeg", source, destExt);
            var Encoder = SetupEncoder();

            var Result = Encoder.EncodeFFmpeg(SrcVideo, Dest, videoCodec, audioCodec, encodeArgs, null, feed.RunCallback);

            Assert.Equal(CompletionStatus.Failed, Result);
        }

        //[Theory]
        //[InlineData(AppPaths.FourStreams, ".mkv", null, 3)]
        //public void EncodeFFmpeg_List_Valid_Success(string source, string destExt, string encodeArgs, int streamCount) {
        //    string SrcVideo = AppPaths.GetInputFile(source);
        //    string Dest = AppPaths.PrepareDestPath("EncodeFFmpeg_List", source, destExt);
        //    var Encoder = SetupEncoder();
        //    string[] VideoCodec = new string[] { "libx264", "libxvid" };
        //    string[] AudioCodec = new string[] { "aac" };

        //    var Result = Encoder.EncodeFFmpeg(SrcVideo, Dest, VideoCodec, AudioCodec, encodeArgs, null, feed.RunCallback);

        //    Assert.Equal(CompletionStatus.Success, Result);
        //    AssertMedia(Dest, streamCount);
        //}

        [Theory]
        [InlineData(AppPaths.Avisynth, ".mp4", "libx264", null, "-preset ultrafast", 1)]
        public void EncodeAvisynthToFFmpeg_Valid_Success(string source, string destExt, string videoCodec, string audioCodec, string encodeArgs, int streamCount) {
            string SrcVideo = AppPaths.GetInputFile(source);
            string Dest = AppPaths.PrepareDestPath("EncodeAvisynthToFFmpeg", source, destExt);
            var Encoder = SetupEncoder();

            var Result = Encoder.EncodeAvisynthToFFmpeg(SrcVideo, Dest, videoCodec, audioCodec, encodeArgs, null, feed.RunCallback);

            Assert.Equal(CompletionStatus.Success, Result);
            AssertMedia(Dest, streamCount);
        }

        [Theory]
        [InlineData(AppPaths.InvalidFile, ".mp4", "libx264", null, null)]
        [InlineData(AppPaths.Mpeg4, ".mp4", "libx264", null, null)]
        public void EncodeAvisynthToFFmpeg_Invalid_Failure(string source, string destExt, string videoCodec, string audioCodec, string encodeArgs) {
            string SrcVideo = AppPaths.GetInputFile(source);
            string Dest = AppPaths.PrepareDestPath("EncodeAvisynthToFFmpeg", source, destExt);
            var Encoder = SetupEncoder();

            var Result = Encoder.EncodeAvisynthToFFmpeg(SrcVideo, Dest, videoCodec, audioCodec, encodeArgs, null, feed.RunCallback);

            Assert.Equal(CompletionStatus.Failed, Result);
        }

        [Theory]
        [InlineData(AppPaths.VapourSynth, ".mp4", "libx264", null, "-preset ultrafast", 1)]
        public void EncodeVapourSynthToFFmpeg_Valid_Success(string source, string destExt, string videoCodec, string audioCodec, string encodeArgs, int streamCount) {
            string SrcVideo = AppPaths.GetInputFile(AppPaths.VapourSynth);
            string Dest = AppPaths.PrepareDestPath("EncodeVapourSynthToFFmpeg", source, destExt);
            var Encoder = SetupEncoder();

            var Result = Encoder.EncodeVapourSynthToFFmpeg(SrcVideo, Dest, videoCodec, audioCodec, encodeArgs, null, feed.RunCallback);

            Assert.Equal(CompletionStatus.Success, Result);
            AssertMedia(Dest, streamCount);
        }

        [Theory]
        [InlineData(AppPaths.InvalidFile, ".mp4", "libx264", null, null)]
        [InlineData(AppPaths.Mpeg4, ".mp4", "libx264", null, null)]
        public void EncodeVapourSynthToFFmpeg_Invalid_Failure(string source, string destExt, string videoCodec, string audioCodec, string encodeArgs) {
            string SrcVideo = AppPaths.GetInputFile(source);
            string Dest = AppPaths.PrepareDestPath("EncodeVapourSynthToFFmpeg", source, destExt);
            var Encoder = SetupEncoder();

            var Result = Encoder.EncodeVapourSynthToFFmpeg(SrcVideo, Dest, videoCodec, audioCodec, encodeArgs, null, feed.RunCallback);

            Assert.Equal(CompletionStatus.Failed, Result);
        }


        // X264

        [Theory]
        [InlineData(AppPaths.Mpeg4WithAudio, null)]
        [InlineData(AppPaths.Mpeg4WithAudio, "--preset ultrafast")]
        [InlineData(AppPaths.StreamVp9, "--preset ultrafast")]
        public void EncodeX264_Valid_Success(string source, string encodeArgs) {
            string SrcVideo = AppPaths.GetInputFile(source);
            string Dest = AppPaths.PrepareDestPath("EncodeX264", source, Mp4Ext);
            var Encoder = SetupEncoder();

            var Result = Encoder.EncodeX264(SrcVideo, Dest, encodeArgs, null, feed.RunCallback);

            Assert.Equal(CompletionStatus.Success, Result);
            AssertMedia(Dest, 1);
        }

        [Theory]
        [InlineData(AppPaths.InvalidFile, null)]
        [InlineData(AppPaths.Mpeg4WithAudio, "invalid")]
        [InlineData(AppPaths.StreamOpus, "")]
        public void EncodeX264_Invalid_Failure(string source, string encodeArgs) {
            string SrcVideo = AppPaths.GetInputFile(source);
            string Dest = AppPaths.PrepareDestPath("EncodeX264", source, Mp4Ext);
            var Encoder = SetupEncoder();

            var Result = Encoder.EncodeX264(SrcVideo, Dest, encodeArgs, null, feed.RunCallback);

            Assert.Equal(CompletionStatus.Failed, Result);
        }

        [Theory]
        [InlineData(AppPaths.Avisynth, "--preset ultrafast")]
        [InlineData(AppPaths.Avisynth10bit, "--preset ultrafast")]
        public void EncodeAvisynthToX264_Valid_Success(string source, string encodeArgs) {
            string SrcVideo = AppPaths.GetInputFile(source);
            string Dest = AppPaths.PrepareDestPath("EncodeAvisynthToX264", source, Mp4Ext);
            var Encoder = SetupEncoder();

            var Result = Encoder.EncodeAvisynthToX264(SrcVideo, Dest, encodeArgs, null, feed.RunCallback);

            Assert.Equal(CompletionStatus.Success, Result);
            AssertMedia(Dest, 1);
        }

        [Theory]
        [InlineData(AppPaths.InvalidFile, null)]
        [InlineData(AppPaths.Mpeg4WithAudio, "invalid")]
        [InlineData(AppPaths.StreamOpus, "")]
        public void EncodeAvisynthToX264_Invalid_Failure(string source, string encodeArgs) {
            string SrcVideo = AppPaths.GetInputFile(source);
            string Dest = AppPaths.PrepareDestPath("EncodeAvisynthToX264", source, Mp4Ext);
            var Encoder = SetupEncoder();

            var Result = Encoder.EncodeAvisynthToX264(SrcVideo, Dest, encodeArgs, null, feed.RunCallback);

            Assert.Equal(CompletionStatus.Failed, Result);
        }

        [Theory]
        [InlineData(AppPaths.VapourSynth, "--preset ultrafast")]
        [InlineData(AppPaths.VapourSynth10bit, "--preset ultrafast")]
        public void EncodeVapourSynthToX264_Valid_Success(string source, string encodeArgs) {
            string SrcVideo = AppPaths.GetInputFile(source);
            string Dest = AppPaths.PrepareDestPath("EncodeVapourSynthToX264", source, Mp4Ext);
            var Encoder = SetupEncoder();

            var Result = Encoder.EncodeVapourSynthToX264(SrcVideo, Dest, encodeArgs, null, feed.RunCallback);

            Assert.Equal(CompletionStatus.Success, Result);
            AssertMedia(Dest, 1);
        }

        [Theory]
        [InlineData(AppPaths.InvalidFile, null)]
        [InlineData(AppPaths.Mpeg4WithAudio, "invalid")]
        [InlineData(AppPaths.StreamOpus, "")]
        public void EncodeVapourSynthToX264_Invalid_Failure(string source, string encodeArgs) {
            string SrcVideo = AppPaths.GetInputFile(source);
            string Dest = AppPaths.PrepareDestPath("EncodeVapourSynthToX264", source, Mp4Ext);
            var Encoder = SetupEncoder();

            var Result = Encoder.EncodeVapourSynthToX264(SrcVideo, Dest, encodeArgs, null, feed.RunCallback);

            Assert.Equal(CompletionStatus.Failed, Result);
        }


        // X265

        // X265 only supports RAW and Y4M input sources.
        //[Theory]
        //[InlineData(AppPaths.Mpeg4WithAudio, null)]
        //[InlineData(AppPaths.Mpeg4WithAudio, "--preset ultrafast")]
        //[InlineData(AppPaths.StreamVp9, "--preset ultrafast")]
        //public void EncodeX265_Valid_Success(string source, string encodeArgs) {
        //    string SrcVideo = AppPaths.GetInputFile(source);
        //    string Dest = AppPaths.PrepareDestPath("EncodeX265", source, Mp4Ext);
        //    var Encoder = SetupEncoder();

        //    var Result = Encoder.EncodeX265(SrcVideo, Dest, encodeArgs, null, feed.RunCallback);

        //    Assert.Equal(CompletionStatus.Success, Result);
        //    AssertMedia(Dest, 1);
        //}

        //[Theory]
        //[InlineData(AppPaths.InvalidFile, null)]
        //[InlineData(AppPaths.Mpeg4WithAudio, "invalid")]
        //[InlineData(AppPaths.StreamOpus, "")]
        //public void EncodeX265_Invalid_Failure(string source, string encodeArgs) {
        //    string SrcVideo = AppPaths.GetInputFile(source);
        //    string Dest = AppPaths.PrepareDestPath("EncodeX265", source, Mp4Ext);
        //    var Encoder = SetupEncoder();

        //    var Result = Encoder.EncodeX265(SrcVideo, Dest, encodeArgs, null, feed.RunCallback);

        //    Assert.Equal(CompletionStatus.Failed, Result);
        //}

        [Theory]
        [InlineData(AppPaths.Avisynth, "--preset ultrafast")]
        public void EncodeAvisynthToX265_Valid_Success(string source, string encodeArgs) {
            string SrcVideo = AppPaths.GetInputFile(source);
            string Dest = AppPaths.PrepareDestPath("EncodeAvisynthToX265", source, Mp4Ext);
            var Encoder = SetupEncoder();

            var Result = Encoder.EncodeAvisynthToX265(SrcVideo, Dest, encodeArgs, null, feed.RunCallback);

            Assert.Equal(CompletionStatus.Success, Result);
            AssertMedia(Dest, 1);
        }

        [Theory]
        [InlineData(AppPaths.InvalidFile, null)]
        [InlineData(AppPaths.Mpeg4WithAudio, "invalid")]
        [InlineData(AppPaths.StreamOpus, "")]
        public void EncodeAvisynthToX265_Invalid_Failure(string source, string encodeArgs) {
            string SrcVideo = AppPaths.GetInputFile(source);
            string Dest = AppPaths.PrepareDestPath("EncodeAvisynthToX265", source, Mp4Ext);
            var Encoder = SetupEncoder();

            var Result = Encoder.EncodeAvisynthToX265(SrcVideo, Dest, encodeArgs, null, feed.RunCallback);

            Assert.Equal(CompletionStatus.Failed, Result);
        }

        [Theory]
        [InlineData(AppPaths.VapourSynth, "--preset ultrafast")]
        public void EncodeVapourSynthToX265_Valid_Success(string source, string encodeArgs) {
            string SrcVideo = AppPaths.GetInputFile(source);
            string Dest = AppPaths.PrepareDestPath("EncodeVapourSynthToX265", source, Mp4Ext);
            var Encoder = SetupEncoder();

            var Result = Encoder.EncodeVapourSynthToX265(SrcVideo, Dest, encodeArgs, null, feed.RunCallback);

            Assert.Equal(CompletionStatus.Success, Result);
            AssertMedia(Dest, 1);
        }

        [Theory]
        [InlineData(AppPaths.InvalidFile, null)]
        [InlineData(AppPaths.Mpeg4WithAudio, "invalid")]
        [InlineData(AppPaths.StreamOpus, "")]
        public void EncodeVapourSynthToX265_Invalid_Failure(string source, string encodeArgs) {
            string SrcVideo = AppPaths.GetInputFile(source);
            string Dest = AppPaths.PrepareDestPath("EncodeVapourSynthToX265", source, Mp4Ext);
            var Encoder = SetupEncoder();

            var Result = Encoder.EncodeVapourSynthToX265(SrcVideo, Dest, encodeArgs, null, feed.RunCallback);

            Assert.Equal(CompletionStatus.Failed, Result);
        }
    }
}
