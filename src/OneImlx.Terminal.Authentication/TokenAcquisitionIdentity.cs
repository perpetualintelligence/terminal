/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace OneImlx.Terminal.Authentication
{
    /// <summary>
    /// Represents a generic identity reference for authentication purposes. Wraps the raw provider identity and
    /// optional display information.
    /// </summary>
    public sealed class TokenAcquisitionIdentity
    {
        /// <summary>
        /// The raw identity object (e.g., MSAL IAccount, login_hint string).
        /// </summary>
        public object? Raw { get; set; }
    }
}
