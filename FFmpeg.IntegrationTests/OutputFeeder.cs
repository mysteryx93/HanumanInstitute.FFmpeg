using System;
using Xunit.Abstractions;

namespace EmergenceGuardian.Encoder.IntegrationTests {
    /// <summary>
    /// Feeds the process output into XUnit's output.
    /// </summary>
    public class OutputFeeder {
        private ITestOutputHelper output;

        public OutputFeeder(ITestOutputHelper output) {
            this.output = output;
        }

        /// <summary>
        /// Pass this method as the callback delegate when running a process.
        /// </summary>
        public void RunCallback(object sender, ProcessStartedEventArgs e) {
            output.WriteLine(e.ProcessWorker.CommandWithArgs);
            output.WriteLine("");
            e.ProcessWorker.ProcessCompleted += (s2, e2) => output.WriteLine(e.ProcessWorker.Output);
        }
    }
}
