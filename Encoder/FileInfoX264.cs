using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace EmergenceGuardian.Encoder {

    #region Interface

    /// <summary>
    /// Returns file information parsed from X264.
    /// </summary>
    public interface IFileInfoParserX264 {
        /// <summary>
        /// Returns the frame count of input file.
        /// </summary>
        long FrameCount { get; }
    }

    #endregion

    /// <summary>
    /// Parses and stores the X264 or X265 console output. Cast this class to IFileInfoX264 to access the file information.
    /// </summary>
    public class FileInfoX264 : IFileInfoParserX264, IFileInfoParser {
        /// <summary>
        /// Returns whether ParseFileInfo has been called.
        /// </summary>
        public bool IsParsed { get; private set; }
        /// <summary>
        /// Returns the estimated frame count of input file.
        /// </summary>
        public long FrameCount { get; private set; }

        public FileInfoX264() { }

        #region IFileInfoParser

        /// <summary>
        /// Returns whether enough information has been received to parse file information.
        /// </summary>
        /// <param name="data">The last line of output received.</param>
        /// <returns>Whether enough information was received to call ParseFileInfo.</returns>
        public bool HasFileInfo(string data) {
            return IsLineProgressUpdate(data);
        }

        /// <summary>
        /// Returns whether specified line of output is a progress update.
        /// </summary>
        /// <param name="data">A line of output.</param>
        /// <returns>Whether the output line is a progress update.</returns>
        public bool IsLineProgressUpdate(string data) {
            if (data == null || data.TrimStart().Length < 40)
                return false;
            // 2 formats possible.
            // - Starts with [] with 6 chars in-between.
            if (IsLongFormat(data))
                return true;
            // - Starts with a digit and has 6 sections separated by spaces.
            if (char.IsDigit(data.TrimStart()[0]) && SplitData(data).Length == 6)
                return true;
            return false;
        }

        /// <summary>
        /// Parses the output of X264 or X265 to return the info of all input streams.
        /// </summary>
        /// <param name="outputText">The text containing the file information to parse.</param>
        public void ParseFileInfo(string outputText, ProcessOptionsEncoder options) {
            IsParsed = true;
            if (options?.FrameCount > 0)
                FrameCount = options.FrameCount;
            else
                FrameCount = ParseFrameCount(outputText);
        }

        /// <summary>
        /// Parses and returns x264's frame count.
        /// </summary>
        /// <param name="outputText">The raw output line from x264.</param>
        /// <returns>A EncoderStatus object.</returns>
        public long ParseFrameCount(string outputText) {
            if (string.IsNullOrEmpty(outputText))
                return 0;

            // Get the last line.
            string[] Lines = outputText.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            string data = Lines[Lines.Length - 1];

            // Parse this format.
            //[  0.2%]      1/438      9.52  4423.20   0:00:00   0:00:45   18.00 KB    7.70 MB  
            if (IsLongFormat(data)) {
                try {
                    string[] Fields = SplitData(data.Substring(8)); // Trim percentage.
                    return long.Parse(Fields[0].Split('/')[1], CultureInfo.InvariantCulture);
                } catch { }
            }
            return 0;
        }

        /// <summary>
        /// Parses a progress update line of output into a ProgressStatusX264 object.
        /// </summary>
        /// <param name="data">A line of output.</param>
        /// <returns>A ProgressStatusX264 object with parsed data.</returns>
        public object ParseProgress(string data) {
            ProgressStatusX264 Result = new ProgressStatusX264();
            if (string.IsNullOrEmpty(data))
                return Result;

            // 2 possible formats:
            try {
                bool LongFormat = IsLongFormat(data);
                if (LongFormat)
                    data = data.Substring(8); // Trim percentage.
                string[] Fields = SplitData(data);
                if (LongFormat) {
                    //[  0.2%]      1/438      9.52  4423.20   0:00:00   0:00:45   18.00 KB    7.70 MB  
                    Result.Frame = long.Parse(Fields[0].Split('/')[0], CultureInfo.InvariantCulture);
                    Result.Fps = float.Parse(Fields[1], CultureInfo.InvariantCulture);
                    Result.Bitrate = float.Parse(Fields[2], CultureInfo.InvariantCulture);
                    Result.Time = TimeSpan.Parse(Fields[4], CultureInfo.InvariantCulture);
                    Result.Size = $"{Fields[5]} {Fields[6]}";
                } else {
                    //     1   0.10  10985.28    0:00:10    22.35 KB  
                    Result.Frame = long.Parse(Fields[0], CultureInfo.InvariantCulture);
                    Result.Fps = float.Parse(Fields[1], CultureInfo.InvariantCulture);
                    Result.Bitrate = float.Parse(Fields[2], CultureInfo.InvariantCulture);
                    Result.Size = $"{Fields[4]} {Fields[5]}";
                }
            } catch { }
            return Result;
        }

        private bool IsLongFormat(string data) => data.Length > 40 && data[0] == '[' && data[7] == ']';

        private string[] SplitData(string data) => data.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        #endregion

    }
}
