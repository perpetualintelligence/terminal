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
            license = new License(TerminalLicensePlans.Corporate, LicenseUsage.RnD, "testLicKey2", MockLicenses.TestClaims, LicenseLimits.Create(TerminalLicensePlans.Corporate), LicensePrice.Create(TerminalLicensePlans.Corporate));
        }

        [Fact]
        public async Task CheckAsync_ExceededCommandLimit_ShouldError()
        {
            // Commands are 11
            license.Limits.CommandLimit = 2;
            Func<Task> func = async () => await licenseChecker.CheckLicenseAsync(license);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidLicense).WithErrorDescription("The command limit exceeded. max_limit=2 current=11");
        }

        [Fact]
        public async Task CheckAsync_ExceededInputLimit_ShouldError()
        {
            // Total options 90
            license.Limits.InputLimit = 89;
            Func<Task> func = async () => await licenseChecker.CheckLicenseAsync(license);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidLicense).WithErrorDescription("The input limit exceeded. max_limit=89 current=90");
        }

        [Fact]
        public async Task CheckAsync_ExceededTerminalLimit_ShouldError()
        {
            // TODO For now the terminal count are 1 always
            license.Limits.TerminalLimit = 0;
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
        public async Task CheckAsync_OptionsValid_ShouldBehaveCorrectly()
        {
            // Valid value should not error
            await licenseChecker.CheckLicenseAsync(license);
        }

        [Fact]
        public async Task CheckAsync_StrictTypeCheck_ShouldBehaveCorrectly()
        {
            // Error, not allowed but configured
            license.Limits.StrictDataType = false;
            terminalOptions.Checker.StrictValueType = true;
            Func<Task> func = async () => await licenseChecker.CheckLicenseAsync(license);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidLicense).WithErrorDescription("The configured strict option value type is not allowed for your license plan.");

            // No error, not allowed configured false
            license.Limits.StrictDataType = false;
            terminalOptions.Checker.StrictValueType = false;
            await licenseChecker.CheckLicenseAsync(license);

            // No error, allowed not configured
            license.Limits.StrictDataType = true;
            terminalOptions.Checker.StrictValueType = false;
            await licenseChecker.CheckLicenseAsync(license);

            // No error, allowed and configured
            license.Limits.StrictDataType = true;
            terminalOptions.Checker.StrictValueType = true;
            await licenseChecker.CheckLicenseAsync(license);
        }

        private readonly ITerminalCommandStore commandStore;
        private readonly License license;
        private readonly ILicenseChecker licenseChecker;
        private readonly TerminalOptions terminalOptions;
    }
}
