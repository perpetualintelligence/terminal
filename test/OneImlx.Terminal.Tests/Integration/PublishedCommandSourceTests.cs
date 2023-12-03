/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;

namespace OneImlx.Terminal.Integration
{
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using OneImlx.Terminal.Commands;
    using OneImlx.Terminal.Commands.Handlers;
    using OneImlx.Terminal.Mocks;
    using OneImlx.Terminal.Stores;
    using PerpetualIntelligence.Terminals.Integration.Mocks;
    using System.Threading.Tasks;
    using Xunit;

    public class PublishedCommandSourceTests
    {
        private readonly MockPublishedCommandSourceChecker terminalCommandSourceChecker;
        private readonly IMutableCommandStore mutableCommandStore;
        private readonly ILogger<PublishedCommandSource> logger;
        private readonly ITerminalCommandSourceAssemblyLoader<PublishedCommandSourceContext> assemblyLoader;
        private readonly PublishedCommandSource publishedCommandSource;
        private readonly ITextHandler textHandler;

        public PublishedCommandSourceTests()
        {
            textHandler = new AsciiTextHandler();
            assemblyLoader = new MockPublishedAssemblyLoader();
            terminalCommandSourceChecker = new MockPublishedCommandSourceChecker();
            mutableCommandStore = new InMemoryMutableCommandStore(textHandler, Array.Empty<CommandDescriptor>());
            MockListLoggerFactory mockListLoggerFactory = new();
            logger = mockListLoggerFactory.CreateLogger<PublishedCommandSource>();
            publishedCommandSource = new PublishedCommandSource(textHandler, assemblyLoader, terminalCommandSourceChecker, mutableCommandStore, logger);
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

        [Fact]
        public async Task LoadCommandSourceAsync_EmptyContext_DoesNotLoadCommands()
        {
            var context = new PublishedCommandSourceContext();

            await publishedCommandSource.LoadCommandSourceAsync(context);

            var result = await mutableCommandStore.AllAsync();
            result.Count.Should().Be(0);
        }
    }
}