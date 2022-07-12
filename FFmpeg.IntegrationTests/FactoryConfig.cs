
// ReSharper disable StringLiteralTypo

namespace HanumanInstitute.FFmpeg.IntegrationTests;

public static class FactoryConfig
{
    public static IProcessService CreateWithConfig()
    {
        return new ProcessService(
            new ProcessManager(new WindowsApiService(), new FileSystemService())
            {
                FFmpegPath = "ffmpeg", // Properties.Settings.Default.FFmpegPath,
                X264Path = "x264", // Properties.Settings.Default.X264Path,
                X265Path = "x265", // Properties.Settings.Default.X265Path,
                Avs2PipeMod = "avs2pipemod", // Properties.Settings.Default.Avs2PipeMod,
                VsPipePath = "vspipe" // Properties.Settings.Default.VsPipePath
            }, 
            null, new FileInfoParserFactory(), new ProcessFactory(), new FileSystemService());
    }
}
