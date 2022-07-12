using System;
using Xunit.Abstractions;

namespace HanumanInstitute.FFmpeg.IntegrationTests;

/// <summary>
/// Feeds the process output into XUnit's output.
/// </summary>
public class OutputFeeder
{
    private readonly ITestOutputHelper _output;

    public OutputFeeder(ITestOutputHelper output)
    {
        _output = output;
    }

    /// <summary>
    /// Pass this method as the callback delegate when running a process.
    /// </summary>
    public void RunCallback(object sender, ProcessStartedEventArgs e)
    {
        if (e == null) { throw new ArgumentNullException(nameof(e)); }

        _output.WriteLine(e.ProcessWorker.CommandWithArgs);
        _output.WriteLine(string.Empty);
        e.ProcessWorker.ProcessCompleted += (s2, e2) => _output.WriteLine(e.ProcessWorker.Output);
    }
}