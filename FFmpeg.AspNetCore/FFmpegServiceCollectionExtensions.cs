using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using HanumanInstitute.FFmpeg;
using HanumanInstitute.FFmpeg.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class FFmpegServiceCollectionExtensions
{
    /// <summary>
    /// Registers FFmpeg classes into the IoC container.
    /// </summary>
    /// <param name="services">The IoC services container.</param>
    public static IServiceCollection AddFFmpeg(this IServiceCollection services)
    {
        if (services == null) { throw new ArgumentNullException(nameof(services)); }

        // FFmpeg
        services.TryAddTransient<IFileInfoParserFactory, FileInfoParserFactory>();
        services.TryAddTransient<IProcessManager, ProcessManager>();
        services.TryAddTransient<IMediaEncoder, MediaEncoder>();
        services.TryAddTransient<IMediaInfoReader, MediaInfoReader>();
        services.TryAddTransient<IMediaMuxer, MediaMuxer>();
        services.TryAddTransient<IMediaScript, MediaScript>();
        services.TryAddTransient<IProcessService, ProcessService>();
        services.TryAddTransient<ITimeLeftCalculatorFactory, TimeLeftCalculatorFactory>();

        // Services
        services.TryAddTransient<IEnvironmentService, EnvironmentService>();
        services.TryAddTransient<IFileSystemService, FileSystemService>();
        services.TryAddTransient<IProcessFactory, ProcessFactory>();
        services.TryAddTransient<IWindowsApiService, WindowsApiService>();

        return services;
    }
}