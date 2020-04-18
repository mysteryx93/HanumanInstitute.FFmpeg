using System;
using HanumanInstitute.FFmpeg.Services;
using Moq;
using Xunit;

namespace HanumanInstitute.FFmpeg.UnitTests
{
    public class ProcessManagerFactoryTests
    {
        private FakeMediaConfig _config;

        protected IProcessWorkerFactory SetupFactory()
        {
            var moq = new MockRepository(MockBehavior.Strict);
            _config = new FakeMediaConfig();
            var parserFactory = new FileInfoParserFactory();
            var processFactory = moq.Create<IProcessFactory>();
            var fileSystem = moq.Create<IFileSystemService>();
            return new ProcessWorkerFactory(_config, null, parserFactory, processFactory.Object, fileSystem.Object);
        }

        [Fact]
        public void Constructor_NoParam_Success() => new ProcessWorkerFactory().Create(null);

        [Fact]
        public void Constructor_Config_Success() => new ProcessWorkerFactory(new MediaConfig(), null).Create(null);

        [Fact]
        public void Constructor_InjectDependencies_Success() => new ProcessWorkerFactory(new MediaConfig(), null, new FileInfoParserFactory(), new ProcessFactory(), new FileSystemService()).Create(null);

        [Fact]
        public void Constructor_NullDependencies_ThrowsException() => Assert.Throws<ArgumentNullException>(() => new ProcessWorkerFactory(null, null, null, null, null));

        [Fact]
        public void Constructor_InjectOneDependency_ThrowsException() => Assert.Throws<ArgumentNullException>(() => new ProcessWorkerFactory(new MediaConfig(), null, null, null, null));

        [Fact]
        public void Create_NoParam_ReturnsProcessManager()
        {
            var factory = SetupFactory();

            var result = factory.Create(null);

            Assert.NotNull(result);
            Assert.IsType<ProcessWorker>(result);
            Assert.Equal(_config, result.Config);
        }

        [Fact]
        public void Create_ParamOptions_ReturnsSameOptions()
        {
            var factory = SetupFactory();
            var options = new ProcessOptions();

            var result = factory.Create(options);

            Assert.Equal(options, result.Options);
        }

        [Fact]
        public void CreateEncoder_NoParam_ReturnsProcessManager()
        {
            var factory = SetupFactory();

            var result = factory.CreateEncoder(null);

            Assert.NotNull(result);
            Assert.IsType<ProcessWorkerEncoder>(result);
            Assert.Equal(_config, result.Config);
        }

        [Fact]
        public void CreateEncoder_ParamOptions_ReturnsSameOptions()
        {
            var factory = SetupFactory();
            var options = new ProcessOptionsEncoder();

            var result = factory.CreateEncoder(options);

            Assert.Equal(options, result.Options);
        }
    }
}
