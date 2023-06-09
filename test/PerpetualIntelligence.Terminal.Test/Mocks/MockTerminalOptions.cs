/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Configuration.Options;

namespace PerpetualIntelligence.Terminal.Mocks
{
    public class MockTerminalOptions
    {
        public static TerminalOptions NewLegacyOptions()
        {
            return new TerminalOptions()
            {
                Logging = new LoggingOptions()
                {
                    ObsureInvalidOptions = false
                },
                Extractor = new ExtractorOptions()
                {
                    OptionPrefix = "-",
                    OptionAliasPrefix = "-",
                    OptionValueSeparator = "=",
                    Separator = " ",
                    CommandIdRegex = "^[A-Za-z0-9:]*$"
                },
            };
        }

        public static TerminalOptions NewAliasOptions()
        {
            return new TerminalOptions()
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
