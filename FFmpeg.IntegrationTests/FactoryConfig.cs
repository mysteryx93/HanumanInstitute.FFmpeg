using System;
using HanumanInstitute.FFmpeg.Services;

namespace HanumanInstitute.FFmpeg.IntegrationTests
{
    public static class FactoryConfig
    {
        public static IProcessWorkerFactory CreateWithConfig()
        {
            return new ProcessWorkerFactory(
                new MediaConfig(new WindowsApiService(), new FileSystemService())
                {
                    FFmpegPath = Properties.Settings.Default.FFmpegPath,
                    X264Path = Properties.Settings.Default.X264Path,
                    X265Path = Properties.Settings.Default.X265Path,
                    Avs2PipeMod = Properties.Settings.Default.Avs2PipeMod,
                    VsPipePath = Properties.Settings.Default.VsPipePath
                }, 
                null, new FileInfoParserFactory(), new ProcessFactory(), new FileSystemService());
        }
    }
}
