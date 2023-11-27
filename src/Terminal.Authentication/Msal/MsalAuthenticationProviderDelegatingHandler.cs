﻿/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Authentication.Msal
{
    /// <summary>
    /// Delegating handler that authenticates an HTTP request using an <see cref="IAuthenticationProvider"/>.
    /// </summary>
    public class MsalAuthenticationProviderDelegatingHandler : DelegatingHandler
    {
        private readonly IAuthenticationProvider authenticationProvider;
        private readonly ILogger<MsalAuthenticationProviderDelegatingHandler> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MsalAuthenticationProviderDelegatingHandler"/> class.
        /// </summary>
        /// <param name="authenticationProvider">The authentication provider.</param>
        /// <param name="logger"></param>
        public MsalAuthenticationProviderDelegatingHandler(IAuthenticationProvider authenticationProvider, ILogger<MsalAuthenticationProviderDelegatingHandler> logger)
        {
            this.authenticationProvider = authenticationProvider ?? throw new ArgumentNullException(nameof(authenticationProvider));
            this.logger = logger;
        }

        /// <summary>
        /// Sends an HTTP request to the inner handler to send to the server as an asynchronous operation.
        /// </summary>
        /// <param name="request">The HTTP request message to send to the server.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A <see cref="Task{HttpResponseMessage}"/> that represents the asynchronous operation.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Map the HttpRequestMessage method to the appropriate Method enum value.
            var method = MapHttpMethod(request.Method);

            // Authenticate the request using the authentication provider.
            Dictionary<string, object>? propertiesDictionary = request.Properties != null ? new Dictionary<string, object>(request.Properties) : new Dictionary<string, object>();
            var requestInformation = new RequestInformation(method, request.RequestUri.ToString(), propertiesDictionary);
            await authenticationProvider.AuthenticateRequestAsync(requestInformation, propertiesDictionary, cancellationToken);

            // Ensure the authentication provider authenticated the request.
            if (!requestInformation.Headers.ContainsKey("Authorization"))
            {
                throw new TerminalException(TerminalErrors.UnauthorizedAccess, "The authentication provider missed an authorization header. provider={0}", authenticationProvider.GetType().Name);
            }

            // Ensure token is not null or empty
            string token = requestInformation.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new TerminalException(TerminalErrors.UnauthorizedAccess, "The access_token is null or empty.");
            }

            // Set the Authorization header with the access token.
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", requestInformation.Headers["Authorization"].FirstOrDefault());

            // Continue sending the request.
            return await base.SendAsync(request, cancellationToken);
        }

        // Helper method to map HttpMethod to Method enum value.
        private Method MapHttpMethod(HttpMethod httpMethod)
        {
            switch (httpMethod.Method.ToUpper())
            {
                case "GET":
                    return Method.GET;

                case "POST":
                    return Method.POST;

                case "PUT":
                    return Method.PUT;

                case "DELETE":
                    return Method.DELETE;

                default:
                    throw new TerminalException(TerminalErrors.InvalidRequest, "The Http method is not supported. method={0}", httpMethod);
            }
        }
    }
}