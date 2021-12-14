/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
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
                    RevealErrorArguments = true
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
