﻿/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Runtime;
using System;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Hosting.Mocks
{
    public class MockTerminalEventsHostedService : TerminalHostedService
    {
        public MockTerminalEventsHostedService(IServiceProvider serviceProvider, TerminalOptions terminalOptions, ITerminalConsole terminalConsole, ILogger<TerminalHostedService> logger) : base(serviceProvider, terminalOptions, terminalConsole, logger)
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