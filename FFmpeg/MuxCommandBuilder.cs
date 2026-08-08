namespace HanumanInstitute.FFmpeg;

// Builds the FFmpeg argument string for a stream-copy mux job.
internal sealed class MuxCommandBuilder
{
    private readonly IMediaInfoReader _infoReader;

    public MuxCommandBuilder(IMediaInfoReader infoReader)
    {
        _infoReader = infoReader.CheckNotNull();
    }

    // Full command:
    // -y -i … codecs -map … per-stream args -map_metadata … "dest"
    public string Build(IReadOnlyList<MediaStream> streams, string destination, MuxOptions muxOptions, ProcessOptionsEncoder? options)
    {
        ResolveTypesFromProbe(streams, options);

        var inputs = InputTable.From(streams, muxOptions);
        var presence = StreamPresence.FromListed(streams);

        var query = new StringBuilder("-y ");
        AppendInputs(query, inputs);

        var map = new StringBuilder();
        var perStream = new StringBuilder();
        var mappedKeys = new HashSet<string>(StringComparer.Ordinal);

        AppendListedStreams(map, perStream, streams, inputs, presence, mappedKeys);
        AppendFromRules(map, muxOptions, inputs, presence, mappedKeys, options);
        AppendGlobalCodecs(query, streams, presence);

        query.Append(map);
        query.Append(perStream);

        AppendContainerMaps(query, muxOptions, inputs);
        AppendOutputMetadata(query, muxOptions);
        AppendAdditionalArguments(query, muxOptions);

        query.Append('"');
        query.Append(destination);
        query.Append('"');

        return query.ToString();
    }

    // Fills type/format on streams still unresolved (Type.None).
    private void ResolveTypesFromProbe(IReadOnlyList<MediaStream> streams, ProcessOptionsEncoder? options)
    {
        foreach (var group in streams.GroupBy(s => s.Path, StringComparer.Ordinal))
        {
            if (group.All(s => s.Type != FFmpegStreamType.None))
            {
                continue;
            }

            FileInfoFFmpeg info;

            try
            {
                info = _infoReader.GetFileInfo(group.Key, options);
            }
            catch
            {
                continue;
            }

            foreach (var stream in group)
            {
                if (stream.Type != FFmpegStreamType.None)
                {
                    continue;
                }

                var source = info.FileStreams.FirstOrDefault(s => s.Index == stream.Index);

                if (source != null)
                {
                    stream.SetFileInfo(source);
                }
            }
        }
    }

    private static void AppendInputs(StringBuilder query, InputTable inputs)
    {
        foreach (var path in inputs.Paths)
        {
            query.Append("-i \"");
            query.Append(path);
            query.Append("\" ");
        }
    }

    // Appends -map and per-output-stream codec/bsf/metadata/disposition
    // for the explicitly listed streams.
    private static void AppendListedStreams(StringBuilder map, StringBuilder perStream, IReadOnlyList<MediaStream> streams, InputTable inputs, StreamPresence presence, HashSet<string> mappedKeys)
    {
        var outputIndex = 0;

        foreach (var stream in streams)
        {
            var inputIndex = inputs.IndexOf(stream.Path);
            mappedKeys.Add(MapKey(inputIndex, stream.Index));
            AppendStream(map, perStream, stream, inputIndex, outputIndex, presence);
            outputIndex++;
        }
    }

    private static void AppendStream(StringBuilder map, StringBuilder perStream, MediaStream stream, int inputIndex, int outputIndex, StreamPresence presence)
    {
        map.AppendFormatInvariant("-map {0}:{1} ", inputIndex, stream.Index);
        AppendStreamCodec(perStream, outputIndex, stream);
        AppendStreamBitstreamFilter(perStream, outputIndex, stream, presence);
        AppendStreamMetadata(perStream, outputIndex, stream);
        AppendStreamDisposition(perStream, outputIndex, stream);
    }

    private static void AppendStreamCodec(StringBuilder query, int outputIndex, MediaStream stream)
    {
        var codec = stream.Codec;

        if (string.IsNullOrEmpty(codec) && stream.Type == FFmpegStreamType.Audio &&
            string.Equals(stream.Format, "pcm_dvd", StringComparison.OrdinalIgnoreCase))
        {
            codec = "pcm_s16le";
        }

        if (!string.IsNullOrEmpty(codec))
        {
            query.AppendFormatInvariant("-c:{0} {1} ", outputIndex, codec);
        }
    }

    private static void AppendStreamBitstreamFilter(StringBuilder query, int outputIndex, MediaStream stream, StreamPresence presence)
    {
        if (stream.Type == FFmpegStreamType.Audio &&
            string.Equals(stream.Format, "aac", StringComparison.OrdinalIgnoreCase) && presence.HasVideo)
        {
            query.AppendFormatInvariant("-bsf:{0} aac_adtstoasc ", outputIndex);
        }
    }

    // Optional maps from From().Video()/Audio()/… and update presence for global codecs.
    private void AppendFromRules(StringBuilder map, MuxOptions muxOptions, InputTable inputs, StreamPresence presence, HashSet<string> mappedKeys, ProcessOptionsEncoder? options)
    {
        foreach (var rule in muxOptions.FromInputs)
        {
            if (!rule.Any || !inputs.TryResolve(rule, out var inputIndex))
            {
                continue;
            }

            presence.HasVideo |= rule.Video || rule.Cover;
            presence.HasAudio |= rule.Audio;
            presence.HasOther |= rule.Subtitles || rule.Attachments || rule.Data;

            if (rule.Video || rule.Audio || rule.Subtitles || rule.Cover || rule.Attachments || rule.Data)
            {
                AppendOptionalStreamMaps(map, mappedKeys, inputs.Paths[inputIndex], inputIndex, rule, options);
            }
        }
    }

    // Stream-copy codecs for the whole job (pcm_dvd is remuxed to pcm_s16le).
    private static void AppendGlobalCodecs(StringBuilder query, IReadOnlyList<MediaStream> streams, StreamPresence presence)
    {
        if (presence.HasOther)
        {
            query.Append("-c copy ");
            return;
        }

        if (presence.HasVideo)
        {
            query.Append("-vcodec copy ");
        }

        if (!presence.HasAudio)
        {
            return;
        }

        var audioStreams = streams.Where(s => s.Type == FFmpegStreamType.Audio).ToList();
        var onlyPcmDvd = audioStreams.Count == 1 &&
                         string.Equals(audioStreams[0].Format, "pcm_dvd", StringComparison.OrdinalIgnoreCase);
        query.Append(onlyPcmDvd ? "-acodec pcm_s16le " : "-acodec copy ");
    }

    private static void AppendContainerMaps(StringBuilder query, MuxOptions muxOptions, InputTable inputs)
    {
        foreach (var rule in muxOptions.FromInputs)
        {
            if (!inputs.TryResolve(rule, out var inputIndex))
            {
                continue;
            }

            if (rule.ContainerTags)
            {
                query.AppendFormatInvariant("-map_metadata {0} ", inputIndex);
            }

            if (rule.Chapters)
            {
                query.AppendFormatInvariant("-map_chapters {0} ", inputIndex);
            }
        }
    }

    private static void AppendOutputMetadata(StringBuilder query, MuxOptions muxOptions)
    {
        foreach (var pair in muxOptions.Metadata)
        {
            AppendMetadata(query, null, pair.Key, pair.Value);
        }
    }

    private static void AppendAdditionalArguments(StringBuilder query, MuxOptions muxOptions)
    {
        if (!muxOptions.AdditionalArguments.HasValue())
        {
            return;
        }

        query.Append(muxOptions.AdditionalArguments.Trim());
        query.Append(' ');
    }

    // Maps streams from a From() rule that are not already in the stream list.
    private void AppendOptionalStreamMaps(StringBuilder map, HashSet<string> mappedKeys, string inputPath, int inputIndex, MuxFromInput rule, ProcessOptionsEncoder? options)
    {
        var info = _infoReader.GetFileInfo(inputPath, options);

        foreach (var stream in info.FileStreams)
        {
            if (!IncludeStream(rule, stream))
            {
                continue;
            }

            var key = MapKey(inputIndex, stream.Index);

            if (!mappedKeys.Add(key))
            {
                continue;
            }

            map.AppendFormatInvariant("-map {0}:{1} ", inputIndex, stream.Index);
        }
    }

    // Whether a probed stream matches the From() rule flags
    // (cover is video + attached_pic).
    private static bool IncludeStream(MuxFromInput rule, MediaStreamInfo stream)
    {
        var isCover = stream.StreamType == FFmpegStreamType.Video && stream.Disposition.Has("attached_pic");

        if (isCover)
        {
            return rule.Cover;
        }

        return stream.StreamType switch
        {
            FFmpegStreamType.Video => rule.Video,
            FFmpegStreamType.Audio => rule.Audio,
            FFmpegStreamType.Subtitle => rule.Subtitles,
            FFmpegStreamType.Attachment => rule.Attachments,
            FFmpegStreamType.Data => rule.Data,
            _ => false
        };
    }

    private static string MapKey(int inputIndex, int streamIndex) => "{0}:{1}".FormatInvariant(inputIndex, streamIndex);

    // Appends -metadata:s:N language and custom tags for one output stream.
    private static void AppendStreamMetadata(StringBuilder query, int outputIndex, MediaStream stream)
    {
        if (stream.Language.HasValue())
        {
            AppendMetadata(query, outputIndex, "language", stream.Language!);
        }

        foreach (var pair in stream.Metadata)
        {
            AppendMetadata(query, outputIndex, pair.Key, pair.Value);
        }
    }

    // Appends -disposition:N
    // (null = omit, empty = clear 0, else flags).
    // Absolute index, not s:N.
    private static void AppendStreamDisposition(StringBuilder query, int outputIndex, MediaStream stream)
    {
        if (stream.Disposition == null)
        {
            return;
        }

        if (!stream.Disposition.Any)
        {
            query.AppendFormatInvariant("-disposition:{0} 0 ", outputIndex);
            return;
        }

        query.AppendFormatInvariant("-disposition:{0} ", outputIndex);

        query.Append(stream.Disposition);
        query.Append(' ');
    }

    // Appends -metadata or -metadata:s:N key=value with escaping.
    private static void AppendMetadata(StringBuilder query, int? streamIndex, string key, string value)
    {
        if (string.IsNullOrEmpty(key))
        {
            return;
        }

        if (streamIndex is { } index)
        {
            query.AppendFormatInvariant("-metadata:s:{0} ", index);
        }
        else
        {
            query.Append("-metadata ");
        }

        query.Append(EscapeMetadataToken(key));
        query.Append('=');
        query.Append(EscapeMetadataToken(value ?? string.Empty));
        query.Append(' ');
    }

    // Escapes a metadata key or value for the FFmpeg command line.
    internal static string EscapeMetadataToken(string value)
    {
        if (value.Length == 0)
        {
            return "\"\"";
        }
        if (value.IndexOfAny([' ', '"', '\'', '=', '\\']) >= 0)
        {
            return "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        }
        return value;
    }

    // Ordered -i paths: From(path) first, then stream-list paths (each path once).
    private sealed class InputTable
    {
        private readonly Dictionary<string, int> _pathToIndex = new(StringComparer.Ordinal);
        public List<string> Paths { get; } = [];

        public static InputTable From(IReadOnlyList<MediaStream> streams, MuxOptions muxOptions)
        {
            var table = new InputTable();

            foreach (var rule in muxOptions.FromInputs)
            {
                if (rule.Path.HasValue())
                {
                    table.Add(rule.Path!);
                }
            }
            foreach (var stream in streams)
            {
                table.Add(stream.Path);
            }
            return table;
        }

        private void Add(string path)
        {
            if (_pathToIndex.ContainsKey(path))
            {
                return;
            }
            _pathToIndex[path] = Paths.Count;
            Paths.Add(path);
        }

        public int IndexOf(string path) => _pathToIndex[path];

        // Resolves a From() rule to an open -i index (by path or InputIndex).
        public bool TryResolve(MuxFromInput rule, out int index)
        {
            if (rule.Path.HasValue())
            {
                return _pathToIndex.TryGetValue(rule.Path!, out index);
            }

            if (rule.InputIndex is { } i && i >= 0 && i < Paths.Count)
            {
                index = i;
                return true;
            }

            index = -1;
            return false;
        }
    }

    // Which stream kinds are present (drives -vcodec / -acodec / -c copy).
    private sealed class StreamPresence
    {
        public bool HasVideo { get; set; }
        public bool HasAudio { get; set; }
        public bool HasOther { get; set; }

        public static StreamPresence FromListed(
            IReadOnlyList<MediaStream> streams) =>
            new()
            {
                HasVideo = streams.Any(s => s.Type == FFmpegStreamType.Video),
                HasAudio = streams.Any(s => s.Type == FFmpegStreamType.Audio),
                HasOther = streams.Any(s => s.Type is FFmpegStreamType.Subtitle or FFmpegStreamType.Data or FFmpegStreamType.Attachment)
            };
    }
}
