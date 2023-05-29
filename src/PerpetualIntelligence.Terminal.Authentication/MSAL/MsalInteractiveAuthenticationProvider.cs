/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Graph;
using Microsoft.Identity.Client;
using PerpetualIntelligence.Terminal.Configuration.Options;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Authentication.Msal
{
    /// <summary>
    /// The <c>MSAL</c> authentication provider.
    /// </summary>
    public class MsalInteractiveAuthenticationProvider : IAuthenticationProvider
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="clientApplicationFactory"></param>
        /// <param name="cliOptions"></param>
        public MsalInteractiveAuthenticationProvider(IMsalPublicClientApplicationFactory clientApplicationFactory, CliOptions cliOptions)
        {
            this.clientApplicationFactory = clientApplicationFactory;
            this.cliOptions = cliOptions;
        }

        /// <summary>
        /// Authenticates the request.
        /// </summary>
        /// <param name="request">The request to authenticate.</param>
        public async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            var accessToken = await GetTokenAsync();
            request.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
        }

        /// <summary>
        /// Gets an <c>access_token</c> to run a protected command.
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetTokenAsync()
        {            
            var clientAppResult = await clientApplicationFactory.CreateAsync(new MsalPublicClientApplicationFactoryContext(cliOptions.Authentication));
            PublicClientApplication publicClientApplication = clientAppResult.As<PublicClientApplication>();

            // https://github.com/Azure-Samples/ms-identity-dotnet-desktop-tutorial/tree/master/2-TokenCache
            var accounts = await publicClientApplication.GetAccountsAsync();

            // https://docs.microsoft.com/en-us/azure/active-directory/develop/scenario-desktop-acquire-token-interactive?tabs=dotnet
            AuthenticationResult result;
            try
            {
                result = await publicClientApplication.AcquireTokenSilent(cliOptions.Authentication.Scopes, accounts.FirstOrDefault())
                                                      .ExecuteAsync();
            }
            catch (MsalUiRequiredException)
            {
                result = await publicClientApplication.AcquireTokenInteractive(cliOptions.Authentication.Scopes)
                                                      .WithUseEmbeddedWebView(cliOptions.Authentication.UseEmbeddedView.GetValueOrDefault())
                                                      .ExecuteAsync();
            }

            return result.AccessToken;
        }

        private readonly CliOptions cliOptions;
        private IMsalPublicClientApplicationFactory clientApplicationFactory;
    }
}
