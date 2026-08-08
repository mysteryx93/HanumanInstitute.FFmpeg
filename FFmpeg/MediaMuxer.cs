using System.Diagnostics.CodeAnalysis;
using System.IO;
using HanumanInstitute.FFmpeg.Services;
// ReSharper disable StringLiteralTypo

namespace HanumanInstitute.FFmpeg;

/// <inheritdoc />
public class MediaMuxer : IMediaMuxer
{
    // Raw elementary video → MKV needs a temp MP4 remux (missing timestamps).
    private static readonly HashSet<string> s_elementaryVideoCodecs = new(StringComparer.OrdinalIgnoreCase)
    {
        "h264", "h265", "hevc", "vvc", "h266"
    };

    private static readonly HashSet<string> s_elementaryVideoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".264", ".h264",
        ".265", ".h265", ".hevc",
        ".266", ".h266", ".vvc"
    };

    private readonly IEncoderService _factory;
    private readonly IFileSystemService _fileSystem;
    private readonly IMediaInfoReader _infoReader;

    /// <summary>
    /// Initializes a new instance of the MediaMuxer class.
    /// </summary>
    /// <param name="processFactory">Factory used to create encoder processes.</param>
    public MediaMuxer(IEncoderService processFactory) : this(processFactory, new FileSystemService(), new MediaInfoReader(processFactory)) { }

    // Test / DI constructor with explicit file system and media info reader.
    internal MediaMuxer(IEncoderService processFactory, IFileSystemService fileSystemService, IMediaInfoReader infoReader)
    {
        _factory = processFactory.CheckNotNull();
        _fileSystem = fileSystemService.CheckNotNull();
        _infoReader = infoReader.CheckNotNull();
    }

    /// <inheritdoc />
    public object? Owner { get; set; }

    /// <inheritdoc />
    public CompletionStatus Muxe(string? videoFile, string? audioFile, string destination, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null)
    {
        if (!audioFile.HasValue()) { videoFile.CheckNotNullOrEmpty(); }
        destination.CheckNotNullOrEmpty();

        var inputStreamList = new List<MediaStream>();
        if (videoFile.HasValue())
        {
            var inputStream = GetStreamInfo(videoFile, FFmpegStreamType.Video, options);
            if (inputStream != null)
            {
                inputStreamList.Add(inputStream);
            }
        }
        if (audioFile.HasValue())
        {
            var inputStream = GetStreamInfo(audioFile, FFmpegStreamType.Audio, options);
            if (inputStream != null)
            {
                inputStreamList.Add(inputStream);
            }
        }

        return inputStreamList.Count > 0
            ? Muxe(inputStreamList, destination, null, options, callback)
            : CompletionStatus.Failed;
    }

    /// <inheritdoc />
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public CompletionStatus Muxe(IEnumerable<MediaStream> fileStreams, string destination, MuxOptions? muxOptions = null, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null)
    {
        fileStreams.CheckNotNull();
        destination.CheckNotNullOrEmpty();
        muxOptions ??= new MuxOptions();

        var streams = fileStreams.ToList();
        ValidateStreams(streams, muxOptions);

        var tempFiles = new List<string>();
        // Do not delete a path that is also an input (would destroy the source before FFmpeg reads it).
        // Writing destination == input is the caller's problem; FFmpeg will reject it.
        if (!streams.Any(s => string.Equals(s.Path, destination, StringComparison.Ordinal)))
        {
            _fileSystem.Delete(destination);
        }

        var result = PrepareElementaryVideoForMkv(streams, destination, options, tempFiles);

        if (result == CompletionStatus.Success)
        {
            var args = new MuxCommandBuilder(_infoReader).Build(streams, destination, muxOptions, options);
            var worker = _factory.CreateEncoder(Owner, options, callback);
            result = worker.RunEncoder(args, EncoderApp.FFmpeg);
        }

        foreach (var item in tempFiles)
        {
            try { _fileSystem.Delete(item); } catch { /* best-effort cleanup */ }
        }
        return result;
    }

    // Remuxes elementary H.26x into temp MP4 when dest is MKV (timestamp issues).
    private CompletionStatus PrepareElementaryVideoForMkv(List<MediaStream> streams, string destination, ProcessOptionsEncoder? options, List<string> tempFiles)
    {
        var result = CompletionStatus.Success;
        for (var i = 0; i < streams.Count; i++)
        {
            var item = streams[i];
            if (!NeedsElementaryVideoToMkvWorkaround(item, destination))
            {
                continue;
            }

            var newFile = CreateTempPathWithExtension(".mp4");
            var tempStream = new MediaStream(item.Path, item.Index, item.Format, item.Type);
            result = Muxe([tempStream], newFile, null, options);
            tempFiles.Add(newFile);
            if (result != CompletionStatus.Success)
            {
                return result;
            }

            // Remuxed elementary → MP4: single stream at index 0; keep type/format and write settings.
            var replaced = new MediaStream(newFile, 0, item.Format, item.Type);
            replaced.CopyWriteSettingsFrom(item);
            streams[i] = replaced;
        }
        return result;
    }

    // Raw H.26x elementary into .mkv (not the same codec inside a container).
    private static bool NeedsElementaryVideoToMkvWorkaround(MediaStream item, string destination)
    {
        if (item.Type != FFmpegStreamType.Video) { return false; }
        if (!destination.EndsWithInvariant(".mkv")) { return false; }
        if (!s_elementaryVideoCodecs.Contains(item.Format ?? string.Empty)) { return false; }
        return s_elementaryVideoExtensions.Contains(Path.GetExtension(item.Path));
    }

    // Requires non-empty paths; stream list may be empty when From() supplies media maps.
    private static void ValidateStreams(IReadOnlyList<MediaStream> streams, MuxOptions muxOptions)
    {
        if (!muxOptions?.FromInputs?.Any(r => r.Any) == true)
        {
            streams.CheckNotNullOrEmpty();
        }
        foreach (var item in streams)
        {
            item.Path.CheckNotNullOrEmpty();
        }
        // Path+index selections may still have Type.None (filled from probe later).
    }

    // Temp path with the given extension (or extension of a path).
    private string CreateTempPathWithExtension(string pathOrExtension)
    {
        var ext = pathOrExtension.StartsWith(".", StringComparison.Ordinal)
            ? pathOrExtension
            : Path.GetExtension(pathOrExtension);
        if (string.IsNullOrEmpty(ext))
        {
            ext = ".tmp";
        }
        var temp = _fileSystem.GetTempFile();
        try
        {
            _fileSystem.Delete(temp);
        }
        catch { /* ignore */ }
        return Path.ChangeExtension(temp, ext) ?? temp + ext;
    }

    // First stream of the given type from a file, or null if none.
    private MediaStream? GetStreamInfo(string path, FFmpegStreamType streamType, ProcessOptionsEncoder? options)
    {
        var fileInfo = _infoReader.GetFileInfo(path, options);
        var streamInfo = fileInfo.FileStreams.FirstOrDefault(x => x.StreamType == streamType);
        return streamInfo != null ? MediaStream.FromStreamInfo(path, streamInfo) : null;
    }

    /// <inheritdoc />
    public CompletionStatus ExtractVideo(string source, string destination, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null) =>
        ExtractStream(@"-y -i ""{0}"" -vcodec copy -an ""{1}""", source, destination, options, callback);

    /// <inheritdoc />
    public CompletionStatus ExtractAudio(string source, string destination, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null) =>
        ExtractStream(@"-y -i ""{0}"" -vn -acodec copy ""{1}""", source, destination, options, callback);

    // Shared extract path: delete dest, run encoder with formatted -i args.
    private CompletionStatus ExtractStream(string args, string source, string destination, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null)
    {
        source.CheckNotNullOrEmpty();
        destination.CheckNotNullOrEmpty();

        _fileSystem.Delete(destination);
        var worker = _factory.CreateEncoder(Owner, options, callback);

        return worker.RunEncoder(args.FormatInvariant(source, destination), EncoderApp.FFmpeg);
    }

    /// <inheritdoc />
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public CompletionStatus Concatenate(IEnumerable<string> files, string destination, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null)
    {
        files.CheckNotNullOrEmpty();
        destination.CheckNotNullOrEmpty();

        var tempFile = _fileSystem.GetTempFile();
        var tempContent = new StringBuilder();
        foreach (var item in files)
        {
            tempContent.AppendFormatInvariant("file '{0}'", item).AppendLine();
        }
        _fileSystem.WriteAllText(tempFile, tempContent.ToString());

        var query = Invariant($@"-y -f concat -fflags +genpts -async 1 -safe 0 -i ""{tempFile}"" -c copy ""{destination}""");
        var worker = _factory.CreateEncoder(Owner, options, callback);
        var result = worker.RunEncoder(query.ToString(CultureInfo.InvariantCulture), EncoderApp.FFmpeg);

        _fileSystem.Delete(tempFile);
        return result;
    }

    /// <inheritdoc />
    public CompletionStatus Truncate(string source, string destination, TimeSpan? startPos, TimeSpan? duration = null, ProcessOptionsEncoder? options = null, ProcessStartedEventHandler? callback = null)
    {
        source.CheckNotNullOrEmpty();
        destination.CheckNotNullOrEmpty();

        _fileSystem.Delete(destination);
        var worker = _factory.CreateEncoder(Owner, options, callback);

        var args = """
                   -i "{0}" -vcodec copy -acodec copy {1}{2}"{3}"
                   """.FormatInvariant(source,
            startPos.HasValue && startPos > TimeSpan.Zero ? $"-ss {startPos:c} " : "",
            duration.HasValue && duration > TimeSpan.Zero ? $"-t {duration:c} " : "",
            destination);
        return worker.RunEncoder(args, EncoderApp.FFmpeg);
    }
}
