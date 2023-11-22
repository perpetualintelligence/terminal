﻿/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace PerpetualIntelligence.Terminal.Configuration.Options
{
    /// <summary>
    /// The authentication configuration options. Reserved for future use.
    /// </summary>
    public class AuthenticationOptions
    {
        /// <summary>
        /// The authorized application or the client identifier.
        /// </summary>
        public string? ApplicationId { get; set; }

        /// <summary>
        /// The authentication authority.
        /// </summary>
        public string? Authority { get; set; }

        /// <summary>
        /// The HTTP client name.
        /// </summary>
        public string? HttpClientName { get; set; }

        /// <summary>
        /// The authentication redirect URI. Defaults to <c>http://localhost</c>.
        /// </summary>
        /// <seealso href="https://github.com/AzureAD/microsoft-authentication-extensions-for-dotnet/wiki/Cross-platform-Token-Cache"/>
        /// <seealso href="https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-net-token-cache-serialization?tabs=desktop"/>
        public string RedirectUri { get; set; } = "http://localhost";

        /// <summary>
        /// The authentication scopes.
        /// </summary>
        public string[]? Scopes { get; set; }

        /// <summary>
        /// The authentication tenant identifier.
        /// </summary>
        public string? TenantId { get; set; }

        /// <summary>
        /// Specifies if the public client application should used an embedded web browser or the system default browser.
        /// </summary>
        public bool? UseEmbeddedView { get; set; }
    }
}