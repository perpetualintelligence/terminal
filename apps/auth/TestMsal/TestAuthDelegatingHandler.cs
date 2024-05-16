using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions.Authentication;
using OneImlx.Terminal.Authentication.Msal;

namespace OneImlx.Terminal.Apps.TestAuth
{
    public class TestAuthDelegatingHandler : MsalAuthenticationProviderDelegatingHandler
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="authenticationProvider"></param>
        public TestAuthDelegatingHandler(IAuthenticationProvider authenticationProvider, ILogger<TestAuthDelegatingHandler> logger)
        : base(authenticationProvider, logger)
        {
        }

        protected override Task PreflightAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return base.PreflightAsync(request, cancellationToken);

            // Add custom logic here, e.g.: adding preflight header
        }
    }
}
