// ReSharper disable StringLiteralTypo

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace HanumanInstitute.FFmpeg.IntegrationTests;

public static class FactoryConfig
{
    public static IProcessService CreateWithConfig()
    {
        var builder = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.json", true, true);
        var config = builder.Build();
        var paths = config.GetSection("AppPaths").Get<FFmpeg.AppPaths>();

        return new ProcessService(
            new ProcessManager(Options.Create(paths), new WindowsApiService(), new FileSystemService()),
            null, new FileInfoParserFactory(), new ProcessFactory(), new FileSystemService());
    }
}
