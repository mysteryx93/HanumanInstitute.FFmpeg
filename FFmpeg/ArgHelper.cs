using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using HanumanInstitute.Encoder.Properties;

namespace HanumanInstitute.Encoder
{
    /// <summary>
    /// Provides helper methods to validate parameters.
    /// </summary>
    internal static class ArgHelper
    {
        /// <summary>
        /// Validates whether specific value is not null, and throws an exception if it is null.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="name">The name of the parameter.</param>
        internal static void ValidateNotNull(object value, string name)
        {
            if (value == null)
            {
                throw new ArgumentNullException(name);
            }
        }

        /// <summary>
        /// Validates whether specific value is not null or empty, and throws an exception if it is null or empty.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="name">The name of the parameter.</param>
        internal static void ValidateNotNullOrEmpty(string value, string name)
        {
            ValidateNotNull(value, name);
            if (string.IsNullOrEmpty(value))
            {
                ThrowArgumentNullOrEmpty(name);
            }
        }

        /// <summary>
        /// Validates whether specific listt is not null or empty, and throws an exception if it is null or empty.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="name">The name of the parameter.</param>
        internal static void ValidateListNotNullOrEmpty<T>(IEnumerable<T> value, string name)
        {
            ValidateNotNull(value, name);
            if (!value.Any())
            {
                ThrowArgumentNullOrEmpty(name);
            }
        }

        /// <summary>
        /// Throws an exception of type ArgumentException saying an argument is null or empty.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        internal static void ThrowArgumentNullOrEmpty(string name)
        {
            throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resources.VideoFileAndAudioFileNull, name), name);
        }
    }
}
