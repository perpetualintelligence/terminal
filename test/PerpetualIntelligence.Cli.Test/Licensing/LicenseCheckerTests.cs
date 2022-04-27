/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using FluentAssertions;
using PerpetualIntelligence.Cli.Commands;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Mocks;
using PerpetualIntelligence.Protocols.Licensing;
using PerpetualIntelligence.Test.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace PerpetualIntelligence.Cli.Licensing
{
    public class LicenseCheckerTests
    {
        public LicenseCheckerTests()
        {
            cliOptions = MockCliOptions.New();
            commandDescriptors = MockCommands.LicensingCommands;
            licenseChecker = new LicenseChecker(commandDescriptors, cliOptions, TestLogger.Create<LicenseChecker>());
            license = new License("testProviderId2", Handlers.OnlineHandler,  SaaSPlans.ISVU, SaaSUsages.RnD, SaaSKeySources.JsonFile, "testLicKey2", MockLicenses.TestClaims, LicenseLimits.Create(SaaSPlans.ISVU), LicensePrice.Create(SaaSPlans.ISVU));
        }

        [Fact]
        public async Task CheckAsync_DataTypeCheck_ShouldBehaveCorrectly()
        {
            cliOptions.Handler.DataTypeHandler = "default";
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            cliOptions.Handler.DataTypeHandler = "custom";
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // Null allowed
            cliOptions.Handler.DataTypeHandler = null;
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // Invalid value should error
            cliOptions.Handler.DataTypeHandler = "test4";
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured data-type handler is not allowed for your license edition. datatype_handler=test4");
        }

        [Fact]
        public async Task CheckAsync_ArgumentAlias_ShouldBehaveCorrectly()
        {
            // Error, not allowed but configured
            license.Limits.ArgumentAlias = false;
            cliOptions.Extractor.ArgumentAlias = true;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured argument alias option is not allowed for your license edition.");

            // No error, not allowed not configured
            license.Limits.ArgumentAlias = false;
            cliOptions.Extractor.ArgumentAlias = false;
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // No error, not allowed not configured
            license.Limits.ArgumentAlias = false;
            cliOptions.Extractor.ArgumentAlias = null;
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // No error, allowed not configured
            license.Limits.ArgumentAlias = true;
            cliOptions.Extractor.ArgumentAlias = false;
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // No error, allowed and configured
            license.Limits.ArgumentAlias = true;
            cliOptions.Extractor.ArgumentAlias = true;
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));
        }


        [Fact]
        public async Task CheckAsync_DefaultArgument_ShouldBehaveCorrectly()
        {
            // Error, not allowed but configured
            license.Limits.DefaultArgument = false;
            cliOptions.Extractor.DefaultArgument = true;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured default argument option is not allowed for your license edition.");

            // No error, not allowed not configured
            license.Limits.DefaultArgument = false;
            cliOptions.Extractor.DefaultArgument = false;
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // No error, not allowed not configured
            license.Limits.DefaultArgument = false;
            cliOptions.Extractor.DefaultArgument = null;
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // No error, allowed not configured
            license.Limits.DefaultArgument = true;
            cliOptions.Extractor.DefaultArgument = false;
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // No error, allowed and configured
            license.Limits.DefaultArgument = true;
            cliOptions.Extractor.DefaultArgument = true;
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));
        }

        [Fact]
        public async Task CheckAsync_DefaultArgumentValue_ShouldBehaveCorrectly()
        {
            // Error, not allowed but configured
            license.Limits.DefaultArgumentValue = false;
            cliOptions.Extractor.DefaultArgumentValue = true;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured default argument value option is not allowed for your license edition.");

            // No error, not allowed not configured
            license.Limits.DefaultArgumentValue = false;
            cliOptions.Extractor.DefaultArgumentValue = false;
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // No error, not allowed not configured
            license.Limits.DefaultArgumentValue = false;
            cliOptions.Extractor.DefaultArgumentValue = null;
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // No error, allowed not configured
            license.Limits.DefaultArgumentValue = true;
            cliOptions.Extractor.DefaultArgumentValue = false;
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // No error, allowed and configured
            license.Limits.DefaultArgumentValue = true;
            cliOptions.Extractor.DefaultArgumentValue = true;
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));
        }

        [Fact]
        public async Task CheckAsync_ErrorHandling_ShouldBehaveCorrectly()
        {
            cliOptions.Handler.ErrorHandler = "default";
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            cliOptions.Handler.ErrorHandler = "custom";
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // Null not allowed
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            cliOptions.Handler.ErrorHandler = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured error handler is not allowed for your license edition. error_handler=");

            // Invalid value should error
            cliOptions.Handler.ErrorHandler = "test4";
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured error handler is not allowed for your license edition. error_handler=test4");
        }

        [Fact]
        public async Task CheckAsync_ExceededArgumentLimit_ShouldError()
        {
            // Args 13
            license.Limits.ArgumentLimit = 2;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The argument limit exceeded. max_limit=2 current=13");
        }

        [Fact]
        public async Task CheckAsync_ExceededCommandGroupLimit_ShouldError()
        {
            // Command groups are 3
            license.Limits.GroupedCommandLimit = 2;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The command group limit exceeded. max_limit=2 current=3");
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
            result.ArgumentCount.Should().Be(13);
        }

        [Fact]
        public async Task CheckAsync_OptionsValid_ShouldBehaveCorrectly()
        {
            // Use Data check as example
            license.Limits.DataTypeHandlers = new[] { "test1", "test2", "test3" };

            // Bull option should not error
            cliOptions.Handler.DataTypeHandler = null;
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // Valid value should not error
            cliOptions.Handler.DataTypeHandler = "test2";
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // Invalid value should error
            cliOptions.Handler.DataTypeHandler = "test4";
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured data-type handler is not allowed for your license edition. datatype_handler=test4");

            // Null limit options but configured option should error
            license.Limits.DataTypeHandlers = null;
            cliOptions.Handler.DataTypeHandler = "test5";
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured data-type handler is not allowed for your license edition. datatype_handler=test5");
        }

        [Fact]
        public async Task CheckAsync_ServiceHandler_ShouldBehaveCorrectly()
        {
            cliOptions.Handler.ServiceHandler = "default";
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            cliOptions.Handler.ServiceHandler = "custom";
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // Null not allowed
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            cliOptions.Handler.ServiceHandler = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured service handler is not allowed for your license edition. service_handler=");

            // Invalid value should error
            cliOptions.Handler.ServiceHandler = "test4";
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured service handler is not allowed for your license edition. service_handler=test4");
        }

        [Fact]
        public async Task CheckAsync_LicenseHandler_ShouldBehaveCorrectly()
        {
            cliOptions.Handler.LicenseHandler = "online";
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            cliOptions.Handler.LicenseHandler = "offline";
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            cliOptions.Handler.LicenseHandler = "byol";
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // Null not allowed
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            cliOptions.Handler.LicenseHandler = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured license handler is not allowed for your license edition. license_handler=");

            // Invalid value should error
            cliOptions.Handler.LicenseHandler = "test4";
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured license handler is not allowed for your license edition. license_handler=test4");
        }

        [Fact]
        public async Task CheckAsync_StoreHandler_ShouldBehaveCorrectly()
        {
            cliOptions.Handler.StoreHandler = "in-memory";
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            cliOptions.Handler.StoreHandler = "json";
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            cliOptions.Handler.StoreHandler = "custom";
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // Null not allowed
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            cliOptions.Handler.StoreHandler = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured store handler is not allowed for your license edition. store_handler=");

            // Invalid value should error
            cliOptions.Handler.StoreHandler = "test4";
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured store handler is not allowed for your license edition. store_handler=test4");
        }

        [Fact]
        public async Task CheckAsync_StrictTypeCheck_ShouldBehaveCorrectly()
        {
            // Error, not allowed but configured
            license.Limits.StrictDataType = false;
            cliOptions.Checker.StrictArgumentValueType = true;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured strict argument value type option is not allowed for your license edition.");

            // No error, not allowed configured false
            license.Limits.StrictDataType = false;
            cliOptions.Checker.StrictArgumentValueType = false;
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // No error, not allowed not configured
            license.Limits.StrictDataType = false;
            cliOptions.Checker.StrictArgumentValueType = null;
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // No error, allowed not configured
            license.Limits.StrictDataType = true;
            cliOptions.Checker.StrictArgumentValueType = false;
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // No error, allowed and configured
            license.Limits.StrictDataType = true;
            cliOptions.Checker.StrictArgumentValueType = true;
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));
        }

        [Fact]
        public async Task CheckAsync_TextHandler_ShouldBehaveCorrectly()
        {
            cliOptions.Handler.TextHandler = "unicode";
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            cliOptions.Handler.TextHandler = "ascii";
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // Null not allowed
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            cliOptions.Handler.TextHandler = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured Unicode handler is not allowed for your license edition. text_handler=");

            // Invalid value should error
            cliOptions.Handler.TextHandler = "test4";
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured Unicode handler is not allowed for your license edition. text_handler=test4");
        }

        private CliOptions cliOptions;
        private IEnumerable<CommandDescriptor> commandDescriptors;
        private License license;
        private ILicenseChecker licenseChecker;
    }
}
