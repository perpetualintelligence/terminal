/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

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
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Licensing
{
    public class LicenseCheckerTests
    {
        public LicenseCheckerTests()
        {
            terminalOptions = MockTerminalOptions.NewLegacyOptions();
            commandStore = new TerminalInMemoryCommandStore(MockCommands.LicensingCommands.TextHandler, MockCommands.LicensingCommands.Values);
            licenseChecker = new LicenseChecker(commandStore, terminalOptions, new LoggerFactory().CreateLogger<LicenseChecker>());
            license = new License(TerminalLicensePlans.Corporate, LicenseUsage.RnD, "testLicKey2", MockLicenses.TestClaims, LicenseQuota.Create(TerminalLicensePlans.Corporate));
        }

        [Fact]
        public async Task CheckAsync_DriverCheck_ShouldBehaveCorrectly()
        {
            // Error, not allowed but configured
            var switches = new Dictionary<string, bool>(license.Quota.Switches)
            {
                ["driver"] = false
            };
            license.Quota.Switches = switches;
            terminalOptions.Driver.Enabled = true;
            Func<Task> func = async () => await licenseChecker.CheckLicenseAsync(license);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidLicense).WithErrorDescription("The terminal driver option is not allowed for your license plan.");

            // No error, not allowed not configured
            switches["driver"] = false;
            license.Quota.Switches = switches;
            terminalOptions.Driver.Enabled = false;
            await licenseChecker.CheckLicenseAsync(license);

            // No error, allowed not configured
            switches["driver"] = true;
            license.Quota.Switches = switches;
            terminalOptions.Driver.Enabled = false;
            await licenseChecker.CheckLicenseAsync(license);

            // No error, allowed and configured
            switches["driver"] = true;
            license.Quota.Switches = switches;
            terminalOptions.Driver.Enabled = true;
            await licenseChecker.CheckLicenseAsync(license);
        }

        [Fact]
        public async Task CheckAsync_ExceededCommandLimit_ShouldError()
        {
            // Commands are 11
            var limits = new Dictionary<string, object?>(license.Quota.Limits)
            {
                ["commands"] = 2
            };
            license.Quota.Limits = limits;
            Func<Task> func = async () => await licenseChecker.CheckLicenseAsync(license);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidLicense).WithErrorDescription("The command limit exceeded. max_limit=2 current=11");
        }

        [Fact]
        public async Task CheckAsync_ExceededInputLimit_ShouldError()
        {
            // Total options 90
            var limits = new Dictionary<string, object?>(license.Quota.Limits)
            {
                ["inputs"] = 89
            };
            license.Quota.Limits = limits;
            Func<Task> func = async () => await licenseChecker.CheckLicenseAsync(license);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidLicense).WithErrorDescription("The input limit exceeded. max_limit=89 current=90");
        }

        [Fact]
        public async Task CheckAsync_ExceededTerminalLimit_ShouldError()
        {
            // TODO For now the terminal count are 1 always
            var limits = new Dictionary<string, object?>(license.Quota.Limits)
            {
                ["terminals"] = 0
            };
            license.Quota.Limits = limits;
            Func<Task> func = async () => await licenseChecker.CheckLicenseAsync(license);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidLicense).WithErrorDescription("The terminal limit exceeded. max_limit=0 current=1");
        }

        [Fact]
        public async Task CheckAsync_InitializeShouldSetPropertiesCorrectly()
        {
            // Use unlimited license so this will not fail.
            var result = await licenseChecker.CheckLicenseAsync(license);
            ((LicenseChecker)licenseChecker).Initialized.Should().Be(true);

            result.License.Should().NotBeNull();
            result.License.Should().BeSameAs(license);

            result.TerminalCount.Should().Be(1);
            result.CommandCount.Should().Be(11);
            result.InputCount.Should().Be(90);
        }

        [Fact]
        public async Task CheckAsync_DynamicsCheck_ShouldBehaveCorrectly()
        {
            // Error, not allowed but configured
            var switches = new Dictionary<string, bool>(license.Quota.Switches)
            {
                ["dynamics"] = false
            };
            license.Quota.Switches = switches;
            terminalOptions.Dynamics.Enabled = true;
            Func<Task> func = async () => await licenseChecker.CheckLicenseAsync(license);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidLicense).WithErrorDescription("The terminal dynamics option is not allowed for your license plan.");

            // No error, not allowed not configured
            switches["dynamics"] = false;
            license.Quota.Switches = switches;
            terminalOptions.Dynamics.Enabled = false;
            await licenseChecker.CheckLicenseAsync(license);

            // No error, allowed not configured
            switches["dynamics"] = true;
            license.Quota.Switches = switches;
            terminalOptions.Dynamics.Enabled = false;
            await licenseChecker.CheckLicenseAsync(license);

            // No error, allowed and configured
            switches["dynamics"] = true;
            license.Quota.Switches = switches;
            terminalOptions.Dynamics.Enabled = true;
            await licenseChecker.CheckLicenseAsync(license);
        }

        [Fact]
        public async Task CheckAsync_OptionsValid_ShouldBehaveCorrectly()
        {
            // Valid value should not error
            await licenseChecker.CheckLicenseAsync(license);
        }

        [Fact]
        public async Task CheckAsync_StrictTypeCheck_ShouldBehaveCorrectly()
        {
            // Error, not allowed but configured
            var switches = new Dictionary<string, bool>(license.Quota.Switches)
            {
                ["datatype"] = false
            };
            license.Quota.Switches = switches;
            terminalOptions.Checker.ValueDataType = true;
            Func<Task> func = async () => await licenseChecker.CheckLicenseAsync(license);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidLicense).WithErrorDescription("The strict option value type is not allowed for your license plan.");

            // No error, not allowed not configured
            switches["datatype"] = false;
            license.Quota.Switches = switches;
            terminalOptions.Checker.ValueDataType = false;
            await licenseChecker.CheckLicenseAsync(license);

            // No error, allowed not configured
            switches["datatype"] = true;
            license.Quota.Switches = switches;
            terminalOptions.Checker.ValueDataType = false;
            await licenseChecker.CheckLicenseAsync(license);

            // No error, allowed and configured
            switches["datatype"] = true;
            license.Quota.Switches = switches;
            terminalOptions.Checker.ValueDataType = true;
            await licenseChecker.CheckLicenseAsync(license);
        }

        private readonly ITerminalCommandStore commandStore;
        private readonly License license;
        private readonly ILicenseChecker licenseChecker;
        private readonly TerminalOptions terminalOptions;
    }
}
