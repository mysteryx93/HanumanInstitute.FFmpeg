using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace HanumanInstitute.FFmpeg.UnitTests
{
    public class MediaInfoTests
    {
        private FakeProcessWorkerFactory _factory;
        private readonly ITestOutputHelper _output;
        private const string TestFile = "test";

        public MediaInfoTests(ITestOutputHelper output)
        {
            _output = output;
        }


        protected IMediaInfoReader SetupInfo()
        {
            _factory = new FakeProcessWorkerFactory();
            return new MediaInfoReader(_factory);
        }

        protected void AssertSingleInstance()
        {
            var resultCommand = _factory.Instances.FirstOrDefault()?.CommandWithArgs;
            _output.WriteLine(resultCommand);
            Assert.Single(_factory.Instances);
            Assert.NotNull(resultCommand);
        }


        [Fact]
        public void Constructor_WithFactory_Success() => new MediaInfoReader(new FakeProcessWorkerFactory());

        [Fact]
        public void Constructor_NullFactory_ThrowsException() => Assert.Throws<ArgumentNullException>(() => new MediaInfoReader(null));


        [Fact]
        public void GetVersion_Valid_ReturnsOutput()
        {
            var info = SetupInfo();

            var result = info.GetVersion();

            AssertSingleInstance();
            Assert.NotNull(result);
        }

        [Fact]
        public void GetVersion_ParamOptions_ReturnsSame()
        {
            var info = SetupInfo();
            var options = new ProcessOptionsEncoder();

            info.GetVersion(options);

            Assert.Same(options, _factory.Instances[0].Options);
        }

        [Fact]
        public void GetVersion_ParamCallback_CallbackCalled()
        {
            var info = SetupInfo();
            var callbackCalled = 0;

            info.GetVersion(null, (s, e) => callbackCalled++);

            Assert.Equal(1, callbackCalled);
        }


        [Theory]
        [InlineData("source")]
        public void GetFileInfo_Valid_ReturnsProcessManager(string source)
        {
            var info = SetupInfo();

            info.GetFileInfo(source);

            AssertSingleInstance();
        }

        [Theory]
        [MemberData(nameof(TestDataSource.NullAndEmptyStrings), 1, MemberType = typeof(TestDataSource))]
        public void GetFileInfo_EmptyArg_ThrowsException(string source, Type ex)
        {
            var info = SetupInfo();

            void Act() => info.GetFileInfo(source);

            Assert.Throws(ex, Act);
        }

        [Theory]
        [InlineData("source")]
        public void GetFileInfo_ParamOptions_ReturnsSame(string source)
        {
            var info = SetupInfo();
            var options = new ProcessOptionsEncoder();

            info.GetFileInfo(source, options);

            Assert.Same(options, _factory.Instances[0].Options);
        }

        [Theory]
        [InlineData("source")]
        public void GetFileInfo_ParamCallback_CallbackCalled(string source)
        {
            var info = SetupInfo();
            var callbackCalled = 0;

            info.GetFileInfo(source, null, (s, e) => callbackCalled++);

            Assert.Equal(1, callbackCalled);
        }


        [Theory]
        [InlineData(OutputSamples.FFmpegInfoFrameCount, 438)]
        public void GetFrameCount_Valid_ReturnsFrameCount(string output, int expected)
        {
            var info = SetupInfo();
            void Callback(object s, ProcessStartedEventArgs e) => FakeProcessWorkerFactory.FeedOutputToProcess(e.ProcessWorker, output);

            var result = info.GetFrameCount(TestFile, null, Callback);

            Assert.Equal(expected, result);
        }

        [Theory]
        [MemberData(nameof(TestDataSource.NullAndEmptyStrings), 1, MemberType = typeof(TestDataSource))]
        public void GetFrameCount_EmptyArg_ThrowsException(string source, Type ex)
        {
            var info = SetupInfo();

            void Act() => info.GetFrameCount(source);

            Assert.Throws(ex, Act);
        }

        [Theory]
        [InlineData("source")]
        public void GetFrameCount_ParamOptions_ReturnsSame(string source)
        {
            var info = SetupInfo();
            var options = new ProcessOptionsEncoder();

            info.GetFrameCount(source, options);

            Assert.Same(options, _factory.Instances[0].Options);
        }

        [Theory]
        [InlineData("source")]
        public void GetFrameCount_ParamCallback_CallbackCalled(string source)
        {
            var info = SetupInfo();
            var callbackCalled = 0;

            info.GetFrameCount(source, null, (s, e) => callbackCalled++);

            Assert.Equal(1, callbackCalled);
        }
    }
}
