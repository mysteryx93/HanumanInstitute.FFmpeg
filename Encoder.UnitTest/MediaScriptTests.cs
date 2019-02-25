using System;
using System.IO;
using System.Linq;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace EmergenceGuardian.Encoder.UnitTests {
    public class MediaScriptTests {

        #region Utility

        protected const string AppAvs2PipeMod = "avs2pipemod.exe";
        protected const string AppVsPipe = "vspipe.exe";
        protected const string MissingFileName = "MissingFile";
        private Mock<FakeMediaConfig> config;
        private FakeProcessWorkerFactory factory = new FakeProcessWorkerFactory();
        private readonly ITestOutputHelper output;

        public MediaScriptTests(ITestOutputHelper output) {
            this.output = output;
        }

        protected IMediaScript SetupScript() {
            config = new Mock<FakeMediaConfig>() { CallBase = true };
            factory.Config = config.Object;
            var fileSystem = Mock.Of<FakeFileSystemService>(x =>
                x.Exists(It.IsAny<string>()) == true && x.Exists(MissingFileName) == false);
            return new MediaScript(factory, fileSystem);
        }

        protected void AssertSingleInstance() {
            string ResultCommand = factory.Instances.FirstOrDefault()?.CommandWithArgs;
            output.WriteLine(ResultCommand);
            Assert.Single(factory.Instances);
            Assert.NotNull(ResultCommand);
        }

        #endregion

        #region Constructors

        [Fact]
        public void Constructor_WithFactory_Success() => new MediaEncoder(factory);

        [Fact]
        public void Constructor_NullFactory_ThrowsException() => Assert.Throws<ArgumentNullException>((Func<object>)(() => new MediaEncoder((IProcessWorkerFactory)null)));

        [Fact]
        public void Constructor_NullDependency_ThrowsException() => Assert.Throws<ArgumentNullException>(() => new MediaScript(factory, null));

        #endregion

        #region RunAvisynth

        [Theory]
        [InlineData("file")]
        public void RunAvisynth_ValidFile_CommandContainsAvs2PipeMod(string path) {
            var Script = SetupScript();

            var Result = Script.RunAvisynth(path);

            Assert.Equal(CompletionStatus.Success, Result);
            AssertSingleInstance();
            Assert.Contains(AppAvs2PipeMod, factory.Instances[0].CommandWithArgs);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void RunAvisynth_NoFile_ThrowsException(string path) {
            var Script = SetupScript();

            Assert.Throws<ArgumentException>(() => Script.RunAvisynth(path));
        }

        [Theory]
        [InlineData("file")]
        public void RunAvisynth_AvsNotFound_ThrowsFileNotFoundException(string path) {
            var Script = SetupScript();
            config.Setup(x => x.Avs2PipeMod).Returns(MissingFileName);

            Assert.Throws<FileNotFoundException>(() => Script.RunAvisynth(path));
        }

        #endregion

        #region RunVapourSynth

        [Theory]
        [InlineData("file")]
        public void RunVapourSynth_ValidFile_CommandContainsVsPipe(string path) {
            var Script = SetupScript();

            var Result = Script.RunVapourSynth(path);

            Assert.Equal(CompletionStatus.Success, Result);
            AssertSingleInstance();
            Assert.Contains(AppVsPipe, factory.Instances[0].CommandWithArgs);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void RunVapourSynth_NoFile_ThrowsException(string path) {
            var Script = SetupScript();

            Assert.Throws<ArgumentException>(() => Script.RunVapourSynth(path));
        }

        [Theory]
        [InlineData("file")]
        public void RunVapourSynth_AvsNotFound_ThrowsFileNotFoundException(string path) {
            var Script = SetupScript();
            config.Setup(x => x.VsPipePath).Returns(MissingFileName);

            Assert.Throws<FileNotFoundException>(() => Script.RunVapourSynth(path));
        }

        #endregion

    }
}
