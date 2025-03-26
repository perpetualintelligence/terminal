﻿/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace OneImlx.Terminal
{
    /// <summary>
    /// The errors for the terminal framework.
    /// </summary>
    public static class TerminalErrors
    {
        /// <summary>
        /// The network connection is closed.
        /// </summary>
        public const string ConnectionClosed = "connection_closed";

        /// <summary>
        /// The option is not already added to the command.
        /// </summary>
        public const string DuplicateOption = "duplicate_option";

        /// <summary>
        /// The argument is not valid.
        /// </summary>
        public const string InvalidArgument = "invalid_argument";

        /// <summary>
        /// The command is not valid.
        /// </summary>
        public const string InvalidCommand = "invalid_command";

        /// <summary>
        /// The configuration is not valid.
        /// </summary>
        public const string InvalidConfiguration = "invalid_configuration";

        /// <summary>
        /// The configuration is not valid.
        /// </summary>
        public const string InvalidDeclaration = "invalid_declaration";

        /// <summary>
        /// The license is not valid.
        /// </summary>
        public const string InvalidLicense = "invalid_license";

        /// <summary>
        /// The option is not valid.
        /// </summary>
        public const string InvalidOption = "invalid_option";

        /// <summary>
        /// The request is not valid.
        /// </summary>
        public const string InvalidRequest = "invalid_request";

        /// <summary>
        /// The argument is missing.
        /// </summary>
        public const string MissingArgument = "missing_argument";

        /// <summary>
        /// The claim is missing.
        /// </summary>
        public const string MissingClaim = "missing_claim";

        /// <summary>
        /// The argument is missing.
        /// </summary>
        public const string MissingCommand = "missing_command";

        /// <summary>
        /// The identity or an account is missing.
        /// </summary>
        public const string MissingIdentity = "missing_identity";

        /// <summary>
        /// The option is missing.
        /// </summary>
        public const string MissingOption = "missing_option";

        /// <summary>
        /// The request is canceled.
        /// </summary>
        public const string RequestCanceled = "request_canceled";

        /// <summary>
        /// The server error.
        /// </summary>
        public const string ServerError = "server_error";

        /// <summary>
        /// The access is not authorized.
        /// </summary>
        public const string UnauthorizedAccess = "unauthorized_access";

        /// <summary>
        /// The argument is not supported.
        /// </summary>
        public const string UnsupportedArgument = "unsupported_argument";

        /// <summary>
        /// The command is not supported.
        /// </summary>
        public const string UnsupportedCommand = "unsupported_command";

        /// <summary>
        /// The option is not supported.
        /// </summary>
        public const string UnsupportedOption = "unsupported_option";
    }
}
