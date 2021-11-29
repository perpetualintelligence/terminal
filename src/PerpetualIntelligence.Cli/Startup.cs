/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;
using PerpetualIntelligence.Cli.Commands;
using PerpetualIntelligence.Cli.Commands.RequestHandlers;
using PerpetualIntelligence.Cli.Configuration.Options;
using System;
using System.Collections.Generic;

namespace PerpetualIntelligence.Cli
{
    /// <summary>
    /// The console startup.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Configures the startup services.
        /// </summary>
        /// <param name="services"></param>
        public static void ConfigureServices(IServiceCollection services)
        {
            // FOMAC: this is not calling OptionsSetupAction so we have to call explicitly using singleton
            //services.AddOptions<CliOptions>();
            //services.Configure<CliOptions>(OptionsSetupAction);
            services.AddSingleton(OptionsSetupAction);

            // The command request router to find the handler and route request.
            services.AddScoped<ICommandRequestRouter, CommandRequestRouter>();
            services.AddScoped<ICommandRequestHandler, CommandRequestHandler>();
            services.AddScoped<ICommandExtractor, StringCommandExtractor>();
            services.AddScoped<IArgumentsExtractor, StringArgumentsExtractor>();
            services.AddScoped<ICommandChecker, CommandChecker>();

            AddOneImlxCommands(services);

            services.AddSingleton(CommandStoreFactory);
            services.AddSingleton(OptionsSetupAction);
        }

        private static ICommandIdentityStore CommandStoreFactory(IServiceProvider arg)
        {
            List<CommandIdentity> commands = new()
            {
                new("urn:oneimlx:cli:map", "map", typeof(MapRunner))
            };

            return new InMemoryCommandIdentityStore(commands);
        }

        private static CliOptions OptionsSetupAction(IServiceProvider arg)
        {
            CliOptions options = new CliOptions();
            OptionsSetupAction(options);
            return options;
        }

        private static void OptionsSetupAction(CliOptions obj)
        {
            obj.Logging.ErrorArguments = true;
        }

        private static void AddOneImlxCommands(IServiceCollection services)
        {
            services.AddSingleton<MapRunner>();
        }
    }
}
