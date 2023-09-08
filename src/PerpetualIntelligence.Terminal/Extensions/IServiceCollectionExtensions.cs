/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PerpetualIntelligence.Terminal.Commands.Extractors;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Terminal.Hosting;
using System;

namespace PerpetualIntelligence.Terminal.Extensions
{
    /// <summary>
    /// The <see cref="IServiceCollection"/> extension methods.
    /// </summary>
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the <c>pi-cli</c> terminal service.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="setupAction">The setup action.</param>
        /// <returns>The configured <see cref="ITerminalBuilder"/> instance.</returns>
        public static ITerminalBuilder AddTerminal(this IServiceCollection services, Action<TerminalOptions> setupAction)
        {
            services.Configure(setupAction);
            return services.AddTerminal();
        }

        /// <summary>
        /// Adds the <c>pi-cli</c> terminal service.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The configured <see cref="ITerminalBuilder"/> instance.</returns>
        public static ITerminalBuilder AddTerminal(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<TerminalOptions>(configuration);
            return services.AddTerminal();
        }

        /// <summary>
        /// Adds the <c>pi-cli</c> terminal service.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns>The configured <see cref="ITerminalBuilder"/> instance.</returns>
        public static ITerminalBuilder AddTerminal(this IServiceCollection services)
        {
            return services.CreateTerminalBuilder()
                           .AddTerminalOptions();
        }

        /// <summary>
        /// Create the core <see cref="ITerminalBuilder"/>.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns>The configured <see cref="ITerminalBuilder"/> instance.</returns>
        public static ITerminalBuilder CreateTerminalBuilder(this IServiceCollection services)
        {
            return new TerminalBuilder(services);
        }

        /// <summary>
        /// Adds the default <c>pi-cli</c> terminal services.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="setupAction">The configuration setup action.</param>
        /// <returns></returns>
        public static ITerminalBuilder AddTerminalDefault(this IServiceCollection services, Action<TerminalOptions> setupAction)
        {
            return services.AddTerminal(setupAction)
                    .AddExtractor<CommandExtractor, CommandRouteParser>()
                    .AddLicenseHandler()
                    .AddTextHandler<AsciiTextHandler>()
                    .AddExceptionHandler<ExceptionHandler>();
        }
    }
}