/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Identity.Client;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Authentication.Msal
{
    /// <summary>
    /// Represents an abstraction for acquiring MSAL application instances.
    /// </summary>
    /// <typeparam name="TContext">The type of context used for creating the client application.</typeparam>
    /// <typeparam name="TClient">The type of the <see cref="IClientApplicationBase"/>.</typeparam>
    public interface IMsalApplicationAcquisition<TContext, TClient> where TContext : class where TClient : class, IClientApplicationBase
    {
        /// <summary>
        /// Acquires an instance of <see cref="IClientApplicationBase"/> based on the provided context.
        /// </summary>
        /// <param name="context">The context used for creating the client application.</param>
        /// <returns>An instance of <see cref="IClientApplicationBase"/>.</returns>
        Task<TClient> AcquireClientApplicationAsync(TContext context);
    }
}