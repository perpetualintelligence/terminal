/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("PerpetualIntelligence.Terminal.Test")]

namespace PerpetualIntelligence.Terminal
{
    /// <summary>
    /// These well-known handlers allow app authors to configure the framework and provide custom implementations.
    /// </summary>
    public static class TerminalHandlers
    {
        /// <summary>
        /// The <c>ascii</c> handler.
        /// </summary>
        public const string AsciiHandler = "ascii";

        /// <summary>
        /// The <c>custom</c> handler.
        /// </summary>
        public const string CustomHandler = "custom";

        /// <summary>
        /// The <c>default</c> handler.
        /// </summary>
        public const string DefaultHandler = "default";

        /// <summary>
        /// The <c>json</c> handler.
        /// </summary>
        public const string InMemoryHandler = "in-memory";

        /// <summary>
        /// The <c>json</c> handler. Reserved for future use.
        /// </summary>
        public const string JsonHandler = "json";

        /// <summary>
        /// The <c>offline</c> handler.
        /// </summary>
        public const string OfflineLicenseHandler = "offline-license";

        /// <summary>
        /// The <c>online</c> handler.
        /// </summary>
        public const string OnlineLicenseHandler = "online-license";

        /// <summary>
        /// The <c>on-premise</c> handler.
        /// </summary>
        public const string OnPremiseLicenseHandler = "onpremise-license";

        /// <summary>
        /// The <c>unicode</c> handler.
        /// </summary>
        public const string UnicodeHandler = "unicode";
    }
}