using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace HanumanInstitute.FFmpeg.IntegrationTests
{
    public class MediaEncoderTests
    {

        private IProcessWorkerFactory _factory;
        private readonly ITestOutputHelper _output;
        private readonly OutputFeeder _feed;
        private const string AviExt = ".avi";
        private const string Mp4Ext = ".mp4";

        public MediaEncoderTests(ITestOutputHelper output)
        {
            _output = output;
            _feed = new OutputFeeder(output);
        }

        private IMediaEncoder SetupEncoder()
        {
            _factory = FactoryConfig.CreateWithConfig();
            return new MediaEncoder(_factory);
        }

        private IFileInfoFFmpeg GetFileInfo(string path)
        {
            var info = new MediaInfoReader(_factory);
            return info.GetFileInfo(path);
        }

        private void AssertMedia(string dest, int streamCount)
        {
            Assert.True(File.Exists(dest));
            var fileInfo = GetFileInfo(dest) as IFileInfoFFmpeg;
            Assert.Equal(streamCount, fileInfo.FileStreams.Count);
        }


        // FFmpeg

        [Theory]
        [InlineData(AppPaths.Mpeg4WithAudio, true, 2)]
        [InlineData(AppPaths.Mpeg4WithAudio, false, 1)]
        [InlineData(AppPaths.StreamVp9, true, 1)]
        [InlineData(AppPaths.StreamVp9, false, 1)]
        [InlineData(AppPaths.StreamOpus, true, 1)]
        public void ConvertToAviUtVideo_Valid_Success(string source, bool audio, int streamCount)
        {
            var srcVideo = AppPaths.GetInputFile(source);
            var dest = AppPaths.PrepareDestPath("ConvertToAvi", source, AviExt);
            var encoder = SetupEncoder();

            var result = encoder.ConvertToAviUtVideo(srcVideo, dest, audio, null, _feed.RunCallback);

            Assert.Equal(CompletionStatus.Success, result);
            AssertMedia(dest, streamCount);
        }

        [Theory]
        [InlineData(AppPaths.InvalidFile, true)]
        [InlineData(AppPaths.StreamOpus, false)]
        public void ConvertToAviUtVideo_Invalid_Failure(string source, bool audio)
        {
            var srcVideo = AppPaths.GetInputFile(source);
            var dest = AppPaths.PrepareDestPath("ConvertToAvi", source, AviExt);
            var encoder = SetupEncoder();

            var result = encoder.ConvertToAviUtVideo(srcVideo, dest, audio, null, _feed.RunCallback);

            Assert.Equal(CompletionStatus.Failed, result);
        }

        [Theory]
        [InlineData(AppPaths.Mpeg4WithAudio, ".mp4", "libx264", null, "-preset ultrafast", 1)]
        [InlineData(AppPaths.Mpeg4WithAudio, ".mp4", "libx264", "aac", "-preset ultrafast", 2)]
        [InlineData(AppPaths.Mpeg4WithAudio, ".m4a", null, "aac", "", 1)]
        [InlineData(AppPaths.StreamOpus, ".m4a", "", "aac", "", 1)]
        [InlineData(AppPaths.StreamVp9, ".mp4", "libx264", "", "-preset ultrafast", 1)]
        public void EncodeFFmpeg_Valid_Success(string source, string destExt, string videoCodec, string audioCodec, string encodeArgs, int streamCount)
        {
            var srcVideo = AppPaths.GetInputFile(source);
            var dest = AppPaths.PrepareDestPath("EncodeFFmpeg", source, destExt);
            var encoder = SetupEncoder();

            var result = encoder.EncodeFFmpeg(srcVideo, dest, videoCodec, audioCodec, encodeArgs, null, _feed.RunCallback);

            Assert.Equal(CompletionStatus.Success, result);
            AssertMedia(dest, streamCount);
        }

        [Theory]
        [InlineData(AppPaths.InvalidFile, ".mp4", "libx264", null, null)]
        [InlineData(AppPaths.Mpeg4, ".mp4", null, "aac", null)]
        [InlineData(AppPaths.Mpeg4WithAudio, ".mp4", "libx264", "aac", "invalid")]
        [InlineData(AppPaths.StreamOpus, ".mp4", "", "invalid", "")]
        [InlineData(AppPaths.StreamVp9, ".mp4", "invalid", "", "-preset ultrafast")]
        public void EncodeFFmpeg_Invalid_Failure(string source, string destExt, string videoCodec, string audioCodec, string encodeArgs)
        {
            var srcVideo = AppPaths.GetInputFile(source);
            var dest = AppPaths.PrepareDestPath("EncodeFFmpeg", source, destExt);
            var encoder = SetupEncoder();

            var result = encoder.EncodeFFmpeg(srcVideo, dest, videoCodec, audioCodec, encodeArgs, null, _feed.RunCallback);

            Assert.Equal(CompletionStatus.Failed, result);
        }

        //[Theory]
        //[InlineData(AppPaths.FourStreams, ".mkv", null, 3)]
        //public void EncodeFFmpeg_List_Valid_Success(string source, string destExt, string encodeArgs, int streamCount) {
        //    string srcVideo = AppPaths.GetInputFile(source);
        //    string dest = AppPaths.PrepareDestPath("EncodeFFmpeg_List", source, destExt);
        //    var encoder = SetupEncoder();
        //    string[] VideoCodec = new string[] { "libx264", "libxvid" };
        //    string[] AudioCodec = new string[] { "aac" };

        //    var result = encoder.EncodeFFmpeg(srcVideo, dest, VideoCodec, AudioCodec, encodeArgs, null, feed.RunCallback);

        //    Assert.Equal(CompletionStatus.Success, result);
        //    AssertMedia(dest, streamCount);
        //}

        [Theory]
        [InlineData(AppPaths.Avisynth, ".mp4", "libx264", null, "-preset ultrafast", 1)]
        public void EncodeAvisynthToFFmpeg_Valid_Success(string source, string destExt, string videoCodec, string audioCodec, string encodeArgs, int streamCount)
        {
            var srcVideo = AppPaths.GetInputFile(source);
            var dest = AppPaths.PrepareDestPath("EncodeAvisynthToFFmpeg", source, destExt);
            var encoder = SetupEncoder();

            var result = encoder.EncodeAvisynthToFFmpeg(srcVideo, dest, videoCodec, audioCodec, encodeArgs, null, _feed.RunCallback);

            Assert.Equal(CompletionStatus.Success, result);
            AssertMedia(dest, streamCount);
        }

        [Theory]
        [InlineData(AppPaths.InvalidFile, ".mp4", "libx264", null, null)]
        [InlineData(AppPaths.Mpeg4, ".mp4", "libx264", null, null)]
        public void EncodeAvisynthToFFmpeg_Invalid_Failure(string source, string destExt, string videoCodec, string audioCodec, string encodeArgs)
        {
            var srcVideo = AppPaths.GetInputFile(source);
            var dest = AppPaths.PrepareDestPath("EncodeAvisynthToFFmpeg", source, destExt);
            var encoder = SetupEncoder();

            var result = encoder.EncodeAvisynthToFFmpeg(srcVideo, dest, videoCodec, audioCodec, encodeArgs, null, _feed.RunCallback);

            Assert.Equal(CompletionStatus.Failed, result);
        }

        [Theory]
        [InlineData(AppPaths.VapourSynth, ".mp4", "libx264", null, "-preset ultrafast", 1)]
        public void EncodeVapourSynthToFFmpeg_Valid_Success(string source, string destExt, string videoCodec, string audioCodec, string encodeArgs, int streamCount)
        {
            var srcVideo = AppPaths.GetInputFile(AppPaths.VapourSynth);
            var dest = AppPaths.PrepareDestPath("EncodeVapourSynthToFFmpeg", source, destExt);
            var encoder = SetupEncoder();

            var result = encoder.EncodeVapourSynthToFFmpeg(srcVideo, dest, videoCodec, audioCodec, encodeArgs, null, _feed.RunCallback);

            Assert.Equal(CompletionStatus.Success, result);
            AssertMedia(dest, streamCount);
        }

        [Theory]
        [InlineData(AppPaths.InvalidFile, ".mp4", "libx264", null, null)]
        [InlineData(AppPaths.Mpeg4, ".mp4", "libx264", null, null)]
        public void EncodeVapourSynthToFFmpeg_Invalid_Failure(string source, string destExt, string videoCodec, string audioCodec, string encodeArgs)
        {
            var srcVideo = AppPaths.GetInputFile(source);
            var dest = AppPaths.PrepareDestPath("EncodeVapourSynthToFFmpeg", source, destExt);
            var encoder = SetupEncoder();

            var result = encoder.EncodeVapourSynthToFFmpeg(srcVideo, dest, videoCodec, audioCodec, encodeArgs, null, _feed.RunCallback);

            Assert.Equal(CompletionStatus.Failed, result);
        }


        // X264

        [Theory]
        [InlineData(AppPaths.Mpeg4WithAudio, null)]
        [InlineData(AppPaths.Mpeg4WithAudio, "--preset ultrafast")]
        [InlineData(AppPaths.StreamVp9, "--preset ultrafast")]
        public void EncodeX264_Valid_Success(string source, string encodeArgs)
        {
            var srcVideo = AppPaths.GetInputFile(source);
            var dest = AppPaths.PrepareDestPath("EncodeX264", source, Mp4Ext);
            var encoder = SetupEncoder();

            var result = encoder.EncodeX264(srcVideo, dest, encodeArgs, null, _feed.RunCallback);

            Assert.Equal(CompletionStatus.Success, result);
            AssertMedia(dest, 1);
        }

        [Theory]
        [InlineData(AppPaths.InvalidFile, null)]
        [InlineData(AppPaths.Mpeg4WithAudio, "invalid")]
        [InlineData(AppPaths.StreamOpus, "")]
        public void EncodeX264_Invalid_Failure(string source, string encodeArgs)
        {
            var srcVideo = AppPaths.GetInputFile(source);
            var dest = AppPaths.PrepareDestPath("EncodeX264", source, Mp4Ext);
            var encoder = SetupEncoder();

            var result = encoder.EncodeX264(srcVideo, dest, encodeArgs, null, _feed.RunCallback);

            Assert.Equal(CompletionStatus.Failed, result);
        }

        [Theory]
        [InlineData(AppPaths.Avisynth, "--preset ultrafast")]
        [InlineData(AppPaths.Avisynth10bit, "--preset ultrafast")]
        public void EncodeAvisynthToX264_Valid_Success(string source, string encodeArgs)
        {
            var srcVideo = AppPaths.GetInputFile(source);
            var dest = AppPaths.PrepareDestPath("EncodeAvisynthToX264", source, Mp4Ext);
            var encoder = SetupEncoder();

            var result = encoder.EncodeAvisynthToX264(srcVideo, dest, encodeArgs, null, _feed.RunCallback);

            Assert.Equal(CompletionStatus.Success, result);
            AssertMedia(dest, 1);
        }

        [Theory]
        [InlineData(AppPaths.InvalidFile, null)]
        [InlineData(AppPaths.Mpeg4WithAudio, "invalid")]
        [InlineData(AppPaths.StreamOpus, "")]
        public void EncodeAvisynthToX264_Invalid_Failure(string source, string encodeArgs)
        {
            var srcVideo = AppPaths.GetInputFile(source);
            var dest = AppPaths.PrepareDestPath("EncodeAvisynthToX264", source, Mp4Ext);
            var encoder = SetupEncoder();

            var result = encoder.EncodeAvisynthToX264(srcVideo, dest, encodeArgs, null, _feed.RunCallback);

            Assert.Equal(CompletionStatus.Failed, result);
        }

        [Theory]
        [InlineData(AppPaths.VapourSynth, "--preset ultrafast")]
        [InlineData(AppPaths.VapourSynth10bit, "--preset ultrafast")]
        public void EncodeVapourSynthToX264_Valid_Success(string source, string encodeArgs)
        {
            var srcVideo = AppPaths.GetInputFile(source);
            var dest = AppPaths.PrepareDestPath("EncodeVapourSynthToX264", source, Mp4Ext);
            var encoder = SetupEncoder();

            var result = encoder.EncodeVapourSynthToX264(srcVideo, dest, encodeArgs, null, _feed.RunCallback);

            Assert.Equal(CompletionStatus.Success, result);
            AssertMedia(dest, 1);
        }

        [Theory]
        [InlineData(AppPaths.InvalidFile, null)]
        [InlineData(AppPaths.Mpeg4WithAudio, "invalid")]
        [InlineData(AppPaths.StreamOpus, "")]
        public void EncodeVapourSynthToX264_Invalid_Failure(string source, string encodeArgs)
        {
            var srcVideo = AppPaths.GetInputFile(source);
            var dest = AppPaths.PrepareDestPath("EncodeVapourSynthToX264", source, Mp4Ext);
            var encoder = SetupEncoder();

            var result = encoder.EncodeVapourSynthToX264(srcVideo, dest, encodeArgs, null, _feed.RunCallback);

            Assert.Equal(CompletionStatus.Failed, result);
        }


        // X265

        // X265 only supports RAW and Y4M input sources.
        //[Theory]
        //[InlineData(AppPaths.Mpeg4WithAudio, null)]
        //[InlineData(AppPaths.Mpeg4WithAudio, "--preset ultrafast")]
        //[InlineData(AppPaths.StreamVp9, "--preset ultrafast")]
        //public void EncodeX265_Valid_Success(string source, string encodeArgs) {
        //    string srcVideo = AppPaths.GetInputFile(source);
        //    string dest = AppPaths.PrepareDestPath("EncodeX265", source, Mp4Ext);
        //    var encoder = SetupEncoder();

        //    var result = encoder.EncodeX265(srcVideo, dest, encodeArgs, null, feed.RunCallback);

        //    Assert.Equal(CompletionStatus.Success, result);
        //    AssertMedia(dest, 1);
        //}

        //[Theory]
        //[InlineData(AppPaths.InvalidFile, null)]
        //[InlineData(AppPaths.Mpeg4WithAudio, "invalid")]
        //[InlineData(AppPaths.StreamOpus, "")]
        //public void EncodeX265_Invalid_Failure(string source, string encodeArgs) {
        //    string srcVideo = AppPaths.GetInputFile(source);
        //    string dest = AppPaths.PrepareDestPath("EncodeX265", source, Mp4Ext);
        //    var encoder = SetupEncoder();

        //    var result = encoder.EncodeX265(srcVideo, dest, encodeArgs, null, feed.RunCallback);

        //    Assert.Equal(CompletionStatus.Failed, result);
        //}

        [Theory]
        [InlineData(AppPaths.Avisynth, "--preset ultrafast")]
        public void EncodeAvisynthToX265_Valid_Success(string source, string encodeArgs)
        {
            var srcVideo = AppPaths.GetInputFile(source);
            var dest = AppPaths.PrepareDestPath("EncodeAvisynthToX265", source, Mp4Ext);
            var encoder = SetupEncoder();

            var result = encoder.EncodeAvisynthToX265(srcVideo, dest, encodeArgs, null, _feed.RunCallback);

            Assert.Equal(CompletionStatus.Success, result);
            AssertMedia(dest, 1);
        }

        [Theory]
        [InlineData(AppPaths.InvalidFile, null)]
        [InlineData(AppPaths.Mpeg4WithAudio, "invalid")]
        [InlineData(AppPaths.StreamOpus, "")]
        public void EncodeAvisynthToX265_Invalid_Failure(string source, string encodeArgs)
        {
            var srcVideo = AppPaths.GetInputFile(source);
            var dest = AppPaths.PrepareDestPath("EncodeAvisynthToX265", source, Mp4Ext);
            var encoder = SetupEncoder();

            var result = encoder.EncodeAvisynthToX265(srcVideo, dest, encodeArgs, null, _feed.RunCallback);

            Assert.Equal(CompletionStatus.Failed, result);
        }

        [Theory]
        [InlineData(AppPaths.VapourSynth, "--preset ultrafast")]
        public void EncodeVapourSynthToX265_Valid_Success(string source, string encodeArgs)
        {
            var srcVideo = AppPaths.GetInputFile(source);
            var dest = AppPaths.PrepareDestPath("EncodeVapourSynthToX265", source, Mp4Ext);
            var encoder = SetupEncoder();

            var result = encoder.EncodeVapourSynthToX265(srcVideo, dest, encodeArgs, null, _feed.RunCallback);

            Assert.Equal(CompletionStatus.Success, result);
            AssertMedia(dest, 1);
        }

        [Theory]
        [InlineData(AppPaths.InvalidFile, null)]
        [InlineData(AppPaths.Mpeg4WithAudio, "invalid")]
        [InlineData(AppPaths.StreamOpus, "")]
        public void EncodeVapourSynthToX265_Invalid_Failure(string source, string encodeArgs)
        {
            var srcVideo = AppPaths.GetInputFile(source);
            var dest = AppPaths.PrepareDestPath("EncodeVapourSynthToX265", source, Mp4Ext);
            var encoder = SetupEncoder();

            var result = encoder.EncodeVapourSynthToX265(srcVideo, dest, encodeArgs, null, _feed.RunCallback);

            Assert.Equal(CompletionStatus.Failed, result);
        }
    }
}
