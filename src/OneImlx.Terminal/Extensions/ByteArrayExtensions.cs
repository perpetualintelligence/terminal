/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using OneImlx.Shared.Attributes;

namespace OneImlx.Terminal.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="byte"/> arrays.
    /// </summary>
    [Performance("Check and improve performance.", Version = "6.x")]
    public static class ByteArrayExtensions
    {
        /// <summary>
        /// Splits a byte array into segments based on a delimiter.
        /// </summary>
        /// <param name="buffer">The source buffer array to split.</param>
        /// <param name="delimiter">The delimiter byte.</param>
        /// <param name="endsWithDelimiter">Indicates whether the source array ends with the delimiter.</param>
        /// <param name="ignoreEmpty">Indicates whether to ignore empty segments.</param>
        /// <returns>An array of byte arrays representing the segments.</returns>
        public static byte[][] Split(this byte[] buffer, byte delimiter, bool ignoreEmpty, out bool endsWithDelimiter)
        {
            if (buffer == null || buffer.Length == 0)
            {
                throw new ArgumentException("Source array cannot be null or empty.", nameof(buffer));
            }

            // Special case: Source contains only the delimiter
            if (buffer.Length == 1 && buffer[0] == delimiter)
            {
                endsWithDelimiter = true;

                // Return either an empty segment or nothing based on `ignoreEmpty`
                return ignoreEmpty ? [] : [[]];
            }

            var segment = new List<byte>(); // Temporary storage for the current segment
            var segments = new List<byte[]>(); // List to store all segments

            for (int idx = 0; idx < buffer.Length; ++idx)
            {
                if (buffer[idx] == delimiter)
                {
                    // Add the current segment if not empty or if ignoreEmpty is false
                    if (!ignoreEmpty || segment.Count > 0)
                    {
                        segments.Add(segment.ToArray());
                    }
                    segment.Clear();
                }
                else
                {
                    segment.Add(buffer[idx]);
                }
            }

            // Add the final segment if not empty or if ignoreEmpty is false
            if (!ignoreEmpty || segment.Count > 0)
            {
                segments.Add(segment.ToArray());
            }

            // Determine if the source ends with the delimiter
            endsWithDelimiter = buffer[buffer.Length - 1] == delimiter;

            return segments.ToArray();
        }
    }
}
