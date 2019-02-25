
namespace EmergenceGuardian.Encoder.IntegrationTests {
    public class FactoryConfig {
        public static IProcessWorkerFactory CreateWithConfig() {
            return new ProcessWorkerFactory(new MediaConfig() {
                FFmpegPath = Properties.Settings.Default.FFmpegPath,
                X264Path = Properties.Settings.Default.X264Path,
                X265Path = Properties.Settings.Default.X265Path,
                Avs2PipeMod = Properties.Settings.Default.Avs2PipeMod,
                VsPipePath = Properties.Settings.Default.VsPipePath
            });
        }
    }
}
