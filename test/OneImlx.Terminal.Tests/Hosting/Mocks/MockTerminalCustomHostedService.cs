/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Licensing;
using OneImlx.Terminal.Runtime;
using System;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Hosting.Mocks
{
    public class MockTerminalCustomHostedService : TerminalHostedService
    {
        public MockTerminalCustomHostedService(IServiceProvider serviceProvider, IOptions<TerminalOptions> terminalOptions, ITerminalConsole terminalConsole, ILogger<TerminalHostedService> logger) : base(serviceProvider, terminalOptions, terminalConsole, logger)
        {
        }

        public ValueTuple<int, bool> CheckAppConfigCalled { get; set; }

        public bool OnStartedCalled { get; set; }

        public bool OnStoppedCalled { get; set; }

        public bool OnStoppingCalled { get; set; }

        public ValueTuple<int, bool> PrintHostAppHeaderCalled { get; set; }

        public ValueTuple<int, bool> PrintHostLicCalled { get; set; }

        public ValueTuple<int, bool> PrintMandatoryLicCalled { get; set; }

        public ValueTuple<int, bool> RegisterEventsCalled { get; set; }

        public ValueTuple<int, bool> RegisterHelpArgumentCalled { get; set; }

        internal override Task PrintHostApplicationMandatoryLicensingAsync(License license)
        {
            PrintMandatoryLicCalled = new(MockTerminalHostedServiceStaticCounter.Increment(), true);
            return Task.CompletedTask;
        }

        internal override Task RegisterHelpAsync()
        {
            RegisterHelpArgumentCalled = new(MockTerminalHostedServiceStaticCounter.Increment(), true);
            return Task.CompletedTask;
        }

        protected override Task CheckHostApplicationConfigurationAsync(IOptions<TerminalOptions> options)
        {
            CheckAppConfigCalled = new(MockTerminalHostedServiceStaticCounter.Increment(), true);
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
            PrintHostAppHeaderCalled = new(MockTerminalHostedServiceStaticCounter.Increment(), true);
            return Task.CompletedTask;
        }

        protected override Task PrintHostApplicationLicensingAsync(License license)
        {
            PrintHostLicCalled = new(MockTerminalHostedServiceStaticCounter.Increment(), true);
            return Task.CompletedTask;
        }

        protected override Task RegisterHostApplicationEventsAsync(IHostApplicationLifetime hostApplicationLifetime)
        {
            RegisterEventsCalled = new(MockTerminalHostedServiceStaticCounter.Increment(), true);
            return Task.CompletedTask;
        }
    }
}
