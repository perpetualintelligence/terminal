/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Runtime;

namespace OneImlx.Terminal.Hosting.Mocks
{
    public class MockTerminalCustomHostedService : TerminalHostedService
    {
        public MockTerminalCustomHostedService(IServiceProvider serviceProvider, IOptions<TerminalOptions> terminalOptions, ITerminalConsole terminalConsole, ITerminalExceptionHandler terminalExceptionHandler, ILogger<TerminalHostedService> logger) : base(serviceProvider, terminalOptions, terminalConsole, terminalExceptionHandler, logger)
        {
        }

        public bool ConfigureLifetimeCalled { get; private set; }

        protected override Task ConfigureLifetimeAsync()
        {
            ConfigureLifetimeCalled = true;
            return Task.CompletedTask;
        }
    }
}
