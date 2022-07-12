using System;
using HanumanInstitute.FFmpeg.Services;

namespace HanumanInstitute.FFmpeg.UnitTests;

public class FakeEnvironmentService : IEnvironmentService
{
    public DateTime CurrentTime { get; set; } = new DateTime(2019, 01, 01);

    public void AddSeconds(int seconds) => CurrentTime = CurrentTime.AddSeconds(seconds);

    public DateTime Now => CurrentTime;

    public DateTime UtcNow => CurrentTime.AddHours(6);

    public string NewLine => Environment.NewLine;
}