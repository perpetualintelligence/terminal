/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.DependencyInjection;
using PerpetualIntelligence.Terminal.Commands.Extractors;
using PerpetualIntelligence.Terminal.Extensions;

namespace PerpetualIntelligence.Terminal.Mocks
{
    public static class MockStartup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddTerminal(options => { options.Logging.ObsureInvalidOptions = true; })
               .AddExtractor<CommandExtractor, OptionExtractor>();
        }
    }
}
