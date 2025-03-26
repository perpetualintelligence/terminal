/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace OneImlx.Terminal.Configuration.Options
{
    /// <summary>
    /// The authentication configuration options.
    /// </summary>
    public sealed class AuthenticationOptions
    {
        /// <summary>
        /// Gets or sets the default authentication scopes.
        /// </summary>
        /// <remarks>
        /// These scopes are used by default if no specific scopes are provided during the authentication request.
        /// </remarks>
        public string[]? DefaultScopes { get; set; }

        /// <summary>
        /// Enables the authentication for the terminal.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the authentication user flow.
        /// </summary>
        public string? UserFlow { get; set; }

        /// <summary>
        /// Gets or sets the valid hosts for generating the authorization token.
        /// </summary>
        /// <remarks>Requests to hosts not listed here will be considered unauthorized.</remarks>
        public string[]? ValidHosts { get; set; }
    }
}
