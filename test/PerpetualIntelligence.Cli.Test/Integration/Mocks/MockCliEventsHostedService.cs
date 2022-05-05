/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Configuration.Options;
using System;

namespace PerpetualIntelligence.Cli.Integration.Mocks
{
    public class MockCliEventsHostedService : CliHostedService
    {
        public MockCliEventsHostedService(IServiceProvider serviceProvider, CliOptions cliOptions, ILogger<CliHostedService> logger) : base(serviceProvider, cliOptions, logger)
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
