// ReSharper disable StringLiteralTypo
namespace HanumanInstitute.FFmpeg.IntegrationTests;

/// <summary>
/// Probes which raw / elementary video formats actually need the temp-MP4 remux before
/// stream-copy into MKV (missing timestamps / invalid-argument mux errors).
/// Results drive <c>MediaMuxer</c>'s elementary-codec allowlist.
/// </summary>
public class ElementaryToMkvWorkaroundTests
{
    private readonly ITestOutputHelper _output;
    private readonly OutputFeeder _feed;
    private IEncoderService _factory = null!;

    public ElementaryToMkvWorkaroundTests(ITestOutputHelper output)
    {
        _output = output;
        _feed = new OutputFeeder(output);
    }

    /// <summary>
    /// Codecs that fail direct elementary→MKV stream-copy but succeed after remuxing into MP4 first.
    /// Must stay in sync with <c>MediaMuxer</c>'s allowlist.
    /// </summary>
    public static readonly string[] CodecsRequiringWorkaround = ["h264", "hevc", "mpeg1video", "mpeg2video"];

    /// <summary>
    /// Codecs whose elementary (or IVF) form stream-copies into MKV without a temp container.
    /// Must NOT be in the allowlist (and for vp8 the MP4 step is impossible).
    /// </summary>
    public static readonly string[] CodecsNotRequiringWorkaround = ["mpeg4", "vp8", "vp9", "av1"];

    // Encoder + raw muxer format for producing a true elementary/bitstream sample from y4m.
    private static readonly (string CodecName, string Encoder, string RawFormat, string Extension)[] s_probeSpecs =
    [
        ("h264", "libx264", "h264", ".264"),
        ("hevc", "libx265", "hevc", ".265"),
        ("mpeg1video", "mpeg1video", "mpeg1video", ".m1v"),
        ("mpeg2video", "mpeg2video", "mpeg2video", ".m2v"),
        ("mpeg4", "mpeg4", "m4v", ".m4v"),
        ("vp8", "libvpx", "ivf", ".ivf"),
        ("vp9", "libvpx-vp9", "ivf", ".ivf"),
        ("av1", "libaom-av1", "ivf", ".ivf")
    ];

    private MediaMuxer SetupMuxer()
    {
        _factory = FactoryConfig.CreateWithConfig();
        return new MediaMuxer(_factory, new FileSystemService(), new MediaInfoReader(_factory));
    }

    private CompletionStatus RunFfmpeg(string args)
    {
        var worker = _factory.CreateEncoder(null, null, _feed.RunCallback);
        return worker.RunEncoder(args, EncoderApp.FFmpeg);
    }

    private bool HasUsableMedia(string path)
    {
        if (!File.Exists(path) || new FileInfo(path).Length < 64)
        {
            return false;
        }
        try
        {
            var info = new MediaInfoReader(_factory).GetFileInfo(path);
            // Prefer real duration; fall back to "has a video stream and non-trivial size"
            // when the probe is sparse on short elementary remuxes.
            return info.FileDuration > TimeSpan.Zero
                   || (info.FileStreams.Any(s => s.StreamType == FFmpegStreamType.Video)
                       && new FileInfo(path).Length > 500);
        }
        catch
        {
            return false;
        }
    }

    private string CreateWorkDir()
    {
        var dir = Path.Combine(AppPaths.OutputDirectory, "ElementaryMkvProbe", Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(dir);
        return dir;
    }

    private string EncodeElementary(string workDir, string encoder, string rawFormat, string extension, int frames = 30)
    {
        var y4m = AppPaths.GetInputFile(AppPaths.StreamY4m);
        Assert.True(File.Exists(y4m), "Missing Y4M sample for encoding elementary streams.");

        var dest = Path.Combine(workDir, encoder.Replace('-', '_') + extension);
        // libaom-av1 is slow; keep frames low for that path.
        var frameCount = encoder.Contains("aom", StringComparison.OrdinalIgnoreCase) ? 12 : frames;
        var cpu = encoder.Contains("aom", StringComparison.OrdinalIgnoreCase) ? " -cpu-used 8" : "";
        var status = RunFfmpeg($@"-y -i ""{y4m}"" -frames:v {frameCount} -c:v {encoder}{cpu} -f {rawFormat} ""{dest}""");
        Assert.Equal(CompletionStatus.Success, status);
        Assert.True(File.Exists(dest) && new FileInfo(dest).Length > 0, $"Failed to encode elementary {encoder}");
        return dest;
    }

    /// <summary>
    /// For each probe codec: try elementary→MKV direct, then elementary→MP4→MKV.
    /// Asserts the known require / not-require sets match observed FFmpeg behavior.
    /// </summary>
    [Fact]
    public void Probe_ElementaryToMkv_WhichFormatsNeedTempMp4()
    {
        SetupMuxer();
        var workDir = CreateWorkDir();
        var rows = new List<(string Codec, bool DirectOk, bool ViaMp4Ok, bool Mp4Ok)>();

        foreach (var (codecName, encoder, rawFormat, extension) in s_probeSpecs)
        {
            var elementary = EncodeElementary(workDir, encoder, rawFormat, extension);

            var directMkv = Path.Combine(workDir, codecName + "_direct.mkv");
            File.Delete(directMkv);
            var directStatus = RunFfmpeg($@"-y -i ""{elementary}"" -c copy ""{directMkv}""");
            var directOk = directStatus == CompletionStatus.Success && HasUsableMedia(directMkv);

            var midMp4 = Path.Combine(workDir, codecName + "_mid.mp4");
            var viaMkv = Path.Combine(workDir, codecName + "_via.mkv");
            File.Delete(midMp4);
            File.Delete(viaMkv);
            var mp4Status = RunFfmpeg($@"-y -i ""{elementary}"" -c copy ""{midMp4}""");
            var mp4Ok = mp4Status == CompletionStatus.Success && File.Exists(midMp4) && new FileInfo(midMp4).Length > 0;
            var viaOk = false;
            if (mp4Ok)
            {
                var viaStatus = RunFfmpeg($@"-y -i ""{midMp4}"" -c copy ""{viaMkv}""");
                viaOk = viaStatus == CompletionStatus.Success && HasUsableMedia(viaMkv);
            }

            rows.Add((codecName, directOk, viaOk, mp4Ok));
            _output.WriteLine(
                $"{codecName,-12} direct→mkv={(directOk ? "OK" : "FAIL"),-4}  " +
                $"elem→mp4={(mp4Ok ? "OK" : "FAIL"),-4}  mp4→mkv={(viaOk ? "OK" : "FAIL"),-4}  " +
                $"needsWorkaround={!directOk && viaOk}");
        }

        _output.WriteLine("");
        _output.WriteLine("Needs temp-MP4 workaround: " +
                          string.Join(", ", rows.Where(r => !r.DirectOk && r.ViaMp4Ok).Select(r => r.Codec)));
        _output.WriteLine("Direct MKV works:          " +
                          string.Join(", ", rows.Where(r => r.DirectOk).Select(r => r.Codec)));
        _output.WriteLine("MP4 intermediate unusable: " +
                          string.Join(", ", rows.Where(r => !r.Mp4Ok).Select(r => r.Codec)));

        foreach (var codec in CodecsRequiringWorkaround)
        {
            var row = rows.Single(r => r.Codec == codec);
            Assert.False(row.DirectOk, $"{codec}: expected direct elementary→MKV to fail (timestamps / mux error)");
            Assert.True(row.ViaMp4Ok, $"{codec}: expected elementary→MP4→MKV to succeed (workaround path)");
        }

        foreach (var codec in CodecsNotRequiringWorkaround)
        {
            var row = rows.Single(r => r.Codec == codec);
            Assert.True(row.DirectOk, $"{codec}: expected direct elementary→MKV to succeed (no workaround needed)");
        }

        // VP8 must not go through MP4 — container rejects the codec.
        Assert.False(rows.Single(r => r.Codec == "vp8").Mp4Ok, "vp8 must not be remuxed into MP4");
    }

    /// <summary>
    /// MediaMuxer must produce a valid MKV for elementary codecs that need the workaround.
    /// </summary>
    [Theory]
    [InlineData("h264", "libx264", "h264", ".264")]
    [InlineData("hevc", "libx265", "hevc", ".265")]
    [InlineData("mpeg2video", "mpeg2video", "mpeg2video", ".m2v")]
    [InlineData("mpeg1video", "mpeg1video", "mpeg1video", ".m1v")]
    public void Muxe_ElementaryRequiringWorkaround_SucceedsToMkv(string codecName, string encoder, string rawFormat, string extension)
    {
        var muxer = SetupMuxer();
        var workDir = CreateWorkDir();
        var elementary = EncodeElementary(workDir, encoder, rawFormat, extension);
        var dest = Path.Combine(workDir, codecName + "_muxer.mkv");
        File.Delete(dest);

        var stream = new MediaStream(elementary, 0, codecName, FFmpegStreamType.Video);
        var status = muxer.Muxe([stream], dest, callback: _feed.RunCallback);

        Assert.Equal(CompletionStatus.Success, status);
        Assert.True(HasUsableMedia(dest), $"{codecName}: MediaMuxer MKV output missing usable media");
        var info = new MediaInfoReader(_factory).GetFileInfo(dest);
        Assert.Contains(info.FileStreams, s => s.StreamType == FFmpegStreamType.Video);
    }

    /// <summary>
    /// Codecs that work elementary→MKV directly must still succeed through MediaMuxer (no false dependency on MP4).
    /// </summary>
    [Theory]
    [InlineData("mpeg4", "mpeg4", "m4v", ".m4v")]
    [InlineData("vp8", "libvpx", "ivf", ".ivf")]
    [InlineData("vp9", "libvpx-vp9", "ivf", ".ivf")]
    [InlineData("av1", "libaom-av1", "ivf", ".ivf")]
    public void Muxe_ElementaryNotRequiringWorkaround_SucceedsToMkv(string codecName, string encoder, string rawFormat, string extension)
    {
        var muxer = SetupMuxer();
        var workDir = CreateWorkDir();
        var elementary = EncodeElementary(workDir, encoder, rawFormat, extension);
        var dest = Path.Combine(workDir, codecName + "_muxer.mkv");
        File.Delete(dest);

        var stream = new MediaStream(elementary, 0, codecName, FFmpegStreamType.Video);
        var status = muxer.Muxe([stream], dest, callback: _feed.RunCallback);

        Assert.Equal(CompletionStatus.Success, status);
        Assert.True(HasUsableMedia(dest), $"{codecName}: MediaMuxer MKV output missing usable media");
    }

    /// <summary>
    /// Existing YouTube-style sample (h264 already in a dash/mp4-like file) still muxes to MKV via MediaMuxer.
    /// </summary>
    [Fact]
    public void Muxe_SampleH264Stream_ToMkv_Succeeds()
    {
        var muxer = SetupMuxer();
        var src = AppPaths.GetInputFile(AppPaths.StreamH264);
        var dest = AppPaths.PrepareDestPath("ElemWorkaround", AppPaths.StreamH264, ".mkv");
        var stream = new MediaStream(src, 0, "h264", FFmpegStreamType.Video);

        var status = muxer.Muxe([stream], dest, callback: _feed.RunCallback);

        Assert.Equal(CompletionStatus.Success, status);
        Assert.True(File.Exists(dest));
    }
}
