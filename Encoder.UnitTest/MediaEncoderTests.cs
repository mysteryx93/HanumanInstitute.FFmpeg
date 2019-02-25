using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace EmergenceGuardian.Encoder.UnitTests {
    public class MediaEncoderTests {

        #region Declarations

        protected FakeProcessWorkerFactory factory = new FakeProcessWorkerFactory();
        private const string SourcePath = "source", DestPath = "dest";
        private readonly ITestOutputHelper output;

        public MediaEncoderTests(ITestOutputHelper output) {
            this.output = output;
        }

        #endregion

        #region Utility Functions

        protected IMediaEncoder SetupEncoder() {
            return new MediaEncoder(factory);
        }

        protected void AssertSingleInstance() {
            string ResultCommand = factory.Instances.FirstOrDefault()?.CommandWithArgs;
            output.WriteLine(ResultCommand);
            Assert.Single(factory.Instances);
            Assert.NotNull(ResultCommand);
        }

        #endregion

        #region Data Sources

        private const string VideoCodecTest = "video";
        private readonly string[] VideoCodecSimpleList = new string[] { "video" };

        public static IEnumerable<object[]> GenerateEncodeFFmpeg_Valid() {
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

        public static IEnumerable<object[]> GenerateEncodeFFmpeg_Empty() {
            yield return new object[] {
                null,
                null
            };
            yield return new object[] {
                new string[] { },
                null,
            };
            yield return new object[] {
                null,
                new string[] { }
            };
            yield return new object[] {
                new string[] { null, "" },
                new string[] { "", null }
            };
        }

        #endregion

        #region Constructors

        [Fact]
        public void Constructor_WithFactory_Success() => new MediaEncoder(factory);

        [Fact]
        public void Constructor_NullFactory_ThrowsException() => Assert.Throws<ArgumentNullException>(() => new MediaEncoder(null));

        #endregion

        #region ConvertToAvi

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ConvertToAvi_Valid_ReturnsSuccess(bool audio) {
            var Encoder = SetupEncoder();

            var Result = Encoder.ConvertToAviUtVideo(SourcePath, DestPath, audio);

            AssertSingleInstance();
            Assert.Equal(CompletionStatus.Success, Result);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        public void ConvertToAvi_EmptyArgs_ThrowsException(string source, string dest) {
            var Encoder = SetupEncoder();

            Assert.Throws<ArgumentException>(() => Encoder.ConvertToAviUtVideo(source, dest, false));
        }

        [Fact]
        public void ConvertToAvi_ParamOptions_ReturnsSame() {
            var Encoder = SetupEncoder();
            var Options = new ProcessOptionsEncoder();

            Encoder.ConvertToAviUtVideo(SourcePath, DestPath, false, Options);

            Assert.Same(Options, factory.Instances[0].Options);
        }

        [Fact]
        public void ConvertToAvi_ParamCallback_CallbackCalled() {
            var Encoder = SetupEncoder();
            int CallbackCalled = 0;

            Encoder.ConvertToAviUtVideo(SourcePath, DestPath, false, null, (s, e) => CallbackCalled++);

            Assert.Equal(1, CallbackCalled);
        }

        #endregion

        #region EncodeFFmpeg Single

        [Theory]
        [InlineData("h264", null, null)]
        [InlineData("h264", "", "")]
        [InlineData(null, "aac", "args")]
        [InlineData("", "aac", "args")]
        [InlineData("\t\n", "\"\"\"", "\0")]
        public void EncodeFFmpeg_Single_Valid_ReturnsSuccess(string videoCodec, string audioCodec, string arguments) {
            var Encoder = SetupEncoder();

            var Result = Encoder.EncodeFFmpeg(SourcePath, DestPath, videoCodec, audioCodec, arguments);

            AssertSingleInstance();
            Assert.Equal(CompletionStatus.Success, Result);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        public void EncodeFFmpeg_Single_EmptySourceDest_ThrowsException(string source, string dest) {
            var Encoder = SetupEncoder();

            Assert.Throws<ArgumentException>(() => Encoder.EncodeFFmpeg(source, dest, VideoCodecTest, null, null));
        }

        [Fact]
        public void EncodeFFmpeg_Single_ParamOptions_ReturnsSame() {
            var Encoder = SetupEncoder();
            var Options = new ProcessOptionsEncoder();

            Encoder.EncodeFFmpeg(SourcePath, DestPath, VideoCodecTest, null, null, Options);

            Assert.Same(Options, factory.Instances[0].Options);
        }

        [Fact]
        public void EncodeFFmpeg_Single_ParamCallback_CallbackCalled() {
            var Encoder = SetupEncoder();
            int CallbackCalled = 0;

            Encoder.EncodeFFmpeg(SourcePath, DestPath, VideoCodecTest, null, null, null, (s, e) => CallbackCalled++);

            Assert.Equal(1, CallbackCalled);
        }

        #endregion

        #region EncodeFFmpeg List

        //[Theory]
        //[MemberData(nameof(GenerateEncodeFFmpeg_Valid))]
        //public void EncodeFFmpeg_List_Valid_ReturnsSuccess(string[] videoCodec, string[] audioCodec, string arguments, bool hasVideo, bool hasAudio) {
        //    var Encoder = SetupEncoder();

        //    var Result = Encoder.EncodeFFmpeg(SourcePath, DestPath, videoCodec, audioCodec, arguments);

        //    AssertSingleInstance();
        //    Assert.Equal(CompletionStatus.Success, Result);
        //    factory.Instances[0].CommandWithArgs.ContainsOrNot("-vn", !hasVideo);
        //    factory.Instances[0].CommandWithArgs.ContainsOrNot("-an", !hasAudio);
        //}

        //[Theory]
        //[MemberData(nameof(GenerateEncodeFFmpeg_Empty))]
        //public void EncodeFFmpeg_List_Empty_ThrowsException(string[] videoCodec, string[] audioCodec) {
        //    var Encoder = SetupEncoder();

        //    Assert.Throws<ArgumentException>(() => Encoder.EncodeFFmpeg(SourcePath, DestPath, videoCodec, audioCodec, null));
        //}

        //[Theory]
        //[InlineData(null, null)]
        //[InlineData("", "")]
        //public void EncodeFFmpeg_List_EmptySourceDest_ThrowsException(string source, string dest) {
        //    var Encoder = SetupEncoder();

        //    Assert.Throws<ArgumentException>(() => Encoder.EncodeFFmpeg(source, dest, VideoCodecSimpleList, null, null));
        //}

        //[Fact]
        //public void EncodeFFmpeg_List_ParamOptions_ReturnsSame() {
        //    var Encoder = SetupEncoder();
        //    var Options = new ProcessOptionsEncoder();

        //    Encoder.EncodeFFmpeg(SourcePath, DestPath, VideoCodecSimpleList, null, null, Options);

        //    Assert.Same(Options, factory.Instances[0].Options);
        //}

        //[Fact]
        //public void EncodeFFmpeg_List_ParamCallback_CallbackCalled() {
        //    var Encoder = SetupEncoder();
        //    int CallbackCalled = 0;

        //    Encoder.EncodeFFmpeg(SourcePath, DestPath, VideoCodecSimpleList, null, null, null, (s, e) => CallbackCalled++);

        //    Assert.Equal(1, CallbackCalled);
        //}

        #endregion

        #region EncodeAvisynthToFFmpeg

        [Theory]
        [InlineData("h264", null, null)]
        [InlineData("h264", "", "")]
        [InlineData(null, "aac", "args")]
        [InlineData("", "aac", "args")]
        [InlineData("\t\n", "\"\"\"", "\0")]
        public void EncodeAvisynthToFFmpeg_Valid_ReturnsSuccess(string videoCodec, string audioCodec, string arguments) {
            var Encoder = SetupEncoder();

            var Result = Encoder.EncodeAvisynthToFFmpeg(SourcePath, DestPath, videoCodec, audioCodec, arguments);

            AssertSingleInstance();
            Assert.Equal(CompletionStatus.Success, Result);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        public void EncodeAvisynthToFFmpeg_EmptySourceDest_ThrowsException(string source, string dest) {
            var Encoder = SetupEncoder();

            Assert.Throws<ArgumentException>(() => Encoder.EncodeAvisynthToFFmpeg(source, dest, null, VideoCodecTest, null));
        }

        #endregion

        #region EncodeVapourSynthToFFmpeg

        [Theory]
        [InlineData("h264", null, null)]
        [InlineData("h264", "", "")]
        [InlineData(null, "aac", "args")]
        [InlineData("", "aac", "args")]
        [InlineData("\t\n", "\"\"\"", "\0")]
        public void EncodeVapourSynthToFFmpeg_Valid_ReturnsSuccess(string videoCodec, string audioCodec, string arguments) {
            var Encoder = SetupEncoder();

            var Result = Encoder.EncodeVapourSynthToFFmpeg(SourcePath, DestPath, videoCodec, audioCodec, arguments);

            AssertSingleInstance();
            Assert.Equal(CompletionStatus.Success, Result);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        public void EncodeVapourSynthToFFmpeg_EmptySourceDest_ThrowsException(string source, string dest) {
            var Encoder = SetupEncoder();

            Assert.Throws<ArgumentException>(() => Encoder.EncodeVapourSynthToFFmpeg(source, dest, null, VideoCodecTest, null));
        }

        #endregion

        #region EncodeX264

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("args")]
        [InlineData("\t\n\0")]
        public void EncodeX264_Valid_ReturnsSuccess(string arguments) {
            var Encoder = SetupEncoder();

            var Result = Encoder.EncodeX264(SourcePath, DestPath, arguments);

            AssertSingleInstance();
            Assert.Equal(CompletionStatus.Success, Result);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        public void EncodeX264_EmptySourceDest_ThrowsException(string source, string dest) {
            var Encoder = SetupEncoder();

            Assert.Throws<ArgumentException>(() => Encoder.EncodeX264(source, dest, null));
        }

        [Fact]
        public void EncodeX264_ParamOptions_ReturnsSame() {
            var Encoder = SetupEncoder();
            var Options = new ProcessOptionsEncoder();

            Encoder.EncodeX264(SourcePath, DestPath, null, Options);

            Assert.Same(Options, factory.Instances[0].Options);
        }

        [Fact]
        public void EncodeX264_ParamCallback_CallbackCalled() {
            var Encoder = SetupEncoder();
            int CallbackCalled = 0;

            Encoder.EncodeX264(SourcePath, DestPath, VideoCodecTest, null, (s, e) => CallbackCalled++);

            Assert.Equal(1, CallbackCalled);
        }

        #endregion

        #region EncodeAvisynthToX264

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("args")]
        [InlineData("\t\n\0")]
        public void EncodeAvisynthToX264_Valid_ReturnsSuccess(string arguments) {
            var Encoder = SetupEncoder();

            var Result = Encoder.EncodeAvisynthToX264(SourcePath, DestPath, arguments);

            AssertSingleInstance();
            Assert.Equal(CompletionStatus.Success, Result);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        public void EncodeAvisynthToX264_EmptySourceDest_ThrowsException(string source, string dest) {
            var Encoder = SetupEncoder();

            Assert.Throws<ArgumentException>(() => Encoder.EncodeAvisynthToX264(source, dest, null));
        }

        #endregion

        #region EncodeVapourSynthToX264

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("args")]
        [InlineData("\t\n\0")]
        public void EncodeVapourSynthToX264_Valid_ReturnsSuccess(string arguments) {
            var Encoder = SetupEncoder();

            var Result = Encoder.EncodeVapourSynthToX264(SourcePath, DestPath, arguments);

            AssertSingleInstance();
            Assert.Equal(CompletionStatus.Success, Result);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        public void EncodeVapourSynthToX264_EmptySourceDest_ThrowsException(string source, string dest) {
            var Encoder = SetupEncoder();

            Assert.Throws<ArgumentException>(() => Encoder.EncodeVapourSynthToX264(source, dest, null));
        }

        #endregion

        #region EncodeX265

        //[Theory]
        //[InlineData(null)]
        //[InlineData("")]
        //[InlineData("args")]
        //[InlineData("\t\n\0")]
        //public void EncodeX265_Valid_ReturnsSuccess(string arguments) {
        //    var Encoder = SetupEncoder();

        //    var Result = Encoder.EncodeX265(SourcePath, DestPath, arguments);

        //    AssertSingleInstance();
        //    Assert.Equal(CompletionStatus.Success, Result);
        //}

        //[Theory]
        //[InlineData(null, null)]
        //[InlineData("", "")]
        //public void EncodeX265_EmptySourceDest_ThrowsException(string source, string dest) {
        //    var Encoder = SetupEncoder();

        //    Assert.Throws<ArgumentException>(() => Encoder.EncodeX265(source, dest, null));
        //}

        //[Fact]
        //public void EncodeX265_ParamOptions_ReturnsSame() {
        //    var Encoder = SetupEncoder();
        //    var Options = new ProcessOptionsEncoder();

        //    Encoder.EncodeX265(SourcePath, DestPath, null, Options);

        //    Assert.Same(Options, factory.Instances[0].Options);
        //}

        //[Fact]
        //public void EncodeX265_ParamCallback_CallbackCalled() {
        //    var Encoder = SetupEncoder();
        //    int CallbackCalled = 0;

        //    Encoder.EncodeX265(SourcePath, DestPath, VideoCodecTest, null, (s, e) => CallbackCalled++);

        //    Assert.Equal(1, CallbackCalled);
        //}

        #endregion

        #region EncodeAvisynthToX265

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("args")]
        [InlineData("\t\n\0")]
        public void EncodeAvisynthToX265_Valid_ReturnsSuccess(string arguments) {
            var Encoder = SetupEncoder();

            var Result = Encoder.EncodeAvisynthToX265(SourcePath, DestPath, arguments);

            AssertSingleInstance();
            Assert.Equal(CompletionStatus.Success, Result);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        public void EncodeAvisynthToX265_EmptySourceDest_ThrowsException(string source, string dest) {
            var Encoder = SetupEncoder();

            Assert.Throws<ArgumentException>(() => Encoder.EncodeAvisynthToX265(source, dest, null));
        }

        #endregion

        #region EncodeVapourSynthToX265

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("args")]
        [InlineData("\t\n\0")]
        public void EncodeVapourSynthToX265_Valid_ReturnsSuccess(string arguments) {
            var Encoder = SetupEncoder();

            var Result = Encoder.EncodeVapourSynthToX265(SourcePath, DestPath, arguments);

            AssertSingleInstance();
            Assert.Equal(CompletionStatus.Success, Result);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        public void EncodeVapourSynthToX265_EmptySourceDest_ThrowsException(string source, string dest) {
            var Encoder = SetupEncoder();

            Assert.Throws<ArgumentException>(() => Encoder.EncodeVapourSynthToX265(source, dest, null));
        }

        #endregion

    }
}