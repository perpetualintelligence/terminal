/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Identity.Client;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Authentication
{
    /// <summary>
    /// An abstraction to create <see cref="IClientApplicationBase"/>.
    /// </summary>
    public interface IMsalPublicClientApplicationFactory
    {
        /// <summary>
        /// Creates an instance of <see cref="IClientApplicationBase"/> asynchronously.
        /// </summary>
        /// <returns>An instance of <see cref="IClientApplicationBase"/>.</returns>
        public Task<MsalPublicClientApplicationFactoryResult> CreateAsync(MsalPublicClientApplicationFactoryContext context);
    }
}
