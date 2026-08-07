// ReSharper disable StringLiteralTypo
namespace HanumanInstitute.FFmpeg.UnitTests;

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

        var info = parser as FileInfoFFmpeg;
        Assert.NotNull(info?.FileStreams);
        Assert.Equal(streamCount, info.FileStreams.Count);
    }

    [Theory]
    [InlineData("Stream #0:1[0x1c0]: Audio: mp2, 44100 Hz, stereo, s16p, 224 kb/s", 1, "mp2", 44100, "stereo", "s16p", 224)]
    [InlineData("  Stream #0:0: Audio: mp3, 44100 Hz, stereo, s16p, 192 kb/s", 0, "mp3", 44100, "stereo", "s16p", 192)]
    [InlineData("    Stream #0:1(und): Audio: aac (LC) (mp4a / 0x6134706D), 44100 Hz, stereo, fltp, 132 kb/s (default)", 1, "aac", 44100, "stereo", "fltp", 132)]
    public void ParseAudioStreamInfo_Valid_ReturnsExpectedData(string text, int index, string format, int sampleRate, string channels, string bitDepth, int bitrate)
    {
        var result = FileInfoFFmpeg.ParseStreamInfo(text);

        var info = result as MediaAudioStreamInfo;
        Assert.NotNull(info);
        Assert.Equal(text.Trim(), info.RawText);
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
    [InlineData("    Stream #0: Audio: mp3, 44100 Hz, stereo, s16p, 192 kb/s")]
    public void ParseAudioStreamInfo_Invalid_ReturnsNull(string text)
    {
        var result = FileInfoFFmpeg.ParseStreamInfo(text);

        Assert.Null(result);
    }

    [Theory]
    [InlineData("Stream #0:1: Video: this, , , is; invalid data", 1, "this", "", "", "", 0, 0, 1, 1, 1, 1, 0, 8, 0)]
    [InlineData(" Stream #0:0[0x1e0]: Video: mpeg1video, yuv420p(tv), 352x288 [SAR 178:163 DAR 1958:1467], 1150 kb/s, 25 fps, 25 tbr, 90k tbn, 25 tbc", 0, "mpeg1video", "yuv420p", "tv", "", 352, 288, 178, 163, 1958, 1467, 25, 8, 1150)]
    [InlineData("   Stream #0:1: Video: mjpeg, yuvj420p(pc, bt470bg/unknown/unknown), 1000x1000 [SAR 1:1 DAR 1:1], 90k tbr, 90k tbn, 90k tbc", 1, "mjpeg", "yuvj420p", "pc", "bt470bg/unknown/unknown", 1000, 1000, 1, 1, 1, 1, 0, 8, 0)]
    [InlineData("    Stream #0:0(und): Video: h264 (High) (avc1 / 0x31637661), yuv420p, 352x288 [SAR 178:163 DAR 1958:1467], 228 kb/s, 25 fps, 25 tbr, 12800 tbn, 50 tbc (default)", 0, "h264", "yuv420p", "", "", 352, 288, 178, 163, 1958, 1467, 25, 8, 228)]
    public void ParseVideoStreamInfo_Valid_ReturnsExpectedData(string text, int index, string format, string colorSpace, string colorRange, string colorMatrix, int width, int height, int sar1, int sar2, int dar1, int dar2, double frameRate, int bitDepth, int bitrate)
    {
        var result = FileInfoFFmpeg.ParseStreamInfo(text);

        var info = result as MediaVideoStreamInfo;
        Assert.NotNull(info);
        Assert.Equal(text.Trim(), info.RawText);
        Assert.Equal(FFmpegStreamType.Video, info.StreamType);
        Assert.Equal(index, info.Index);
        Assert.Equal(format, info.Format);
        Assert.Equal(colorSpace, info.ColorSpace);
        Assert.Equal(colorRange, info.ColorRange);
        Assert.Equal(colorMatrix, info.ColorMatrix);
        Assert.Equal(width, info.Width);
        Assert.Equal(height, info.Height);
        Assert.Equal(sar1, info.Sar1);
        Assert.Equal(sar2, info.Sar2);
        Assert.Equal(dar1, info.Dar1);
        Assert.Equal(dar2, info.Dar2);
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
    [InlineData(null, null, "")]
    [InlineData("", "", "")]
    [InlineData(" ", "     ", "")]
    [InlineData("   =   ", " ", "")]
    [InlineData("mode=1 key=MyKey key=value2 ", "key", "MyKey")]
    [InlineData("mode=1 key2=MyKey key=value2", "key", "value2")]
    [InlineData("mode=1 key2=MyKey key=value2   ", "key", "value2")]
    [InlineData("mode=1 key2=MyKey key3=value2", "key", "")]
    public void ParseAttribute_Any_ReturnsExpectedValue(string text, string key, string expected)
    {
        var result = FileInfoFFmpeg.ParseAttribute(text, key);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ParseFileInfo_WithMetadata_ParsesMetadataAndFormatName()
    {
        var parser = (FileInfoFFmpeg)SetupParser();

        parser.ParseFileInfo(OutputSamples.FFmpegInfo1);

        Assert.Equal("mp3", parser.FormatName);
        Assert.Equal("Nu", parser.Metadata["title"]);
        Assert.Equal("DJ Project", parser.Metadata["artist"]);
        Assert.Equal("Soapte", parser.Metadata["album"]);
        Assert.Equal("2005", parser.Metadata["date"]);
        Assert.Equal("Dance", parser.Metadata["genre"]);
    }

    [Fact]
    public void ParseFileInfo_WithStreamMetadata_ParsesStreamTags()
    {
        var parser = (FileInfoFFmpeg)SetupParser();

        parser.ParseFileInfo(OutputSamples.FFmpegInfo1);

        Assert.Single(parser.FileStreams);
        Assert.Equal("LAME3.92", parser.FileStreams[0].Metadata["encoder"]);
    }

    [Fact]
    public void ParseFileInfo_TaggedMultiStream_ParsesTagsLanguageAndDisposition()
    {
        var parser = (FileInfoFFmpeg)SetupParser();

        parser.ParseFileInfo(OutputSamples.FFmpegInfoTagged);

        Assert.Equal("matroska,webm", parser.FormatName);
        Assert.Equal("Sample Title", parser.Metadata["title"]);
        Assert.Equal("Sample Artist", parser.Metadata["artist"]);
        Assert.Equal("format-level comment", parser.Metadata["COMMENT"]);
        // Case-insensitive tag keys
        Assert.Equal("Sample Title", parser.Metadata["TITLE"]);

        // video + 2 audio + subtitle + audio after subtitle
        Assert.Equal(5, parser.FileStreams.Count);

        var video = Assert.IsType<MediaVideoStreamInfo>(parser.FileStreams[0]);
        Assert.Equal("eng", video.Language);
        Assert.True(video.Disposition.Has("default"));
        Assert.Equal("Main Video", video.Metadata["title"]);
        Assert.Equal("VideoHandler", video.Metadata["handler_name"]);

        var audio0 = Assert.IsType<MediaAudioStreamInfo>(parser.FileStreams[1]);
        Assert.Equal("eng", audio0.Language);
        Assert.True(audio0.Disposition.Has("default"));
        Assert.Equal("Original Audio", audio0.Metadata["title"]);
        Assert.Equal("440", audio0.Metadata["frequency"]);

        var audio1 = Assert.IsType<MediaAudioStreamInfo>(parser.FileStreams[2]);
        Assert.Equal("eng", audio1.Language);
        Assert.False(audio1.Disposition.Has("default"));
        Assert.Equal("432", audio1.Metadata["frequency"]);
        Assert.Equal("Pitched Audio", audio1.Metadata["title"]);

        Assert.IsType<MediaSubtitleStreamInfo>(parser.FileStreams[3]);

        var audio2 = Assert.IsType<MediaAudioStreamInfo>(parser.FileStreams[4]);
        Assert.Equal("528", audio2.Metadata["frequency"]);
    }

    [Fact]
    public void ParseFileInfo_Subtitle_ParsesCorrectly()
    {
        var parser = (FileInfoFFmpeg)SetupParser();

        parser.ParseFileInfo(OutputSamples.FFmpegInfoSubtitle);

        Assert.Equal(2, parser.FileStreams.Count);
        Assert.IsType<MediaVideoStreamInfo>(parser.FileStreams[0]);

        var sub = Assert.IsType<MediaSubtitleStreamInfo>(parser.FileStreams[1]);
        Assert.Equal(FFmpegStreamType.Subtitle, sub.StreamType);
        Assert.Equal("subrip", sub.Format);
        Assert.Equal("eng", sub.Language);
        Assert.Equal("English Captions", sub.Metadata["title"]);
        Assert.Same(sub, parser.SubtitleStream);
    }

    [Fact]
    public void ParseFileInfo_Data_ParsesCorrectly()
    {
        var parser = (FileInfoFFmpeg)SetupParser();

        parser.ParseFileInfo(OutputSamples.FFmpegInfoData);

        Assert.Equal(2, parser.FileStreams.Count);

        var data = Assert.IsType<MediaDataStreamInfo>(parser.FileStreams[0]);
        Assert.Equal(FFmpegStreamType.Data, data.StreamType);
        Assert.Equal("none", data.Format);
        Assert.Equal("und", data.Language);
        Assert.True(data.Disposition.Has("default"));
        Assert.Equal("odsm", data.Metadata["handler_name"]);

        Assert.IsType<MediaVideoStreamInfo>(parser.FileStreams[1]);
    }

    [Fact]
    public void ParseFileInfo_Attachment_ParsesCorrectly()
    {
        var parser = (FileInfoFFmpeg)SetupParser();

        parser.ParseFileInfo(OutputSamples.FFmpegInfoAttachment);

        Assert.Equal(2, parser.FileStreams.Count);
        Assert.IsType<MediaVideoStreamInfo>(parser.FileStreams[0]);

        var attach = Assert.IsType<MediaAttachmentStreamInfo>(parser.FileStreams[1]);
        Assert.Equal(FFmpegStreamType.Attachment, attach.StreamType);
        Assert.Equal("ttf", attach.Format);
        Assert.Equal("test.ttf", attach.Metadata["filename"]);
        Assert.Equal("application/x-truetype-font", attach.Metadata["mimetype"]);
        Assert.Equal("With Attachment", parser.Metadata["title"]);
    }

    [Theory]
    [InlineData("Stream #0:0(und): Video: h264, yuv420p, 100x100, 25 fps (default)", true, false, "und")]
    [InlineData("Stream #0:1(eng): Audio: aac, 48000 Hz, stereo, fltp, 128 kb/s (default)", true, false, "eng")]
    [InlineData("Stream #0:2: Audio: aac, 48000 Hz, stereo, fltp, 128 kb/s", false, false, null)]
    [InlineData("Stream #0:3(fra): Audio: aac, 48000 Hz, mono, fltp (forced)", false, true, "fra")]
    [InlineData("Stream #0:0: Audio: aac (LC), 44100 Hz, mono, fltp (default) (forced)", true, true, null)]
    [InlineData("Stream #0:0: Audio: aac (LC), 44100 Hz, mono, fltp (default, forced)", true, true, null)]
    public void ParseStreamInfo_DispositionAndLanguage_Parsed(string line, bool isDefault, bool isForced, string language)
    {
        var result = FileInfoFFmpeg.ParseStreamInfo(line);

        Assert.NotNull(result);
        Assert.Equal(isDefault, result.Disposition.Has("default"));
        Assert.Equal(isForced, result.Disposition.Has("forced"));
        Assert.Equal(language, result.Language);
    }

    [Fact]
    public void StreamDisposition_SetAndHas_ByName()
    {
        var d = new StreamDisposition();
        Assert.False(d.Any);

        d.Set("default");
        d.Set("forced");
        Assert.True(d.Has("DEFAULT"));
        Assert.True(d.Has("forced"));
        Assert.Equal("default+forced", d.ToString());

        d.Set("forced", enabled: false);
        Assert.False(d.Has("forced"));
        Assert.True(d.Has("default"));
    }

    [Theory]
    [InlineData("Input #0, mp3, from 'a.mp3':", "mp3")]
    [InlineData("Input #0, mov,mp4,m4a,3gp,3g2,mj2, from 'a.mp4':", "mov,mp4,m4a,3gp,3g2,mj2")]
    [InlineData("Input #0, matroska,webm, from 'a.mkv':", "matroska,webm")]
    [InlineData("not an input line", null)]
    public void ParseFormatName_Valid_ReturnsExpected(string line, string expected)
    {
        Assert.Equal(expected, FileInfoFFmpeg.ParseFormatName(line));
    }

    [Theory]
    [InlineData("    title           : Nu", true, "title", "Nu")]
    [InlineData("      frequency       : 432", true, "frequency", "432")]
    [InlineData("    handler_name    : SoundHandler", true, "handler_name", "SoundHandler")]
    [InlineData("Stream #0:0: Audio: mp3", false, "", "")]
    [InlineData("  Duration: 00:00:01.00", false, "", "")]
    public void TryParseMetadataEntry_Lines_AsExpected(string line, bool ok, string key, string value)
    {
        var result = FileInfoFFmpeg.TryParseMetadataEntry(line, out var k, out var v);

        Assert.Equal(ok, result);
        if (ok)
        {
            Assert.Equal(key, k);
            Assert.Equal(value, v);
        }
    }

    [Fact]
    public void ParseFileInfo_FrameCountSample_ParsesFormatAndStreamTags()
    {
        var parser = (FileInfoFFmpeg)SetupParser();

        parser.ParseFileInfo(OutputSamples.FFmpegInfoFrameCount);

        Assert.Contains("mov", parser.FormatName ?? "", StringComparison.Ordinal);
        Assert.Equal("dash", parser.Metadata["major_brand"]);
        Assert.Single(parser.FileStreams);
        Assert.True(parser.FileStreams[0].Disposition.Has("default"));
        Assert.Equal("und", parser.FileStreams[0].Language);
        Assert.Equal("VideoHandler", parser.FileStreams[0].Metadata["handler_name"]);
    }
}
