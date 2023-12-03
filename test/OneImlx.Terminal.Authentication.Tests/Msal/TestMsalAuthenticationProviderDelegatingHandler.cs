/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions.Authentication;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Authentication.Msal
{
    public class TestMsalAuthenticationProviderDelegatingHandler : MsalAuthenticationProviderDelegatingHandler
    {
        public bool PreflightAsyncCalled { get; private set; } = false;

        public TestMsalAuthenticationProviderDelegatingHandler(IAuthenticationProvider authenticationProvider, ILogger<MsalAuthenticationProviderDelegatingHandler> logger)
            : base(authenticationProvider, logger)
        {
        }

        protected override async Task PreflightAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            PreflightAsyncCalled = true;
            await base.PreflightAsync(request, cancellationToken);
        }
    }
}