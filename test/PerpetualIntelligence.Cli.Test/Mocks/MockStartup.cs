/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.DependencyInjection;
using PerpetualIntelligence.Cli.Commands.Extractors;
using PerpetualIntelligence.Cli.Extensions;

namespace PerpetualIntelligence.Cli.Mocks
{
    public static class MockStartup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddCli(options => { options.Logging.ObsureInvalidOptions = true; })
               .AddExtractor<CommandExtractor, OptionExtractor>();
        }
    }
}
