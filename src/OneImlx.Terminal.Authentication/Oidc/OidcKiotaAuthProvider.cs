/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Duende.IdentityModel.Client;
using Duende.IdentityModel.OidcClient;

namespace OneImlx.Terminal.Authentication.Oidc
{
    /// <summary>
    /// Kiota-compatible authentication provider for OpenID Connect (OIDC) identity providers. This provider supports
    /// both silent and interactive login flows using OIDC session context.
    /// </summary>
    public sealed class OidcKiotaAuthProvider : IAuthenticationProvider, IAccessTokenProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OidcKiotaAuthProvider"/> class.
        /// </summary>
        /// <param name="oidcClient">The configured OIDC client. It must already have a valid IBrowser instance set.</param>
        public OidcKiotaAuthProvider(OidcClient oidcClient)
        {
            this.oidcClient = oidcClient ?? throw new ArgumentNullException(nameof(oidcClient));
        }

        /// <inheritdoc/>
        public AllowedHostsValidator AllowedHostsValidator => new AllowedHostsValidator(new[] { "*" });

        /// <inheritdoc/>
        public async Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object>? context = null, CancellationToken cancellationToken = default)
        {
            string token = await GetAuthorizationTokenAsync(request.URI, context, cancellationToken);
            request.Headers["Authorization"] = [$"Bearer {token}"];
        }

        /// <inheritdoc/>
        public async Task<string> GetAuthorizationTokenAsync(Uri uri, Dictionary<string, object>? context = null, CancellationToken cancellationToken = default)
        {
            // Attempt silent login using prompt=none query parameter manually
            var silentRequest = new LoginRequest
            {
                FrontChannelExtraParameters = new Parameters { { "prompt", "none" } }
            };

            var silentResult = await oidcClient.LoginAsync(silentRequest);
            if (!silentResult.IsError)
            {
                return silentResult.AccessToken;
            }

            // Silent login failed; fallback to interactive login
            var interactiveResult = await oidcClient.LoginAsync(new LoginRequest());
            if (interactiveResult.IsError)
            {
                throw new InvalidOperationException($"OIDC interactive login failed. {interactiveResult.Error}");
            }

            return interactiveResult.AccessToken;
        }

        private readonly OidcClient oidcClient;
        private DateTimeOffset? accessTokenExpiry;
    }
}
