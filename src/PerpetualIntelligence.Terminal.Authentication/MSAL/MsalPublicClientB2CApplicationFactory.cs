/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Identity.Client;
using PerpetualIntelligence.Shared.Authorization;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Authentication.Msal
{
    /// <summary>
    /// The <see cref="IMsalPublicClientApplicationFactory"/> to create <see cref="IPublicClientApplication"/>.
    /// </summary>
    public class MsalPublicClientB2CApplicationFactory : IMsalPublicClientApplicationFactory
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        public MsalPublicClientB2CApplicationFactory(IClientCrossPlatformTokenCache clientCrossPlatformTokenCache)
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
            // https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-net-aad-b2c-considerations
            // This enables the singleton implementation for IClientApplicationFactory to return only 1 client instance.
            // If IClientApplicationFactory is scoped then client will always be null for each new
            // MsalPublicClientApplicationFactory instance.
            if (client == null)
            {
                client = PublicClientApplicationBuilder.Create("1ec3af12-346f-4742-bb29-b23656676177")
                                                       .WithB2CAuthority("https://perpetualintelligenceb2c.b2clogin.com/tfp/perpetualintelligenceb2c.onmicrosoft.com/b2c_1_signinsignup")
                                                       .WithRedirectUri(context.AuthenticationOptions.RedirectUri)
                                                       .WithTenantId(context.AuthenticationOptions.TenantId)
                                                       .Build();

                clientCrossPlatformTokenCache.RegisterCacheAsync(client);
            }

            return Task.FromResult(new MsalPublicClientApplicationFactoryResult(client));
        }

        private readonly IClientCrossPlatformTokenCache clientCrossPlatformTokenCache;
        private IPublicClientApplication? client;
    }
}
