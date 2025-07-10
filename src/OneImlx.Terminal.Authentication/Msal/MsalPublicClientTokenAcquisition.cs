/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace OneImlx.Terminal.Authentication.Msal
{
    /// <summary>
    /// The default <see cref="ITokenAcquisition"/> MSAL implementation using the <see cref="IPublicClientApplication"/>.
    /// </summary>
    public class MsalPublicClientTokenAcquisition : ITokenAcquisition
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="publicClientApplication">The public client.</param>
        /// <param name="logger"></param>
        public MsalPublicClientTokenAcquisition(IPublicClientApplication publicClientApplication, ILogger<MsalPublicClientTokenAcquisition> logger)
        {
            this.publicClientApplication = publicClientApplication;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<TokenAcquisitionResult> AcquireTokenInteractiveAsync(TokenAcquisitionInteractiveInput input)
        {
            var accounts = await publicClientApplication.GetAccountsAsync();
            var firstAccount = accounts.FirstOrDefault();

            var result = await publicClientApplication.AcquireTokenInteractive(input.Scopes)
                                                      .WithAccount(firstAccount)
                                                      .WithPrompt(Prompt.SelectAccount)
                                                      .ExecuteAsync();

            return new TokenAcquisitionResult
            {
                AccessToken = result.AccessToken,
                IdToken = result.IdToken,
                Claims = result.ClaimsPrincipal?.Claims
            };
        }

        /// <inheritdoc/>
        public async Task<TokenAcquisitionResult> AcquireTokenSilentAsync(TokenAcquisitionSilentInput input)
        {
            IAccount? account = input.Identity?.Raw as IAccount;
            var result = await publicClientApplication.AcquireTokenSilent(input.Scopes, account).ExecuteAsync();

            return new TokenAcquisitionResult
            {
                AccessToken = result.AccessToken,
                IdToken = result.IdToken,
                Claims = result.ClaimsPrincipal?.Claims
            };
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TokenAcquisitionIdentity>> GetIdentitiesAsync(string? userFlow = null)
        {
            var accounts = userFlow == null
                ? await publicClientApplication.GetAccountsAsync()
                : await publicClientApplication.GetAccountsAsync(userFlow);

            return accounts.Select(a => new TokenAcquisitionIdentity { Raw = a });
        }

        private readonly ILogger<MsalPublicClientTokenAcquisition> logger;
        private readonly IPublicClientApplication publicClientApplication;
    }
}
