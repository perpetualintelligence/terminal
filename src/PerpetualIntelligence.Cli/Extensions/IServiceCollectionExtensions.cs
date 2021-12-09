/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Cli.Commands.Routers;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Integration;
using System;

namespace PerpetualIntelligence.Cli.Extensions
{
    /// <summary>
    /// The <see cref="IServiceCollection"/> extension methods.
    /// </summary>
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the Perpetual Intelligence default <c>cli</c> services.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="setupAction">The setup action.</param>
        public static ICliBuilder AddCli(this IServiceCollection services, Action<CliOptions> setupAction)
        {
            services.Configure(setupAction);
            return services.AddCli();
        }

        /// <summary>
        /// Adds the Perpetual Intelligence default <c>cli</c> services.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns></returns>
        public static ICliBuilder AddCli(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<CliOptions>(configuration);
            return services.AddCli();
        }

        /// <summary>
        /// Adds the Perpetual Intelligence default <c>cli</c> services.
        /// </summary>
        /// <param name="services">The services.</param>
        public static ICliBuilder AddCli(this IServiceCollection services)
        {
            return services.AddCliBuilder()
                .AddCliOptions()
                .AddRouting<CommandRouter, CommandHandler>();
        }

        /// <summary>
        /// Adds the Perpetual Intelligence default <c>cli</c> builder to the service collection.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns></returns>
        public static ICliBuilder AddCliBuilder(this IServiceCollection services)
        {
            return new CliBuilder(services);
        }
    }
}