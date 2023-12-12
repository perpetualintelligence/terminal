/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Shared.Extensions;
using System.Collections.Generic;

namespace OneImlx.Terminal.Mocks
{
    internal class MockCustomLicenseClaims
    {
        internal static Dictionary<string, object> CustomClaims()
        {
            Dictionary<string, object> claims = new()
            {
                { "terminal_limit", 1 },
                { "redistribution_limit", 0 },
                { "root_command_limit", 1 },
                { "grouped_command_limit", 2 },
                { "sub_command_limit", 15 },
                { "option_limit", 100 },

                { "strict_data_type", true },

                { "data_type_handlers", TerminalHandlers.DefaultHandler },
                { "text_handlers", new[] { TerminalHandlers.UnicodeHandler, TerminalHandlers.AsciiHandler }.JoinBySpace() },
                { "error_handlers", TerminalHandlers.DefaultHandler },
                { "store_handlers", TerminalHandlers.InMemoryHandler },
                { "service_handlers", TerminalHandlers.DefaultHandler },
                { "license_handlers", TerminalHandlers.OnlineLicenseHandler },

                { "currency", "USD" },
                { "monthly_price", 0.0 },
                { "yearly_price", 0.0 }
            };

            return claims;
        }
    }
}