/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
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
            services.AddCli(options => { options.Logging.ObsureErrorArguments = true; })
               .AddExtractor<CommandExtractor, ArgumentExtractor>();
        }
    }
}
