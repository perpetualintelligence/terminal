/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Xunit;

namespace PerpetualIntelligence.Terminal.Hosting
{
    public class CliBuilderTests
    {
        public CliBuilderTests()
        {
            var hostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesDelegate);
            host = hostBuilder.Build();
        }

        [Fact]
        public void CliBuilder_ShouldReturn_Same_IServiceCollection()
        {
            TerminalBuilder cliBuilder = new(serviceCollection);
            cliBuilder.Services.Should().BeSameAs(serviceCollection);
        }

        ~CliBuilderTests()
        {
            host.Dispose();
        }

        private void ConfigureServicesDelegate(IServiceCollection arg2)
        {
            serviceCollection = arg2;
        }

        private readonly IHost host = null!;
        private IServiceCollection serviceCollection = null!;
    }
}
