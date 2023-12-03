/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Configuration.Options;
using System;

namespace OneImlx.Terminal.Hosting.Mocks
{
    internal class MockTerminalHostedService : TerminalHostedService
    {
        public MockTerminalHostedService(IServiceProvider serviceProvider, TerminalOptions options, ILogger<TerminalHostedService> logger) : base(serviceProvider, options, logger)
        {
        }
    }
}