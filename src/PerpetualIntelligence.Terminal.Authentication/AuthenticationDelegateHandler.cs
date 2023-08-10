/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Graph;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Authentication
{
    /// <summary>
    /// Submits an authentication request asynchronously.
    /// </summary>
    public class AuthenticationDelegateHandler : DelegatingHandler
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="authenticationProvider"></param>
        public AuthenticationDelegateHandler(IAuthenticationProvider authenticationProvider)
        {
            _authenticationProvider = authenticationProvider;
        }

        /// <summary>
        /// Sends a request asynchronously.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await _authenticationProvider.AuthenticateRequestAsync(request);
            return await base.SendAsync(request, cancellationToken);
        }

        private readonly IAuthenticationProvider _authenticationProvider;
    }
}
