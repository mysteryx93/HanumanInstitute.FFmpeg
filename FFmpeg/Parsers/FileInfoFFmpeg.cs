// ReSharper disable CommentTypo
namespace HanumanInstitute.FFmpeg;

/// <summary>
/// Parses and stores the FFmpeg console output. Cast this class to IFileInfoFFmpeg to access the file information.
/// </summary>
public class FileInfoFFmpeg : IFileInfoParser
{
    /// <inheritdoc />
    public bool IsParsed { get; private set; }
    /// <summary>
    /// Returns the estimated frame count of input file.
    /// </summary>
    public long FrameCount { get; set; }
    /// <summary>
    /// Returns the duration of input file.
    /// </summary>
    public TimeSpan FileDuration { get; set; }
    /// <summary>
    /// Container / demuxer name as reported by FFmpeg (e.g. mp3, mov,mp4,m4a,3gp,3g2,mj2, matroska,webm).
    /// </summary>
    public string? FormatName { get; set; }
    /// <summary>
    /// Format-level (container) metadata key/value pairs (e.g. title, artist).
    /// </summary>
    public IDictionary<string, string> Metadata { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    /// <summary>
    /// Returns information about input streams.
    /// </summary>
    public List<MediaStreamInfo> FileStreams { get; private set; } = [];

    /// <summary>
    /// Gets the first video stream from FileStreams.
    /// </summary>
    /// <returns>A FFmpegVideoStreamInfo object.</returns>
    public MediaVideoStreamInfo? VideoStream => GetStream(FFmpegStreamType.Video) as MediaVideoStreamInfo;

    /// <summary>
    /// Gets the first audio stream from FileStreams.
    /// </summary>
    public MediaAudioStreamInfo? AudioStream => GetStream(FFmpegStreamType.Audio) as MediaAudioStreamInfo;

    /// <summary>
    /// Gets the first subtitle stream from FileStreams.
    /// </summary>
    public MediaSubtitleStreamInfo? SubtitleStream => GetStream(FFmpegStreamType.Subtitle) as MediaSubtitleStreamInfo;

    /// <summary>
    /// Returns the first stream of specified type.
    /// </summary>
    private MediaStreamInfo? GetStream(FFmpegStreamType streamType) => FileStreams.FirstOrDefault(f => f.StreamType == streamType);


    // IFileInfoParser

    /// <inheritdoc />
    public bool HasFileInfo(string data)
    {
        data.CheckNotNull();
        return data.StartsWithInvariant("Output ") ||
               data.StartsWithInvariant("Press [q] to stop");
    }

    /// <inheritdoc />
    public bool IsLineProgressUpdate(string data)
    {
        data.CheckNotNull();
        return data.StartsWithInvariant("frame=");
    }

    /// <inheritdoc />
    public void ParseFileInfo(string outputText, ProcessOptionsEncoder? options = null)
    {
        options ??= new ProcessOptionsEncoder();
        IsParsed = true;
        FileDuration = new TimeSpan();
        FormatName = null;
        Metadata.Clear();
        FileStreams.Clear();

        if (string.IsNullOrEmpty(outputText))
        {
            return;
        }

        // Captured FFmpeg output may use CRLF.
        var outLines = outputText.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');

        // Input #0, matroska,webm, from 'TaggedMedia.mkv':
        foreach (var t in outLines)
        {
            if (t.StartsWithInvariant("Input #0,"))
            {
                FormatName = ParseFormatName(t);
                break;
            }
        }

        //   Metadata: /     title           : Sample Title  (format tags, before Duration)
        for (var i = 0; i < outLines.Length; i++)
        {
            if (outLines[i].StartsWithInvariant("  Duration: "))
            {
                break;
            }
            if (IsMetadataHeader(outLines[i]))
            {
                i = ParseMetadataBlock(outLines, i + 1, Metadata);
            }
        }

        //   Duration: 00:00:02.02, start: 0.000000, bitrate: 210 kb/s
        var durationIndex = -1;
        for (var i = 0; i < outLines.Length; i++)
        {
            if (outLines[i].StartsWithInvariant("  Duration: "))
            {
                durationIndex = i;
                var durationInfo = outLines[i].Trim().Split([", "], StringSplitOptions.None);
                var durationString = durationInfo[0].Split(' ')[1];
                if (durationString == "N/A")
                {
                    FileDuration = new TimeSpan(0);
                }
                else if (!string.IsNullOrWhiteSpace(durationString))
                {
                    try
                    {
                        FileDuration = TimeSpan.Parse(durationString, CultureInfo.InvariantCulture);
                    }
                    catch (FormatException) { }
                    catch (OverflowException) { }
                }
                break;
            }
        }

        //   Stream #0:1: Audio: aac (LC), 44100 Hz, mono, fltp (default)
        //     Metadata:
        //       FREQUENCY       : 440
        // Stops at: Output #0, ...  /  Stream mapping:  /  Press [q] to stop
        MediaStreamInfo? lastStream = null;
        for (var i = durationIndex + 1; i < outLines.Length; i++)
        {
            var line = outLines[i];
            if (line.StartsWithInvariant("Output ") ||
                line.StartsWithInvariant("Stream mapping:") ||
                line.StartsWithInvariant("Press [q]"))
            {
                break;
            }

            if (line.TrimStart().StartsWithInvariant("Stream #0:"))
            {
                lastStream = ParseStreamInfo(line);
                if (lastStream != null)
                {
                    FileStreams.Add(lastStream);
                }
            }
            else if (lastStream != null && IsMetadataHeader(line))
            {
                i = ParseMetadataBlock(outLines, i + 1, lastStream.Metadata);
            }
            else if (line.Length > 0 && !char.IsWhiteSpace(line[0]) && !line.StartsWithInvariant("  "))
            {
                // [libx264 @ 000000000269e480] using SAR=178/163
                lastStream = null;
            }
        }

        // Calculate FrameCount.
        if (options.FrameCount > 0)
        {
            FrameCount = options.FrameCount;
        }
        else if (VideoStream != null)
        {
            FrameCount = (long)(FileDuration.TotalSeconds * VideoStream.FrameRate);
        }
    }

    // Input #0, mov,mp4,m4a,3gp,3g2,mj2
    internal static string? ParseFormatName(string inputLine)
    {
        if (string.IsNullOrEmpty(inputLine) || !inputLine.StartsWithInvariant("Input #0,"))
        {
            return null;
        }
        var fromIdx = inputLine.IndexOf(", from ", StringComparison.InvariantCulture);
        if (fromIdx < 0)
        {
            return null;
        }
        var name = inputLine.Substring("Input #0,".Length, fromIdx - "Input #0,".Length).Trim();
        return name.Length > 0 ? name : null;
    }

    //   Metadata:
    private static bool IsMetadataHeader(string line)
    {
        var t = line.Trim();
        return t.Equals("Metadata:", StringComparison.InvariantCulture);
    }

    //     title           : Nu
    //       FREQUENCY       : 440
    // Multi-line values (FFmpeg dump):
    //     comment         : line1
    //                     : line2
    internal static int ParseMetadataBlock(string[] lines, int startIndex, IDictionary<string, string> target)
    {
        var i = startIndex;
        string? lastKey = null;
        for (; i < lines.Length; i++)
        {
            var line = lines[i];
            if (!TryParseMetadataEntry(line, out var key, out var value, out var isContinuation))
            {
                return i - 1;
            }

            if (isContinuation)
            {
                // Append to previous tag; orphan continuations are ignored.
                if (lastKey != null && target.TryGetValue(lastKey, out var prior))
                {
                    target[lastKey] = prior + "\n" + value;
                }
                continue;
            }

            if (key.Length > 0)
            {
                target[key] = value;
                lastKey = key;
            }
        }
        return i - 1;
    }

    //     title           : Nu   /   not:   Duration: 00:00:02.00  /  Stream #0:0: Audio: ...
    internal static bool TryParseMetadataEntry(string line, out string key, out string value) =>
        TryParseMetadataEntry(line, out key, out value, out _);

    // Continuation lines have an empty key (": rest of value") after trim.
    internal static bool TryParseMetadataEntry(string line, out string key, out string value, out bool isContinuation)
    {
        key = string.Empty;
        value = string.Empty;
        isContinuation = false;
        if (string.IsNullOrEmpty(line))
        {
            return false;
        }

        if (line.Length == 0 || (line[0] != ' ' && line[0] != '\t'))
        {
            return false;
        }

        var trimmed = line.Trim();
        if (trimmed.StartsWithInvariant("Stream #") ||
            trimmed.StartsWithInvariant("Duration:") ||
            trimmed.StartsWithInvariant("Metadata:") ||
            trimmed.StartsWithInvariant("Chapter") ||
            trimmed.StartsWithInvariant("Side data:") ||
            trimmed.StartsWithInvariant("Output ") ||
            trimmed.StartsWithInvariant("Input #"))
        {
            return false;
        }

        // Multi-line tag continuation: "                    : line2"
        if (trimmed.StartsWith(":", StringComparison.Ordinal))
        {
            isContinuation = true;
            value = trimmed.Length > 1 && trimmed[1] == ' '
                ? trimmed.Substring(2)
                : trimmed.Substring(1);
            return true;
        }

        // title           : Nu
        var match = System.Text.RegularExpressions.Regex.Match(trimmed, @"^([A-Za-z0-9_/-]+)\s*:\s*(.*)$");
        if (!match.Success)
        {
            return false;
        }
        key = match.Groups[1].Value;
        value = match.Groups[2].Value.Trim();
        return key.Length > 0;
    }

    /// <summary>
    /// Parses stream info from specified string returned from FFmpeg.
    /// </summary>
    /// <param name="text">A line of text to parse.</param>
    /// <returns>The stream info, or null if parsing failed.</returns>
    // Stream #0:0: Audio: mp3, 44100 Hz, stereo, s16p, 192 kb/s
    // Stream #0:1(eng): Audio: aac (LC), 48000 Hz, stereo, fltp, 128 kb/s (default)
    // Stream #0:0(und): Video: h264 (High) (avc1 / 0x31637661), yuv420p, 352x288 [SAR 178:163 DAR 1958:1467], 228 kb/s, 25 fps, 25 tbr, 12800 tbn, 50 tbc (default)
    // Stream #0:3(eng): Subtitle: subrip (srt)
    // Stream #0:2: Data: bin_data / Stream #0:2: Attachment: ttf
    internal static MediaStreamInfo? ParseStreamInfo(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        text = text.Trim();
        var rawText = text;
        var disposition = StreamDisposition.FromStreamLine(rawText);
        var language = ParseStreamLanguage(rawText);

        // Within parenthesis, replace ',' with ';' to be able to split properly.
        // yuv420p(tv, progressive) / (avc1 / 0x31637661) — commas inside () must not split fields
        var itemChars = text.ToCharArray();
        var isInParenthesis = false;
        for (var i = 0; i < itemChars.Length; i++)
        {
            if (itemChars[i] == '(')
            {
                isInParenthesis = true;
            }
            else if (itemChars[i] == ')')
            {
                isInParenthesis = false;
            }

            if (isInParenthesis && itemChars[i] == ',')
            {
                itemChars[i] = ';';
            }
        }
        text = new string(itemChars);

        var posStart = 10;
        var posEnd = -1;
        for (var i = posStart; i < text.Length; i++)
        {
            if (!char.IsDigit(text[i]))
            {
                posEnd = i;
                break;
            }
        }
        if (posEnd < 0 || !int.TryParse(text.Substring(posStart, posEnd - posStart), out var streamIndex))
        {
            return null;
        }
        // Read StreamType
        posStart = text.IndexOf(": ", posStart, StringComparison.InvariantCulture) + 2;
        posEnd = text.IndexOf(": ", posStart, StringComparison.InvariantCulture);
        if (posStart < 0 || posEnd < 0)
        {
            return null;
        }
        var streamType = text.Substring(posStart, posEnd - posStart);
        // Split stream data
        posStart = posEnd + 2;
        var streamInfo = text.Substring(posStart).Split([", "], StringSplitOptions.None);
        if (!streamInfo.Any())
        {
            return null;
        }
        var streamFormat = streamInfo[0].Split(' ')[0];

        if (streamType == "Video")
        {
            var v = new MediaVideoStreamInfo
            {
                RawText = rawText,
                Index = streamIndex,
                Format = streamFormat,
                Language = language,
                Disposition = disposition
            };

            // Stream #0:0[0x1e0]: Video: mpeg1video, yuv420p(tv), 352x288 [SAR 178:163 DAR 1958:1467], 1152 kb/s, 25 fps, 25 tbr, 90k tbn
            try
            {
                var colorSpaceValues = streamInfo[1].Split('(', ')');
                v.ColorSpace = colorSpaceValues[0];
                if (colorSpaceValues.Length > 1)
                {
                    var colorRange = colorSpaceValues[1].Split(["; "], StringSplitOptions.RemoveEmptyEntries);
                    if (colorRange.Any(c => c == "tv"))
                    {
                        v.ColorRange = "tv";
                    }
                    else if (colorRange.Any(c => c == "pc"))
                    {
                        v.ColorRange = "pc";
                    }

                    var colorMatrix = colorRange.FirstOrDefault(c => c.StartsWithInvariant("bt"));
                    if (colorMatrix != null)
                    {
                        v.ColorMatrix = colorMatrix;
                    }
                }
                var size = streamInfo[2].Split(["x", " [", ":", " ", "]"], StringSplitOptions.None);
                v.Width = int.Parse(size[0], CultureInfo.InvariantCulture);
                v.Height = int.Parse(size[1], CultureInfo.InvariantCulture);
                if (size.Length > 2 && size[2] == "SAR")
                {
                    v.Sar1 = int.Parse(size[3], CultureInfo.InvariantCulture);
                    v.Sar2 = int.Parse(size[4], CultureInfo.InvariantCulture);
                    if (v.Sar1 > 0 && v.Sar2 > 0)
                    {
                        v.PixelAspectRatio = Math.Round((double)v.Sar1 / v.Sar2, 3);
                    }

                    v.Dar1 = int.Parse(size[6], CultureInfo.InvariantCulture);
                    v.Dar2 = int.Parse(size[7], CultureInfo.InvariantCulture);
                    if (v.Dar1 > 0 && v.Dar2 > 0)
                    {
                        v.DisplayAspectRatio = Math.Round((double)v.Dar1 / v.Dar2, 3);
                    }
                }
                var fps = streamInfo.FirstOrDefault(s => s.EndsWithInvariant("fps"));
                if (fps is { Length: > 4 })
                {
                    fps = fps.Substring(0, fps.Length - 4);
                    if (fps != "1k") // sometimes it returns 1k ?
                    {
                        v.FrameRate = double.Parse(fps, CultureInfo.InvariantCulture);
                    }
                }
                var bitrate = streamInfo.FirstOrDefault(s => s.EndsWithInvariant("kb/s"));
                if (bitrate is { Length: > 5 })
                {
                    bitrate = bitrate.Substring(0, bitrate.Length - 5);
                    v.Bitrate = int.Parse(bitrate, CultureInfo.InvariantCulture);
                }
            }
            catch (FormatException) { }
            catch (OverflowException) { }

            return v;
        }
        else if (streamType == "Audio")
        {
            var v = new MediaAudioStreamInfo
            {
                RawText = rawText,
                Index = streamIndex,
                Format = streamFormat,
                Language = language,
                Disposition = disposition
            };

            // Stream #0:1[0x1c0]: Audio: mp2, 44100 Hz, stereo, s16p, 224 kb/s
            try
            {
                v.SampleRate = int.Parse(streamInfo[1].Split(' ')[0], CultureInfo.InvariantCulture);
                v.Channels = streamInfo[2];
                if (streamInfo.Length > 3)
                {
                    v.BitDepth = streamInfo[3];
                }
                if (streamInfo.Length > 4 && streamInfo[4].Contains(" kb/s"))
                {
                    v.Bitrate = int.Parse(streamInfo[4].Split(' ')[0], CultureInfo.InvariantCulture);
                }
            }
            catch (ArgumentNullException) { }
            catch (FormatException) { }
            catch (OverflowException) { }
            return v;
        }

        // Stream #0:3(eng): Subtitle: subrip (srt)
        // Stream #0:2: Data: bin_data
        // Stream #0:2: Attachment: ttf
        MediaStreamInfo? result = streamType switch
        {
            "Subtitle" => new MediaSubtitleStreamInfo(),
            "Data" => new MediaDataStreamInfo(),
            "Attachment" => new MediaAttachmentStreamInfo(),
            _ => null
        };
        if (result != null)
        {
            result.RawText = rawText;
            result.Index = streamIndex;
            result.Format = streamFormat;
            result.Language = language;
            result.Disposition = disposition;
        }
        return result;
    }

    // Stream #0:1(eng): Audio: ...   /   Stream #0:0(und): Video: ...
    // not language: Stream #0:0[0x1e0]: Video: mpeg1video, yuv420p(tv), ...
    internal static string? ParseStreamLanguage(string streamLine)
    {
        if (string.IsNullOrEmpty(streamLine)) { return null; }
        var m = System.Text.RegularExpressions.Regex.Match(streamLine, @"Stream #0:\d+(?:\[[^\]]*\])?\(([^)]+)\)");
        if (!m.Success)
        {
            return null;
        }
        var lang = m.Groups[1].Value.Trim();
        // not: trailing (default) mis-read as language
        if (lang is "default" or "forced" or "dub" or "original" or "comment" or "lyrics" or "karaoke" or
            "hearing_impaired" or "visual_impaired" or "clean_effects" or "attached_pic" or
            "timed_thumbnails" or "captions" or "descriptions" or "metadata" or "dependent" or "still_image")
        {
            return null;
        }
        return lang.Length > 0 ? lang : null;
    }

    /// <inheritdoc />
    public object ParseProgress(string text)
    {
        var result = new ProgressStatusFFmpeg();
        if (!string.IsNullOrEmpty(text))
        {
            // frame=  929 fps=0.0 q=-0.0 size=   68483kB time=00:00:37.00 bitrate=15162.6kbits/s speed=  74x    
            try
            {
                result.Frame = long.Parse(ParseAttribute(text, "frame"), CultureInfo.InvariantCulture);
                result.Fps = float.Parse(ParseAttribute(text, "fps"), CultureInfo.InvariantCulture);
                result.Quantizer = float.Parse(ParseAttribute(text, "q"), CultureInfo.InvariantCulture);
                result.Size = ParseAttribute(text, "size");
                result.Time = TimeSpan.Parse(ParseAttribute(text, "time"), CultureInfo.InvariantCulture);
                result.Bitrate = ParseAttribute(text, "bitrate");
                var speedString = ParseAttribute(text, "speed");
                if (speedString != "N/A")
                {
                    result.Speed = float.Parse(speedString.TrimEnd('x'), CultureInfo.InvariantCulture);
                }
            }
            catch (ArgumentNullException) { }
            catch (FormatException) { }
            catch (OverflowException) { }
        }
        return result;
    }

    /// <summary>
    /// Returns the value of specified attribute within a line of text. It will search 'key=' and return the following value until a space is found.
    /// </summary>
    /// <param name="text">The line of text to parse.</param>
    /// <param name="key">The key of the attribute to look for.</param>
    /// <returns>The attribute value.</returns>
    public static string ParseAttribute(string text, string key)
    {
        if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(key))
        {
            var pos = text.IndexOf(key + "=", StringComparison.InvariantCulture);
            if (pos >= 0)
            {
                // Find first non-space character.
                pos += key.Length + 1;
                while (pos < text.Length && text[pos] == ' ')
                {
                    pos++;
                }
                // Find space after value.
                var posEnd = text.IndexOf(' ', pos);
                if (posEnd == -1)
                {
                    posEnd = text.Length;
                }

                return text.Substring(pos, posEnd - pos);
            }
        }
        return string.Empty;
    }
}
