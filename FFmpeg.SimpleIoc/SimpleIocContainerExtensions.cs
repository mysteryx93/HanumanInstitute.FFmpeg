using System;
using GalaSoft.MvvmLight.Ioc;
using HanumanInstitute.FFmpeg;
using HanumanInstitute.FFmpeg.Services;

// ReSharper disable once CheckNamespace - MS guidelines say put DI registration in this NS
namespace HanumanInstitute.FFmpeg;

public static class SimpleIocContainerExtensions
{
    /// <summary>
    /// Registers FFmpeg classes into SimpleIoc container.
    /// </summary>
    /// <param name="services">The IoC services container.</param>
    public static SimpleIoc AddFFmpeg(this SimpleIoc services)
    {
        if (services == null) { throw new ArgumentNullException(nameof(services)); }

        // FFmpeg
        services.Register<IFileInfoParserFactory, FileInfoParserFactory>();
        services.Register<IProcessManager>(() => new ProcessManager(services.GetInstance<IWindowsApiService>(), services.GetInstance<IFileSystemService>()));
        services.Register<IMediaEncoder, MediaEncoder>();
        services.Register<IMediaInfoReader, MediaInfoReader>();
        services.Register<IMediaMuxer, MediaMuxer>();
        services.Register<IMediaScript, MediaScript>();
        services.Register<IProcessService>(() => new ProcessService(services.GetInstance<IProcessManager>(), services.GetInstance<IUserInterfaceManager>()));
        services.Register<ITimeLeftCalculatorFactory, TimeLeftCalculatorFactory>();

        // Services
        services.Register<IEnvironmentService, EnvironmentService>();
        services.Register<IFileSystemService, FileSystemService>();
        services.Register<IProcessFactory, ProcessFactory>();
        services.Register<IWindowsApiService, WindowsApiService>();

        return services;
    }
}