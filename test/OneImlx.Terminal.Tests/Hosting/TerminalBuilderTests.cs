/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OneImlx.Terminal.Commands.Handlers;
using System;
using Xunit;

namespace OneImlx.Terminal.Hosting
{
    public class TerminalBuilderTests
    {
        public TerminalBuilderTests()
        {
            var hostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesDelegate);
            host = hostBuilder.Build();
        }

        [Fact]
        public void TerminalBuilder_ShouldReturn_Same_IServiceCollection()
        {
            TerminalBuilder terminalBuilder = new(serviceCollection, new AsciiTextHandler());
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