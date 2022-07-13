namespace HanumanInstitute.FFmpeg;

/// <summary>
/// Parses and stores the X264 or X265 console output. Cast this class to IFileInfoX264 to access the file information.
/// </summary>
public class FileInfoX264 : IFileInfoParser
{
    /// <inheritdoc />
    public bool IsParsed { get; private set; }
    /// <summary>
    /// Returns the estimated frame count of input file.
    /// </summary>
    public long FrameCount { get; private set; }


    // IFileInfoParser

    /// <inheritdoc />
    public bool HasFileInfo(string data) => IsLineProgressUpdate(data);

    /// <inheritdoc />
    public bool IsLineProgressUpdate(string? data)
    {
        if (data == null || data.TrimStart().Length < 40)
        {
            return false;
        }
        // 2 formats possible.
        // - Starts with [] with 6 chars in-between.
        if (IsLongFormat(data))
        {
            return true;
        }
        // - Starts with a digit and has 6 sections separated by spaces.
        if (char.IsDigit(data.TrimStart()[0]) && SplitData(data).Length == 6)
        {
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public void ParseFileInfo(string outputText, ProcessOptionsEncoder? options = null)
    {
        options ??= new ProcessOptionsEncoder();
        IsParsed = true;
        FrameCount = options.FrameCount > 0 ?
            options.FrameCount :
            ParseFrameCount(outputText);
    }

    /// <summary>
    /// Parses and returns x264's frame count.
    /// </summary>
    /// <param name="outputText">The raw output line from x264.</param>
    /// <returns>A EncoderStatus object.</returns>
    internal static long ParseFrameCount(string outputText)
    {
        if (!outputText.HasValue())
        {
            return 0;
        }

        // Get the last line.
        var lines = outputText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        var data = lines[lines.Length - 1];

        // Parse this format.
        //[  0.2%]      1/438      9.52  4423.20   0:00:00   0:00:45   18.00 KB    7.70 MB  
        if (IsLongFormat(data))
        {
            try
            {
                var fields = SplitData(data.Substring(8)); // Trim percentage.
                return long.Parse(fields[0].Split('/')[1], CultureInfo.InvariantCulture);
            }
            catch (ArgumentNullException) { }
            catch (FormatException) { }
            catch (OverflowException) { }
        }
        return 0;
    }

    /// <inheritdoc />
    public object ParseProgress(string data)
    {
        var result = new ProgressStatusX264();
        if (string.IsNullOrEmpty(data))
        {
            return result;
        }

        // 2 possible formats:
        try
        {
            var longFormat = IsLongFormat(data);
            if (longFormat)
            {
                data = data.Substring(8); // Trim percentage.
            }

            var fields = SplitData(data);
            if (longFormat)
            {
                //[  0.2%]      1/438      9.52  4423.20   0:00:00   0:00:45   18.00 KB    7.70 MB  
                result.Frame = long.Parse(fields[0].Split('/')[0], CultureInfo.InvariantCulture);
                result.Fps = float.Parse(fields[1], CultureInfo.InvariantCulture);
                result.Bitrate = float.Parse(fields[2], CultureInfo.InvariantCulture);
                result.Time = TimeSpan.Parse(fields[4], CultureInfo.InvariantCulture);
                result.Size = Invariant($"{fields[5]} {fields[6]}");
            }
            else
            {
                //     1   0.10  10985.28    0:00:10    22.35 KB  
                result.Frame = long.Parse(fields[0], CultureInfo.InvariantCulture);
                result.Fps = float.Parse(fields[1], CultureInfo.InvariantCulture);
                result.Bitrate = float.Parse(fields[2], CultureInfo.InvariantCulture);
                result.Size = Invariant($"{fields[4]} {fields[5]}");
            }
        }
        catch (ArgumentNullException) { }
        catch (FormatException) { }
        catch (OverflowException) { }
        return result;
    }

    private static bool IsLongFormat(string data)
    {
        return data.Length > 40 && data[0] == '[' && data[7] == ']';
    }

    private static string[] SplitData(string data)
    {
        return data.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
    }
}
