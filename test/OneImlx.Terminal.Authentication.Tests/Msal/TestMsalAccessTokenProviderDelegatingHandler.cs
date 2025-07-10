﻿/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

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
    public class TestMsalAccessTokenProviderDelegatingHandler(IAccessTokenProvider accessTokenProvider, ILogger<AccessTokenProviderDelegatingHandler> logger) : AccessTokenProviderDelegatingHandler(accessTokenProvider, logger)
    {
        public bool PreflightAsyncCalled { get; private set; } = false;

        protected override async Task PreflightAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            PreflightAsyncCalled = true;
            await base.PreflightAsync(request, cancellationToken);
        }
    }
}
