/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using PerpetualIntelligence.Cli.Authentication;
using PerpetualIntelligence.Cli.Authentication.Msal;
using PerpetualIntelligence.Cli.Hosting;
using System;
using System.Net.Http;
using System.Threading;

namespace PerpetualIntelligence.Cli.Extensions
{
    /// <summary>
    /// </summary>
    public static class ICliBuilderExtensions
    {
        /// <summary>
        /// Adds authentication to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="name">The HTTP client name.</param>
        /// <param name="baseAddress">The HTTP base address. Specify <c>null</c> for no specific well known base address.</param>
        /// <param name="timeout">
        /// The HTTP request timeout in milliseconds. Defaults to <c>2</c> minutes or <c>120000</c> milliseconds. We
        /// recommend a timeout of at least a few minutes, to take into account cases where the user is prompted to
        /// change password or perform 2FA.
        /// </param>
        /// <typeparam name="TProvider">The authentication provider.</typeparam>
        /// <typeparam name="TAppFactory">The authentication application factory.</typeparam>
        /// <typeparam name="TAppCache">The authentication application cache.</typeparam>
        /// <typeparam name="TDelegateHandler">The authentication application delegate handler.</typeparam>
        /// <returns>The configured <see cref="ICliBuilder"/>.</returns>
        /// <remarks>
        /// Use <see cref="ClientCrossPlatformNoTokenCache"/> if your application does not require token caching.
        /// </remarks>
        public static ICliBuilder AddAuthentication<TProvider, TAppFactory, TAppCache, TDelegateHandler>(this ICliBuilder builder, string name, string? baseAddress = null, int? timeout = 120000) where TProvider : class, IAuthenticationProvider where TAppFactory : class, IMsalPublicClientApplicationFactory where TAppCache : class, IClientCrossPlatformTokenCache where TDelegateHandler : DelegatingHandler
        {
            builder.Services.AddSingleton<IAuthenticationProvider, TProvider>();

            builder.Services.AddSingleton<IMsalPublicClientApplicationFactory, TAppFactory>();

            builder.Services.AddSingleton<IClientCrossPlatformTokenCache, TAppCache>();

            // TDelegateHandler cannot be singleton.
            builder.Services.AddScoped<TDelegateHandler>();

            // Configure to call the authority
            builder.Services.AddHttpClient(name, client =>
            {
                client.BaseAddress = (baseAddress != null) ? new Uri(baseAddress) : null;
                client.Timeout = (timeout == null) ? Timeout.InfiniteTimeSpan : TimeSpan.FromMilliseconds(timeout.GetValueOrDefault());
            }).AddHttpMessageHandler<TDelegateHandler>();

            return builder;
        }
    }
}
