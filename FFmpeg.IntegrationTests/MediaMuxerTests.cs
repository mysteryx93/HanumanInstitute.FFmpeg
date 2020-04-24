using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DjvuNet.Tests.Xunit;
using Xunit;
using Xunit.Abstractions;
using HanumanInstitute.FFmpeg.Services;

namespace HanumanInstitute.FFmpeg.IntegrationTests
{
    public class MediaMuxerTests
    {
        private readonly ITestOutputHelper _output;
        private readonly OutputFeeder _feed;
        private IProcessWorkerFactory _factory;

        public MediaMuxerTests(ITestOutputHelper output)
        {
            _output = output;
            _feed = new OutputFeeder(output);
        }

        private IMediaMuxer SetupMuxer()
        {
            _factory = FactoryConfig.CreateWithConfig();
            return new MediaMuxer(_factory, new FileSystemService(), new MediaInfoReader(_factory));
        }

        private FileInfoFFmpeg GetFileInfo(string path)
        {
            var info = new MediaInfoReader(_factory);
            return info.GetFileInfo(path);
        }

        public static IEnumerable<object[]> GenerateMuxeLists_Valid()
        {
            yield return new object[] {
                new List<MediaStream>() {
                    new MediaStream(AppPaths.Mpeg4, 2, "h264", FFmpegStreamType.Video),
                },
                ".mp4", 1
            };
            yield return new object[] {
                new List<MediaStream>() {
                    new MediaStream(AppPaths.Flv, 1, "flv", FFmpegStreamType.Audio)
                },
                ".mkv", 1
            };
            yield return new object[] {
                new List<MediaStream>() {
                    new MediaStream(AppPaths.StreamAac, 0, "aac", FFmpegStreamType.Audio),
                    new MediaStream(AppPaths.StreamH264, 0, "h264", FFmpegStreamType.Video),
                    new MediaStream(AppPaths.StreamVp9, 0, "vp9", FFmpegStreamType.Video)
                },
                ".mkv", 3
            };
            yield return new object[] {
                new List<MediaStream>() {
                    new MediaStream(AppPaths.StreamAac, 0, "aac", FFmpegStreamType.Audio),
                    new MediaStream(AppPaths.StreamH264, 0, "h264", FFmpegStreamType.Video),
                    new MediaStream(AppPaths.StreamVp9, 0, "vp9", FFmpegStreamType.Video),
                    new MediaStream(AppPaths.StreamOpus, 0, "opus", FFmpegStreamType.Audio)
                },
                ".mkv", 4
            };
        }

        public static IEnumerable<object[]> GenerateMuxeLists_Invalid()
        {
            yield return new object[] {
                new List<MediaStream>() {
                    new MediaStream("invalidfile", 0, "", FFmpegStreamType.Video),
                },
                ".mp4", 1
            };
        }

        public static IEnumerable<object[]> GenerateConcatenate_Valid()
        {
            yield return new object[] {
                new List<string>() {
                    AppPaths.Part1
                },
                ".mp4"
            };
            yield return new object[] {
                new List<string>() {
                    AppPaths.Part1, AppPaths.Part2, AppPaths.Part3
                },
                ".mp4"
            };
        }

        public static IEnumerable<object[]> GenerateConcatenate_Invalid()
        {
            yield return new object[] {
                new List<string>() {
                    "invalidfile"
                },
                ".mp4"
            };
        }

        public static IEnumerable<object[]> GenerateTruncate_Valid()
        {
            yield return new object[] {
                AppPaths.StreamVp9,
                ".webm",
                null,
                TimeSpan.FromSeconds(5)
            };
            yield return new object[] {
                AppPaths.Mpeg4WithAudio,
                ".mp4",
                TimeSpan.FromSeconds(4),
                TimeSpan.FromSeconds(3)
            };
            yield return new object[] {
                AppPaths.StreamOpus,
                ".ogg",
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(8)
            };
        }

        public static IEnumerable<object[]> GenerateTruncate_Invalid()
        {
            yield return new object[] {
                "invalidfile",
                ".webm",
                null,
                TimeSpan.FromSeconds(5)
            };
        }


        [Theory]
        [InlineData(AppPaths.StreamH264, AppPaths.StreamAac, ".mp4", 2)]
        [InlineData(AppPaths.StreamVp9, AppPaths.StreamOpus, ".webm", 2)]
        [InlineData(AppPaths.StreamH264, AppPaths.StreamOpus, ".mkv", 2)]
        [InlineData(AppPaths.Mpeg2, AppPaths.Flv, ".mkv", 2)]
        [InlineData(AppPaths.Flv, AppPaths.StreamOpus, ".mkv", 2)]
        [InlineData(AppPaths.StreamH264, null, ".mp4", 1)]
        [InlineData("", AppPaths.StreamOpus, ".webm", 1)]
        public void Muxe_Simple_Valid_Success(string videoFile, string audioFile, string destExt, int streamCount)
        {
            var srcVideo = AppPaths.GetInputFile(videoFile);
            var srcAudio = AppPaths.GetInputFile(audioFile);
            var dest = AppPaths.PrepareDestPath("Muxe", videoFile, destExt);
            var muxer = SetupMuxer();

            var result = muxer.Muxe(srcVideo, srcAudio, dest, null, _feed.RunCallback);

            Assert.Equal(CompletionStatus.Success, result);
            Assert.True(File.Exists(dest));
            var fileInfo = GetFileInfo(dest);
            Assert.Equal(streamCount, fileInfo.FileStreams.Count);
        }

        [DjvuTheory]
        [MemberData(nameof(GenerateMuxeLists_Valid))]
        public void Muxe_List_Valid_Success(IEnumerable<MediaStream> fileStreams, string destExt, int streamCount)
        {
            var dest = AppPaths.PrepareDestPath("MuxeList", fileStreams.First().Path, destExt);
            foreach (var item in fileStreams)
            {
                item.Path = AppPaths.GetInputFile(item.Path);
            }
            var muxer = SetupMuxer();

            var result = muxer.Muxe(fileStreams, dest, null, _feed.RunCallback);

            Assert.Equal(CompletionStatus.Success, result);
            Assert.True(File.Exists(dest));
            var fileInfo = GetFileInfo(dest);
            Assert.Equal(streamCount, fileInfo.FileStreams.Count);
        }

        [DjvuTheory]
        [MemberData(nameof(GenerateMuxeLists_Invalid))]
        public void Muxe_List_Invalid_ReturnsStatusFailed(IEnumerable<MediaStream> fileStreams, string destExt, int _)
        {
            var dest = AppPaths.PrepareDestPath("MuxeFailed", fileStreams.First().Path, destExt);
            foreach (var item in fileStreams)
            {
                item.Path = AppPaths.GetInputFile(item.Path);
            }
            var muxer = SetupMuxer();

            var result = muxer.Muxe(fileStreams, dest, null, _feed.RunCallback);

            Assert.Equal(CompletionStatus.Failed, result);
        }

        [Theory]
        [InlineData(AppPaths.StreamOpus, ".ogg")]
        [InlineData(AppPaths.Mpeg4WithAudio, ".mkv")]
        [InlineData(AppPaths.Flv, ".mkv")]
        public void ExtractAudio_Valid_Success(string source, string destExt)
        {
            var src = AppPaths.GetInputFile(source);
            var dest = AppPaths.PrepareDestPath("ExtractAudio", source, destExt);
            var muxer = SetupMuxer();

            var result = muxer.ExtractAudio(src, dest, null, _feed.RunCallback);

            Assert.Equal(CompletionStatus.Success, result);
            Assert.True(File.Exists(dest));
        }

        [Theory]
        [InlineData(AppPaths.Mpeg2, ".aaa")]
        public void ExtractAudio_WrongExtension_StatusFailed(string source, string destExt)
        {
            var src = AppPaths.GetInputFile(source);
            var dest = AppPaths.PrepareDestPath("ExtractAudio", source, destExt);
            var muxer = SetupMuxer();

            var result = muxer.ExtractAudio(src, dest, null, _feed.RunCallback);

            Assert.Equal(CompletionStatus.Failed, result);
            Assert.False(File.Exists(dest));
        }


        [Theory]
        [InlineData(AppPaths.Mpeg2, ".mp4")]
        [InlineData(AppPaths.Mpeg4, ".mp4")]
        [InlineData(AppPaths.Flv, ".mkv")]
        public void ExtractVideo_Valid_Success(string source, string destExt)
        {
            var src = AppPaths.GetInputFile(source);
            var dest = AppPaths.PrepareDestPath("ExtractVideo", source, destExt);
            var muxer = SetupMuxer();

            var result = muxer.ExtractVideo(src, dest, null, _feed.RunCallback);

            Assert.Equal(CompletionStatus.Success, result);
            Assert.True(File.Exists(dest));
        }

        [Theory]
        [InlineData(AppPaths.Mpeg4, ".bbb")]
        public void ExtractVideo_WrongExtension_StatusFailed(string source, string destExt)
        {
            var src = AppPaths.GetInputFile(source);
            var dest = AppPaths.PrepareDestPath("ExtractVideo", source, destExt);
            var muxer = SetupMuxer();

            var result = muxer.ExtractVideo(src, dest, null, _feed.RunCallback);

            Assert.Equal(CompletionStatus.Failed, result);
            Assert.False(File.Exists(dest));
        }

        [Theory]
        [MemberData(nameof(GenerateConcatenate_Valid))]
        public void Concatenate_Valid_Success(IEnumerable<string> source, string destExt)
        {
            var src = source.Select(x => AppPaths.GetInputFile(x)).ToList();
            var dest = AppPaths.PrepareDestPath("Concatenate", source.First(), destExt);
            var muxer = SetupMuxer();

            var result = muxer.Concatenate(src, dest, null, _feed.RunCallback);

            Assert.Equal(CompletionStatus.Success, result);
            Assert.True(File.Exists(dest));
        }

        [Theory]
        [MemberData(nameof(GenerateConcatenate_Invalid))]
        public void Concatenate_Invalid_StatusFailed(IEnumerable<string> source, string destExt)
        {
            var src = source.Select(x => AppPaths.GetInputFile(x)).ToList();
            var dest = AppPaths.PrepareDestPath("Concatenate", source.First(), destExt);
            var muxer = SetupMuxer();

            var result = muxer.Concatenate(src, dest, null, _feed.RunCallback);

            Assert.Equal(CompletionStatus.Failed, result);
        }

        [DjvuTheory]
        [MemberData(nameof(GenerateTruncate_Valid))]
        public void Truncate_Valid_Success(string source, string destExt, TimeSpan? startPos, TimeSpan? duration)
        {
            var src = AppPaths.GetInputFile(source);
            var dest = AppPaths.PrepareDestPath("Truncate", source, destExt);
            var muxer = SetupMuxer();
            IProcessWorker manager = null;
            void Started(object s, ProcessStartedEventArgs e)
            {
                manager = e.ProcessWorker;
                _feed.RunCallback(s, e);
            }

            var result = muxer.Truncate(src, dest, startPos, duration, null, Started);

            Assert.Equal(CompletionStatus.Success, result);
            Assert.True(File.Exists(dest));
            var fileInfo = GetFileInfo(dest);
            if (duration.HasValue)
            {
                Assert.True(Math.Abs((duration.Value - fileInfo.FileDuration).TotalSeconds) < .1, "Truncate did not produce expected file duration.");
            }
        }

        [DjvuTheory]
        [MemberData(nameof(GenerateTruncate_Invalid))]
        public void Truncate_Invalid_StatusFailed(string source, string destExt, TimeSpan? startPos, TimeSpan? duration)
        {
            var src = AppPaths.GetInputFile(source);
            var dest = AppPaths.PrepareDestPath("Truncate", source, destExt);
            var muxer = SetupMuxer();

            var result = muxer.Truncate(src, dest, startPos, duration, null, _feed.RunCallback);

            Assert.Equal(CompletionStatus.Failed, result);
        }
    }
}
