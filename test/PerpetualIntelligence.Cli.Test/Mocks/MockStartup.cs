/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
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
            services.AddCli(options => { options.Logging.ErrorArguments = true; })
               .AddExtractor<SeparatorCommandExtractor, SeparatorArgumentExtractor>();
        }
    }
}
