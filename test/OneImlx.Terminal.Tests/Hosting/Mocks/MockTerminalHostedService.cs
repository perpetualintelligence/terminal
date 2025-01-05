/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Runtime;
using System;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Hosting.Mocks
{
    internal class MockTerminalHostedService : TerminalHostedService
    {
        public MockTerminalHostedService(IServiceProvider serviceProvider, IOptions<TerminalOptions> options, ITerminalConsole terminalConsole, ITerminalExceptionHandler terminalExceptionHandler, ILogger<TerminalHostedService> logger) : base(serviceProvider, options, terminalConsole, terminalExceptionHandler, logger)
        {
        }

        public bool PrintHostApplicationHeaderCalled { get; private set; }

        protected override Task PrintHostApplicationHeaderAsync()
        {
            PrintHostApplicationHeaderCalled = true;
            return Task.CompletedTask;
        }
    }
}
