using System;
using Xunit;
using Xunit.Abstractions;

namespace EmergenceGuardian.Encoder.IntegrationTests {
    public class MediaScriptTests {
        private readonly ITestOutputHelper output;
        private readonly OutputFeeder feed;

        public MediaScriptTests(ITestOutputHelper output) {
            this.output = output;
            feed = new OutputFeeder(output);
        }

        private IMediaScript SetupScript() {
            IProcessWorkerFactory Factory = FactoryConfig.CreateWithConfig();
            return new MediaScript(Factory);
        }

        [Theory]
        [InlineData(AppPaths.Avisynth)]
        public void RunAvisynth_Valid_ResultSuccess(string source) {
            var Script = SetupScript();
            var Src = AppPaths.GetInputFile(source);

            var Result = Script.RunAvisynth(Src, null, feed.RunCallback);

            Assert.Equal(CompletionStatus.Success, Result);
        }

        [Theory]
        [InlineData(AppPaths.VapourSynth)]
        public void RunVapourSynth_Valid_ResultSuccess(string source) {
            var Script = SetupScript();
            var Src = AppPaths.GetInputFile(source);

            var Result = Script.RunVapourSynth(Src, null, feed.RunCallback);

            Assert.Equal(CompletionStatus.Success, Result);
        }

        
    }
}
