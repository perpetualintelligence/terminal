/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Licensing;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Integration.Mocks
{
    public class MockCliCustomHostedService : CliHostedService
    {
        public MockCliCustomHostedService(IHost host, IHostApplicationLifetime hostApplicationLifetime, ILicenseExtractor licenseExtractor, ILicenseChecker licenseChecker, CliOptions cliOptions, ILogger<CliHostedService> logger) : base(host, hostApplicationLifetime, licenseExtractor, licenseChecker, cliOptions, logger)
        {
        }

        public ValueTuple<int, bool> CheckAppConfigCalled { get; set; }

        public ValueTuple<int, bool> CheckMandatoryAppConfigCalled { get; set; }

        public bool OnStartedCalled { get; set; }

        public bool OnStoppedCalled { get; set; }

        public bool OnStoppingCalled { get; set; }

        public ValueTuple<int, bool> PrintHostAppHeaderCalled { get; set; }

        public ValueTuple<int, bool> PrintHostLicCalled { get; set; }

        public ValueTuple<int, bool> PrintMandatoryLicCalled { get; set; }

        public ValueTuple<int, bool> RegisterEventsCalled { get; set; }

        internal override Task CheckHostApplicationMandatoryConfigurationAsync(CliOptions options)
        {
            CheckMandatoryAppConfigCalled = new(MockCliHostedServiceStaticCounter.Increment(), true);
            return Task.CompletedTask;
        }

        internal override Task PrintHostApplicationMandatoryLicensingAsync(License license)
        {
            PrintMandatoryLicCalled = new(MockCliHostedServiceStaticCounter.Increment(), true);
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
