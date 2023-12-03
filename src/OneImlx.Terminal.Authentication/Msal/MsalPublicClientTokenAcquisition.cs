/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Authentication.Msal
{
    /// <summary>
    /// The default <see cref="IMsalTokenAcquisition"/> implementation using the <see cref="IPublicClientApplication"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="MsalPublicClientTokenAcquisition"/> is responsible for handling token acquisition from the Microsoft Identity platform.
    /// </remarks>
    public class MsalPublicClientTokenAcquisition : IMsalTokenAcquisition
    {
        private readonly IPublicClientApplication publicClientApplication;
        private readonly ILogger<MsalPublicClientTokenAcquisition> logger;

        /// <summary>
        /// Initializes a new instance of the MsalPublicClientTokenAcquisition class.
        /// Requires an instance of IPublicClientApplication, which is the entry point for using MSAL,
        /// and a logger for logging activities and errors.
        /// </summary>
        /// <param name="publicClientApplication">The MSAL public client application instance.</param>
        /// <param name="logger">The logger to use for logging information and errors.</param>
        public MsalPublicClientTokenAcquisition(IPublicClientApplication publicClientApplication, ILogger<MsalPublicClientTokenAcquisition> logger)
        {
            this.publicClientApplication = publicClientApplication;
            this.logger = logger;
        }

        /// <summary>
        /// Attempts to acquire a token silently for the specified scopes and account.
        /// </summary>
        /// <param name="scopes">The scopes for which the token is requested.</param>
        /// <param name="account">The account for which the token is requested.</param>
        /// <returns>A task representing the asynchronous operation and returning the authentication result.</returns>
        public Task<AuthenticationResult> AcquireTokenSilentAsync(IEnumerable<string> scopes, IAccount account)
        {
            return publicClientApplication.AcquireTokenSilent(scopes, account).ExecuteAsync();
        }

        /// <summary>
        /// Attempts to acquire a token interactively for the specified scopes.
        /// </summary>
        /// <param name="scopes">The scopes for which the token is requested.</param>
        /// <returns>A task representing the asynchronous operation and returning the authentication result.</returns>
        public async Task<AuthenticationResult> AcquireTokenInteractiveAsync(IEnumerable<string> scopes)
        {
            IEnumerable<IAccount> accounts = await publicClientApplication.GetAccountsAsync();
            var firstAccount = accounts.FirstOrDefault();

            return await publicClientApplication.AcquireTokenInteractive(scopes)
                                                .WithAccount(firstAccount)
                                                .WithPrompt(Prompt.SelectAccount)
                                                .ExecuteAsync();
        }

        /// <summary>
        /// Returns all the available accounts in the user token cache for the application.
        /// </summary>
        public Task<IEnumerable<IAccount>> GetAccountsAsync(string? userFlow = null)
        {
            if (userFlow == null)
            {
                return publicClientApplication.GetAccountsAsync();
            }
            else
            {
                return publicClientApplication.GetAccountsAsync(userFlow);
            }
        }
    }
}