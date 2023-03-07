/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Extensions;
using PerpetualIntelligence.Cli.Mocks;
using System.Threading.Tasks;
using Xunit;

namespace PerpetualIntelligence.Cli.Runtime
{
    public class TerminalLoggerTests : IAsyncLifetime
    {
        public TerminalLoggerTests()
        {
            // Create a host builder with mock event hosted service
            hostBuilder = Host.CreateDefaultBuilder();
            host = hostBuilder.ConfigureServices(services =>
            {
            }).ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddTerminalLogger<MockTerminalLogger>();
            }).Build();
        }

        [Fact]
        public void Services_Should_Create_TerminalLogger_Instance_Correclty()
        {
            ILogger logger = host.Services.GetRequiredService<ILogger<TerminalLoggerTests>>();
            logger.LogInformation("Test message1");
            logger.LogInformation("Test message2");
            logger.LogInformation("Test message3");
            MockTerminalLogger.StaticMessages.Count.Should().Be(3);
            MockTerminalLogger.StaticMessages[0].Should().Be("Test message1");
            MockTerminalLogger.StaticMessages[1].Should().Be("Test message2");
            MockTerminalLogger.StaticMessages[2].Should().Be("Test message3");
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            // Make sure the logs are cleared for each test run. ILogger does not expose each logger.
            MockTerminalLogger.StaticMessages.Clear();
            return Task.CompletedTask;
        }

        private IHost host;
        private IHostBuilder hostBuilder;
    }
}