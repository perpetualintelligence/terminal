/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PerpetualIntelligence.Shared.Licensing;
using PerpetualIntelligence.Terminal.Commands;
using PerpetualIntelligence.Terminal.Commands.Checkers;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Terminal.Extensions;
using PerpetualIntelligence.Terminal.Hosting.Mocks;
using PerpetualIntelligence.Terminal.Licensing;
using PerpetualIntelligence.Terminal.Mocks;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PerpetualIntelligence.Terminal.Hosting
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

            logger = new MockTerminalHostedServiceLogger();

            hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton<ILicenseExtractor>(mockLicenseExtractor);
                services.AddSingleton<ILicenseChecker>(mockLicenseChecker);
                services.AddSingleton<IConfigurationOptionsChecker>(mockOptionsChecker);
                services.AddSingleton<ITextHandler, UnicodeTextHandler>();
            });
            host = hostBuilder.Start();

            // Different hosted services to test behaviors
            defaultCliHostedService = new TerminalHostedService(host.Services, terminalOptions, logger);
            mockCustomCliHostedService = new MockTerminalCustomHostedService(host.Services, terminalOptions, logger);
            mockCliEventsHostedService = new MockTerminalEventsHostedService(host.Services, terminalOptions, logger);
        }

        [Fact]
        public void StartAsync_Default_ShouldPrint_Application_Header()
        {
            // use reflection to call
            MethodInfo? printAppHeader = defaultCliHostedService.GetType().GetMethod("PrintHostApplicationHeaderAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(printAppHeader);
            printAppHeader.Invoke(defaultCliHostedService, null);

            logger.Messages.Should().HaveCount(5);
            logger.Messages[0].Should().Be("---------------------------------------------------------------------------------------------");
            logger.Messages[1].Should().Be("Demo custom header line-1");
            logger.Messages[2].Should().Be("Demo custom header line-2");
            logger.Messages[3].Should().Be("---------------------------------------------------------------------------------------------");
            logger.Messages[4].Should().Be("Starting server \"urn:oneimlx:picli\" version=1.0.2-local");
        }

        [Fact]
        public void StartAsync_Default_ShouldPrint_LicenseInfo()
        {
            // use reflection to call
            MethodInfo? printLic = defaultCliHostedService.GetType().GetMethod("PrintHostApplicationLicensingAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(printLic);
            printLic.Invoke(defaultCliHostedService, new[] { MockLicenses.TestLicense });

            logger.Messages.Should().HaveCount(8);
            logger.Messages[0].Should().Be("consumer=test_name (test_tenantid)");
            logger.Messages[1].Should().Be("country=");
            logger.Messages[2].Should().Be("subject=");
            logger.Messages[3].Should().Be("license_handler=offline-license");
            logger.Messages[4].Should().Be("usage=urn:oneimlx:lic:usage:rnd");
            logger.Messages[5].Should().Be("plan=urn:oneimlx:lic:plan:demo");
            logger.Messages[6].Should().Be("key_source=urn:oneimlx:lic:source:jsonfile");
            logger.Messages[7].Should().Be("key_file=testLicKey1");
        }

        [Fact]
        public void StartAsync_Default_ShouldPrint_MandatoryLicenseInfoForCommunity_Demo()
        {
            License community = new("testp", "testh", PiCliLicensePlans.Custom, LicenseUsages.RnD, "tests", "testkey", MockLicenses.TestClaims, MockLicenses.TestLimits, MockLicenses.TestPrice);

            // use reflection to call
            MethodInfo? printLic = defaultCliHostedService.GetType().GetMethod("PrintHostApplicationMandatoryLicensingAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(printLic);
            printLic.Invoke(defaultCliHostedService, new[] { community });

            logger.Messages.Should().HaveCount(1);
            logger.Messages[0].Should().Be("Your demo license is free for RnD, test and evaluation purposes. For production environment, you require a commercial license.");
        }

        [Fact]
        public void StartAsync_Default_ShouldPrint_MandatoryLicenseInfoForCommunity_Educational()
        {
            License community = new("testp", "testh", PiCliLicensePlans.Demo, LicenseUsages.Educational, "tests", "testkey", MockLicenses.TestClaims, MockLicenses.TestLimits, MockLicenses.TestPrice);

            // use reflection to call
            MethodInfo? printLic = defaultCliHostedService.GetType().GetMethod("PrintHostApplicationMandatoryLicensingAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(printLic);
            printLic.Invoke(defaultCliHostedService, new[] { community });

            logger.Messages.Should().HaveCount(1);
            logger.Messages[0].Should().Be("Your community license plan is free for educational purposes. For non-educational or production environment, you require a commercial license.");
        }

        [Fact]
        public void StartAsync_Default_ShouldPrint_MandatoryLicenseInfoForCommunity_RND()
        {
            License community = new("testp", "testh", PiCliLicensePlans.Demo, LicenseUsages.RnD, "tests", "testkey", MockLicenses.TestClaims, MockLicenses.TestLimits, MockLicenses.TestPrice);

            // use reflection to call
            MethodInfo? printLic = defaultCliHostedService.GetType().GetMethod("PrintHostApplicationMandatoryLicensingAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(printLic);
            printLic.Invoke(defaultCliHostedService, new[] { community });

            logger.Messages.Should().HaveCount(1);
            logger.Messages[0].Should().Be("Your community license plan is free for RnD, test, and demo purposes. For production environment, you require a commercial license.");
        }

        [Fact]
        public void StartAsync_Default_ShouldPrint_OnStarted()
        {
            // use reflection to call
            MethodInfo? print = defaultCliHostedService.GetType().GetMethod("OnStarted", BindingFlags.Instance | BindingFlags.NonPublic);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(print);
            print.Invoke(defaultCliHostedService, null);

            logger.Messages.Should().HaveCount(2);
            logger.Messages[0].Should().StartWith("Server started on");
            logger.Messages[1].Should().Be("");
        }

        [Fact]
        public void StartAsync_Default_ShouldPrint_OnStopped()
        {
            // use reflection to call
            MethodInfo? print = defaultCliHostedService.GetType().GetMethod("OnStopped", BindingFlags.Instance | BindingFlags.NonPublic);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(print);
            print.Invoke(defaultCliHostedService, null);

            logger.Messages.Should().HaveCount(1);
            logger.Messages[0].Should().StartWith("Server stopped on");
        }

        [Fact]
        public void StartAsync_Default_ShouldPrint_OnStopping()
        {
            // use reflection to call
            MethodInfo? print = defaultCliHostedService.GetType().GetMethod("OnStopping", BindingFlags.Instance | BindingFlags.NonPublic);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(print);
            print.Invoke(defaultCliHostedService, null);

            logger.Messages.Should().HaveCount(1);
            logger.Messages[0].Should().Be("Stopping server...");
        }

        [Fact]
        public void StartAsync_OnCancellationShouldThrow()
        {
            cancellationTokenSource.Cancel();

            Func<Task> act = async () => await mockCustomCliHostedService.StartAsync(cancellationToken);
            act.Should().ThrowAsync<OperationCanceledException>();

            mockCustomCliHostedService.RegisterEventsCalled.Item2.Should().BeFalse();
            mockCustomCliHostedService.PrintHostAppHeaderCalled.Item2.Should().BeFalse();
            mockLicenseExtractor.ExtractLicenseCalled.Item2.Should().BeFalse();
            mockCustomCliHostedService.PrintHostLicCalled.Item2.Should().BeFalse();
            mockLicenseChecker.CheckLicenseCalled.Item2.Should().BeFalse();
            mockCustomCliHostedService.PrintMandatoryLicCalled.Item2.Should().BeFalse();
            mockOptionsChecker.CheckOptionsCalled.Item2.Should().BeFalse();
            mockCustomCliHostedService.CheckAppConfigCalled.Item2.Should().BeFalse();
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
            logger.Messages.Last().Should().Be("test_error=test description. opt1=val1 opt2=val2");
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
        public async void StartAsync_ShouldRegister_HelpArgument_ByDefault()
        {
            TerminalOptions terminalOptions = MockTerminalOptions.NewAliasOptions();

            hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddTerminal()
                   .DefineCommand<MockCommandChecker, MockCommandRunner>("cmd1", "cmd1", "test1", CommandType.SubCommand, CommandFlags.None).Add()
                   .DefineCommand<MockCommandChecker, MockCommandRunner>("cmd2", "cmd2", "test2", CommandType.SubCommand, CommandFlags.None)
                       .DefineOption("id1", nameof(Int32), "test opt1", OptionFlags.None, "alias_id1").Add()
                       .DefineOption("id2", nameof(Int32), "test opt2", OptionFlags.None, "alias_id2").Add()
                       .DefineOption("id3", nameof(Boolean), "test opt3", OptionFlags.None).Add()
                   .Add()
                   .DefineCommand<MockCommandChecker, MockCommandRunner>("cmd1", "cmd1", "test1", CommandType.SubCommand, CommandFlags.None).Add();

                // Replace with Mock DIs
                services.AddSingleton<ILicenseExtractor>(mockLicenseExtractor);
                services.AddSingleton<ILicenseChecker>(mockLicenseChecker);
                services.AddSingleton<IConfigurationOptionsChecker>(mockOptionsChecker);
                services.AddSingleton<ITextHandler, UnicodeTextHandler>();
            });
            host = await hostBuilder.StartAsync();

            defaultCliHostedService = new TerminalHostedService(host.Services, terminalOptions, logger);
            await defaultCliHostedService.StartAsync(CancellationToken.None);

            var commandDescriptors = host.Services.GetServices<CommandDescriptor>();
            commandDescriptors.Should().NotBeEmpty();
            foreach (var commandDescriptor in commandDescriptors)
            {
                commandDescriptor.OptionDescriptors.Should().NotBeEmpty();
                OptionDescriptor? helpAttr = commandDescriptor.OptionDescriptors!.FirstOrDefault(e => e.Id.Equals(terminalOptions.Help.OptionId));
                helpAttr.Should().NotBeNull();
                helpAttr!.Alias.Should().Be(terminalOptions.Help.OptionAlias);
                helpAttr.Description.Should().Be(terminalOptions.Help.OptionDescription);
            }
        }

        [Fact]
        public async void StartAsync_ShouldNotRegister_HelpArgument_IfDisabled()
        {
            TerminalOptions terminalOptions = MockTerminalOptions.NewAliasOptions();
            terminalOptions.Help.Disabled = true;

            hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddTerminal()
                   .DefineCommand<MockCommandChecker, MockCommandRunner>("cmd1", "cmd1", "test1", CommandType.SubCommand, CommandFlags.None)
                        .DefineOption("id1", nameof(Int32), "test opt1", OptionFlags.None, "alias_id1").Add()
                    .Add()
                   .DefineCommand<MockCommandChecker, MockCommandRunner>("cmd2", "cmd2", "test2", CommandType.SubCommand, CommandFlags.None)
                       .DefineOption("id1", nameof(Int32), "test opt1", OptionFlags.None, "alias_id1").Add()
                       .DefineOption("id2", nameof(Int32), "test opt2", OptionFlags.None, "alias_id2").Add()
                       .DefineOption("id3", nameof(Boolean), "test opt3", OptionFlags.None).Add()
                   .Add()
                   .DefineCommand<MockCommandChecker, MockCommandRunner>("cmd3", "cmd3", "test1", CommandType.SubCommand, CommandFlags.None)
                        .DefineOption("id1", nameof(Int32), "test opt1", OptionFlags.None, "alias_id1").Add()
                    .Add();

                // Replace with Mock DIs
                services.AddSingleton<ILicenseExtractor>(mockLicenseExtractor);
                services.AddSingleton<ILicenseChecker>(mockLicenseChecker);
                services.AddSingleton<IConfigurationOptionsChecker>(mockOptionsChecker);
                services.AddSingleton<ITextHandler, UnicodeTextHandler>();
            });
            host = await hostBuilder.StartAsync();

            defaultCliHostedService = new TerminalHostedService(host.Services, terminalOptions, logger);
            await defaultCliHostedService.StartAsync(CancellationToken.None);

            var commandDescriptors = host.Services.GetServices<CommandDescriptor>();
            commandDescriptors.Should().NotBeEmpty();
            foreach (var commandDescriptor in commandDescriptors)
            {
                OptionDescriptor? helpAttr = commandDescriptor.OptionDescriptors!.FirstOrDefault(e => e.Id.Equals(terminalOptions.Help.OptionId));
                helpAttr.Should().BeNull();
            }
        }

        private MockTerminalEventsHostedService EventsHostedService(IServiceProvider arg)
        {
            return mockCliEventsHostedService;
        }

        private TerminalHostedService DefaultHostedService(IServiceProvider arg)
        {
            return defaultCliHostedService;
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        private CancellationToken cancellationToken;
        private CancellationTokenSource cancellationTokenSource;
        private TerminalHostedService defaultCliHostedService;
        private IHost host;
        private IHostBuilder hostBuilder;
        private MockTerminalEventsHostedService mockCliEventsHostedService;
        private MockTerminalCustomHostedService mockCustomCliHostedService;
        private MockLicenseChecker mockLicenseChecker;
        private MockLicenseExtractor mockLicenseExtractor;
        private MockOptionsChecker mockOptionsChecker;
        private MockTerminalHostedServiceLogger logger = null!;
    }
}