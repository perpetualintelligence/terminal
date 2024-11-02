/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using System.Text;
using OneImlx.Terminal.Configuration.Options;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// Provides common terminal services.
    /// </summary>
    public static class TerminalServices
    {
        /// <summary>
        /// Constructs a batch of commands by joining each command with the command delimiter and appending the message
        /// delimiter at the end to signify batch completion.
        /// </summary>
        /// <param name="cmdDelimiter">The command delimiter to use between commands.</param>
        /// <param name="msgDelimiter">The message delimiter to append at the end of the batch.</param>
        /// <param name="commands">A collection of commands to join into a single batch.</param>
        /// <returns>A string representing the complete batch of delimited commands, ending with the message delimiter.</returns>
        public static string CreateBatch(string cmdDelimiter, string msgDelimiter, params string[] commands)
        {
            // Use String.Join to join commands efficiently and append the message delimiter at the end
            return string.Join(cmdDelimiter, commands) + msgDelimiter;
        }

        /// <summary>
        /// Constructs a batch of commands by joining each command with the command delimiter and appending the batch
        /// delimiter defined in terminal options to signify batch completion.
        /// </summary>
        /// <param name="terminalOptions">The terminal options that include the command and message delimiter settings.</param>
        /// <param name="commands">A collection of commands to join into a single batch.</param>
        /// <returns>A string representing the complete batch of delimited commands, ending with the message delimiter.</returns>
        public static string CreateBatch(TerminalOptions terminalOptions, params string[] commands)
        {
            return CreateBatch(terminalOptions.Router.RemoteCommandDelimiter, terminalOptions.Router.RemoteBatchDelimiter, commands);
        }

        /// <summary>
        /// Decodes the license contents to be used by the license extractor.
        /// </summary>
        public static string DecodeLicenseContents(string encodedLicenseContents)
        {
            return Encoding.ASCII.GetString(Convert.FromBase64String(encodedLicenseContents));
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
