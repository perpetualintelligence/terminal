﻿/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;
using PerpetualIntelligence.Cli.Commands.Extractors;
using PerpetualIntelligence.Cli.Extensions;

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
            services.AddCli(options => { options.Logging.ErrorArguments = true; })
                .AddExtractor<SeparatorCommandExtractor, SeparatorArgumentExtractor>()
                .AddOneImlxCommandIdentities();
        }
    }
}
