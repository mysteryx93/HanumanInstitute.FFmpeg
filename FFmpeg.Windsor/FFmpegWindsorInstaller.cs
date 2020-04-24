using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using HanumanInstitute.FFmpeg.Services;

namespace HanumanInstitute.FFmpeg
{
    public class FFmpegWindsorInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            if (container == null) { throw new ArgumentNullException(nameof(container)); }

            // FFmpeg

            container.Register(
                Component.For<IFileInfoParserFactory>()
                .ImplementedBy<FileInfoParserFactory>()
                .LifeStyle.Transient);

            container.Register(
                Component.For<IMediaConfig>()
                .ImplementedBy<MediaConfig>()
                .LifeStyle.Transient);

            container.Register(
                Component.For<IMediaEncoder>()
                .ImplementedBy<MediaEncoder>()
                .LifeStyle.Transient);

            container.Register(
                Component.For<IMediaInfoReader>()
                .ImplementedBy<MediaInfoReader>()
                .LifeStyle.Transient);

            container.Register(
                Component.For<IMediaMuxer>()
                .ImplementedBy<MediaMuxer>()
                .LifeStyle.Transient);

            container.Register(
                Component.For<IMediaScript>()
                .ImplementedBy<MediaScript>()
                .LifeStyle.Transient);

            container.Register(
                Component.For<IProcessWorkerFactory>()
                .ImplementedBy<ProcessWorkerFactory>()
                .LifeStyle.Transient);

            container.Register(
                Component.For<ITimeLeftCalculatorFactory>()
                .ImplementedBy<TimeLeftCalculatorFactory>()
                .LifeStyle.Transient);

            // Services

            container.Register(
                Component.For<IEnvironmentService>()
                .ImplementedBy<EnvironmentService>()
                .LifeStyle.Singleton);

            container.Register(
                Component.For<IFileSystemService>()
                .ImplementedBy<FileSystemService>()
                .LifeStyle.Singleton);

            container.Register(
                Component.For<IProcessFactory>()
                .ImplementedBy<ProcessFactory>()
                .LifeStyle.Transient);

            container.Register(
                Component.For<IWindowsApiService>()
                .ImplementedBy<WindowsApiService>()
                .LifeStyle.Singleton);
        }
    }
}
