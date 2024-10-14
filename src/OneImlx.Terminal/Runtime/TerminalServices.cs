/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using System.Linq;
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
        /// Decodes the license contents to be used by the license extractor.
        /// </summary>
        public static string DecodeLicenseContents(string encodedLicenseContents)
        {
            // The license contents are always ASCII.
            return Encoding.ASCII.GetString(Convert.FromBase64String(encodedLicenseContents));
        }

        /// <summary>
        /// Constructs a delimited message from specified raw command strings using the settings defined in terminal options.
        /// </summary>
        /// <param name="commands">The raw command strings to process with the message delimiter.</param>
        /// <param name="terminalOptions">The terminal options that include the delimiter settings.</param>
        /// <returns>A message string with added delimiters.</returns>
        /// <remarks>
        /// This method verifies that each command string ends with the
        /// <see cref="RouterOptions.RemoteCommandDelimiter"/>. It appends the delimiter if absent and combines the
        /// strings into a single message using the <see cref="RouterOptions.RemoteMessageDelimiter"/>.
        /// </remarks>
        /// <seealso cref="DelimitedMessage(string, string, string[])"/>
        public static string DelimitedMessage(TerminalOptions terminalOptions, params string[] commands)
        {
            return DelimitedMessage(terminalOptions.Router.RemoteCommandDelimiter, terminalOptions.Router.RemoteMessageDelimiter, commands);
        }

        /// <summary>
        /// Constructs a delimited message from specified raw strings using provided command and message delimiters.
        /// </summary>
        /// <param name="commands">The raw strings to append with a command delimiter.</param>
        /// <param name="cmdDelimiter">The command delimiter to use.</param>
        /// <param name="msgDelimiter">The message delimiter to use.</param>
        /// <returns>A delimited message ready for transmission or processing.</returns>
        /// <remarks>
        /// This method checks each command string for the presence of the command delimiter and appends it if missing.
        /// It then combines these strings into a single message.
        /// </remarks>
        /// <seealso cref="DelimitedMessage(TerminalOptions, string[])"/>
        public static string DelimitedMessage(string cmdDelimiter, string msgDelimiter, params string[] commands)
        {
            StringBuilder delimitedMessage = new();
            foreach (string command in commands)
            {
                if (!command.EndsWith(cmdDelimiter))
                {
                    delimitedMessage.Append(command).Append(cmdDelimiter);
                }
                else
                {
                    delimitedMessage.Append(command);
                }
            }

            return delimitedMessage.Append(msgDelimiter).ToString();
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
