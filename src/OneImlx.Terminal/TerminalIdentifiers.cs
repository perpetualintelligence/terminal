/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Runtime.CompilerServices;

// This is used to unit test the behavior of the terminal framework's internal types.
[assembly: InternalsVisibleTo("OneImlx.Terminal.Tests")]

namespace OneImlx.Terminal
{
    /// <summary>
    /// The standard terminal identifiers.
    /// </summary>
    public sealed class TerminalIdentifiers
    {
        /// <summary>
        /// The <c>custom</c> handler.
        /// </summary>
        public const string CustomHandler = "custom";

        /// <summary>
        /// The <c>default</c> handler.
        /// </summary>
        public const string DefaultHandler = "default";

        /// <summary>
        /// The <c>offline</c> license mode.
        /// </summary>
        public const string OfflineLicenseMode = "offline";

        /// <summary>
        /// The <c>online</c> license mode.
        /// </summary>
        public const string OnlineLicenseMode = "online";

        /// <summary>
        /// The <c>onpremise</c> deployment.
        /// </summary>
        public const string OnPremiseDeployment = "onpremise";

        /// <summary>
        /// The remote batch delimiter.
        /// </summary>
        public const string RemoteBatchDelimiter = "$b$";

        /// <summary>
        /// The remote command delimiter.
        /// </summary>
        public const string RemoteCommandDelimiter = "$c$";

        /// <summary>
        /// The <c>sender_endpoint</c> token.
        /// </summary>
        public const string SenderEndpointToken = "sender_endpoint";

        /// <summary>
        /// The <c>sender_id</c> token.
        /// </summary>
        public const string SenderIdToken = "sender_id";

        /// <summary>
        /// The test application identifier for internal testing.
        /// </summary>
        /// <remarks>
        /// NOTE: This application is reserved for our internal testing purposes. Do not use in your application code.
        /// </remarks>
        public const string TestApplicationId = "08c6925f-a734-4e24-8d84-e06737420766";
    }
}
