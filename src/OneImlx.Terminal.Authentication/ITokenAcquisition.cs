/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Authentication
{
    /// <summary>
    /// An abstraction to acquire authentication tokens from identity providers. Supports both silent and interactive
    /// login flows for terminal-based applications.
    /// </summary>
    public interface ITokenAcquisition
    {
        /// <summary>
        /// An abstraction to initiate an interactive login flow using the specified input.
        /// </summary>
        /// <param name="input">The input parameters for interactive token acquisition.</param>
        /// <returns>The result containing access tokens and optionally an ID token or claims.</returns>
        Task<TokenAcquisitionResult> AcquireTokenInteractiveAsync(TokenAcquisitionInteractiveInput input);

        /// <summary>
        /// An abstraction to attempt silent token acquisition using a cached identity reference.
        /// </summary>
        /// <param name="input">The input parameters for silent token acquisition.</param>
        /// <returns>The result containing access tokens and optionally an ID token or claims.</returns>
        Task<TokenAcquisitionResult> AcquireTokenSilentAsync(TokenAcquisitionSilentInput input);

        /// <summary>
        /// An abstraction to retrieve all known user identities supported by the identity provider.
        /// </summary>
        /// <param name="userFlow">Optional user flow or policy name (e.g., for B2C scenarios).</param>
        /// <returns>A list of available identities such as user accounts or sessions.</returns>
        Task<IEnumerable<TokenAcquisitionIdentity>> GetIdentitiesAsync(string? userFlow = null);
    }
}
