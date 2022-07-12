
// ReSharper disable ClassWithVirtualMembersNeverInherited.Global

namespace HanumanInstitute.FFmpeg;

/// <inheritdoc />
public class FileInfoParserFactory : IFileInfoParserFactory
{
    /// <inheritdoc />
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
