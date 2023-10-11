/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using PerpetualIntelligence.Shared.Licensing;
using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Terminal.Mocks;
using PerpetualIntelligence.Terminal.Stores;
using PerpetualIntelligence.Terminal.Stores.InMemory;
using PerpetualIntelligence.Test.Services;
using System.Threading.Tasks;
using Xunit;

namespace PerpetualIntelligence.Terminal.Licensing
{
    public class LicenseCheckerTests
    {
        public LicenseCheckerTests()
        {
            terminalOptions = MockTerminalOptions.NewLegacyOptions();
            commandStoreHandler = new InMemoryCommandStore(MockCommands.LicensingCommands.TextHandler, MockCommands.LicensingCommands.Values);
            licenseChecker = new LicenseChecker(commandStoreHandler, terminalOptions, TestLogger.Create<LicenseChecker>());
            license = new License("testProviderId2", TerminalHandlers.OnlineLicenseHandler, TerminalLicensePlans.Unlimited, LicenseUsages.RnD, LicenseSources.JsonFile, "testLicKey2", MockLicenses.TestClaims, LicenseLimits.Create(TerminalLicensePlans.Unlimited), LicensePrice.Create(TerminalLicensePlans.Unlimited));
        }

        [Fact]
        public async Task CheckAsync_DataTypeCheck_ShouldBehaveCorrectly()
        {
            terminalOptions.Handler.DataTypeHandler = "default";
            await licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license));

            terminalOptions.Handler.DataTypeHandler = "custom";
            await licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license));

            // Null allowed
            terminalOptions.Handler.DataTypeHandler = null;
            await licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license));

            // Invalid value should error
            terminalOptions.Handler.DataTypeHandler = "test4";
            await TestHelper.AssertThrowsErrorExceptionAsync<TerminalException>(() => licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license)), TerminalErrors.InvalidLicense, "The configured data-type handler is not allowed for your license edition. datatype_handler=test4");
        }

        [Fact]
        public async Task CheckAsync_ErrorHandling_ShouldBehaveCorrectly()
        {
            terminalOptions.Handler.ErrorHandler = "default";
            await licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license));

            terminalOptions.Handler.ErrorHandler = "custom";
            await licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license));

            // Null not allowed
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            terminalOptions.Handler.ErrorHandler = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            await TestHelper.AssertThrowsErrorExceptionAsync<TerminalException>(() => licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license)), TerminalErrors.InvalidLicense, "The configured error handler is not allowed for your license edition. error_handler=");

            // Invalid value should error
            terminalOptions.Handler.ErrorHandler = "test4";
            await TestHelper.AssertThrowsErrorExceptionAsync<TerminalException>(() => licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license)), TerminalErrors.InvalidLicense, "The configured error handler is not allowed for your license edition. error_handler=test4");
        }

        [Fact]
        public async Task CheckAsync_ExceededOptionLimit_ShouldError()
        {
            // Args 13
            license.Limits.OptionLimit = 2;
            await TestHelper.AssertThrowsErrorExceptionAsync<TerminalException>(() => licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license)), TerminalErrors.InvalidLicense, "The option limit exceeded. max_limit=2 current=90");
        }

        [Fact]
        public async Task CheckAsync_ExceededCommandGroupLimit_ShouldError()
        {
            // grouped commands are 3
            license.Limits.GroupedCommandLimit = 2;
            await TestHelper.AssertThrowsErrorExceptionAsync<TerminalException>(() => licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license)), TerminalErrors.InvalidLicense, "The grouped command limit exceeded. max_limit=2 current=3");
        }

        [Fact]
        public async Task CheckAsync_ExceededTerminalLimit_ShouldError()
        {
            // TODO
            // For now the terminal count are 1 always
            license.Limits.TerminalLimit = 0;
            await TestHelper.AssertThrowsErrorExceptionAsync<TerminalException>(() => licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license)), TerminalErrors.InvalidLicense, "The terminal limit exceeded. max_limit=0 current=1");
        }

        [Fact]
        public async Task CheckAsync_ExceededRootCommandLimit_ShouldError()
        {
            // Root Commands are 3
            license.Limits.RootCommandLimit = 2;
            await TestHelper.AssertThrowsErrorExceptionAsync<TerminalException>(() => licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license)), TerminalErrors.InvalidLicense, "The root command limit exceeded. max_limit=2 current=3");
        }

        [Fact]
        public async Task CheckAsync_ExceededSubCommandLimit_ShouldError()
        {
            // Subs commands 5
            license.Limits.SubCommandLimit = 2;
            await TestHelper.AssertThrowsErrorExceptionAsync<TerminalException>(() => licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license)), TerminalErrors.InvalidLicense, "The sub command limit exceeded. max_limit=2 current=5");
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
            license.Limits.DataTypeHandlers = new[] { "test1", "test2", "test3" };

            // Bull option should not error
            terminalOptions.Handler.DataTypeHandler = null;
            await licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license));

            // Valid value should not error
            terminalOptions.Handler.DataTypeHandler = "test2";
            await licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license));

            // Invalid value should error
            terminalOptions.Handler.DataTypeHandler = "test4";
            await TestHelper.AssertThrowsErrorExceptionAsync<TerminalException>(() => licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license)), TerminalErrors.InvalidLicense, "The configured data-type handler is not allowed for your license edition. datatype_handler=test4");

            // Null limit options but configured option should error
            license.Limits.DataTypeHandlers = null;
            terminalOptions.Handler.DataTypeHandler = "test5";
            await TestHelper.AssertThrowsErrorExceptionAsync<TerminalException>(() => licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license)), TerminalErrors.InvalidLicense, "The configured data-type handler is not allowed for your license edition. datatype_handler=test5");
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
            await TestHelper.AssertThrowsErrorExceptionAsync<TerminalException>(() => licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license)), TerminalErrors.InvalidLicense, "The configured service handler is not allowed for your license edition. service_handler=");

            // Invalid value should error
            terminalOptions.Handler.ServiceHandler = "test4";
            await TestHelper.AssertThrowsErrorExceptionAsync<TerminalException>(() => licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license)), TerminalErrors.InvalidLicense, "The configured service handler is not allowed for your license edition. service_handler=test4");
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
            await TestHelper.AssertThrowsErrorExceptionAsync<TerminalException>(() => licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license)), TerminalErrors.InvalidLicense, "The configured license handler is not allowed for your license edition. license_handler=");

            // Invalid value should error
            terminalOptions.Handler.LicenseHandler = "test4";
            await TestHelper.AssertThrowsErrorExceptionAsync<TerminalException>(() => licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license)), TerminalErrors.InvalidLicense, "The configured license handler is not allowed for your license edition. license_handler=test4");
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
            await TestHelper.AssertThrowsErrorExceptionAsync<TerminalException>(() => licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license)), TerminalErrors.InvalidLicense, "The configured store handler is not allowed for your license edition. store_handler=");

            // Invalid value should error
            terminalOptions.Handler.StoreHandler = "test4";
            await TestHelper.AssertThrowsErrorExceptionAsync<TerminalException>(() => licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license)), TerminalErrors.InvalidLicense, "The configured store handler is not allowed for your license edition. store_handler=test4");
        }

        [Fact]
        public async Task CheckAsync_StrictTypeCheck_ShouldBehaveCorrectly()
        {
            // Error, not allowed but configured
            license.Limits.StrictDataType = false;
            terminalOptions.Checker.StrictValueType = true;
            await TestHelper.AssertThrowsErrorExceptionAsync<TerminalException>(() => licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license)), TerminalErrors.InvalidLicense, "The configured strict option value type is not allowed for your license edition.");

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

        [Fact]
        public async Task CheckAsync_TextHandler_ShouldBehaveCorrectly()
        {
            terminalOptions.Handler.TextHandler = "unicode";
            await licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license));

            terminalOptions.Handler.TextHandler = "ascii";
            await licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license));

            // Null not allowed
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            terminalOptions.Handler.TextHandler = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            await TestHelper.AssertThrowsErrorExceptionAsync<TerminalException>(() => licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license)), TerminalErrors.InvalidLicense, "The configured text handler is not allowed for your license edition. text_handler=");

            // Invalid value should error
            terminalOptions.Handler.TextHandler = "test4";
            await TestHelper.AssertThrowsErrorExceptionAsync<TerminalException>(() => licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(license)), TerminalErrors.InvalidLicense, "The configured text handler is not allowed for your license edition. text_handler=test4");
        }

        private readonly TerminalOptions terminalOptions;
        private ICommandStoreHandler commandStoreHandler;
        private readonly License license;
        private readonly ILicenseChecker licenseChecker;
    }
}