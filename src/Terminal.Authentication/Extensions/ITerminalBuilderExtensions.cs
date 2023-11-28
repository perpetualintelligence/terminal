/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Terminal.Hosting;
using System;
using System.Net.Http;
using System.Threading;

namespace PerpetualIntelligence.Terminal.Authentication.Extensions
{
    /// <summary>
    /// </summary>
    public static class ITerminalBuilderExtensions
    {
        /// <summary>
        /// Adds <c>MSAL</c> authentication to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="publicClientApplication"></param>
        /// <param name="name">The HTTP client name.</param>
        /// The HTTP request timeout in milliseconds. Defaults to <c>2</c> minutes or <c>120000</c> milliseconds. We
        /// recommend a timeout of at least a few minutes, to take into account cases where the user is prompted to
        /// change password or perform 2FA.
        /// </param>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        public static ITerminalBuilder AddPublicClientMsal<TDelegatingHandler>(
            this ITerminalBuilder builder,
            IPublicClientApplication publicClientApplication,
            string name
        )
            where TDelegatingHandler : DelegatingHandler
        {
            // We don't change public client per terminal.
            builder.Services.AddSingleton(publicClientApplication);

            // TDelegateHandler cannot be singleton.
            builder.Services.AddScoped<TDelegatingHandler>();

            // Configure HttpClient using TerminalOptions
            builder.Services.AddHttpClient<TDelegatingHandler>(name, (serviceProvider, client) =>
            {
                TerminalOptions terminalOptions = serviceProvider.GetRequiredService<TerminalOptions>();
                AuthenticationOptions authOptions = terminalOptions.Authentication;
                client.BaseAddress = new Uri(authOptions.BaseAddress);
                client.Timeout = authOptions.Timeout ?? Timeout.InfiniteTimeSpan;
            }).AddHttpMessageHandler<TDelegatingHandler>();

            return builder;
        }
    }
}