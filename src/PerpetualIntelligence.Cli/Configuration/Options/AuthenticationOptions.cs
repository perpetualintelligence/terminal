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
        /// The authentication redirect URI.
        /// </summary>
        public string? RedirectUri { get; set; }

        /// <summary>
        /// The authentication scopes.
        /// </summary>
        public string[]? Scopes { get; set; }

        /// <summary>
        /// The authentication tenant identifier.
        /// </summary>
        public string? TenantId { get; set; }
    }
}
