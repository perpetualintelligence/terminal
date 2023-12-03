/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using Microsoft.Kiota.Abstractions.Authentication;
using PerpetualIntelligence.Terminal.Authentication.Msal;
using PerpetualIntelligence.Terminal.Hosting;
using System.Net.Http;

namespace PerpetualIntelligence.Terminal.Authentication.Extensions
{
    /// <summary>
    /// </summary>
    public static class ITerminalBuilderExtensions
    {
        /// <summary>
        /// Adds MSAL authentication using <see cref="IPublicClientApplication"/> to the service collection.
        /// </summary>
        /// <typeparam name="TAuthenticationProvider">The type of the authentication provider.</typeparam>
        /// <typeparam name="TAccessTokenProvider">The type of the access token provider.</typeparam>
        /// <typeparam name="TDelegatingHandler">The type of the custom HTTP delegating handler.</typeparam>
        /// <param name="builder">The terminal builder.</param>
        /// <param name="publicClientApplication">The public client application.</param>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        public static ITerminalBuilder AddMsalAuthentication<TAuthenticationProvider, TAccessTokenProvider, TDelegatingHandler>(
            this ITerminalBuilder builder,
            IPublicClientApplication publicClientApplication
        )
            where TAuthenticationProvider : class, IAuthenticationProvider
            where TAccessTokenProvider : class, IAccessTokenProvider
            where TDelegatingHandler : DelegatingHandler
        {
            // Singleton for the public client application
            builder.Services.AddSingleton(publicClientApplication);

            // Scoped services for token acquisition and providers
            builder.Services.AddScoped<IMsalTokenAcquisition, MsalPublicClientTokenAcquisition>();
            builder.Services.AddScoped<IAuthenticationProvider, TAuthenticationProvider>();
            builder.Services.AddScoped<IAccessTokenProvider, TAccessTokenProvider>();

            // Directly add scoped TDelegatingHandler to support custom Http message handler.
            builder.Services.AddScoped<TDelegatingHandler>();

            return builder;
        }
    }
}