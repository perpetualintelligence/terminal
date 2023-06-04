/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Shared.Extensions;
using System.Collections.Generic;

namespace PerpetualIntelligence.Terminal.Mocks
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

                { "option_alias", true },
                { "default_option", true },
                { "default_option_value", true },
                { "strict_data_type", true },

                { "data_type_handlers", Handlers.DefaultHandler },
                { "text_handlers", new[] { Handlers.UnicodeHandler, Handlers.AsciiHandler }.JoinBySpace() },
                { "error_handlers", Handlers.DefaultHandler },
                { "store_handlers", Handlers.InMemoryHandler },
                { "service_handlers", Handlers.DefaultHandler },
                { "license_handlers", Handlers.OnlineLicenseHandler },

                { "currency", "USD" },
                { "monthly_price", 0.0 },
                { "yearly_price", 0.0 }
            };

            return claims;
        }
    }
}