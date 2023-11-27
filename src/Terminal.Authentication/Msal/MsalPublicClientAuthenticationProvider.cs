/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.Kiota.Abstractions;
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
    /// Provides authentication for HTTP requests using the MSAL (Microsoft Authentication Library).
    /// Handles both silent and interactive token acquisitions based on the provided scopes and account information.
    /// </summary>
    /// <seealso cref="IPublicClientApplication"/>
    public class MsalPublicClientAuthenticationProvider : IAuthenticationProvider
    {
        private readonly IMsalTokenAcquisition msalTokenAcquisition;
        private readonly IEnumerable<string>? defaultScopes;
        private readonly ILogger<MsalPublicClientAuthenticationProvider> logger;

        /// <summary>
        /// Initializes a new instance of the MsalPublicClientAuthenticationProvider class.
        /// </summary>
        /// <param name="msalTokenAcquisition">The MSAL token acquisition service.</param>
        /// <param name="logger">The logger for logging messages.</param>
        /// <param name="defaultScopes">Optional. The default scopes to be used for token acquisition.</param>
        /// <exception cref="ArgumentNullException">Thrown if msalTokenAcquisition or logger is null.</exception>
        public MsalPublicClientAuthenticationProvider(
            IMsalTokenAcquisition msalTokenAcquisition,
            ILogger<MsalPublicClientAuthenticationProvider> logger,
            IEnumerable<string>? defaultScopes = null
        )
        {
            this.msalTokenAcquisition = msalTokenAcquisition ?? throw new ArgumentNullException(nameof(msalTokenAcquisition));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.defaultScopes = defaultScopes;
        }

        /// <summary>
        /// Authenticates an HTTP request by acquiring an access token and setting it in the request's Authorization header.
        /// </summary>
        /// <param name="request">The HTTP request to authenticate.</param>
        /// <param name="additionalAuthenticationContext">Optional. Additional authentication context that may contain extra scopes or other information.</param>
        /// <param name="cancellationToken">Optional. The cancellation token to cancel the asynchronous operation.</param>
        /// <exception cref="TerminalException">Thrown if the URI is not allowed by the AllowedHostsValidator or if token acquisition fails.</exception>
        /// <remarks>
        /// This method supports additional scopes provided either as an <see cref="IEnumerable{T}"/> or a single string with scopes separated by spaces.
        /// The scopes are expected to be provided in the additionalAuthenticationContext dictionary with the key <c>scopes</c>.
        /// </remarks>
        public async Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

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

            AuthenticationResult result;
            try
            {
                result = await msalTokenAcquisition.AcquireTokenSilentAsync(scopes, account);
                logger.LogInformation("Acquired token silently. scopes={0}", scopes);
            }
            catch (MsalUiRequiredException ex)
            {
                logger.LogError(ex, "Acquiring token interactively. scopes={0}", scopes);
                result = await msalTokenAcquisition.AcquireTokenInteractiveAsync(scopes);
                logger.LogInformation("Acquired token interactively. scopes={0}", scopes);
            }

            // Fix: Ensure result is not null before accessing AccessToken
            if (result != null)
            {
                request.Headers["Authorization"] = new List<string> { $"Bearer {result.AccessToken}" };
                logger.LogInformation("Bearer token set in the request header. uri={0}", request.URI);
            }
            else
            {
                throw new TerminalException(TerminalErrors.MissingIdentity, "Failed to acquire an access token.");
            }
        }
    }
}