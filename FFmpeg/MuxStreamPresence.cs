namespace HanumanInstitute.FFmpeg;

// Tracks the stream types present in the output.
internal sealed class MuxStreamPresence
{
    public bool HasVideo { get; private set; }
    public bool HasAudio { get; private set; }
    public bool HasOther { get; private set; }

    // Builds stream presence information.
    public static MuxStreamPresence From(IReadOnlyList<MediaStream> streams)
    {
        return new MuxStreamPresence
        {
            HasVideo = streams.Any(s => s.Type == FFmpegStreamType.Video),
            HasAudio = streams.Any(s => s.Type == FFmpegStreamType.Audio),
            HasOther = streams.Any(s => s.Type is FFmpegStreamType.Subtitle or FFmpegStreamType.Data or FFmpegStreamType.Attachment)
        };
    }
}
