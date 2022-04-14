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
    /// The <see cref="IMsalPublicClientApplicationFactory"/> to create <see cref="IPublicClientApplication"/>.
    /// </summary>
    public class MsalPublicClientApplicationFactory : IMsalPublicClientApplicationFactory
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        public MsalPublicClientApplicationFactory(IClientCrossPlatformTokenCache clientCrossPlatformTokenCache)
        {
            this.clientCrossPlatformTokenCache = clientCrossPlatformTokenCache;
        }

        /// <summary>
        /// Create a new <see cref="IPublicClientApplication"/> for authentication.
        /// </summary>
        /// <param name="context">The authentication factory context.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public Task<MsalPublicClientApplicationFactoryResult> CreateAsync(MsalPublicClientApplicationFactoryContext context)
        {
            // This enables the singleton implementation for IClientApplicationFactory to return only 1 client instance.
            // If IClientApplicationFactory is scoped then client will always be null for each new
            // MsalPublicClientApplicationFactory instance.
            if (client == null)
            {
                client = PublicClientApplicationBuilder.Create(context.AuthenticationOptions.ApplicationId)
                                                       .WithAuthority(context.AuthenticationOptions.Authority)
                                                       .WithTenantId(context.AuthenticationOptions.TenantId)
                                                       .WithRedirectUri(context.AuthenticationOptions.RedirectUri)
                                                       .Build();

                clientCrossPlatformTokenCache.RegisterCacheAsync(client);
            }

            return Task.FromResult(new MsalPublicClientApplicationFactoryResult(client));
        }

        private readonly IClientCrossPlatformTokenCache clientCrossPlatformTokenCache;
        private IPublicClientApplication? client;
    }
}
