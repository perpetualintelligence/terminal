/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Configuration.Options;
using System;

namespace PerpetualIntelligence.Terminal.Authentication.Msal
{
    /// <summary>
    /// The <see cref="IMsalPublicClientApplicationFactory"/> context.
    /// </summary>
    public class MsalPublicClientApplicationFactoryContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="authenticationOptions">The authentication options.</param>
        public MsalPublicClientApplicationFactoryContext(AuthenticationOptions authenticationOptions)
        {
            AuthenticationOptions = authenticationOptions ?? throw new ArgumentNullException(nameof(authenticationOptions));
        }

        /// <summary>
        /// The authentication options.
        /// </summary>
        public AuthenticationOptions AuthenticationOptions { get; }
    }
}
