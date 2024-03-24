/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Configuration.Options;
using System;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Hosting.Mocks
{
    internal class MockTerminalHostedService : TerminalHostedService
    {
        public bool PrintHostApplicationHeaderCalled { get; private set; }

        public MockTerminalHostedService(IServiceProvider serviceProvider, TerminalOptions options, ILogger<TerminalHostedService> logger) : base(serviceProvider, options, logger)
        {
        }

        protected override Task PrintHostApplicationHeaderAsync()
        {
            PrintHostApplicationHeaderCalled = true;
            return Task.CompletedTask;
        }
    }
}