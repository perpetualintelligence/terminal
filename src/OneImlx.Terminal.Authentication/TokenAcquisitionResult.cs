/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Collections.Generic;
using System.Security.Claims;

namespace OneImlx.Terminal.Authentication
{
    /// <summary>
    /// Represents the result of a token acquisition operation.
    /// </summary>
    public sealed class TokenAcquisitionResult
    {
        /// <summary>
        /// Gets or sets the acquired access token.
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the claims about the authenticated user (from ID token or user info endpoint).
        /// </summary>
        public IEnumerable<Claim>? Claims { get; set; }

        /// <summary>
        /// Gets or sets the acquired ID token, if returned by the identity provider.
        /// </summary>
        public string? IdToken { get; set; }

        /// <summary>
        /// Gets or sets optional metadata such as token type, expiration, scope, etc.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }
}
