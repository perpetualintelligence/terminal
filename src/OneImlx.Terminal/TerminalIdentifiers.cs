/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace OneImlx.Terminal
{
    /// <summary>
    /// The standard terminal identifiers.
    /// </summary>
    public class TerminalIdentifiers
    {
        /// <summary>
        /// The <c>primary</c> key.
        /// </summary>
        public string PrimaryKey = "primary";

        /// <summary>
        /// The <c>secondary</c> key.
        /// </summary>
        public string SecondaryKey = "secondary";

        /// <summary>
        /// The <c>online</c> license mode.
        /// </summary>
        public const string OnlineLicenseMode = "online";

        /// <summary>
        /// The <c>offline</c> license mode.
        /// </summary>
        public const string OfflineLicenseMode = "offline";

        /// <summary>
        /// The <c>onpremise</c> deployment.
        /// </summary>
        public const string OnPremiseDeployment = "onpremise";
    }
}