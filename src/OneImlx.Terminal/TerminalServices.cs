/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Text;

namespace OneImlx.Terminal
{
    /// <summary>
    /// Provides common terminal services.
    /// </summary>
    public static class TerminalServices
    {
        /// <summary>
        /// Decodes the license contents to be used by the license extractor.
        /// </summary>
        public static string DecodeLicenseContents(string encodedLicenseContents)
        {
            return Encoding.ASCII.GetString(Convert.FromBase64String(encodedLicenseContents));
        }

        /// <summary>
        /// Delimits the byte array with the specified delimiter.
        /// </summary>
        /// <param name="bytes">The byte array to delimit.</param>
        /// <param name="delimiter">The delimiter byte.</param>
        /// <returns></returns>
        public static byte[] DelimitBytes(byte[] bytes, byte delimiter)
        {
            if (bytes == null || bytes.Length == 0)
            {
                throw new ArgumentException("Byte array cannot be null or empty.", nameof(bytes));
            }

            byte[] delimitedBytes = new byte[bytes.Length + 1];
            Array.Copy(bytes, delimitedBytes, bytes.Length);
            delimitedBytes[bytes.Length] = delimiter;

            return delimitedBytes;
        }

        /// <summary>
        /// Encodes the license contents to set it in the licensing options.
        /// </summary>
        public static string EncodeLicenseContents(string licenseContents)
        {
            return Convert.ToBase64String(Encoding.ASCII.GetBytes(licenseContents));
        }
    }
}
