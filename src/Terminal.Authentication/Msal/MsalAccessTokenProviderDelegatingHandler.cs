/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions.Authentication;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Authentication.Msal
{
    /// <summary>
    /// Delegating handler that adds an access token to the request headers using an <see cref="IAccessTokenProvider"/>.
    /// </summary>
    public class MsalAccessTokenProviderDelegatingHandler : DelegatingHandler
    {
        private readonly IAccessTokenProvider accessTokenProvider;
        private readonly ILogger<MsalAccessTokenProviderDelegatingHandler> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MsalAccessTokenProviderDelegatingHandler"/> class.
        /// </summary>
        /// <param name="accessTokenProvider">The access token provider.</param>
        /// <param name="logger">The logger.</param>
        public MsalAccessTokenProviderDelegatingHandler(IAccessTokenProvider accessTokenProvider, ILogger<MsalAccessTokenProviderDelegatingHandler> logger)
        {
            this.accessTokenProvider = accessTokenProvider;
            this.logger = logger;
        }

        /// <summary>
        /// Performs preflight processing on the HTTP request message.
        /// </summary>
        /// <param name="request">The HTTP request message to be processed.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <remarks>
        /// This method can be overridden in a derived class to perform custom pre-processing
        /// on the request before it is sent. The default implementation does nothing.
        /// </remarks>
        protected virtual async Task PreflightAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Sends an HTTP request to the inner handler to send to the server as an asynchronous operation.
        /// </summary>
        /// <param name="request">The HTTP request message to send to the server.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>A <see cref="Task{HttpResponseMessage}"/> that represents the asynchronous operation.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Preflight
            await PreflightAsync(request, cancellationToken);

            // Get the access token using the provider.
            Dictionary<string, object>? propertiesDictionary = request.Properties != null ? new Dictionary<string, object>(request.Properties) : null;
            string token = await accessTokenProvider.GetAuthorizationTokenAsync(request.RequestUri, propertiesDictionary, cancellationToken);

            // Ensure token is not null of empty
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new TerminalException(TerminalErrors.UnauthorizedAccess, "The access token is null or empty.");
            }

            // Set the Authorization header with the access token.
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Continue sending the request.
            return await base.SendAsync(request, cancellationToken);
        }
    }
}