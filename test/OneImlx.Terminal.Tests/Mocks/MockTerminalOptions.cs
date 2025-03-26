/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Shared;

namespace OneImlx.Terminal.Mocks
{
    public class MockTerminalOptions
    {
        public static TerminalOptions NewAliasOptions()
        {
            return new TerminalOptions()
            {
                Id = "test_id_2",
                Parser = new ParserOptions()
                {
                    ValueDelimiter = '"',
                    OptionPrefix = '-',
                    OptionValueSeparator = TerminalIdentifiers.SpaceSeparator,
                    Separator = TerminalIdentifiers.SpaceSeparator,
                },
            };
        }

        public static TerminalOptions NewLegacyOptions()
        {
            return new TerminalOptions()
            {
                Id = "test_id_1",
                Parser = new ParserOptions()
                {
                    OptionPrefix = '-',
                    OptionValueSeparator = '=',
                    Separator = TerminalIdentifiers.SpaceSeparator,
                },
            };
        }
    }
}
