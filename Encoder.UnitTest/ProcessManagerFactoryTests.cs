using System;
using Moq;
using Xunit;
using EmergenceGuardian.Encoder.Services;

namespace EmergenceGuardian.Encoder.UnitTests {
    public class ProcessManagerFactoryTests {
        protected FakeMediaConfig config;

        protected IProcessWorkerFactory SetupFactory() {
            var moq = new MockRepository(MockBehavior.Strict);
            config = new FakeMediaConfig();
            var ParserFactory = new FileInfoParserFactory();
            var ProcessFactory = moq.Create<IProcessFactory>();
            var FileSystem = moq.Create<IFileSystemService>();
            return new ProcessWorkerFactory(config, ParserFactory, ProcessFactory.Object, FileSystem.Object);
        }

        [Fact]
        public void Constructor_NoParam_Success() => new ProcessWorkerFactory().Create();

        [Fact]
        public void Constructor_Config_Success() => new ProcessWorkerFactory(new MediaConfig()).Create();

        [Fact]
        public void Constructor_InjectDependencies_Success() => new ProcessWorkerFactory(new MediaConfig(), new FileInfoParserFactory(), new ProcessFactory(), new FileSystemService()).Create();

        [Fact]
        public void Constructor_NullDependencies_ThrowsException() => Assert.Throws<ArgumentNullException>(() => new ProcessWorkerFactory(null, null, null, null));

        [Fact]
        public void Constructor_InjectOneDependency_ThrowsException() => Assert.Throws<ArgumentNullException>(() => new ProcessWorkerFactory(new MediaConfig(), null, null, null));

        [Fact]
        public void Create_NoParam_ReturnsProcessManager() {
            var Factory = SetupFactory();

            var Result = Factory.Create();

            Assert.NotNull(Result);
            Assert.IsType<ProcessWorker>(Result);
            Assert.Equal(config, Result.Config);
        }

        [Fact]
        public void Create_ParamOptions_ReturnsSameOptions() {
            var Factory = SetupFactory();
            ProcessOptions options = new ProcessOptions();

            var Result = Factory.Create(options);

            Assert.Equal(options, Result.Options);
        }

        [Fact]
        public void CreateEncoder_NoParam_ReturnsProcessManager() {
            var Factory = SetupFactory();

            var Result = Factory.CreateEncoder();

            Assert.NotNull(Result);
            Assert.IsType<ProcessWorkerEncoder>(Result);
            Assert.Equal(config, Result.Config);
        }

        [Fact]
        public void CreateEncoder_ParamOptions_ReturnsSameOptions() {
            var Factory = SetupFactory();
            ProcessOptionsEncoder options = new ProcessOptionsEncoder();

            var Result = Factory.CreateEncoder(options);

            Assert.Equal(options, Result.Options);
        }
    }
}
