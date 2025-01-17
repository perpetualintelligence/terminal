﻿/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OneImlx.Shared.Licensing;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Extensions;
using OneImlx.Terminal.Hosting.Mocks;
using OneImlx.Terminal.Licensing;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Stores;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Hosting
{
    /// <summary>
    /// Run test sequentially because we modify the static Console.SetOut
    /// </summary>
    [Collection("Sequential")]
    public class TerminalHostedServiceTests : IAsyncLifetime
    {
        public TerminalHostedServiceTests()
        {
            cancellationTokenSource = new();
            cancellationToken = cancellationTokenSource.Token;

            TerminalOptions terminalOptions = MockTerminalOptions.NewAliasOptions();
            mockLicenseExtractor = new();
            mockLicenseChecker = new();
            mockOptionsChecker = new();
            mockExceptionPublisher = new MockExceptionPublisher();

            logger = new MockTerminalHostedServiceLogger();
            mockTerminalConsole = new MockTerminalConsole();

            hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton<ILicenseExtractor>(mockLicenseExtractor);
                services.AddSingleton<ILicenseChecker>(mockLicenseChecker);
                services.AddSingleton<IConfigurationOptionsChecker>(mockOptionsChecker);
                services.AddSingleton<ITerminalTextHandler, TerminalTextHandler>();
            });
            host = hostBuilder.Start();

            // Different hosted services to test behaviors
            var terminalIOptions = Microsoft.Extensions.Options.Options.Create<TerminalOptions>(terminalOptions);
            defaultCliHostedService = new MockTerminalHostedService(host.Services, terminalIOptions, mockTerminalConsole, mockExceptionPublisher, logger);
            mockCustomCliHostedService = new MockTerminalCustomHostedService(host.Services, terminalIOptions, mockTerminalConsole, mockExceptionPublisher, logger);
            mockCliEventsHostedService = new MockTerminalEventsHostedService(host.Services, terminalIOptions, mockTerminalConsole, mockExceptionPublisher, logger);
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public void StartAsync_Default_ShouldPrint_LicenseInfo()
        {
            // use reflection to call
            MethodInfo? printLic = defaultCliHostedService.GetType().GetMethod("PrintHostApplicationLicensingAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(printLic);
            printLic.Invoke(defaultCliHostedService, [MockLicenses.TestLicense]);

            logger.Messages.Should().BeEmpty();

            mockTerminalConsole.Messages.Should().HaveCount(9);
            mockTerminalConsole.Messages[0].Should().Be("tenant=test_name (test_tenantid)");
            mockTerminalConsole.Messages[1].Should().Be("country=test_country");
            mockTerminalConsole.Messages[2].Should().Be("license=test_id");
            mockTerminalConsole.Messages[3].Should().Be("mode=test_mode");
            mockTerminalConsole.Messages[4].Should().Be("deployment=test_deployment");
            mockTerminalConsole.Messages[5].Should().Be("usage=urn:oneimlx:lic:usage:rnd");
            mockTerminalConsole.Messages[6].Should().Be("plan=urn:oneimlx:terminal:plan:demo");
            mockTerminalConsole.Messages[7].Should().StartWith("iat=");
            mockTerminalConsole.Messages[8].Should().StartWith("exp=");
        }

        [Fact]
        public void StartAsync_Default_ShouldPrint_MandatoryLicenseInfo_For_Custom_RND()
        {
            License community = new(TerminalLicensePlans.Custom, LicenseUsage.RnD, "testkey", MockLicenses.TestClaims, MockLicenses.TestQuota);

            // use reflection to call
            MethodInfo? printLic = defaultCliHostedService.GetType().GetMethod("PrintHostApplicationMandatoryLicensingAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(printLic);
            printLic.Invoke(defaultCliHostedService, [community]);

            logger.Messages.Should().BeEmpty();
        }

        [Fact]
        public void StartAsync_Default_ShouldPrint_MandatoryLicenseInfoFor_Demo_Education()
        {
            License community = new(TerminalLicensePlans.Demo, LicenseUsage.Educational, "testkey", MockLicenses.TestClaims, MockLicenses.TestQuota);

            // use reflection to call
            MethodInfo? printLic = defaultCliHostedService.GetType().GetMethod("PrintHostApplicationMandatoryLicensingAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(printLic);
            printLic.Invoke(defaultCliHostedService, [community]);

            logger.Messages.Should().BeEmpty();

            mockTerminalConsole.Messages.Should().HaveCount(1);
            mockTerminalConsole.Messages[0].Should().Be("[Color: Yellow] The demo license is free for educational purposes, but non-educational use requires a commercial license.");
        }

        [Fact]
        public void StartAsync_Default_ShouldPrint_MandatoryLicenseInfoFor_Demo_RND()
        {
            License community = new(TerminalLicensePlans.Demo, LicenseUsage.RnD, "testkey", MockLicenses.TestClaims, MockLicenses.TestQuota);

            // use reflection to call
            MethodInfo? printLic = defaultCliHostedService.GetType().GetMethod("PrintHostApplicationMandatoryLicensingAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(printLic);
            printLic.Invoke(defaultCliHostedService, [community]);

            logger.Messages.Should().BeEmpty();

            mockTerminalConsole.Messages.Should().HaveCount(1);
            mockTerminalConsole.Messages[0].Should().Be("[Color: Yellow] The demo license is free for research and development, but production use requires a commercial license.");
        }

        [Fact]
        public void StartAsync_Default_ShouldPrint_OnStarted()
        {
            // use reflection to call
            MethodInfo? print = defaultCliHostedService.GetType().GetMethod("OnStarted", BindingFlags.Instance | BindingFlags.NonPublic);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(print);
            print.Invoke(defaultCliHostedService, null);

            logger.Messages.Should().BeEmpty();

            mockTerminalConsole.Messages.Should().HaveCount(1);
            mockTerminalConsole.Messages[0].Should().StartWith("Application started on");
        }

        [Fact]
        public void StartAsync_Default_ShouldPrint_OnStopped()
        {
            // use reflection to call
            MethodInfo? print = defaultCliHostedService.GetType().GetMethod("OnStopped", BindingFlags.Instance | BindingFlags.NonPublic);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(print);
            print.Invoke(defaultCliHostedService, null);

            logger.Messages.Should().BeEmpty();

            mockTerminalConsole.Messages.Should().HaveCount(1);
            mockTerminalConsole.Messages[0].Should().StartWith("Application stopped on");
        }

        [Fact]
        public void StartAsync_Default_ShouldPrint_OnStopping()
        {
            // use reflection to call
            MethodInfo? print = defaultCliHostedService.GetType().GetMethod("OnStopping", BindingFlags.Instance | BindingFlags.NonPublic);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(print);
            print.Invoke(defaultCliHostedService, null);

            logger.Messages.Should().BeEmpty();

            mockTerminalConsole.Messages.Should().HaveCount(1);
            mockTerminalConsole.Messages[0].Should().Be("Stopping application...");
        }

        [Fact]
        public async Task StartAsync_ShouldCallCustomizationInCorrectOrderAsync()
        {
            MockTerminalHostedServiceStaticCounter.Restart();
            await mockCustomCliHostedService.StartAsync(cancellationToken);

            // #1 call
            mockCustomCliHostedService.RegisterEventsCalled.Should().NotBeNull();
            mockCustomCliHostedService.RegisterEventsCalled.Item1.Should().Be(1);
            mockCustomCliHostedService.RegisterEventsCalled.Item2.Should().BeTrue();

            // #2 call
            mockCustomCliHostedService.PrintHostAppHeaderCalled.Should().NotBeNull();
            mockCustomCliHostedService.PrintHostAppHeaderCalled.Item1.Should().Be(2);
            mockCustomCliHostedService.PrintHostAppHeaderCalled.Item2.Should().BeTrue();

            // #3 call
            mockLicenseExtractor.ExtractLicenseCalled.Should().NotBeNull();
            mockLicenseExtractor.ExtractLicenseCalled.Item1.Should().Be(3);
            mockLicenseExtractor.ExtractLicenseCalled.Item2.Should().BeTrue();

            // #4 call
            mockCustomCliHostedService.PrintHostLicCalled.Should().NotBeNull();
            mockCustomCliHostedService.PrintHostLicCalled.Item1.Should().Be(4);
            mockCustomCliHostedService.PrintHostLicCalled.Item2.Should().BeTrue();

            // #5 call
            mockLicenseChecker.CheckLicenseCalled.Should().NotBeNull();
            mockLicenseChecker.CheckLicenseCalled.Item1.Should().Be(5);
            mockLicenseChecker.CheckLicenseCalled.Item2.Should().BeTrue();

            // #6 call
            mockCustomCliHostedService.PrintMandatoryLicCalled.Should().NotBeNull();
            mockCustomCliHostedService.PrintMandatoryLicCalled.Item1.Should().Be(6);
            mockCustomCliHostedService.PrintMandatoryLicCalled.Item2.Should().BeTrue();

            // #7 call
            mockOptionsChecker.CheckOptionsCalled.Should().NotBeNull();
            mockOptionsChecker.CheckOptionsCalled.Item1.Should().Be(7);
            mockOptionsChecker.CheckOptionsCalled.Item2.Should().BeTrue();

            // #8 call
            mockCustomCliHostedService.CheckAppConfigCalled.Should().NotBeNull();
            mockCustomCliHostedService.CheckAppConfigCalled.Item1.Should().Be(8);
            mockCustomCliHostedService.CheckAppConfigCalled.Item2.Should().BeTrue();

            // #9 call
            mockCustomCliHostedService.RegisterHelpArgumentCalled.Should().NotBeNull();
            mockCustomCliHostedService.RegisterHelpArgumentCalled.Item1.Should().Be(9);
            mockCustomCliHostedService.RegisterHelpArgumentCalled.Item2.Should().BeTrue();
        }

        [Fact]
        public async Task StartAsync_ShouldHandleErrorExceptionCorrectly()
        {
            mockLicenseExtractor.ThrowError = true;
            await defaultCliHostedService.StartAsync(cancellationToken);

            // Last is a new line
            mockExceptionPublisher.Called.Should().BeTrue();
            mockExceptionPublisher.MultiplePublishedMessages.Should().HaveCount(1);
            mockExceptionPublisher.PublishedMessage.Should().Be("test description. opt1=val1 opt2=val2");
        }

        [Fact]
        public async Task StartAsync_ShouldNotRegister_HelpArgument_IfDisabled()
        {
            TerminalOptions terminalOptions = MockTerminalOptions.NewAliasOptions();
            terminalOptions.Help.Disabled = true;

            var terminalIOptions = Microsoft.Extensions.Options.Options.Create<TerminalOptions>(terminalOptions);

            hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddTerminal<TerminalInMemoryCommandStore>(new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.Unicode))
                    .DefineCommand<MockCommandRunner>("cmd1", "cmd1", "test1", CommandType.SubCommand, CommandFlags.None)
                        .DefineOption("id1", nameof(Int32), "test opt1", OptionFlags.None, "alias_id1").Add()
                    .Checker<MockCommandChecker>()
                    .Add()
                    .DefineCommand<MockCommandRunner>("cmd2", "cmd2", "test2", CommandType.SubCommand, CommandFlags.None)
                        .DefineOption("id1", nameof(Int32), "test opt1", OptionFlags.None, "alias_id1").Add()
                        .DefineOption("id2", nameof(Int32), "test opt2", OptionFlags.None, "alias_id2").Add()
                        .DefineOption("id3", nameof(Boolean), "test opt3", OptionFlags.None).Add()
                    .Checker<MockCommandChecker>()
                    .Add()
                    .DefineCommand<MockCommandRunner>("cmd3", "cmd3", "test1", CommandType.SubCommand, CommandFlags.None)
                        .DefineOption("id1", nameof(Int32), "test opt1", OptionFlags.None, "alias_id1").Add()
                    .Checker<MockCommandChecker>()
                    .Add();

                // Replace with Mock DIs
                services.AddSingleton<ILicenseExtractor>(mockLicenseExtractor);
                services.AddSingleton<ILicenseChecker>(mockLicenseChecker);
                services.AddSingleton<IConfigurationOptionsChecker>(mockOptionsChecker);
                services.AddSingleton<ITerminalTextHandler, TerminalTextHandler>();
            });
            host = await hostBuilder.StartAsync();

            defaultCliHostedService = new MockTerminalHostedService(host.Services, terminalIOptions, mockTerminalConsole, mockExceptionPublisher, logger);
            await defaultCliHostedService.StartAsync(CancellationToken.None);

            var commandDescriptors = host.Services.GetServices<CommandDescriptor>();
            commandDescriptors.Should().NotBeEmpty();
            foreach (var commandDescriptor in commandDescriptors)
            {
                bool foundHelp = commandDescriptor.OptionDescriptors!.TryGetValue(terminalOptions.Help.OptionId, out OptionDescriptor? helpAttr);
                foundHelp.Should().BeFalse();
                helpAttr.Should().BeNull();
            }
        }

        [Fact]
        public async Task StartAsync_ShouldRegister_Application_EventsAsync()
        {
            IHostApplicationLifetime hostApplicationLifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();

            // Create a host builder with mock event hosted service
            hostBuilder = Host.CreateDefaultBuilder();
            hostBuilder.ConfigureServices(services =>
            {
                // Make sure we use the instance created for test
                services.AddHostedService(EventsHostedService);
            });

            mockCliEventsHostedService.OnStartedCalled.Should().BeFalse();
            host = await hostBuilder.StartAsync();
            mockCliEventsHostedService.OnStartedCalled.Should().BeTrue();

            mockCliEventsHostedService.OnStoppingCalled.Should().BeFalse();
            mockCliEventsHostedService.OnStoppedCalled.Should().BeFalse();
            hostApplicationLifetime.StopApplication();
            mockCliEventsHostedService.OnStoppingCalled.Should().BeTrue();

            // TODO: OnStopped not called, check with dotnet team
            //mockCliEventsHostedService.OnStoppedCalled.Should().BeTrue();
        }

        [Fact]
        public async Task StartAsync_ShouldRegister_HelpArgument_ByDefault()
        {
            TerminalOptions terminalOptions = MockTerminalOptions.NewAliasOptions();
            var terminalIOptions = Microsoft.Extensions.Options.Options.Create<TerminalOptions>(terminalOptions);
            TerminalTextHandler textHandler = new(StringComparison.OrdinalIgnoreCase, Encoding.Unicode);

            hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddTerminal<TerminalInMemoryCommandStore>(textHandler)
                    .DefineCommand<MockCommandRunner>("cmd1", "cmd1", "test1", CommandType.SubCommand, CommandFlags.None)
                        .Checker<MockCommandChecker>()
                        .Add()
                    .DefineCommand<MockCommandRunner>("cmd2", "cmd2", "test2", CommandType.SubCommand, CommandFlags.None)
                        .DefineOption("id1", nameof(Int32), "test opt1", OptionFlags.None, "alias_id1").Add()
                        .DefineOption("id2", nameof(Int32), "test opt2", OptionFlags.None, "alias_id2").Add()
                        .DefineOption("id3", nameof(Boolean), "test opt3", OptionFlags.None).Add()
                    .Checker<MockCommandChecker>()
                    .Add()
                    .DefineCommand<MockCommandRunner>("cmd1", "cmd1", "test1", CommandType.SubCommand, CommandFlags.None)
                    .Checker<MockCommandChecker>()
                    .Add();

                // Replace with Mock DIs
                services.AddSingleton<ILicenseExtractor>(mockLicenseExtractor);
                services.AddSingleton<ILicenseChecker>(mockLicenseChecker);
                services.AddSingleton<IConfigurationOptionsChecker>(mockOptionsChecker);
                services.AddSingleton<ITerminalTextHandler>(textHandler);
            });
            host = await hostBuilder.StartAsync();

            defaultCliHostedService = new MockTerminalHostedService(host.Services, terminalIOptions, mockTerminalConsole, mockExceptionPublisher, logger);
            await defaultCliHostedService.StartAsync(CancellationToken.None);

            var commandDescriptors = host.Services.GetServices<CommandDescriptor>();
            commandDescriptors.Should().NotBeEmpty();
            foreach (var commandDescriptor in commandDescriptors)
            {
                commandDescriptor.OptionDescriptors.Should().NotBeEmpty();
                OptionDescriptor? helpAttr = commandDescriptor.OptionDescriptors![terminalOptions.Help.OptionAlias];
                helpAttr.Should().NotBeNull();
                helpAttr!.Alias.Should().Be(terminalOptions.Help.OptionAlias);
                helpAttr.Description.Should().Be(terminalOptions.Help.OptionDescription);
            }
        }

        private TerminalHostedService DefaultHostedService(IServiceProvider arg)
        {
            return defaultCliHostedService;
        }

        private MockTerminalEventsHostedService EventsHostedService(IServiceProvider arg)
        {
            return mockCliEventsHostedService;
        }

        private readonly CancellationToken cancellationToken;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly MockTerminalHostedServiceLogger logger = null!;
        private readonly MockTerminalEventsHostedService mockCliEventsHostedService;
        private readonly MockTerminalCustomHostedService mockCustomCliHostedService;
        private readonly MockExceptionPublisher mockExceptionPublisher;
        private readonly MockLicenseChecker mockLicenseChecker;
        private readonly MockLicenseExtractor mockLicenseExtractor;
        private readonly MockOptionsChecker mockOptionsChecker;
        private readonly MockTerminalConsole mockTerminalConsole;
        private TerminalHostedService defaultCliHostedService;
        private IHost host;
        private IHostBuilder hostBuilder;
    }
}
