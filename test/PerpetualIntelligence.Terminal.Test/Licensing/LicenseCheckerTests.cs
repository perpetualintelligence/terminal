/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using PerpetualIntelligence.Shared.Licensing;
using PerpetualIntelligence.Terminal.Commands;
using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Terminal.Mocks;
using PerpetualIntelligence.Test.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace PerpetualIntelligence.Terminal.Licensing
{
    public class LicenseCheckerTests
    {
        public LicenseCheckerTests()
        {
            terminalOptions = MockTerminalOptions.NewLegacyOptions();
            commandDescriptors = MockCommands.LicensingCommands;
            licenseChecker = new LicenseChecker(commandDescriptors, terminalOptions, TestLogger.Create<LicenseChecker>());
            license = new License("testProviderId2", Handlers.OnlineLicenseHandler, PiCliLicensePlans.Unlimited, LicenseUsages.RnD, LicenseSources.JsonFile, "testLicKey2", MockLicenses.TestClaims, LicenseLimits.Create(PiCliLicensePlans.Unlimited), LicensePrice.Create(PiCliLicensePlans.Unlimited));
        }

        [Fact]
        public async Task CheckAsync_DataTypeCheck_ShouldBehaveCorrectly()
        {
            terminalOptions.Handler.DataTypeHandler = "default";
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            terminalOptions.Handler.DataTypeHandler = "custom";
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // Null allowed
            terminalOptions.Handler.DataTypeHandler = null;
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // Invalid value should error
            terminalOptions.Handler.DataTypeHandler = "test4";
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured data-type handler is not allowed for your license edition. datatype_handler=test4");
        }

        [Fact]
        public async Task CheckAsync_OptionAlias_ShouldBehaveCorrectly()
        {
            // Error, not allowed but configured
            license.Limits.OptionAlias = false;
            terminalOptions.Extractor.OptionAlias = true;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured option alias is not allowed for your license edition.");

            // No error, not allowed not configured
            license.Limits.OptionAlias = false;
            terminalOptions.Extractor.OptionAlias = false;
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // No error, not allowed not configured
            license.Limits.OptionAlias = false;
            terminalOptions.Extractor.OptionAlias = null;
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // No error, allowed not configured
            license.Limits.OptionAlias = true;
            terminalOptions.Extractor.OptionAlias = false;
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // No error, allowed and configured
            license.Limits.OptionAlias = true;
            terminalOptions.Extractor.OptionAlias = true;
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));
        }

        [Fact]
        public async Task CheckAsync_DefaultOption_ShouldBehaveCorrectly()
        {
            // Error, not allowed but configured
            license.Limits.DefaultOption = false;
            terminalOptions.Extractor.DefaultOption = true;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured default option is not allowed for your license edition.");

            // No error, not allowed not configured
            license.Limits.DefaultOption = false;
            terminalOptions.Extractor.DefaultOption = false;
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // No error, not allowed not configured
            license.Limits.DefaultOption = false;
            terminalOptions.Extractor.DefaultOption = null;
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // No error, allowed not configured
            license.Limits.DefaultOption = true;
            terminalOptions.Extractor.DefaultOption = false;
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // No error, allowed and configured
            license.Limits.DefaultOption = true;
            terminalOptions.Extractor.DefaultOption = true;
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));
        }

        [Fact]
        public async Task CheckAsync_DefaultOptionValue_ShouldBehaveCorrectly()
        {
            // Error, not allowed but configured
            license.Limits.DefaultOptionValue = false;
            terminalOptions.Extractor.DefaultOptionValue = true;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured default option value is not allowed for your license edition.");

            // No error, not allowed not configured
            license.Limits.DefaultOptionValue = false;
            terminalOptions.Extractor.DefaultOptionValue = false;
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // No error, not allowed not configured
            license.Limits.DefaultOptionValue = false;
            terminalOptions.Extractor.DefaultOptionValue = null;
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // No error, allowed not configured
            license.Limits.DefaultOptionValue = true;
            terminalOptions.Extractor.DefaultOptionValue = false;
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // No error, allowed and configured
            license.Limits.DefaultOptionValue = true;
            terminalOptions.Extractor.DefaultOptionValue = true;
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));
        }

        [Fact]
        public async Task CheckAsync_ErrorHandling_ShouldBehaveCorrectly()
        {
            terminalOptions.Handler.ErrorHandler = "default";
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            terminalOptions.Handler.ErrorHandler = "custom";
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // Null not allowed
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            terminalOptions.Handler.ErrorHandler = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured error handler is not allowed for your license edition. error_handler=");

            // Invalid value should error
            terminalOptions.Handler.ErrorHandler = "test4";
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured error handler is not allowed for your license edition. error_handler=test4");
        }

        [Fact]
        public async Task CheckAsync_ExceededArgumentLimit_ShouldError()
        {
            // Args 13
            license.Limits.OptionLimit = 2;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The option limit exceeded. max_limit=2 current=13");
        }

        [Fact]
        public async Task CheckAsync_ExceededCommandGroupLimit_ShouldError()
        {
            // grouped commands are 3
            license.Limits.GroupedCommandLimit = 2;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The grouped command limit exceeded. max_limit=2 current=3");
        }

        [Fact]
        public async Task CheckAsync_ExceededTerminalLimit_ShouldError()
        {
            // TODO
            // For now the terminal count are 1 always
            license.Limits.TerminalLimit = 0;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The terminal limit exceeded. max_limit=0 current=1");
        }

        [Fact]
        public async Task CheckAsync_ExceededRootCommandLimit_ShouldError()
        {
            // Root Commands are 3
            license.Limits.RootCommandLimit = 2;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The root command limit exceeded. max_limit=2 current=3");
        }

        [Fact]
        public async Task CheckAsync_ExceededSubCommandLimit_ShouldError()
        {
            // Subs commands 14
            license.Limits.SubCommandLimit = 2;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The sub command limit exceeded. max_limit=2 current=14");
        }

        [Fact]
        public async Task CheckAsync_InitializeShouldSetPropertiesCorrectly()
        {
            // Use unlimited license so this will not fail.
            var result = await licenseChecker.CheckAsync(new LicenseCheckerContext(license));
            ((LicenseChecker)licenseChecker).Initialized.Should().Be(true);

            result.License.Should().NotBeNull();
            result.License.Should().BeSameAs(license);

            result.TerminalCount.Should().Be(1);
            result.RootCommandCount.Should().Be(3);
            result.CommandGroupCount.Should().Be(3);
            result.SubCommandCount.Should().Be(14);
            result.OptionCount.Should().Be(13);
        }

        [Fact]
        public async Task CheckAsync_OptionsValid_ShouldBehaveCorrectly()
        {
            // Use Data check as example
            license.Limits.DataTypeHandlers = new[] { "test1", "test2", "test3" };

            // Bull option should not error
            terminalOptions.Handler.DataTypeHandler = null;
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // Valid value should not error
            terminalOptions.Handler.DataTypeHandler = "test2";
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // Invalid value should error
            terminalOptions.Handler.DataTypeHandler = "test4";
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured data-type handler is not allowed for your license edition. datatype_handler=test4");

            // Null limit options but configured option should error
            license.Limits.DataTypeHandlers = null;
            terminalOptions.Handler.DataTypeHandler = "test5";
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured data-type handler is not allowed for your license edition. datatype_handler=test5");
        }

        [Fact]
        public async Task CheckAsync_ServiceHandler_ShouldBehaveCorrectly()
        {
            terminalOptions.Handler.ServiceHandler = "default";
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            terminalOptions.Handler.ServiceHandler = "custom";
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // Null not allowed
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            terminalOptions.Handler.ServiceHandler = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured service handler is not allowed for your license edition. service_handler=");

            // Invalid value should error
            terminalOptions.Handler.ServiceHandler = "test4";
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured service handler is not allowed for your license edition. service_handler=test4");
        }

        [Fact]
        public async Task CheckAsync_LicenseHandler_ShouldBehaveCorrectly()
        {
            terminalOptions.Handler.LicenseHandler = "online-license";
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            terminalOptions.Handler.LicenseHandler = "offline-license";
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            terminalOptions.Handler.LicenseHandler = "onpremise-license";
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // Null not allowed
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            terminalOptions.Handler.LicenseHandler = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured license handler is not allowed for your license edition. license_handler=");

            // Invalid value should error
            terminalOptions.Handler.LicenseHandler = "test4";
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured license handler is not allowed for your license edition. license_handler=test4");
        }

        [Fact]
        public async Task CheckAsync_StoreHandler_ShouldBehaveCorrectly()
        {
            terminalOptions.Handler.StoreHandler = "in-memory";
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            terminalOptions.Handler.StoreHandler = "json";
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            terminalOptions.Handler.StoreHandler = "custom";
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // Null not allowed
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            terminalOptions.Handler.StoreHandler = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured store handler is not allowed for your license edition. store_handler=");

            // Invalid value should error
            terminalOptions.Handler.StoreHandler = "test4";
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured store handler is not allowed for your license edition. store_handler=test4");
        }

        [Fact]
        public async Task CheckAsync_StrictTypeCheck_ShouldBehaveCorrectly()
        {
            // Error, not allowed but configured
            license.Limits.StrictDataType = false;
            terminalOptions.Checker.StrictOptionValueType = true;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured strict option value type is not allowed for your license edition.");

            // No error, not allowed configured false
            license.Limits.StrictDataType = false;
            terminalOptions.Checker.StrictOptionValueType = false;
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // No error, not allowed not configured
            license.Limits.StrictDataType = false;
            terminalOptions.Checker.StrictOptionValueType = null;
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // No error, allowed not configured
            license.Limits.StrictDataType = true;
            terminalOptions.Checker.StrictOptionValueType = false;
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // No error, allowed and configured
            license.Limits.StrictDataType = true;
            terminalOptions.Checker.StrictOptionValueType = true;
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));
        }

        [Fact]
        public async Task CheckAsync_TextHandler_ShouldBehaveCorrectly()
        {
            terminalOptions.Handler.TextHandler = "unicode";
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            terminalOptions.Handler.TextHandler = "ascii";
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // Null not allowed
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            terminalOptions.Handler.TextHandler = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured text handler is not allowed for your license edition. text_handler=");

            // Invalid value should error
            terminalOptions.Handler.TextHandler = "test4";
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured text handler is not allowed for your license edition. text_handler=test4");
        }

        private readonly TerminalOptions terminalOptions;
        private readonly IEnumerable<CommandDescriptor> commandDescriptors;
        private readonly License license;
        private readonly ILicenseChecker licenseChecker;
    }
}