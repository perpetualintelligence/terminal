/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/
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
                Id = "test_id_1",
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
                },
            };
        }

        public static TerminalOptions NewAliasOptions()
        {
            return new TerminalOptions()
            {
                Id = "test_id_2",
                Logging = new LoggingOptions()
                {
                    ObsureInvalidOptions = false
                },
                Extractor = new ExtractorOptions()
                {
                    ValueDelimiter = "\"",
                    OptionPrefix = "--",
                    OptionAliasPrefix = "-",
                    OptionValueSeparator = " ",
                    Separator = " ",
                },
            };
        }
    }
}