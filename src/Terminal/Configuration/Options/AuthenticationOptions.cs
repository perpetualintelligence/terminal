/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;

namespace PerpetualIntelligence.Terminal.Configuration.Options
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
        /// Gets or sets the valid hosts for generating the authorization token.
        /// </summary>
        /// <remarks>
        /// Requests to hosts not listed here will be considered unauthorized.
        /// </remarks>
        public string[]? ValidHosts { get; set; }

        /// <summary>
        /// Gets or sets the base address for HTTP requests.
        /// </summary>
        /// <remarks>
        /// This is the base URL to which request URIs will be appended.
        /// </remarks>
        public string? BaseAddress { get; set; }

        /// <summary>
        /// Gets or sets the timeout duration for HTTP requests.
        /// </summary>
        /// <remarks>
        /// Specifies the time period within which an HTTP request must complete. If exceeded, the request will be aborted.
        /// </remarks>
        public TimeSpan? Timeout { get; set; }
    }
}