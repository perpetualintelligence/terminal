/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Licensing;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Hosting.Mocks
{
    public class MockCliCustomHostedService : TerminalHostedService
    {
        public MockCliCustomHostedService(IServiceProvider serviceProvider, CliOptions cliOptions, ILogger<TerminalHostedService> logger) : base(serviceProvider, cliOptions, logger)
        {
        }

        public ValueTuple<int, bool> CheckAppConfigCalled { get; set; }

        public ValueTuple<int, bool> RegisterHelpArgumentCalled { get; set; }

        public bool OnStartedCalled { get; set; }

        public bool OnStoppedCalled { get; set; }

        public bool OnStoppingCalled { get; set; }

        public ValueTuple<int, bool> PrintHostAppHeaderCalled { get; set; }

        public ValueTuple<int, bool> PrintHostLicCalled { get; set; }

        public ValueTuple<int, bool> PrintMandatoryLicCalled { get; set; }

        public ValueTuple<int, bool> RegisterEventsCalled { get; set; }

        internal override Task PrintHostApplicationMandatoryLicensingAsync(License license)
        {
            PrintMandatoryLicCalled = new(MockCliHostedServiceStaticCounter.Increment(), true);
            return Task.CompletedTask;
        }

        internal override Task RegisterHelpAsync()
        {
            RegisterHelpArgumentCalled = new(MockCliHostedServiceStaticCounter.Increment(), true);
            return Task.CompletedTask;
        }

        protected override Task CheckHostApplicationConfigurationAsync(CliOptions options)
        {
            CheckAppConfigCalled = new(MockCliHostedServiceStaticCounter.Increment(), true);
            return Task.CompletedTask;
        }

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
            PrintHostAppHeaderCalled = new(MockCliHostedServiceStaticCounter.Increment(), true);
            return Task.CompletedTask;
        }

        protected override Task PrintHostApplicationLicensingAsync(License license)
        {
            PrintHostLicCalled = new(MockCliHostedServiceStaticCounter.Increment(), true);
            return Task.CompletedTask;
        }

        protected override Task RegisterHostApplicationEventsAsync(IHostApplicationLifetime hostApplicationLifetime)
        {
            RegisterEventsCalled = new(MockCliHostedServiceStaticCounter.Increment(), true);
            return Task.CompletedTask;
        }
    }
}