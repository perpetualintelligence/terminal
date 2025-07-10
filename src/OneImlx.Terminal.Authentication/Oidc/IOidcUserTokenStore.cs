/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Security.Claims;
using System.Threading.Tasks;
using OneImlx.Terminal.Authentication;

namespace OneImlx.Terminal.Authentication.Oidc
{
    /// <summary>
    /// Provides methods to store, retrieve, and clear OIDC tokens associated with a user.
    /// </summary>
    public interface IOidcUserTokenStore
    {
        /// <summary>
        /// Clears the stored token for the specified user.
        /// </summary>
        Task ClearTokenAsync(ClaimsPrincipal user);

        /// <summary>
        /// Retrieves the token for the specified user.
        /// </summary>
        Task<TokenAcquisitionResult?> GetTokenAsync(ClaimsPrincipal user);

        /// <summary>
        /// Stores the token for the specified user.
        /// </summary>
        Task StoreTokenAsync(ClaimsPrincipal user, TokenAcquisitionResult token);
    }
}
