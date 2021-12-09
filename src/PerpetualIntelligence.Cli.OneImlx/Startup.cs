/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;
using PerpetualIntelligence.Cli.Commands;
using PerpetualIntelligence.Cli.Commands.Checkers;
using PerpetualIntelligence.Cli.Commands.Extractors;
using PerpetualIntelligence.Cli.Extensions;
using PerpetualIntelligence.Cli.Integration;
using PerpetualIntelligence.Cli.Stores.InMemory;
using PerpetualIntelligence.OneImlx.Cli.Runners;

namespace PerpetualIntelligence.OneImlx.Cli
{
    /// <summary>
    /// The console startup.
    /// </summary>
    public static class Startup
    {
        /// <summary>
        /// Configures the startup services.
        /// </summary>
        /// <param name="services"></param>
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddCli(options => { options.Logging.ErrorArguments = true; })
                .AddExtractor<SeparatorCommandExtractor, SeparatorArgumentExtractor>()
                .AddOneImlxCommandIdentities();
        }

        /// <summary>
        /// Adds the <c>oneimlx</c> cli commands to the service collection.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        internal static ICliBuilder AddOneImlxCommandIdentities(this ICliBuilder builder)
        {
            CommandIdentity map = new("urn:oneimlx:cli:map", "map", "map", new()
            {
                new ArgumentIdentity("r", System.ComponentModel.DataAnnotations.DataType.Text, true, "The root path for source projects."),
                new ArgumentIdentity("p", System.ComponentModel.DataAnnotations.DataType.Text, true, "The comma (,) separated source project names. Projects must organized be in the standard src and test hierarchy."),
                new ArgumentIdentity("c", System.ComponentModel.DataAnnotations.DataType.Text, true, "The configuration, Debug or Release.", new string[] { "Debug", "Release" }),
                new ArgumentIdentity("f", System.ComponentModel.DataAnnotations.DataType.Text, true, "The .NET framework identifier."),
                new ArgumentIdentity("o", System.ComponentModel.DataAnnotations.DataType.Text, true, "The mapping JSON file path.")
            });
            builder.AddCommandIdentity<OneImlxMapRunner, CommandChecker>(map);

            builder.AddCommandIdentityStore<InMemoryCommandIdentityStore>();

            return builder;
        }
    }
}
