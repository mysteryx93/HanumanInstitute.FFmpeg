﻿// ReSharper disable CommentTypo
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
    /// Returns information about input streams.
    /// </summary>
    public List<MediaStreamInfo> FileStreams { get; private set; } = new List<MediaStreamInfo>();

    /// <summary>
    /// Gets the first video stream from FileStreams.
    /// </summary>
    /// <returns>A FFmpegVideoStreamInfo object.</returns>
    public MediaVideoStreamInfo? VideoStream => GetStream(FFmpegStreamType.Video) as MediaVideoStreamInfo;

    /// <summary>
    /// Gets the first audio stream from FileStreams.
    /// </summary>
    /// <returns>A FFmpegAudioStreamInfo object.</returns>
    public MediaAudioStreamInfo? AudioStream => GetStream(FFmpegStreamType.Audio) as MediaAudioStreamInfo;

    /// <summary>
    /// Returns the first stream of specified type.
    /// </summary>
    /// <param name="streamType">The type of stream to search for.</param>
    /// <returns>A FFmpegStreamInfo object.</returns>
    private MediaStreamInfo? GetStream(FFmpegStreamType streamType) => FileStreams.FirstOrDefault(f => f.StreamType == streamType);


    // IFileInfoParser

    /// <inheritdoc />
    public bool HasFileInfo(string data)
    {
        data.CheckNotNull(nameof(data));
        return data.StartsWith("Output ", StringComparison.InvariantCulture) ||
               data.StartsWith("Press [q] to stop", StringComparison.InvariantCulture);
    }

    /// <inheritdoc />
    public bool IsLineProgressUpdate(string data)
    {
        data.CheckNotNull(nameof(data));
        return data.StartsWith("frame=", StringComparison.InvariantCulture);
    }

    /// <inheritdoc />
    public void ParseFileInfo(string outputText, ProcessOptionsEncoder? options = null)
    {
        options ??= new ProcessOptionsEncoder();
        IsParsed = true;
        FileDuration = new TimeSpan();
        FileStreams.Clear();

        if (string.IsNullOrEmpty(outputText))
        {
            return;
        }

        var outLines = outputText.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

        // Find duration line.
        var durationIndex = -1;
        for (var i = 0; i < outLines.Length; i++)
        {
            if (outLines[i].StartsWith("  Duration: ", StringComparison.InvariantCulture))
            {
                durationIndex = i;
                // Parse duration line.
                var durationInfo = outLines[i].Trim().Split(new[] { ", " }, StringSplitOptions.None);
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

        // Find input streams.
        for (var i = durationIndex + 1; i < outLines.Length; i++)
        {
            if (outLines[i].TrimStart().StartsWith("Stream #0:", StringComparison.InvariantCulture))
            {
                // Parse input stream.
                var itemInfo = ParseStreamInfo(outLines[i]);
                if (itemInfo != null)
                {
                    FileStreams.Add(itemInfo);
                }
            }
            else if (outLines[i].StartsWith("Output ", StringComparison.InvariantCulture))
            {
                break;
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

    /// <summary>
    /// Parses stream info from specified string returned from FFmpeg.
    /// </summary>
    /// <param name="text">A line of text to parse.</param>
    /// <returns>The stream info, or null if parsing failed.</returns>
    internal static MediaStreamInfo? ParseStreamInfo(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        text = text.Trim();
        var rawText = text;
        // Within parenthesis, replace ',' with ';' to be able to split properly.
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
        var streamInfo = text.Substring(posStart).Split(new[] { ", " }, StringSplitOptions.None);
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
                Format = streamFormat
            };

            // Stream #0:0[0x1e0]: Video: mpeg1video, yuv420p(tv), 352x288 [SAR 178:163 DAR 1958:1467], 1152 kb/s, 25 fps, 25 tbr, 90k tbn
            try
            {
                var colorSpaceValues = streamInfo[1].Split('(', ')');
                v.ColorSpace = colorSpaceValues[0];
                if (colorSpaceValues.Length > 1)
                {
                    var colorRange = colorSpaceValues[1].Split(new[] { "; " }, StringSplitOptions.RemoveEmptyEntries);
                    if (colorRange.Any(c => c == "tv"))
                    {
                        v.ColorRange = "tv";
                    }
                    else if (colorRange.Any(c => c == "pc"))
                    {
                        v.ColorRange = "pc";
                    }

                    var colorMatrix = colorRange.FirstOrDefault(c => c.StartsWith("bt", StringComparison.InvariantCulture));
                    if (colorMatrix != null)
                    {
                        v.ColorMatrix = colorMatrix;
                    }
                }
                var size = streamInfo[2].Split(new[] { "x", " [", ":", " ", "]" }, StringSplitOptions.None);
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
                var fps = streamInfo.FirstOrDefault(s => s.EndsWith("fps", StringComparison.InvariantCulture));
                if (fps is { Length: > 4 })
                {
                    fps = fps.Substring(0, fps.Length - 4);
                    if (fps != "1k") // sometimes it returns 1k ?
                    {
                        v.FrameRate = double.Parse(fps, CultureInfo.InvariantCulture);
                    }
                }
                var bitrate = streamInfo.FirstOrDefault(s => s.EndsWith("kb/s", StringComparison.InvariantCulture));
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
                Format = streamFormat
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
        return null;
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
