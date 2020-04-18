using System;

namespace HanumanInstitute.FFmpeg
{
    /// <summary>
    /// Creates new instances of IFileInfoParser.
    /// </summary>
    public interface IFileInfoParserFactory
    {
        /// <summary>
        /// Creates a new IFileInfoParser for specified application.
        /// </summary>
        /// <param name="encoderApp">The application to parse.</param>
        /// <returns>A new IFileInfoParser.</returns>
        IFileInfoParser Create(string encoderApp);
    }
}
