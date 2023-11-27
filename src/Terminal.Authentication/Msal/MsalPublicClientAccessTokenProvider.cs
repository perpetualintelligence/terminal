/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.Kiota.Abstractions.Authentication;
using PerpetualIntelligence.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Authentication.Msal
{
    /// <summary>
    /// Provides access tokens using the MSAL (Microsoft Authentication Library) and <see cref="IPublicClientApplication"/>.
    /// </summary>
    /// <seealso cref="IPublicClientApplication"/>
    /// <see cref="IMsalTokenAcquisition"/>
    public class MsalPublicClientAccessTokenProvider : IAccessTokenProvider
    {
        private readonly IMsalTokenAcquisition msalTokenAcquisition;
        private readonly IEnumerable<string>? defaultScopes;
        private readonly IEnumerable<string>? validHosts;
        private readonly ILogger<MsalPublicClientAccessTokenProvider> logger;

        /// <summary>
        /// Initializes a new instance of the MsalPublicClientAccessTokenProvider class.
        /// </summary>
        /// <param name="msalTokenAcquisition">The MSAL token acquisition service.</param>
        /// <param name="logger">The logger for logging messages.</param>
        /// <param name="defaultScopes">Optional. The default scopes to be used for token acquisition.</param>
        /// <param name="validHosts">Optional. The collection of valid hosts for which the token can be acquired.</param>
        /// <exception cref="ArgumentNullException">Thrown if msalTokenAcquisition or logger is null.</exception>
        public MsalPublicClientAccessTokenProvider(
            IMsalTokenAcquisition msalTokenAcquisition,
            ILogger<MsalPublicClientAccessTokenProvider> logger,
            IEnumerable<string>? defaultScopes = null,
            IEnumerable<string>? validHosts = null)
        {
            this.msalTokenAcquisition = msalTokenAcquisition ?? throw new ArgumentNullException(nameof(msalTokenAcquisition));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.defaultScopes = defaultScopes;
            this.validHosts = validHosts;
        }

        /// <summary>
        /// Gets the validator that ensures the authorization request is for an allowed host.
        /// </summary>
        public AllowedHostsValidator AllowedHostsValidator
        {
            get
            {
                return new AllowedHostsValidator(validHosts);
            }
        }

        /// <summary>
        /// Asynchronously gets an authorization token for the specified URI.
        /// </summary>
        /// <param name="uri">The URI for which the authorization token is required.</param>
        /// <param name="additionalAuthenticationContext">Optional. Additional authentication context that may contain extra scopes or other information.</param>
        /// <param name="cancellationToken">Optional. The cancellation token to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation, containing the authorization token.</returns>
        /// <remarks>
        /// This method supports additional scopes provided either as an <see cref="IEnumerable{T}"/> or a single string with scopes separated by spaces.
        /// The scopes are expected to be provided in the additionalAuthenticationContext dictionary with the key <c>scopes</c>.
        /// </remarks>
        /// <exception cref="TerminalException">Thrown if the URI is not allowed by the AllowedHostsValidator.</exception>
        public async Task<string> GetAuthorizationTokenAsync(Uri uri, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            logger.LogInformation("Acquire authorization token. host={0}", uri);
            if (!AllowedHostsValidator.IsUrlHostValid(uri))
            {
                throw new TerminalException(TerminalErrors.UnauthorizedAccess, "The host is not authorized. uri={0}", uri);
            }

            List<string> scopes = new(defaultScopes ?? Enumerable.Empty<string>());
            if (additionalAuthenticationContext != null && additionalAuthenticationContext.TryGetValue("scopes", out var additionalScopesObj))
            {
                if (additionalScopesObj is IEnumerable<string> additionalScopesEnumerable)
                {
                    scopes.AddRange(additionalScopesEnumerable);
                }
                else if (additionalScopesObj is string additionalScopesString)
                {
                    scopes.AddRange(additionalScopesString.SplitBySpace());
                }
                else
                {
                    throw new TerminalException(TerminalErrors.InvalidRequest, "Additional scopes are not in an expected format. They should be either `IEnumerable<string>` or a space-separated string.");
                }
            }

            IEnumerable<IAccount> accounts = await msalTokenAcquisition.GetAccountsAsync(userFlow: null);
            IAccount account = accounts.FirstOrDefault() ?? throw new TerminalException(TerminalErrors.MissingIdentity, "The MSAL account is missing in the request.");
            logger.LogDebug("Acquired accounts, using the first account. environment={0} account={1}", account.Environment, account.Username);

            // Acquire the token silently
            AuthenticationResult? result = await msalTokenAcquisition.AcquireTokenSilentAsync(scopes, account);
            logger.LogInformation("Acquired token silently. scopes={0}", scopes);

            // Ensure result is not null before accessing AccessToken
            if (result != null)
            {
                return result.AccessToken;
            }
            else
            {
                throw new TerminalException(TerminalErrors.MissingIdentity, "Failed to acquire an access token.");
            }
        }
    }
}