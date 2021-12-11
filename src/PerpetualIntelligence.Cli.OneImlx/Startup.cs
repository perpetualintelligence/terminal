/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;
using PerpetualIntelligence.Cli.Commands;
using PerpetualIntelligence.Cli.Commands.Checkers;
using PerpetualIntelligence.Cli.Commands.Defaults;
using PerpetualIntelligence.Cli.Commands.Extractors;
using PerpetualIntelligence.Cli.Commands.Mappers;
using PerpetualIntelligence.Cli.Extensions;
using PerpetualIntelligence.Cli.Integration;
using PerpetualIntelligence.Cli.Stores.InMemory;
using PerpetualIntelligence.OneImlx.Cli.Runners;
using PerpetualIntelligence.Shared.Attributes.Validation;

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
                .AddArgumentChecker<DataAnnotationsArgumentDataTypeMapper, ArgumentChecker>()
                .AddOneImlxCommandIdentities();

            services.AddHostedService<CliHostedService>();
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
                new ArgumentIdentity("c", System.ComponentModel.DataAnnotations.DataType.Text, true, "The configuration, Debug or Release.", new System.ComponentModel.DataAnnotations.ValidationAttribute[] { new OneOfAttribute("Debug", "Release") }),
                new ArgumentIdentity("f", System.ComponentModel.DataAnnotations.DataType.Text, true, "The .NET framework identifier."),
                new ArgumentIdentity("i", System.ComponentModel.DataAnnotations.DataType.Text, true, "The input mapping JSON file path."),
                new ArgumentIdentity("o", System.ComponentModel.DataAnnotations.DataType.Text, true, "The output mapping JSON file path.")
            });
            builder.AddCommandIdentity<OneImlxMapRunner, CommandChecker>(map);

            CommandIdentity exit = new CommandIdentity("urn:oneimlx:cli:exit", "exit", "exit");
            builder.AddCommandIdentity<ExitRunner, CommandChecker>(exit);

            builder.AddCommandIdentityStore<InMemoryCommandIdentityStore>();

            return builder;
        }
    }
}
