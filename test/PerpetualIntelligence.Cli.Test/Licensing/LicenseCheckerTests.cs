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
            license = new License("testProviderId2", SaaSCheckModes.Offline, SaaSPlans.ISVU, SaaSUsages.RnD, SaaSKeySources.JsonFile, "testLicKey2", MockLicenses.TestClaims, LicenseLimits.Create(SaaSPlans.ISVU), LicensePrice.Create(SaaSPlans.ISVU));
        }

        [Fact]
        public async Task CheckAsync_DataTypeCheck_ShouldBehaveCorrectly()
        {
            cliOptions.Checker.DataTypeHandler = "default";
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            cliOptions.Checker.DataTypeHandler = "custom";
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // Null allowed
            cliOptions.Checker.DataTypeHandler = null;
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // Invalid value should error
            cliOptions.Checker.DataTypeHandler = "test4";
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured data type check is not allowed for your license edition. data_type_check=test4");
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
            cliOptions.Hosting.ErrorHandler = "default";
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            cliOptions.Hosting.ErrorHandler = "custom";
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // Null not allowed
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            cliOptions.Hosting.ErrorHandler = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured error handling is not allowed for your license edition. error_handling=");

            // Invalid value should error
            cliOptions.Hosting.ErrorHandler = "test4";
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured error handling is not allowed for your license edition. error_handling=test4");
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
            cliOptions.Checker.DataTypeHandler = null;
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // Valid value should not error
            cliOptions.Checker.DataTypeHandler = "test2";
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // Invalid value should error
            cliOptions.Checker.DataTypeHandler = "test4";
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured data type check is not allowed for your license edition. data_type_check=test4");

            // Null limit options but configured option should error
            license.Limits.DataTypeHandlers = null;
            cliOptions.Checker.DataTypeHandler = "test5";
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured data type check is not allowed for your license edition. data_type_check=test5");
        }

        [Fact]
        public async Task CheckAsync_ServiceImplementation_ShouldBehaveCorrectly()
        {
            cliOptions.Hosting.Service = "default";
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            cliOptions.Hosting.Service = "custom";
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // Null not allowed
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            cliOptions.Hosting.Service = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured service implementation is not allowed for your license edition. service_implementation=");

            // Invalid value should error
            cliOptions.Hosting.Service = "test4";
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured service implementation is not allowed for your license edition. service_implementation=test4");
        }

        [Fact]
        public async Task CheckAsync_Store_ShouldBehaveCorrectly()
        {
            cliOptions.Hosting.Store = "in-memory";
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            cliOptions.Hosting.Store = "json";
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            cliOptions.Hosting.Store = "custom";
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // Null not allowed
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            cliOptions.Hosting.Store = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured store is not allowed for your license edition. store=");

            // Invalid value should error
            cliOptions.Hosting.Store = "test4";
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured store is not allowed for your license edition. store=test4");
        }

        [Fact]
        public async Task CheckAsync_StrictTypeCheck_ShouldBehaveCorrectly()
        {
            // Error, not allowed but configured
            license.Limits.StrictDataType = false;
            cliOptions.Checker.StrictArgumentValueType = true;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured strict type checking is not allowed for your license edition.");

            // No error, not allowed not configured
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
        public async Task CheckAsync_UnicodeSupport_ShouldBehaveCorrectly()
        {
            cliOptions.Hosting.UnicodeHandler = "default";
            await licenseChecker.CheckAsync(new LicenseCheckerContext(license));

            // Null not allowed
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            cliOptions.Hosting.UnicodeHandler = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured unicode support is not allowed for your license edition. unicode_support=");

            // Invalid value should error
            cliOptions.Hosting.UnicodeHandler = "test4";
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseChecker.CheckAsync(new LicenseCheckerContext(license)), Errors.InvalidLicense, "The configured unicode support is not allowed for your license edition. unicode_support=test4");
        }

        private CliOptions cliOptions;
        private IEnumerable<CommandDescriptor> commandDescriptors;
        private License license;
        private ILicenseChecker licenseChecker;
    }
}
