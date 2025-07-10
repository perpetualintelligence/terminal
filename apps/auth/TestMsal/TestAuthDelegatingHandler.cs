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
using OneImlx.Terminal.Authentication;

namespace OneImlx.Terminal.Apps.TestAuth
{
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="authenticationProvider"></param>
    public class TestAuthDelegatingHandler(IAuthenticationProvider authenticationProvider, ILogger<TestAuthDelegatingHandler> logger) : AuthenticationProviderDelegatingHandler(authenticationProvider, logger)
    {
        protected override Task PreflightAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return base.PreflightAsync(request, cancellationToken);

            // Add custom logic here, e.g.: adding preflight header
        }
    }
}
