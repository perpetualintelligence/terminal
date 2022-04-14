/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Extensions;

namespace PerpetualIntelligence.Cli.Configuration.Options
{
    /// <summary>
    /// The authentication configuration options.
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
        /// <remarks>The name must match the <see cref="ICliBuilderExtensions.AddAuthentication{TProvider, TAppFactory, TAppCache}(Integration.ICliBuilder, string, string?, int?)"/></remarks>
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
