/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.Logging;
using OneImlx.Shared.Licensing;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Stores;
using OneImlx.Test.FluentAssertions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Licensing
{
    public class LicenseCheckerTests
    {
        public LicenseCheckerTests()
        {
            terminalOptions = MockTerminalOptions.NewLegacyOptions();
            commandStore = new InMemoryImmutableCommandStore(MockCommands.LicensingCommands.TextHandler, MockCommands.LicensingCommands.Values);
            licenseChecker = new LicenseChecker(commandStore, terminalOptions, new LoggerFactory().CreateLogger<LicenseChecker>());
            license = new License(TerminalHandlers.OnlineLicenseHandler, TerminalLicensePlans.Unlimited, LicenseUsage.RnD, "testLicKey2", MockLicenses.TestClaims, LicenseLimits.Create(TerminalLicensePlans.Unlimited), LicensePrice.Create(TerminalLicensePlans.Unlimited));
        }

        [Fact]
        public async Task CheckAsync_ExceededOptionLimit_ShouldError()
        {
            // Args 13
            license.Limits.OptionLimit = 2;
            Func<Task> func = async () => await licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license));
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidLicense).WithErrorDescription("The option limit exceeded. max_limit=2 current=90");
        }

        [Fact]
        public async Task CheckAsync_ExceededCommandGroupLimit_ShouldError()
        {
            // grouped commands are 3
            license.Limits.GroupedCommandLimit = 2;
            Func<Task> func = async () => await licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license));
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidLicense).WithErrorDescription("The grouped command limit exceeded. max_limit=2 current=3");
        }

        [Fact]
        public async Task CheckAsync_ExceededTerminalLimit_ShouldError()
        {
            // TODO
            // For now the terminal count are 1 always
            license.Limits.TerminalLimit = 0;
            Func<Task> func = async () => await licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license));
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidLicense).WithErrorDescription("The terminal limit exceeded. max_limit=0 current=1");
        }

        [Fact]
        public async Task CheckAsync_ExceededRootCommandLimit_ShouldError()
        {
            // Root Commands are 3
            license.Limits.RootCommandLimit = 2;
            Func<Task> func = async () => await licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license));
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidLicense).WithErrorDescription("The root command limit exceeded. max_limit=2 current=3");
        }

        [Fact]
        public async Task CheckAsync_ExceededSubCommandLimit_ShouldError()
        {
            // Subs commands 5
            license.Limits.SubCommandLimit = 2;
            Func<Task> func = async () => await licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license));
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidLicense).WithErrorDescription("The sub command limit exceeded. max_limit=2 current=5");
        }

        [Fact]
        public async Task CheckAsync_InitializeShouldSetPropertiesCorrectly()
        {
            // Use unlimited license so this will not fail.
            var result = await licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license));
            ((LicenseChecker)licenseChecker).Initialized.Should().Be(true);

            result.License.Should().NotBeNull();
            result.License.Should().BeSameAs(license);

            result.TerminalCount.Should().Be(1);
            result.RootCommandCount.Should().Be(3);
            result.CommandGroupCount.Should().Be(3);
            result.SubCommandCount.Should().Be(5);
            result.OptionCount.Should().Be(90);
        }

        [Fact]
        public async Task CheckAsync_OptionsValid_ShouldBehaveCorrectly()
        {
            // Use Data check as example
            license.Limits.ServiceHandlers = new[] { "test1", "test2", "test3" };

            // Valid value should not error
            terminalOptions.Handler.ServiceHandler = "test2";
            await licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license));

            // Invalid value should error
            terminalOptions.Handler.ServiceHandler = "test4";
            Func<Task> func = async () => await licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license));
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidLicense).WithErrorDescription("The configured service handler is not allowed for your license edition. service_handler=test4");

            // Null limit options but configured option should error
            license.Limits.ServiceHandlers = null;
            terminalOptions.Handler.ServiceHandler = "test5";
            func = async () => await licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license));
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidLicense).WithErrorDescription("The configured service handler is not allowed for your license edition. service_handler=test5");
        }

        [Fact]
        public async Task CheckAsync_ServiceHandler_ShouldBehaveCorrectly()
        {
            terminalOptions.Handler.ServiceHandler = "default";
            await licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license));

            terminalOptions.Handler.ServiceHandler = "custom";
            await licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license));

            // Null not allowed
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            terminalOptions.Handler.ServiceHandler = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            Func<Task> func = async () => await licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license));
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidLicense).WithErrorDescription("The configured service handler is not allowed for your license edition. service_handler=");

            // Invalid value should error
            terminalOptions.Handler.ServiceHandler = "test4";
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidLicense).WithErrorDescription("The configured service handler is not allowed for your license edition. service_handler=test4");
        }

        [Fact]
        public async Task CheckAsync_LicenseHandler_ShouldBehaveCorrectly()
        {
            terminalOptions.Handler.LicenseHandler = "online-license";
            await licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license));

            terminalOptions.Handler.LicenseHandler = "offline-license";
            await licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license));

            terminalOptions.Handler.LicenseHandler = "onpremise-license";
            await licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license));

            // Null not allowed
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            terminalOptions.Handler.LicenseHandler = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            Func<Task> func = async () => await licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license));
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidLicense).WithErrorDescription("The configured license handler is not allowed for your license edition. license_handler=");

            // Invalid value should error
            terminalOptions.Handler.LicenseHandler = "test4";
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidLicense).WithErrorDescription("The configured license handler is not allowed for your license edition. license_handler=test4");
        }

        [Fact]
        public async Task CheckAsync_StoreHandler_ShouldBehaveCorrectly()
        {
            terminalOptions.Handler.StoreHandler = "in-memory";
            await licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license));

            terminalOptions.Handler.StoreHandler = "json";
            await licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license));

            terminalOptions.Handler.StoreHandler = "custom";
            await licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license));

            // Null not allowed
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            terminalOptions.Handler.StoreHandler = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            Func<Task> func = async () => await licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license));
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidLicense).WithErrorDescription("The configured store handler is not allowed for your license edition. store_handler=");

            // Invalid value should error
            terminalOptions.Handler.StoreHandler = "test4";
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidLicense).WithErrorDescription("The configured store handler is not allowed for your license edition. store_handler=test4");
        }

        [Fact]
        public async Task CheckAsync_StrictTypeCheck_ShouldBehaveCorrectly()
        {
            // Error, not allowed but configured
            license.Limits.StrictDataType = false;
            terminalOptions.Checker.StrictValueType = true;
            Func<Task> func = async () => await licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license));
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidLicense).WithErrorDescription("The configured strict option value type is not allowed for your license edition.");

            // No error, not allowed configured false
            license.Limits.StrictDataType = false;
            terminalOptions.Checker.StrictValueType = false;
            await licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license));

            // No error, not allowed not configured
            license.Limits.StrictDataType = false;
            terminalOptions.Checker.StrictValueType = null;
            await licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license));

            // No error, allowed not configured
            license.Limits.StrictDataType = true;
            terminalOptions.Checker.StrictValueType = false;
            await licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license));

            // No error, allowed and configured
            license.Limits.StrictDataType = true;
            terminalOptions.Checker.StrictValueType = true;
            await licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license));
        }

        private readonly TerminalOptions terminalOptions;
        private IImmutableCommandStore commandStore;
        private readonly License license;
        private readonly ILicenseChecker licenseChecker;
    }
}