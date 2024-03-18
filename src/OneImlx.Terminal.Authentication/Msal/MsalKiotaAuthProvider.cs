/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using OneImlx.Shared.Extensions;
using OneImlx.Terminal.Configuration.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Authentication.Msal
{
    /// <summary>
    /// The <c>Kiota</c> authentication and authorization provider for <c>MSAL</c> identity platform.
    /// </summary>
    public sealed class MsalKiotaAuthProvider : IAuthenticationProvider, IAccessTokenProvider
    {
        private readonly TerminalOptions terminalOptions;
        private readonly IMsalTokenAcquisition msalTokenAcquisition;
        private readonly ILogger<MsalKiotaAuthProvider> logger;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="terminalOptions">The terminal options.</param>
        /// <param name="msalTokenAcquisition">The MSAL token acquisition.</param>
        /// <param name="logger">The logger.</param>
        public MsalKiotaAuthProvider(
            TerminalOptions terminalOptions,
            IMsalTokenAcquisition msalTokenAcquisition,
            ILogger<MsalKiotaAuthProvider> logger)
        {
            this.terminalOptions = terminalOptions ?? throw new ArgumentNullException(nameof(terminalOptions));
            this.msalTokenAcquisition = msalTokenAcquisition ?? throw new ArgumentNullException(nameof(msalTokenAcquisition));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the validator that ensures the authorization request is for an allowed host.
        /// </summary>
        public AllowedHostsValidator AllowedHostsValidator
        {
            get
            {
                return new AllowedHostsValidator(terminalOptions.Authentication.ValidHosts);
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

            // Verify host
            logger.LogDebug("Acquire authorization token. host={0}", uri);
            if (!AllowedHostsValidator.IsUrlHostValid(uri))
            {
                throw new TerminalException(TerminalErrors.UnauthorizedAccess, "The host is not authorized. uri={0}", uri);
            }

            // Get default and additional scopes.
            List<string> scopes = ExtractScopesFromContext(additionalAuthenticationContext);

            // Get the first account
            IEnumerable<IAccount> accounts = await msalTokenAcquisition.GetAccountsAsync(terminalOptions.Authentication.UserFlow);
            IAccount? account = accounts.FirstOrDefault();
            if (account == null)
            {
                logger.LogDebug("Failed to acquire accounts. user_flow={1}", terminalOptions.Authentication.UserFlow);
            }
            else
            {
                logger.LogDebug("Acquired accounts, using the first account. environment={0} account={1}", account.Environment, account.Username);
            }

            AuthenticationResult? result;
            try
            {
                result = await msalTokenAcquisition.AcquireTokenSilentAsync(scopes, account!);
                logger.LogInformation("Acquired token silently. scopes={0}", scopes.JoinBySpace());
            }
            catch (MsalUiRequiredException ex)
            {
                logger.LogDebug("Acquiring token interactively. info={0}", ex.Message);
                result = await msalTokenAcquisition.AcquireTokenInteractiveAsync(scopes);
                logger.LogInformation("Acquired token interactively. scopes={0}", scopes.JoinBySpace());
            }

            return result.AccessToken;
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
            string accessToken = await GetAuthorizationTokenAsync(request.URI, additionalAuthenticationContext, cancellationToken);
            request.Headers["Authorization"] = new List<string> { $"Bearer {accessToken}" };
            logger.LogInformation("Bearer token set. request_uri={0}", request.URI);
        }

        private List<string> ExtractScopesFromContext(Dictionary<string, object>? additionalAuthenticationContext)
        {
            List<string> scopes = new(terminalOptions.Authentication.DefaultScopes ?? Enumerable.Empty<string>());
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

            logger.LogDebug("Acquired authentication scopes. scopes={0}", scopes.JoinBySpace());
            return scopes;
        }
    }
}