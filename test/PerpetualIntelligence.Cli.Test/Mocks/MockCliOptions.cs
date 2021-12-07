/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Configuration.Options;

namespace PerpetualIntelligence.Cli.Mocks
{
    public class MockCliOptions
    {
        public static CliOptions New()
        {
            return new CliOptions()
            {
                Logging = new LoggingOptions()
                {
                    ErrorArguments = true
                },
                Extractor = new ExtractorOptions()
                {
                    ArgumentPrefix = "-",
                    ArgumentSeparator = "=",
                    Separator = " ",
                },
            };
        }
    }
}
