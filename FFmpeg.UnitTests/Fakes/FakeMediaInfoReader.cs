using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;

namespace HanumanInstitute.FFmpeg.UnitTests;

public class FakeMediaInfoReader : IMediaInfoReader
{
    private const string OutputText =
        @"Input #0, flv, from 'C:\GitHub\HanumanInstitute.FFmpeg\FFmpeg.IntegrationTests\bin\Debug\SampleFiles\zelda.flv':
  Duration: 00:00:29.68, start: 0.000000, bitrate: 162 kb/s
    Stream #0:0: Video: flv1, yuv420p, 160x120, 12 fps, 12 tbr, 1k tbn
    Stream #0:1: Audio: adpcm_swf, 22050 Hz, mono, s16, 88 kb/s";

    public FileInfoFFmpeg GetFileInfo(string source, ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null)
    {
        var result = new FileInfoFFmpeg();
        result.ParseFileInfo(OutputText, null);
        return result;
    }

    public long GetFrameCount(string source, ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null) => 1;

    public string GetVersion(ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null) => "Version number";

    public IMediaInfoReader SetOwner(object owner) => this;
}