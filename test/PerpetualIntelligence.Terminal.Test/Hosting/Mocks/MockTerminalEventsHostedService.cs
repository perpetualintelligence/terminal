/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Terminal.Configuration.Options;
using System;

namespace PerpetualIntelligence.Terminal.Hosting.Mocks
{
    public class MockTerminalEventsHostedService : TerminalHostedService
    {
        public MockTerminalEventsHostedService(IServiceProvider serviceProvider, TerminalOptions terminalOptions, ILogger<TerminalHostedService> logger) : base(serviceProvider, terminalOptions, logger)
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
    }
}
