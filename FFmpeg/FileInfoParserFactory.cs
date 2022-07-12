using HanumanInstitute.FFmpeg.Properties;

namespace HanumanInstitute.FFmpeg;

/// <summary>
/// Creates new instances of IFileInfoParser.
/// </summary>
public class FileInfoParserFactory : IFileInfoParserFactory
{
    /// <summary>
    /// Creates a new IFileInfoParser for specified application.
    /// </summary>
    /// <param name="encodeApp">The application to parse.</param>
    /// <returns>A new IFileInfoParser.</returns>
    public virtual IFileInfoParser Create(string encodeApp)
    {
        if (encodeApp == EncoderApp.FFmpeg.ToString())
        {
            return new FileInfoFFmpeg();
        }
        else if (encodeApp == EncoderApp.x264.ToString() || encodeApp == EncoderApp.x265.ToString())
        {
            return new FileInfoX264();
        }
        else
        {
            throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resources.EncodeAppInvalid, encodeApp));
        }
    }
}