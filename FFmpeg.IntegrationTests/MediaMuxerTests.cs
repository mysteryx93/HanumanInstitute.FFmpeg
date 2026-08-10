using HanumanInstitute.Validators;
// ReSharper disable PossibleMultipleEnumeration

namespace HanumanInstitute.FFmpeg.IntegrationTests;

public class MediaMuxerTests
{
    private readonly OutputFeeder _feed;
    private IEncoderService _factory = null!;

    public MediaMuxerTests(ITestOutputHelper output) => _feed = new OutputFeeder(output);

    private MediaMuxer SetupMuxer()
    {
        _factory = FactoryConfig.CreateWithConfig();
        return new MediaMuxer(_factory, new FileSystemService(), new MediaInfoReader(_factory));
    }

    private FileInfoFFmpeg GetFileInfo(string path) => new MediaInfoReader(_factory).GetFileInfo(path);

    private (MediaMuxer Muxer, string Source, string Destination) Setup(string operation, string source, string destExt)
    {
        var muxer = SetupMuxer();
        var src = AppPaths.GetInputFile(source);
        var dest = AppPaths.PrepareDestPath(operation, source, destExt);
        return (muxer, src, dest);
    }

    private MediaStream FromInfo(string path, MediaStreamInfo info) => MediaStream.FromStreamInfo(path, info);

    private MediaStream FirstStream(string path) => FromInfo(path, GetFileInfo(path).FileStreams[0]);

    public static IEnumerable<object[]> MuxeLists_ValidData()
    {
        yield return [new List<MediaStream> { new(AppPaths.Mpeg4, 2, "h264", FFmpegStreamType.Video) }, ".mp4", 1];
        // adpcm_swf: modern FFmpeg only accepts FLV/WAVE for that codec with -c copy.
        yield return [new List<MediaStream> { new(AppPaths.Flv, 1, "flv", FFmpegStreamType.Audio) }, ".flv", 1];
        yield return
        [
            new List<MediaStream>
            {
                new(AppPaths.StreamAac, 0, "aac", FFmpegStreamType.Audio),
                new(AppPaths.StreamH264, 0, "h264", FFmpegStreamType.Video),
                new(AppPaths.StreamVp9, 0, "vp9", FFmpegStreamType.Video)
            },
            ".mkv", 3
        ];
        yield return
        [
            new List<MediaStream>
            {
                new(AppPaths.StreamAac, 0, "aac", FFmpegStreamType.Audio),
                new(AppPaths.StreamH264, 0, "h264", FFmpegStreamType.Video),
                new(AppPaths.StreamVp9, 0, "vp9", FFmpegStreamType.Video),
                new(AppPaths.StreamOpus, 0, "opus", FFmpegStreamType.Audio)
            },
            ".mkv", 4
        ];
    }

    public static IEnumerable<object[]> MuxeLists_InvalidData()
    {
        yield return [new List<MediaStream> { new("invalidfile", 0, "", FFmpegStreamType.Video) }, ".mp4"];
    }

    public static IEnumerable<object[]> Concatenate_ValidData()
    {
        yield return [new List<string> { AppPaths.Part1 }, ".mp4"];
        yield return [new List<string> { AppPaths.Part1, AppPaths.Part2, AppPaths.Part3 }, ".mp4"];
    }

    public static IEnumerable<object[]> Concatenate_InvalidData()
    {
        yield return [new List<string> { "invalidfile" }, ".mp4"];
    }

    public static IEnumerable<object[]> Truncate_ValidData()
    {
        yield return [AppPaths.StreamVp9, ".webm", null, TimeSpan.FromSeconds(5)];
        yield return [AppPaths.Mpeg4WithAudio, ".mp4", TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(3)];
        yield return [AppPaths.StreamOpus, ".ogg", TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(8)];
    }

    public static IEnumerable<object[]> Truncate_InvalidData()
    {
        yield return ["invalidfile", ".webm", null, TimeSpan.FromSeconds(5)];
    }

    [Theory]
    [InlineData(AppPaths.StreamH264, AppPaths.StreamAac, ".mp4", 2)]
    [InlineData(AppPaths.StreamVp9, AppPaths.StreamOpus, ".webm", 2)]
    [InlineData(AppPaths.StreamH264, AppPaths.StreamOpus, ".mkv", 2)]
    [InlineData(AppPaths.Mpeg2, AppPaths.Flv, ".mov", 2)]
    [InlineData(AppPaths.Flv, AppPaths.StreamOpus, ".mkv", 2)]
    [InlineData(AppPaths.StreamH264, null, ".mp4", 1)]
    [InlineData("", AppPaths.StreamOpus, ".webm", 1)]
    public void Muxe_VideoAndOrAudio_ProducesExpectedStreamCount(string videoFile, string audioFile, string destExt, int streamCount)
    {
        var (muxer, _, dest) = Setup("Muxe", videoFile, destExt);
        var video = string.IsNullOrEmpty(videoFile) ? null : AppPaths.GetInputFile(videoFile);
        var audio = string.IsNullOrEmpty(audioFile) ? null : AppPaths.GetInputFile(audioFile);

        var result = muxer.Muxe(video, audio, dest, callback: _feed.RunCallback);

        Assert.Equal(CompletionStatus.Success, result);
        Assert.True(File.Exists(dest));
        Assert.Equal(streamCount, GetFileInfo(dest).FileStreams.Count);
    }

    [Theory]
    [MemberData(nameof(MuxeLists_ValidData))]
    public void Muxe_StreamList_ProducesExpectedStreamCount(IEnumerable<MediaStream> definitions, string destExt, int streamCount)
    {
        var streams = definitions
            .Select(s => new MediaStream(AppPaths.GetInputFile(s.Path), s.Index, s.Format, s.Type))
            .ToList();
        var (muxer, _, dest) = Setup("MuxeList", streams[0].Path, destExt);

        var result = muxer.Muxe(streams, dest, callback: _feed.RunCallback);

        Assert.Equal(CompletionStatus.Success, result);
        Assert.True(File.Exists(dest));
        Assert.Equal(streamCount, GetFileInfo(dest).FileStreams.Count);
    }

    [Theory]
    [MemberData(nameof(MuxeLists_InvalidData))]
    public void Muxe_MissingInputFile_ReturnsFailed(IEnumerable<MediaStream> definitions, string destExt)
    {
        var streams = definitions
            .Select(s => new MediaStream(AppPaths.GetInputFile(s.Path), s.Index, s.Format, s.Type))
            .ToList();
        var (muxer, _, dest) = Setup("MuxeFailed", streams[0].Path, destExt);

        var result = muxer.Muxe(streams, dest, callback: _feed.RunCallback);

        Assert.Equal(CompletionStatus.Failed, result);
    }

    [Theory]
    [InlineData(AppPaths.StreamOpus, ".ogg")]
    [InlineData(AppPaths.Mpeg4WithAudio, ".mkv")]
    [InlineData(AppPaths.Flv, ".flv")]
    public void ExtractAudio_ValidSource_WritesOutputFile(string source, string destExt)
    {
        var (muxer, src, dest) = Setup("ExtractAudio", source, destExt);

        var result = muxer.ExtractAudio(src, dest, callback: _feed.RunCallback);

        Assert.Equal(CompletionStatus.Success, result);
        Assert.True(File.Exists(dest));
    }

    [Fact]
    public void ExtractAudio_UnsupportedExtension_ReturnsFailed()
    {
        var (muxer, src, dest) = Setup("ExtractAudio", AppPaths.Mpeg2, ".aaa");

        var result = muxer.ExtractAudio(src, dest, callback: _feed.RunCallback);

        Assert.Equal(CompletionStatus.Failed, result);
        Assert.False(File.Exists(dest));
    }

    [Theory]
    [InlineData(AppPaths.Mpeg2, ".mp4")]
    [InlineData(AppPaths.Mpeg4, ".mp4")]
    [InlineData(AppPaths.Flv, ".mkv")]
    public void ExtractVideo_ValidSource_WritesOutputFile(string source, string destExt)
    {
        var (muxer, src, dest) = Setup("ExtractVideo", source, destExt);

        var result = muxer.ExtractVideo(src, dest, callback: _feed.RunCallback);

        Assert.Equal(CompletionStatus.Success, result);
        Assert.True(File.Exists(dest));
    }

    [Fact]
    public void ExtractVideo_UnsupportedExtension_ReturnsFailed()
    {
        var (muxer, src, dest) = Setup("ExtractVideo", AppPaths.Mpeg4, ".bbb");

        var result = muxer.ExtractVideo(src, dest, callback: _feed.RunCallback);

        Assert.Equal(CompletionStatus.Failed, result);
        Assert.False(File.Exists(dest));
    }

    [Theory]
    [MemberData(nameof(Concatenate_ValidData))]
    public void Concatenate_ValidFiles_WritesOutputFile(IEnumerable<string> sources, string destExt)
    {
        var files = sources.Select(AppPaths.GetInputFile).ToList();
        var (muxer, _, dest) = Setup("Concatenate", sources.First(), destExt);

        var result = muxer.Concatenate(files, dest, callback: _feed.RunCallback);

        Assert.Equal(CompletionStatus.Success, result);
        Assert.True(File.Exists(dest));
    }

    [Theory]
    [MemberData(nameof(Concatenate_InvalidData))]
    public void Concatenate_MissingInput_ReturnsFailed(IEnumerable<string> sources, string destExt)
    {
        var files = sources.Select(AppPaths.GetInputFile).ToList();
        var (muxer, _, dest) = Setup("Concatenate", sources.First(), destExt);

        var result = muxer.Concatenate(files, dest, callback: _feed.RunCallback);

        Assert.Equal(CompletionStatus.Failed, result);
    }

    [Theory]
    [MemberData(nameof(Truncate_ValidData))]
    public void Truncate_ValidRange_MatchesDuration(string source, string destExt, TimeSpan? start, TimeSpan? duration)
    {
        var (muxer, src, dest) = Setup("Truncate", source, destExt);

        var result = muxer.Truncate(src, dest, start, duration, callback: _feed.RunCallback);

        Assert.Equal(CompletionStatus.Success, result);
        Assert.True(File.Exists(dest));
        if (duration.HasValue)
            Assert.InRange(Math.Abs((duration.Value - GetFileInfo(dest).FileDuration).TotalSeconds), 0, .1);
    }

    [Theory]
    [MemberData(nameof(Truncate_InvalidData))]
    public void Truncate_MissingInput_ReturnsFailed(string source, string destExt, TimeSpan? start, TimeSpan? duration)
    {
        var (muxer, src, dest) = Setup("Truncate", source, destExt);

        var result = muxer.Truncate(src, dest, start, duration, callback: _feed.RunCallback);

        Assert.Equal(CompletionStatus.Failed, result);
    }

    [Fact]
    public void Muxe_SeparateAVWithFromRest_CopiesContainerAndAttachment()
    {
        var (muxer, original, dest) = Setup("MuxeFromRest", AppPaths.TaggedAttachment, ".mkv");
        var videoPath = AppPaths.GetInputFile(AppPaths.StreamH264);
        var audioPath = AppPaths.GetInputFile(AppPaths.StreamAac);
        var streams = new[] { FirstStream(videoPath), FirstStream(audioPath) };
        var options = new MuxOptions().From(original).All().Video(false).Audio(false);

        var result = muxer.Muxe(streams, dest, options, callback: _feed.RunCallback);

        Assert.Equal(CompletionStatus.Success, result);
        var outInfo = GetFileInfo(dest);
        Assert.Contains(outInfo.FileStreams, s => s.StreamType == FFmpegStreamType.Video);
        Assert.Contains(outInfo.FileStreams, s => s.StreamType == FFmpegStreamType.Audio);
        Assert.Contains(outInfo.FileStreams, s => s.StreamType == FFmpegStreamType.Attachment);
        Assert.Equal("With Attachment", outInfo.Metadata["title"]);
    }

    [Fact]
    public void Muxe_PrimaryAac432Hz_IsOnlyDefaultAudio()
    {
        var (muxer, src, dest) = Setup("MuxePrimary432", AppPaths.Mpeg4WithAudio, ".mkv");
        var newAac = AppPaths.GetInputFile(AppPaths.StreamAac);
        var info = GetFileInfo(src);
        // Client: mark new primary default only — library demotes other audio defaults (FromStreamInfo keeps source default).
        var primary = FirstStream(newAac);
        primary.Metadata["frequency"] = "432Hz";
        primary.Disposition = new StreamDisposition().Set("default");
        var streams = new[]
        {
            FromInfo(src, info.VideoStream!),
            primary,
            FromInfo(src, info.AudioStream!)
        };
        var options = new MuxOptions().From(src).Container();

        var result = muxer.Muxe(streams, dest, options, callback: _feed.RunCallback);

        Assert.Equal(CompletionStatus.Success, result);
        var audios = GetFileInfo(dest).FileStreams.OfType<MediaAudioStreamInfo>().ToList();
        Assert.Equal(2, audios.Count);
        Assert.Equal("432Hz", audios[0].Metadata.GetValueOrDefault("frequency", ""));
        Assert.True(audios[0].Disposition.Has("default"));
        Assert.False(audios[1].Disposition.Has("default"));
        Assert.Single(audios, a => a.Disposition.Has("default"));
    }

    [Fact]
    public void Muxe_TwoStreamTitles_Mp4_PreservesBothOnRemux()
    {
        // First mux: write title on the new audio track (MP4 stores it as name).
        var (muxer, src, mid) = Setup("MuxeTwoTitlesMid", AppPaths.Mpeg4WithAudio, ".mp4");
        var aac = AppPaths.GetInputFile(AppPaths.StreamAac);
        var info = GetFileInfo(src);
        var first = FirstStream(aac);
        first.Metadata["title"] = "432Hz";
        first.Disposition = new StreamDisposition().Set("default");
        var firstStreams = new[]
        {
            first,
            FromInfo(src, info.VideoStream!),
            FromInfo(src, info.AudioStream!)
        };

        Assert.Equal(CompletionStatus.Success, muxer.Muxe(firstStreams, mid, new MuxOptions().From(src).Container(), callback: _feed.RunCallback));

        var midInfo = GetFileInfo(mid);
        var midAudios = midInfo.FileStreams.OfType<MediaAudioStreamInfo>().ToList();
        Assert.Equal(2, midAudios.Count);
        Assert.True(
            midAudios[0].Metadata.GetValueOrDefault("title", "") == "432Hz" ||
            midAudios[0].Metadata.GetValueOrDefault("name", "") == "432Hz");

        // Second mux: probe mid (title appears as name on MP4), add another titled stream.
        // Both titles must survive — writing "name=" fails on MP4; builder must emit "title=".
        var dest = AppPaths.PrepareDestPath("MuxeTwoTitlesFinal", AppPaths.Mpeg4WithAudio, ".mp4");
        var newest = FirstStream(aac);
        newest.Metadata["title"] = "528Hz";
        newest.Disposition = new StreamDisposition().Set("default");
        var remuxStreams = new List<MediaStream> { newest };
        remuxStreams.AddRange(midInfo.FileStreams.Select(s => FromInfo(mid, s)));

        Assert.Equal(CompletionStatus.Success, muxer.Muxe(remuxStreams, dest, new MuxOptions().From(mid).Container(), callback: _feed.RunCallback));

        var outAudios = GetFileInfo(dest).FileStreams.OfType<MediaAudioStreamInfo>().ToList();
        Assert.Equal(3, outAudios.Count);
        static string TitleOf(MediaStreamInfo s) =>
            s.Metadata.TryGetValue("title", out var t) && t.HasValue() ? t
            : s.Metadata.TryGetValue("name", out var n) && n.HasValue() ? n
            : null;
        Assert.Contains(outAudios, a => TitleOf(a) == "528Hz");
        Assert.Contains(outAudios, a => TitleOf(a) == "432Hz");
        Assert.True(outAudios[0].Disposition.Has("default"));
        Assert.Single(outAudios, a => a.Disposition.Has("default"));
    }

    [Fact]
    public void Muxe_ThreeAudio_FirstTrackIsOnlyDefault()
    {
        var (muxer, src, dest) = Setup("MuxeThreeAudio", AppPaths.Mpeg4WithAudio, ".mkv");
        var info = GetFileInfo(src);
        var newest = FirstStream(AppPaths.GetInputFile(AppPaths.StreamAac));
        newest.Metadata["frequency"] = "432Hz";
        newest.Disposition = new StreamDisposition().Set("default");
        // middle/oldest: no manual clear — source default on original must not remain default.
        var streams = new[]
        {
            FromInfo(src, info.VideoStream!),
            newest,
            FirstStream(AppPaths.GetInputFile(AppPaths.StreamOpus)),
            FromInfo(src, info.AudioStream!)
        };
        var options = new MuxOptions().From(src).Container();

        var result = muxer.Muxe(streams, dest, options, callback: _feed.RunCallback);

        Assert.Equal(CompletionStatus.Success, result);
        var audios = GetFileInfo(dest).FileStreams.OfType<MediaAudioStreamInfo>().ToList();
        Assert.Equal(3, audios.Count);
        Assert.Equal("432Hz", audios[0].Metadata.GetValueOrDefault("frequency", ""));
        Assert.True(audios[0].Disposition.Has("default"));
        Assert.False(audios[1].Disposition.Has("default"));
        Assert.False(audios[2].Disposition.Has("default"));
        Assert.Single(audios, a => a.Disposition.Has("default"));
    }

    [Fact]
    public void Muxe_WithoutFrequencyStreams_PreservesOtherStreams()
    {
        var (muxer, src, dest) = Setup("MuxeStripFreq", AppPaths.TaggedMkv, ".mkv");
        var info = GetFileInfo(src);
        var streams = info.FileStreams
            .Where(s => !s.Metadata.ContainsKey("frequency"))
            .Select(s => FromInfo(src, s))
            .ToList();
        var options = new MuxOptions().From(src).Container();

        var result = muxer.Muxe(streams, dest, options, callback: _feed.RunCallback);

        Assert.Equal(CompletionStatus.Success, result);
        var output = GetFileInfo(dest);
        Assert.DoesNotContain(output.FileStreams, s => s.Metadata.ContainsKey("frequency"));
        Assert.Contains(output.FileStreams, s => s.StreamType == FFmpegStreamType.Video);
        Assert.Contains(output.FileStreams, s => s.StreamType == FFmpegStreamType.Subtitle);
        Assert.Empty(output.FileStreams.OfType<MediaAudioStreamInfo>());
        Assert.Equal("Sample Title", output.Metadata["title"]);
    }

    [Fact]
    public void Muxe_SwapDefault_KeepsOrderAndTags()
    {
        var (muxer, src, dest) = Setup("MuxeSwapDefault", AppPaths.TaggedMkv, ".mkv");
        var info = GetFileInfo(src);
        var streams = info.FileStreams.Select(s => FromInfo(src, s)).ToList();
        var audios = streams.Where(s => s.Type == FFmpegStreamType.Audio).ToList();
        audios[0].Disposition = new StreamDisposition().Clear();
        audios[1].Disposition = new StreamDisposition().Set("default");
        var options = new MuxOptions().From(src).Container();

        var result = muxer.Muxe(streams, dest, options, callback: _feed.RunCallback);

        Assert.Equal(CompletionStatus.Success, result);
        var outAudios = GetFileInfo(dest).FileStreams.OfType<MediaAudioStreamInfo>().ToList();
        Assert.Equal(info.FileStreams.Count, GetFileInfo(dest).FileStreams.Count);
        Assert.False(outAudios[0].Disposition.Has("default"));
        Assert.True(outAudios[1].Disposition.Has("default"));
        Assert.Single(outAudios, a => a.Disposition.Has("default"));
        Assert.Equal("440", outAudios[0].Metadata.GetValueOrDefault("frequency", ""));
        Assert.Equal("432", outAudios[1].Metadata.GetValueOrDefault("frequency", ""));
    }

    [Fact]
    public void Muxe_TaggedMkv_RoundTripsStreamsAndTags()
    {
        var (muxer, src, dest) = Setup("MuxeTaggedRoundTrip", AppPaths.TaggedMkv, ".mkv");
        var info = GetFileInfo(src);
        var streams = info.FileStreams.Select(s => FromInfo(src, s)).ToList();
        var options = new MuxOptions().From(src).Container();

        var result = muxer.Muxe(streams, dest, options, callback: _feed.RunCallback);

        Assert.Equal(CompletionStatus.Success, result);
        var output = GetFileInfo(dest);
        Assert.Equal(info.FileStreams.Count, output.FileStreams.Count);
        Assert.Equal("Sample Title", output.Metadata["title"]);
        Assert.Equal("440", output.FileStreams.OfType<MediaAudioStreamInfo>().First().Metadata.GetValueOrDefault("frequency", ""));
        Assert.Contains(output.FileStreams, s => s.StreamType == FFmpegStreamType.Subtitle);
    }
}
