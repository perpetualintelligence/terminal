/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Commands.Checkers;
using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Integration.Mocks;
using PerpetualIntelligence.Cli.Licensing;
using PerpetualIntelligence.Cli.Mocks;
using PerpetualIntelligence.Protocols.Licensing;
using PerpetualIntelligence.Shared.Extensions;
using System;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PerpetualIntelligence.Cli.Integration
{
    /// <summary>
    /// Run test sequentially because we modify the static Console.SetOut
    /// </summary>
    [Collection("Sequential")]
    public class CliHostedServiceTests : IAsyncLifetime
    {
        public CliHostedServiceTests()
        {
            cancellationTokenSource = new();
            cancellationToken = cancellationTokenSource.Token;

            CliOptions cliOptions = MockCliOptions.NewOptions();
            mockLicenseExtractor = new();
            mockLicenseChecker = new();
            mockOptionsChecker = new();

            hostBuilder = Host.CreateDefaultBuilder().ConfigureServices(services =>
            {
                services.AddSingleton<ILicenseExtractor>(mockLicenseExtractor);
                services.AddSingleton<ILicenseChecker>(mockLicenseChecker);
                services.AddSingleton<IOptionsChecker>(mockOptionsChecker);
                services.AddSingleton<ITextHandler, UnicodeTextHandler>();
            });
            host = hostBuilder.Start();

            // Different hosted services to test behaviors
            defaultCliHostedService = new CliHostedService(host.Services, cliOptions, new LoggerFactory().CreateLogger<CliHostedService>());
            mockCustomCliHostedService = new MockCliCustomHostedService(host.Services, cliOptions, new LoggerFactory().CreateLogger<CliHostedService>());
            mockCliEventsHostedService = new MockCliEventsHostedService(host.Services, cliOptions, new LoggerFactory().CreateLogger<CliHostedService>());
        }

   

        [Fact]
        public void StartAsync_Default_ShouldPrint_AppHeader()
        {
            stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            // use reflection to call
            MethodInfo? printAppHeader = defaultCliHostedService.GetType().GetMethod("PrintHostApplicationHeaderAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(printAppHeader);
            printAppHeader.Invoke(defaultCliHostedService, null);

            string[] printedHeaders = stringWriter.ToString().SplitByNewline();
            printedHeaders.Should().HaveCount(7);
            printedHeaders[0].Should().Be("---------------------------------------------------------------------------------------------");
            printedHeaders[1].Should().Be("Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.");
            printedHeaders[2].Should().Be("For license, terms, and data policies, go to:");
            printedHeaders[3].Should().Be("https://terms.perpetualintelligence.com");
            printedHeaders[4].Should().Be("---------------------------------------------------------------------------------------------");
            printedHeaders[5].Should().Be("Starting server \"urn:oneimlx:cli\" version=1.0.2-local");
            printedHeaders[6].Should().Be("");
        }

        [Fact]
        public void StartAsync_Default_ShouldPrint_LicenseInfo()
        {
            stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            // use reflection to call
            MethodInfo? printLic = defaultCliHostedService.GetType().GetMethod("PrintHostApplicationLicensingAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(printLic);
            printLic.Invoke(defaultCliHostedService, new[] { MockLicenses.TestLicense });

            string[] printedHeaders = stringWriter.ToString().SplitByNewline();
            printedHeaders.Should().HaveCount(9);
            printedHeaders[0].Should().Be("consumer=test_name (test_tenantid)");
            printedHeaders[1].Should().Be("country=");
            printedHeaders[2].Should().Be("subject=");
            printedHeaders[3].Should().Be("license_handler=offline");
            printedHeaders[4].Should().Be("usage=urn:oneimlx:lic:usage:rnd");
            printedHeaders[5].Should().Be("plan=urn:oneimlx:lic:plan:community");
            printedHeaders[6].Should().Be("key_source=urn:oneimlx:lic:source:jsonfile");
            printedHeaders[7].Should().Be("key_file=testLicKey1");
            printedHeaders[8].Should().Be("");
        }

        [Fact]
        public void StartAsync_Default_ShouldPrint_MandatoryLicenseInfoForCommunity_Demo()
        {
            stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            Licensing.License community = new Licensing.License("testp", "testh", LicensePlans.Custom, LicenseUsages.RnD, "tests", "testkey", MockLicenses.TestClaims, MockLicenses.TestLimits, MockLicenses.TestPrice);

            // use reflection to call
            MethodInfo? printLic = defaultCliHostedService.GetType().GetMethod("PrintHostApplicationMandatoryLicensingAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(printLic);
            printLic.Invoke(defaultCliHostedService, new[] { community });

            string[] printedHeaders = stringWriter.ToString().SplitByNewline();
            printedHeaders.Should().HaveCount(2);
            printedHeaders[0].Should().Be("Your demo license is free for RnD, test and evaluation purposes. For production environment, you require a commercial license.");
            printedHeaders[1].Should().Be("");
        }

        [Fact]
        public void StartAsync_Default_ShouldPrint_MandatoryLicenseInfoForCommunity_Educational()
        {
            stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            Licensing.License community = new Licensing.License("testp", "testh", LicensePlans.Community, LicenseUsages.Educational, "tests", "testkey", MockLicenses.TestClaims, MockLicenses.TestLimits, MockLicenses.TestPrice);

            // use reflection to call
            MethodInfo? printLic = defaultCliHostedService.GetType().GetMethod("PrintHostApplicationMandatoryLicensingAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(printLic);
            printLic.Invoke(defaultCliHostedService, new[] { community });

            string[] printedHeaders = stringWriter.ToString().SplitByNewline();
            printedHeaders.Should().HaveCount(2);
            printedHeaders[0].Should().Be("Your community license plan is free for educational purposes. For non-educational or production environment, you require a commercial license.");
            printedHeaders[1].Should().Be("");
        }

        [Fact]
        public void StartAsync_Default_ShouldPrint_MandatoryLicenseInfoForCommunity_RND()
        {
            stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            Licensing.License community = new Licensing.License("testp", "testh", LicensePlans.Community, LicenseUsages.RnD, "tests", "testkey", MockLicenses.TestClaims, MockLicenses.TestLimits, MockLicenses.TestPrice);

            // use reflection to call
            MethodInfo? printLic = defaultCliHostedService.GetType().GetMethod("PrintHostApplicationMandatoryLicensingAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(printLic);
            printLic.Invoke(defaultCliHostedService, new[] { community });

            string[] printedHeaders = stringWriter.ToString().SplitByNewline();
            printedHeaders.Should().HaveCount(2);
            printedHeaders[0].Should().Be("Your community license plan is free for RnD, test, and demo purposes. For production environment, you require a commercial license.");
            printedHeaders[1].Should().Be("");
        }

        [Fact]
        public void StartAsync_Default_ShouldPrint_OnStarted()
        {
            stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            // use reflection to call
            MethodInfo? print = defaultCliHostedService.GetType().GetMethod("OnStarted", BindingFlags.Instance | BindingFlags.NonPublic);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(print);
            print.Invoke(defaultCliHostedService, null);

            string[] printedHeaders = stringWriter.ToString().SplitByNewline();
            printedHeaders.Should().HaveCount(3);
            printedHeaders[0].Should().StartWith("Server started on");
            printedHeaders[1].Should().Be("");
            printedHeaders[2].Should().Be("");
        }

        [Fact]
        public void StartAsync_Default_ShouldPrint_OnStopped()
        {
            stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            // use reflection to call
            MethodInfo? print = defaultCliHostedService.GetType().GetMethod("OnStopped", BindingFlags.Instance | BindingFlags.NonPublic);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(print);
            print.Invoke(defaultCliHostedService, null);

            string[] printedHeaders = stringWriter.ToString().SplitByNewline();
            printedHeaders.Should().HaveCount(2);
            printedHeaders[0].Should().StartWith("Server stopped on");
            printedHeaders[1].Should().Be("");
        }

        [Fact]
        public void StartAsync_Default_ShouldPrint_OnStopping()
        {
            stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            // use reflection to call
            MethodInfo? print = defaultCliHostedService.GetType().GetMethod("OnStopping", BindingFlags.Instance | BindingFlags.NonPublic);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(print);
            print.Invoke(defaultCliHostedService, null);

            string[] printedHeaders = stringWriter.ToString().SplitByNewline();
            printedHeaders.Should().HaveCount(2);
            printedHeaders[0].Should().Be("Stopping server...");
            printedHeaders[1].Should().Be("");
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
            MockCliHostedServiceStaticCounter.Restart();
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
        }

        [Fact]
        public async Task StartAsync_ShouldHandleErrorExceptionCorrectly()
        {
            stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            mockLicenseExtractor.ThrowError = true;
            await defaultCliHostedService.StartAsync(cancellationToken);

            string[] printedHeaders = stringWriter.ToString().SplitByNewline();

            // Last is a new line
            printedHeaders[printedHeaders.Length - 2].Should().Be("test_error=test description. arg1=val1 arg2=val2");
        }

        [Fact]
        public async Task StartAsync_ShouldRegister_AppEventsAsync()
        {
            IHostApplicationLifetime hostApplicationLifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();

            // Create a host builder with mock event hosted service
            hostBuilder = Host.CreateDefaultBuilder();
            hostBuilder.ConfigureServices(services =>
            {
                // Make sure we use the instance created for test
                services.AddHostedService(CreateEventsHostedService);
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

        private MockCliEventsHostedService CreateEventsHostedService(IServiceProvider arg)
        {
            return mockCliEventsHostedService;
        }

        public Task InitializeAsync()
        {
            originalWriter = Console.Out;
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            Console.SetOut(originalWriter);

            host.Dispose();

            if (stringWriter != null)
            {
                stringWriter.Dispose();
            }

            return Task.CompletedTask;
        }

        private CancellationToken cancellationToken;
        private CancellationTokenSource cancellationTokenSource;
        private CliHostedService defaultCliHostedService;
        private IHost host;
        private IHostBuilder hostBuilder;
        private MockCliEventsHostedService mockCliEventsHostedService;
        private MockCliCustomHostedService mockCustomCliHostedService;
        private MockLicenseChecker mockLicenseChecker;
        private MockLicenseExtractor mockLicenseExtractor;
        private MockOptionsChecker mockOptionsChecker;
        private StringWriter? stringWriter = null!;
        private TextWriter originalWriter =null!;
    }
}
