/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using PerpetualIntelligence.Terminal.Configuration.Options;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Authentication.Msal
{
    /// <summary>
    /// The <see cref="IMsalApplicationAcquisition"/> to create <see cref="IPublicClientApplication"/> for Azure AD.
    /// </summary>
    public class MsalPublicClientAzureADApplicationAcquisition : IMsalApplicationAcquisition<NoAcquisitionContext, IPublicClientApplication>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MsalPublicClientAzureADApplicationAcquisition"/> class.
        /// </summary>
        /// <param name="msalTokenCache">The MSAL token cache.</param>
        /// <param name="terminalOptions">The terminal options.</param>
        /// <param name="logger">The logger.</param>
        public MsalPublicClientAzureADApplicationAcquisition(
            IMsalTokenCache msalTokenCache,
            TerminalOptions terminalOptions,
            ILogger<MsalPublicClientAzureADApplicationAcquisition> logger)
        {
            this.msalTokenCache = msalTokenCache;
            this.terminalOptions = terminalOptions;
            this.logger = logger;
        }

        /// <summary>
        /// Acquires an instance of <see cref="IPublicClientApplication"/> for Azure AD authentication.
        /// </summary>
        /// <param name="context">The authentication acquisition context (Not used in this implementation).</param>
        /// <returns>An instance of <see cref="IPublicClientApplication"/>.</returns>
        public async Task<IPublicClientApplication> AcquireClientApplicationAsync(NoAcquisitionContext context)
        {
            IPublicClientApplication clientApp = PublicClientApplicationBuilder.Create(terminalOptions.Authentication.ApplicationId)
                                                   .WithAuthority(terminalOptions.Authentication.Authority)
                                                   .WithTenantId(terminalOptions.Authentication.TenantId)
                                                   .WithRedirectUri(terminalOptions.Authentication.RedirectUri)
                                                   .Build();

            await msalTokenCache.RegisterCacheAsync(clientApp);

            return clientApp;
        }

        private readonly IMsalTokenCache msalTokenCache;
        private readonly TerminalOptions terminalOptions;
        private readonly ILogger<MsalPublicClientAzureADApplicationAcquisition> logger;
    }
}