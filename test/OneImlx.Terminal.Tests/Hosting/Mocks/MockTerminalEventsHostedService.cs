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
    public class MockTerminalEventsHostedService : TerminalHostedService
    {
        public MockTerminalEventsHostedService(IServiceProvider serviceProvider, IOptions<TerminalOptions> terminalOptions, ITerminalConsole terminalConsole, ITerminalExceptionHandler terminalExceptionHandler, ILogger<TerminalHostedService> logger) : base(serviceProvider, terminalOptions, terminalConsole, terminalExceptionHandler, logger)
        {
        }

        public bool OnStartedCalled { get; set; }

        public bool OnStoppedCalled { get; set; }

        public bool OnStoppingCalled { get; set; }

        protected override void OnStarted()
        {
            OnStartedCalled = true;
        }

        protected override void OnStopped()
        {
            OnStoppedCalled = true;
        }

        protected override void OnStopping()
        {
            OnStoppingCalled = true;
        }

        protected override Task PrintHostApplicationHeaderAsync()
        {
            return Task.CompletedTask;
        }
    }
}
