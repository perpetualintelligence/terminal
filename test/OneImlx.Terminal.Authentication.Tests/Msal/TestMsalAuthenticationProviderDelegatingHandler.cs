/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions.Authentication;

namespace OneImlx.Terminal.Authentication.Msal
{
    public class TestMsalAuthenticationProviderDelegatingHandler(IAuthenticationProvider authenticationProvider, ILogger<AuthenticationProviderDelegatingHandler> logger) : AuthenticationProviderDelegatingHandler(authenticationProvider, logger)
    {
        public bool PreflightAsyncCalled { get; private set; } = false;

        protected override async Task PreflightAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            PreflightAsyncCalled = true;
            await base.PreflightAsync(request, cancellationToken);
        }
    }
}
