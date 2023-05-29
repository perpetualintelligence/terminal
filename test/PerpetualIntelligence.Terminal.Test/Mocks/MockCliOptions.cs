/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Configuration.Options;

namespace PerpetualIntelligence.Terminal.Mocks
{
    public class MockCliOptions
    {
        public static CliOptions New()
        {
            return new CliOptions()
            {
                Logging = new LoggingOptions()
                {
                    ObsureInvalidOptions = false
                },
                Extractor = new ExtractorOptions()
                {
                    OptionPrefix = "-",
                    OptionValueSeparator = "=",
                    Separator = " ",
                    CommandIdRegex = "^[A-Za-z0-9:]*$"
                },
            };
        }

        public static CliOptions NewOptions()
        {
            return new CliOptions()
            {
                Logging = new LoggingOptions()
                {
                    ObsureInvalidOptions = false
                },
                Extractor = new ExtractorOptions()
                {
                    OptionAlias = true,
                    OptionValueWithIn = "\"",
                    OptionPrefix = "--",
                    OptionAliasPrefix = "-",
                    OptionValueSeparator = " ",
                    Separator = " ",
                    CommandIdRegex = "^[A-Za-z0-9:]*$"
                },
            };
        }
    }
}
