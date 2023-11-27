/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Identity.Client;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Authentication.Msal
{
    /// <summary>
    /// Defines an interface for acquiring tokens using MSAL.
    /// </summary>
    public interface IMsalTokenAcquisition
    {
        /// <summary>
        /// Asynchronously gets a list of IAccount.
        /// </summary>
        /// <param name="userFlow">The authentication user flow.</param>
        /// <returns>A task that represents the asynchronous operation, containing a list of accounts.</returns>
        Task<IEnumerable<IAccount>> GetAccountsAsync(string? userFlow = null);

        /// <summary>
        /// Acquires a token silently for the specified scopes and account.
        /// </summary>
        /// <param name="scopes">The scopes for which the token is requested.</param>
        /// <param name="account">The account for which the token is requested.</param>
        /// <returns>A task that represents the asynchronous operation and returns the authentication result.</returns>
        Task<AuthenticationResult> AcquireTokenSilentAsync(IEnumerable<string> scopes, IAccount account);

        /// <summary>
        /// Acquires a token interactively for the specified scopes.
        /// </summary>
        /// <param name="scopes">The scopes for which the token is requested.</param>
        /// <returns>A task that represents the asynchronous operation and returns the authentication result.</returns>
        Task<AuthenticationResult> AcquireTokenInteractiveAsync(IEnumerable<string> scopes);
    }
}