using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace HanumanInstitute.FFmpeg.UnitTests;

public class MediaEncoderTests
{
    private readonly FakeProcessService _factory = new FakeProcessService();
    private const string SourcePath = "source", DestPath = "dest";
    private readonly ITestOutputHelper _output;

    public MediaEncoderTests(ITestOutputHelper output)
    {
        _output = output;
    }


    protected IMediaEncoder SetupEncoder()
    {
        return new MediaEncoder(_factory);
    }

    protected void AssertSingleInstance()
    {
        var resultCommand = _factory.Instances.FirstOrDefault()?.CommandWithArgs;
        _output.WriteLine(resultCommand);
        Assert.Single(_factory.Instances);
        Assert.NotNull(resultCommand);
    }


    private const string VideoCodecTest = "video";

    public static IEnumerable<object[]> GenerateEncodeFFmpeg_Valid()
    {
        yield return new object[] {
            new string[] { "a", "b", "c" },
            new string[] { "d", "e", "f" },
            "args", true, true
        };
        yield return new object[] {
            new string[] { "a", "b", "c" },
            null,
            null, true, false
        };
        yield return new object[] {
            null,
            new string[] { "d", "e", "f" },
            "args", false, true
        };
        yield return new object[] {
            new string[] { null, "", "c" },
            new string[] { "", "", null },
            "", true, false
        };
        yield return new object[] {
            new string[] { null },
            new string[] { "a" },
            "", false, true
        };
    }

    public static IEnumerable<object[]> GenerateEncodeFFmpeg_Empty()
    {
        yield return new object[] {
            null,
            null
        };
        yield return new object[] {
            Array.Empty<string>(),
            null,
        };
        yield return new object[] {
            null,
            Array.Empty<string>()
        };
        yield return new object[] {
            new string[] { null, "" },
            new string[] { "", null }
        };
    }


    [Fact]
    public void Constructor_WithFactory_Success() => new MediaEncoder(_factory);

    [Fact]
    public void Constructor_NullFactory_ThrowsException() => Assert.Throws<ArgumentNullException>(() => new MediaEncoder(null));


    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ConvertToAvi_Valid_ReturnsSuccess(bool audio)
    {
        var encoder = SetupEncoder();

        var result = encoder.ConvertToAviUtVideo(SourcePath, DestPath, audio);

        AssertSingleInstance();
        Assert.Equal(CompletionStatus.Success, result);
    }

    [Theory]
    [MemberData(nameof(TestDataSource.NullAndEmptyStrings), 2, MemberType = typeof(TestDataSource))]
    public void ConvertToAvi_EmptyArgs_ThrowsException(string source, string dest, Type ex)
    {
        var encoder = SetupEncoder();

        void Act() => encoder.ConvertToAviUtVideo(source, dest, false);

        Assert.Throws(ex, Act);
    }

    [Fact]
    public void ConvertToAvi_ParamOptions_ReturnsSame()
    {
        var encoder = SetupEncoder();
        var options = new ProcessOptionsEncoder();

        encoder.ConvertToAviUtVideo(SourcePath, DestPath, false, options);

        Assert.Same(options, _factory.Instances[0].Options);
    }

    [Fact]
    public void ConvertToAvi_ParamCallback_CallbackCalled()
    {
        var encoder = SetupEncoder();
        var callbackCalled = 0;

        encoder.ConvertToAviUtVideo(SourcePath, DestPath, false, null, (s, e) => callbackCalled++);

        Assert.Equal(1, callbackCalled);
    }


    [Theory]
    [InlineData("h264", null, null)]
    [InlineData("h264", "", "")]
    [InlineData(null, "aac", "args")]
    [InlineData("", "aac", "args")]
    [InlineData("\t\n", "\"\"\"", "\0")]
    public void EncodeFFmpeg_Single_Valid_ReturnsSuccess(string videoCodec, string audioCodec, string arguments)
    {
        var encoder = SetupEncoder();

        var result = encoder.EncodeFFmpeg(SourcePath, DestPath, videoCodec, audioCodec, arguments);

        AssertSingleInstance();
        Assert.Equal(CompletionStatus.Success, result);
    }

    [Theory]
    [MemberData(nameof(TestDataSource.NullAndEmptyStrings), 2, MemberType = typeof(TestDataSource))]
    public void EncodeFFmpeg_Single_NullSourceDest_ThrowsException(string source, string dest, Type ex)
    {
        var encoder = SetupEncoder();

        void Act() => encoder.EncodeFFmpeg(source, dest, VideoCodecTest, null, null);

        Assert.Throws(ex, Act);
    }

    [Fact]
    public void EncodeFFmpeg_Single_ParamOptions_ReturnsSame()
    {
        var encoder = SetupEncoder();
        var options = new ProcessOptionsEncoder();

        encoder.EncodeFFmpeg(SourcePath, DestPath, VideoCodecTest, null, null, options);

        Assert.Same(options, _factory.Instances[0].Options);
    }

    [Fact]
    public void EncodeFFmpeg_Single_ParamCallback_CallbackCalled()
    {
        var encoder = SetupEncoder();
        var callbackCalled = 0;

        encoder.EncodeFFmpeg(SourcePath, DestPath, VideoCodecTest, null, null, null, (s, e) => callbackCalled++);

        Assert.Equal(1, callbackCalled);
    }


    //[Theory]
    //[MemberData(nameof(GenerateEncodeFFmpeg_Valid))]
    //public void EncodeFFmpeg_List_Valid_ReturnsSuccess(string[] videoCodec, string[] audioCodec, string arguments, bool hasVideo, bool hasAudio) {
    //    var encoder = SetupEncoder();

    //    var result = encoder.EncodeFFmpeg(SourcePath, DestPath, videoCodec, audioCodec, arguments);

    //    AssertSingleInstance();
    //    Assert.Equal(CompletionStatus.Success, result);
    //    factory.Instances[0].CommandWithArgs.ContainsOrNot("-vn", !hasVideo);
    //    factory.Instances[0].CommandWithArgs.ContainsOrNot("-an", !hasAudio);
    //}

    //[Theory]
    //[MemberData(nameof(GenerateEncodeFFmpeg_Empty))]
    //public void EncodeFFmpeg_List_Empty_ThrowsException(string[] videoCodec, string[] audioCodec) {
    //    var encoder = SetupEncoder();

    //    Assert.Throws<ArgumentException>(() => encoder.EncodeFFmpeg(SourcePath, DestPath, videoCodec, audioCodec, null));
    //}

    //[Theory]
    //[InlineData(null, null)]
    //[InlineData("", "")]
    //public void EncodeFFmpeg_List_EmptySourceDest_ThrowsException(string source, string dest) {
    //    var encoder = SetupEncoder();

    //    Assert.Throws<ArgumentException>(() => encoder.EncodeFFmpeg(source, dest, VideoCodecSimpleList, null, null));
    //}

    //[Fact]
    //public void EncodeFFmpeg_List_ParamOptions_ReturnsSame() {
    //    var encoder = SetupEncoder();
    //    var options = new ProcessOptionsEncoder();

    //    encoder.EncodeFFmpeg(SourcePath, DestPath, VideoCodecSimpleList, null, null, options);

    //    Assert.Same(options, factory.Instances[0].Options);
    //}

    //[Fact]
    //public void EncodeFFmpeg_List_ParamCallback_CallbackCalled() {
    //    var encoder = SetupEncoder();
    //    int callbackCalled = 0;

    //    encoder.EncodeFFmpeg(SourcePath, DestPath, VideoCodecSimpleList, null, null, null, (s, e) => callbackCalled++);

    //    Assert.Equal(1, callbackCalled);
    //}


    [Theory]
    [InlineData("h264", null, null)]
    [InlineData("h264", "", "")]
    [InlineData(null, "aac", "args")]
    [InlineData("", "aac", "args")]
    [InlineData("\t\n", "\"\"\"", "\0")]
    public void EncodeAvisynthToFFmpeg_Valid_ReturnsSuccess(string videoCodec, string audioCodec, string arguments)
    {
        var encoder = SetupEncoder();

        var result = encoder.EncodeAvisynthToFFmpeg(SourcePath, DestPath, videoCodec, audioCodec, arguments);

        AssertSingleInstance();
        Assert.Equal(CompletionStatus.Success, result);
    }

    [Theory]
    [MemberData(nameof(TestDataSource.NullAndEmptyStrings), 2, MemberType = typeof(TestDataSource))]
    public void EncodeAvisynthToFFmpeg_NullSourceDest_ThrowsException(string source, string dest, Type ex)
    {
        var encoder = SetupEncoder();

        void Act() => encoder.EncodeAvisynthToFFmpeg(source, dest, null, VideoCodecTest, null);

        Assert.Throws(ex, Act);
    }

    [Theory]
    [InlineData("h264", null, null)]
    [InlineData("h264", "", "")]
    [InlineData(null, "aac", "args")]
    [InlineData("", "aac", "args")]
    [InlineData("\t\n", "\"\"\"", "\0")]
    public void EncodeVapourSynthToFFmpeg_Valid_ReturnsSuccess(string videoCodec, string audioCodec, string arguments)
    {
        var encoder = SetupEncoder();

        var result = encoder.EncodeVapourSynthToFFmpeg(SourcePath, DestPath, videoCodec, audioCodec, arguments);

        AssertSingleInstance();
        Assert.Equal(CompletionStatus.Success, result);
    }

    [Theory]
    [MemberData(nameof(TestDataSource.NullAndEmptyStrings), 2, MemberType = typeof(TestDataSource))]
    public void EncodeVapourSynthToFFmpeg_NulllSourceDest_ThrowsException(string source, string dest, Type ex)
    {
        var encoder = SetupEncoder();

        void Act() => encoder.EncodeVapourSynthToFFmpeg(source, dest, null, VideoCodecTest, null);

        Assert.Throws(ex, Act);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("args")]
    [InlineData("\t\n\0")]
    public void EncodeX264_Valid_ReturnsSuccess(string arguments)
    {
        var encoder = SetupEncoder();

        var result = encoder.EncodeX264(SourcePath, DestPath, arguments);

        AssertSingleInstance();
        Assert.Equal(CompletionStatus.Success, result);
    }

    [Theory]
    [MemberData(nameof(TestDataSource.NullAndEmptyStrings), 2, MemberType = typeof(TestDataSource))]
    public void EncodeX264_EmptySourceDest_ThrowsException(string source, string dest, Type ex)
    {
        var encoder = SetupEncoder();

        void Act() => encoder.EncodeX264(source, dest, null);

        Assert.Throws(ex, Act);
    }

    [Fact]
    public void EncodeX264_ParamOptions_ReturnsSame()
    {
        var encoder = SetupEncoder();
        var options = new ProcessOptionsEncoder();

        encoder.EncodeX264(SourcePath, DestPath, null, options);

        Assert.Same(options, _factory.Instances[0].Options);
    }

    [Fact]
    public void EncodeX264_ParamCallback_CallbackCalled()
    {
        var encoder = SetupEncoder();
        var callbackCalled = 0;

        encoder.EncodeX264(SourcePath, DestPath, VideoCodecTest, null, (s, e) => callbackCalled++);

        Assert.Equal(1, callbackCalled);
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("args")]
    [InlineData("\t\n\0")]
    public void EncodeAvisynthToX264_Valid_ReturnsSuccess(string arguments)
    {
        var encoder = SetupEncoder();

        var result = encoder.EncodeAvisynthToX264(SourcePath, DestPath, arguments);

        AssertSingleInstance();
        Assert.Equal(CompletionStatus.Success, result);
    }

    [Theory]
    [MemberData(nameof(TestDataSource.NullAndEmptyStrings), 2, MemberType = typeof(TestDataSource))]
    public void EncodeAvisynthToX264_EmptySourceDest_ThrowsException(string source, string dest, Type ex)
    {
        var encoder = SetupEncoder();

        void Act() => encoder.EncodeAvisynthToX264(source, dest, null);

        Assert.Throws(ex, Act);
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("args")]
    [InlineData("\t\n\0")]
    public void EncodeVapourSynthToX264_Valid_ReturnsSuccess(string arguments)
    {
        var encoder = SetupEncoder();

        var result = encoder.EncodeVapourSynthToX264(SourcePath, DestPath, arguments);

        AssertSingleInstance();
        Assert.Equal(CompletionStatus.Success, result);
    }

    [Theory]
    [MemberData(nameof(TestDataSource.NullAndEmptyStrings), 2, MemberType = typeof(TestDataSource))]
    public void EncodeVapourSynthToX264_EmptySourceDest_ThrowsException(string source, string dest, Type ex)
    {
        var encoder = SetupEncoder();

        void Act() => encoder.EncodeVapourSynthToX264(source, dest, null);

        Assert.Throws(ex, Act);
    }


    //[Theory]
    //[InlineData(null)]
    //[InlineData("")]
    //[InlineData("args")]
    //[InlineData("\t\n\0")]
    //public void EncodeX265_Valid_ReturnsSuccess(string arguments) {
    //    var encoder = SetupEncoder();

    //    var result = encoder.EncodeX265(SourcePath, DestPath, arguments);

    //    AssertSingleInstance();
    //    Assert.Equal(CompletionStatus.Success, result);
    //}

    //[Theory]
    //[InlineData(null, null)]
    //[InlineData("", "")]
    //public void EncodeX265_EmptySourceDest_ThrowsException(string source, string dest) {
    //    var encoder = SetupEncoder();

    //    Assert.Throws<ArgumentException>(() => encoder.EncodeX265(source, dest, null));
    //}

    //[Fact]
    //public void EncodeX265_ParamOptions_ReturnsSame() {
    //    var encoder = SetupEncoder();
    //    var options = new ProcessOptionsEncoder();

    //    encoder.EncodeX265(SourcePath, DestPath, null, options);

    //    Assert.Same(options, factory.Instances[0].Options);
    //}

    //[Fact]
    //public void EncodeX265_ParamCallback_CallbackCalled() {
    //    var encoder = SetupEncoder();
    //    int callbackCalled = 0;

    //    encoder.EncodeX265(SourcePath, DestPath, VideoCodecTest, null, (s, e) => callbackCalled++);

    //    Assert.Equal(1, callbackCalled);
    //}


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("args")]
    [InlineData("\t\n\0")]
    public void EncodeAvisynthToX265_Valid_ReturnsSuccess(string arguments)
    {
        var encoder = SetupEncoder();

        var result = encoder.EncodeAvisynthToX265(SourcePath, DestPath, arguments);

        AssertSingleInstance();
        Assert.Equal(CompletionStatus.Success, result);
    }

    [Theory]
    [MemberData(nameof(TestDataSource.NullAndEmptyStrings), 2, MemberType = typeof(TestDataSource))]
    public void EncodeAvisynthToX265_EmptySourceDest_ThrowsException(string source, string dest, Type ex)
    {
        var encoder = SetupEncoder();

        void Act() => encoder.EncodeAvisynthToX265(source, dest, null);

        Assert.Throws(ex, Act);
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("args")]
    [InlineData("\t\n\0")]
    public void EncodeVapourSynthToX265_Valid_ReturnsSuccess(string arguments)
    {
        var encoder = SetupEncoder();

        var result = encoder.EncodeVapourSynthToX265(SourcePath, DestPath, arguments);

        AssertSingleInstance();
        Assert.Equal(CompletionStatus.Success, result);
    }

    [Theory]
    [MemberData(nameof(TestDataSource.NullAndEmptyStrings), 2, MemberType = typeof(TestDataSource))]
    public void EncodeVapourSynthToX265_EmptySourceDest_ThrowsException(string source, string dest, Type ex)
    {
        var encoder = SetupEncoder();

        void Act() => encoder.EncodeVapourSynthToX265(source, dest, null);

        Assert.Throws(ex, Act);
    }
}