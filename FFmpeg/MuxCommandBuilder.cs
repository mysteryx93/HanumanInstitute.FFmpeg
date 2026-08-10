namespace HanumanInstitute.FFmpeg;

// Builds the FFmpeg argument string for a stream-copy mux job.
internal sealed class MuxCommandBuilder
{
    private readonly IMediaInfoReader _infoReader;

    // Initializes the command builder.
    public MuxCommandBuilder(IMediaInfoReader infoReader)
    {
        _infoReader = infoReader.CheckNotNull();
    }

    // Builds the complete FFmpeg mux command.
    public string Build(IReadOnlyList<MediaStream> streams, string destination, MuxOptions muxOptions, ProcessOptionsEncoder? options)
    {
        ResolveTypesFromProbe(streams, options);
        var inputs = MuxInputTable.From(streams, muxOptions);
        var selected = CollectStreams(streams, muxOptions, inputs, options);
        var presence = MuxStreamPresence.From(selected);
        var defaultOwners = FindDefaultOwners(selected);
        var query = new StringBuilder("-y ");
        AppendInputs(query, inputs);
        AppendStreams(query, selected, inputs, presence, defaultOwners);
        AppendContainerMaps(query, muxOptions, inputs);
        AppendOutputMetadata(query, muxOptions);
        AppendAdditionalArguments(query, muxOptions);
        query.Append('"').Append(destination).Append('"');
        return query.ToString();
    }

    // Adds streams selected by MuxOptions to the explicit stream list.
    private List<MediaStream> CollectStreams(IReadOnlyList<MediaStream> streams, MuxOptions muxOptions, MuxInputTable muxInputs, ProcessOptionsEncoder? options)
    {
        var result = new List<MediaStream>(streams);
        var mapped = new HashSet<string>(streams.Select(s => MapKey(muxInputs.IndexOf(s.Path), s.Index)), StringComparer.Ordinal);
        foreach (var rule in muxOptions.FromInputs)
        {
            if (!rule.Any || !muxInputs.TryResolve(rule, out var inputIndex))
            {
                continue;
            }

            var path = muxInputs.Paths[inputIndex];
            var info = _infoReader.GetFileInfo(path, options);
            foreach (var stream in info.FileStreams)
            {
                if (!IncludeStream(rule, stream))
                {
                    continue;
                }

                var key = MapKey(inputIndex, stream.Index);
                if (!mapped.Add(key))
                {
                    continue;
                }

                result.Add(MediaStream.FromStreamInfo(path, stream));
            }
        }

        return result;
    }

    // Appends maps and per-stream output options.
    private static void AppendStreams(StringBuilder query, IReadOnlyList<MediaStream> streams, MuxInputTable muxInputs, MuxStreamPresence presence, IReadOnlyDictionary<FFmpegStreamType, MediaStream> defaultOwners)
    {
        AppendGlobalCodecs(query, streams, presence);
        for (var i = 0; i < streams.Count; i++)
        {
            var stream = streams[i];
            var inputIndex = muxInputs.IndexOf(stream.Path);
            query.AppendFormatInvariant("-map {0}:{1} ", inputIndex, stream.Index);
            AppendStreamCodec(query, i, stream);
            AppendStreamBitstreamFilter(query, i, stream, presence);
            AppendStreamMetadata(query, i, stream);
            AppendStreamDisposition(query, i, EffectiveDisposition(stream, defaultOwners));
        }
    }

    // Finds the first default stream of each type.
    private static Dictionary<FFmpegStreamType, MediaStream> FindDefaultOwners(IReadOnlyList<MediaStream> streams)
    {
        var owners = new Dictionary<FFmpegStreamType, MediaStream>();
        foreach (var stream in streams)
        {
            if (stream.Type == FFmpegStreamType.None || stream.Disposition?.Has("default") != true)
            {
                continue;
            }
            if (!owners.ContainsKey(stream.Type))
            {
                owners[stream.Type] = stream;
            }
        }
        return owners;
    }

    // Resolves unresolved stream types and formats from the source files.
    private void ResolveTypesFromProbe(IReadOnlyList<MediaStream> streams, ProcessOptionsEncoder? options)
    {
        foreach (var group in streams.Where(s => s.Type == FFmpegStreamType.None).GroupBy(s => s.Path, StringComparer.Ordinal))
        {
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
                var source = info.FileStreams.FirstOrDefault(s => s.Index == stream.Index);
                if (source != null)
                {
                    stream.SetFileInfo(source);
                }
            }
        }
    }

    // Resolves the effective disposition when another stream owns the default flag.
    private static StreamDisposition? EffectiveDisposition(MediaStream stream, IReadOnlyDictionary<FFmpegStreamType, MediaStream> defaultOwners)
    {
        if (!defaultOwners.TryGetValue(stream.Type, out var owner) || ReferenceEquals(stream, owner))
        {
            return stream.Disposition;
        }

        // Another stream owns default for this type — ensure this stream is not default.
        // Null means "omit" (FFmpeg would copy source, which may still be default). We must
        // clear default only; using absolute 0 would also wipe attached_pic / forced / etc.
        if (stream.Disposition == null)
        {
            return StreamDisposition.RemoveDefault;
        }
        if (!stream.Disposition.Has("default"))
        {
            return stream.Disposition;
        }

        var result = new StreamDisposition();
        foreach (var flag in stream.Disposition.Flags)
        {
            if (!flag.Equals("default", StringComparison.OrdinalIgnoreCase))
            {
                result.Set(flag);
            }
        }

        // Only default was set → absolute clear is correct (no other flags to keep).
        return result;
    }

    // Appends all input files to the FFmpeg command.
    private static void AppendInputs(StringBuilder query, MuxInputTable muxInputs)
    {
        foreach (var path in muxInputs.Paths)
        {
            query.Append("-i \"").Append(path).Append("\" ");
        }
    }

    // Appends the codec override for an output stream.
    private static void AppendStreamCodec(StringBuilder query, int outputIndex, MediaStream stream)
    {
        var codec = stream.Codec;
        if (string.IsNullOrEmpty(codec) && stream.Type == FFmpegStreamType.Audio && string.Equals(stream.Format, "pcm_dvd", StringComparison.OrdinalIgnoreCase))
        {
            codec = "pcm_s16le";
        }
        if (!string.IsNullOrEmpty(codec))
        {
            query.AppendFormatInvariant("-c:{0} {1} ", outputIndex, codec);
        }
    }

    // Appends stream-specific bitstream filters required for remuxing.
    private static void AppendStreamBitstreamFilter(StringBuilder query, int outputIndex, MediaStream stream, MuxStreamPresence presence)
    {
        if (stream.Type == FFmpegStreamType.Audio && string.Equals(stream.Format, "aac", StringComparison.OrdinalIgnoreCase) && presence.HasVideo)
        {
            query.AppendFormatInvariant("-bsf:{0} aac_adtstoasc ", outputIndex);
        }
    }

    // Appends global stream-copy codec settings.
    private static void AppendGlobalCodecs(StringBuilder query, IReadOnlyList<MediaStream> streams, MuxStreamPresence presence)
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
        var onlyPcmDvd = audioStreams.Count == 1 && string.Equals(audioStreams[0].Format, "pcm_dvd", StringComparison.OrdinalIgnoreCase);
        query.Append(onlyPcmDvd ? "-acodec pcm_s16le " : "-acodec copy ");
    }

    // Appends container-level metadata and chapter mappings.
    private static void AppendContainerMaps(StringBuilder query, MuxOptions muxOptions, MuxInputTable muxInputs)
    {
        foreach (var rule in muxOptions.FromInputs)
        {
            if (!muxInputs.TryResolve(rule, out var inputIndex))
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

    // Appends output-level metadata.
    private static void AppendOutputMetadata(StringBuilder query, MuxOptions muxOptions)
    {
        foreach (var pair in muxOptions.Metadata)
        {
            AppendMetadata(query, null, pair.Key, pair.Value);
        }
    }

    // Appends caller-supplied FFmpeg arguments.
    private static void AppendAdditionalArguments(StringBuilder query, MuxOptions muxOptions)
    {
        if (muxOptions.AdditionalArguments.HasValue())
        {
            query.Append(muxOptions.AdditionalArguments.Trim()).Append(' ');
        }
    }

    // Determines whether a probed stream matches a MuxOptions inclusion rule.
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

    // Appends language and custom metadata for an output stream.
    private static void AppendStreamMetadata(StringBuilder query, int outputIndex, MediaStream stream)
    {
        if (stream.Language.HasValue())
        {
            AppendMetadata(query, outputIndex, "language", stream.Language!);
        }

        var hasTitle = stream.Metadata.ContainsKey("title");
        foreach (var pair in stream.Metadata)
        {
            // MP4 stores 'name' instead of 'title'; but can only be set as 'title'.
            // Prefer explicit title when both are present.
            var isName = pair.Key.Equals("name", StringComparison.OrdinalIgnoreCase);
            if (isName && hasTitle)
            {
                continue;
            }
            AppendMetadata(query, outputIndex, isName ? "title" : pair.Key, pair.Value);
        }
    }

    // Appends the FFmpeg disposition for an output stream.
    private static void AppendStreamDisposition(StringBuilder query, int outputIndex, StreamDisposition? disposition)
    {
        if (disposition == null)
        {
            return;
        }
        if (!disposition.Any)
        {
            query.AppendFormatInvariant("-disposition:{0} 0 ", outputIndex);
            return;
        }

        query.AppendFormatInvariant("-disposition:{0} ", outputIndex);
        query.Append(disposition);
        query.Append(' ');
    }

    // Appends output or stream metadata with FFmpeg escaping.
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

    // Escapes a metadata token for the FFmpeg command line.
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

    // Creates a unique key for an input stream mapping.
    private static string MapKey(int inputIndex, int streamIndex) =>
        "{0}:{1}".FormatInvariant(inputIndex, streamIndex);
}
