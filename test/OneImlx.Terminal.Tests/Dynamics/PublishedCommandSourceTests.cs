/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Shared;
using OneImlx.Terminal.Stores;
using OneImlx.Terminals.Integration.Mocks;
using OneImlx.Test.FluentAssertions;
using System;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Dynamics
{
    public class PublishedCommandSourceTests
    {
        public PublishedCommandSourceTests()
        {
            textHandler = new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.ASCII);
            assemblyLoader = new MockPublishedAssemblyLoader();
            terminalCommandSourceChecker = new MockPublishedCommandSourceChecker();
            mutableCommandStore = new TerminalInMemoryCommandStore(textHandler, Array.Empty<CommandDescriptor>());
            MockListLoggerFactory mockListLoggerFactory = new();
            logger = mockListLoggerFactory.CreateLogger<PublishedCommandSource>();

            TerminalOptions options = MockTerminalOptions.NewAliasOptions();
            options.Dynamics.Enabled = true;
            terminalOptions = Microsoft.Extensions.Options.Options.Create<TerminalOptions>(options);

            publishedCommandSource = new PublishedCommandSource(textHandler, assemblyLoader, terminalCommandSourceChecker, mutableCommandStore, terminalOptions, logger);
        }

        [Fact]
        public async Task LoadCommandSourceAsync_Calls_CheckSourceAsync()
        {
            PublishedCommandSourceContext commandSourceContext = new();
            commandSourceContext.PublishedAssemblies.Add("MockAssembly1.dll", "mock//path//to//assembly");

            terminalCommandSourceChecker.Called.Should().BeFalse();
            await publishedCommandSource.LoadCommandSourceAsync(commandSourceContext);

            terminalCommandSourceChecker.Called.Should().BeTrue();
            terminalCommandSourceChecker.PassedContext.Should().BeSameAs(commandSourceContext);
        }

        [Fact]
        public async Task LoadCommandSourceAsync_EmptyContext_DoesNotLoadCommands()
        {
            var context = new PublishedCommandSourceContext();

            await publishedCommandSource.LoadCommandSourceAsync(context);

            var result = await mutableCommandStore.AllAsync();
            result.Count.Should().Be(0);
        }

        [Fact]
        public async Task LoadCommandSourceAsync_Throws_If_Integration_Disabled()
        {
            terminalOptions.Value.Dynamics.Enabled = false;

            PublishedCommandSourceContext commandSourceContext = new();
            commandSourceContext.PublishedAssemblies.Add("MockAssembly1.dll", "mock//path//to//assembly");

            terminalCommandSourceChecker.Called.Should().BeFalse();
            Func<Task> act = async () => await publishedCommandSource.LoadCommandSourceAsync(commandSourceContext);
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode(TerminalErrors.InvalidConfiguration)
                .WithErrorDescription("The terminal dynamics is not enabled.");

            terminalCommandSourceChecker.Called.Should().BeFalse();
        }

        [Fact]
        public async Task LoadCommandSourceAsync_ValidAssemblies_LoadsCommands()
        {
            var result = await mutableCommandStore.AllAsync();
            result.Count.Should().Be(0);

            var context = new PublishedCommandSourceContext();

            context.PublishedAssemblies.Add("MockAssembly1.dll", "mock//path//to//assembly");
            context.PublishedAssemblies.Add("MockAssembly2.dll", "mock//path//to//assembly");
            context.PublishedAssemblies.Add("MockAssembly3.dll", "mock//path//to//assembly");

            await publishedCommandSource.LoadCommandSourceAsync(context);

            result = await mutableCommandStore.AllAsync();
            result.Count.Should().Be(9);

            result.Keys.Should().Contain("MockAssembly1MockClass1");
            result.Keys.Should().Contain("MockAssembly1MockClass2");
            result.Keys.Should().Contain("MockAssembly1MockClass3");

            result.Keys.Should().Contain("MockAssembly2MockClass1");
            result.Keys.Should().Contain("MockAssembly2MockClass2");
            result.Keys.Should().Contain("MockAssembly2MockClass3");

            result.Keys.Should().Contain("MockAssembly3MockClass1");
            result.Keys.Should().Contain("MockAssembly3MockClass2");
            result.Keys.Should().Contain("MockAssembly3MockClass3");
        }

        private readonly ITerminalCommandSourceAssemblyLoader<PublishedCommandSourceContext> assemblyLoader;
        private readonly ILogger<PublishedCommandSource> logger;
        private readonly ITerminalCommandStore mutableCommandStore;
        private readonly PublishedCommandSource publishedCommandSource;
        private readonly MockPublishedCommandSourceChecker terminalCommandSourceChecker;
        private readonly IOptions<TerminalOptions> terminalOptions;
        private readonly ITerminalTextHandler textHandler;
    }
}
