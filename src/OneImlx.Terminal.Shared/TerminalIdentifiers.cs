﻿/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace OneImlx.Terminal.Shared
{
    /// <summary>
    /// The standard terminal identifiers.
    /// </summary>
    public sealed class TerminalIdentifiers
    {
        /// <summary>
        /// The air gapped deployment. Ideal for on-premise secured installations.
        /// </summary>
        public const string AirGappedDeployment = "air_gapped";

        /// <summary>
        /// The <c>standard</c> deployment. 
        /// </summary>
        public const string StandardDeployment = "standard";

        /// <summary>
        /// The air gapped key that is automatically set.
        /// </summary>
        public const string AirGappedKey = "air_gapped_key";

        /// <summary>
        /// The air gapped usage that is automatically set.
        /// </summary>
        public const string AirGappedUsage = "air_gapped_usage";

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
        /// The <c>sender_endpoint</c> token.
        /// </summary>
        public const string SenderEndpointToken = "sender_endpoint";

        /// <summary>
        /// The <c>sender_id</c> token.
        /// </summary>
        public const string SenderIdToken = "sender_id";

        /// <summary>
        /// The space character used as a separator.
        /// </summary>
        public const char SpaceSeparator = ' ';

        /// <summary>
        /// The data stream delimiter byte. Defaults to <c>0x1E</c> or Record Separator.
        /// </summary>
        public const byte StreamDelimiter = 0x1E;

        /// <summary>
        /// The test application identifier for internal testing.
        /// </summary>
        /// <remarks>
        /// NOTE: This application is reserved for our internal testing purposes. Do not use in your application code.
        /// </remarks>
        public const string TestApplicationId = "08c6925f-a734-4e24-8d84-e06737420766";
    }
}
