/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Commands.Routers;
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
        /// Adds the core <c>pi-cli</c> services.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="setupAction">The setup action.</param>
        /// <returns>The configured <see cref="ITerminalBuilder"/> instance.</returns>
        public static ITerminalBuilder AddCli(this IServiceCollection services, Action<CliOptions> setupAction)
        {
            services.Configure(setupAction);
            return services.AddCli();
        }

        /// <summary>
        /// Adds the core <c>pi-cli</c> services.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The configured <see cref="ITerminalBuilder"/> instance.</returns>
        public static ITerminalBuilder AddCli(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<CliOptions>(configuration);
            return services.AddCli();
        }

        /// <summary>
        /// Adds the core <c>pi-cli</c> services.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns>The configured <see cref="ITerminalBuilder"/> instance.</returns>
        public static ITerminalBuilder AddCli(this IServiceCollection services)
        {
            return services.AddCliBuilder()
                .AddCliOptions()
                .AddRouter<CommandRouter, CommandHandler>()
                .AddLicenseHandler();
        }

        /// <summary>
        /// Adds the core <see cref="ITerminalBuilder"/>.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns>The configured <see cref="ITerminalBuilder"/> instance.</returns>
        public static ITerminalBuilder AddCliBuilder(this IServiceCollection services)
        {
            return new TerminalBuilder(services);
        }
    }
}
