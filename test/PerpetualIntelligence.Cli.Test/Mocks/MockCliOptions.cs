/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
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

        public static CliOptions NewOptions()
        {
            return new CliOptions()
            {
                Logging = new LoggingOptions()
                {
                    RevealErrorArguments = true
                },
                Extractor = new ExtractorOptions()
                {
                    ArgumentAlias = true,
                    ArgumentValueWithIn = "\"",
                    ArgumentPrefix = "--",
                    ArgumentAliasPrefix = "-",
                    ArgumentSeparator = " ",
                    Separator = " ",
                },
            };
        }
    }
}
