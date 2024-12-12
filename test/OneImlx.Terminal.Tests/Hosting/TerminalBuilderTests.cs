/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FluentAssertions;
using OneImlx.Terminal.Runtime;
using Xunit;

namespace OneImlx.Terminal.Hosting
{
    public class TerminalBuilderTests
    {
        public TerminalBuilderTests()
        {
            var hostBuilder = Host.CreateDefaultBuilder([]).ConfigureServices(ConfigureServicesDelegate);
            host = hostBuilder.Build();
        }

        [Fact]
        public void TerminalBuilder_ShouldReturn_Same_IServiceCollection()
        {
            TerminalBuilder terminalBuilder = new(serviceCollection, new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.ASCII));
            terminalBuilder.Services.Should().BeSameAs(serviceCollection);
        }

        ~TerminalBuilderTests()
        {
            host.Dispose();
        }

        private void ConfigureServicesDelegate(IServiceCollection opt2)
        {
            serviceCollection = opt2;
        }

        private readonly IHost host = null!;
        private IServiceCollection serviceCollection = null!;
    }
}
