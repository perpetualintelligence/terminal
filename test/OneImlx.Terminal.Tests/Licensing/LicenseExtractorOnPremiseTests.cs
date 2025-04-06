/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using OneImlx.Shared.Licensing;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Shared;
using OneImlx.Terminal.Stores;
using OneImlx.Test.FluentAssertions;
using Xunit;

namespace OneImlx.Terminal.Licensing
{
    public class LicenseExtractorOnPremiseTests : IAsyncLifetime
    {
        public LicenseExtractorOnPremiseTests()
        {
            // Read the lic file from Github secrets
            testOfflineLicPath = GetJsonLicenseFileForLocalHostGitHubSecretForCICD("PI_TERMINAL_TEST_OFFLINE_LIC");

            terminalOptions = MockTerminalOptions.NewLegacyOptions();
            terminalOptions.Licensing.LicensePlan = TerminalLicensePlans.Corporate;

            textHandler = new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.UTF8);

            commandStore = new TerminalInMemoryCommandStore(MockCommands.LicensingCommands.TextHandler, MockCommands.LicensingCommands.Values);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task AirGapped_Invalid_LicensePlan_Throws(bool isDebuggerAttached)
        {
            licenseDebugger = new MockLicenseDebugger(isDebuggerAttached);
            terminalOptions.Licensing.Deployment = TerminalIdentifiers.AirGappedDeployment;

            terminalOptions.Id = TerminalIdentifiers.TestApplicationId;
            terminalOptions.Licensing.LicenseFile = testOfflineLicPath;
            terminalOptions.Licensing.LicensePlan = "invalid_plan";

            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>());
            Func<Task> act = async () => await licenseExtractor.ExtractLicenseAsync();
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode(TerminalErrors.InvalidConfiguration)
                .WithErrorDescription("The license plan is not valid. plan=invalid_plan");
        }

        [Theory]
        [InlineData(TerminalLicensePlans.Enterprise)]
        [InlineData(TerminalLicensePlans.Corporate)]
        public async Task AirGapped_LicensePlan_Does_Not_Throws(string licPlan)
        {
            licenseDebugger = new MockLicenseDebugger(false);
            terminalOptions.Licensing.Deployment = TerminalIdentifiers.AirGappedDeployment;
            terminalOptions.Licensing.LicensePlan = licPlan;

            terminalOptions.Id = TerminalIdentifiers.TestApplicationId;
            terminalOptions.Licensing.LicenseFile = testOfflineLicPath;

            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>());
            await licenseExtractor.ExtractLicenseAsync();
        }

        [Theory]
        [InlineData(TerminalLicensePlans.Demo)]
        [InlineData(TerminalLicensePlans.Solo)]
        [InlineData(TerminalLicensePlans.Micro)]
        [InlineData(TerminalLicensePlans.Smb)]
        public async Task AirGapped_With_Invalid_LicensePlan_Throws(string licPlan)
        {
            licenseDebugger = new MockLicenseDebugger(false);
            terminalOptions.Licensing.Deployment = TerminalIdentifiers.AirGappedDeployment;
            terminalOptions.Licensing.LicensePlan = licPlan;

            terminalOptions.Id = TerminalIdentifiers.TestApplicationId;
            terminalOptions.Licensing.LicenseFile = testOfflineLicPath;

            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>());
            Func<Task> act = async () => await licenseExtractor.ExtractLicenseAsync();
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode(TerminalErrors.InvalidConfiguration)
                .WithErrorDescription($"The license plan is not authorized for air gapped deployment. plan={licPlan}");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task AirGappedDeployment_Does_Not_Throws_For_Valid_License(bool isDebuggerAttached)
        {
            // On-prem license is processed only if debugger is attached
            licenseDebugger = new MockLicenseDebugger(isDebuggerAttached);
            terminalOptions.Licensing.Deployment = TerminalIdentifiers.AirGappedDeployment;

            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>());

            terminalOptions.Id = TerminalIdentifiers.TestApplicationId;
            terminalOptions.Licensing.LicenseFile = testOfflineLicPath;

            await licenseExtractor.ExtractLicenseAsync();
        }

        [Fact]
        public async Task AirGappedDeployment_No_Debugger_Grants_Claims_Based_On_Configured_License_Plan()
        {
            // Before extract get should be null
            licenseDebugger = new MockLicenseDebugger(false);
            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>());

            License? licenseFromGet = await licenseExtractor.GetLicenseAsync();
            licenseFromGet.Should().BeNull();

            terminalOptions.Id = TerminalIdentifiers.TestApplicationId;
            terminalOptions.Licensing.LicenseFile = testOfflineLicPath;
            terminalOptions.Licensing.Deployment = TerminalIdentifiers.AirGappedDeployment;
            terminalOptions.Licensing.LicensePlan = TerminalLicensePlans.Corporate;
            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>());

            var result = await licenseExtractor.ExtractLicenseAsync();
            result.License.Should().NotBeNull();

            // ensure passed and extraction handler
            result.ExtractionMode.Should().Be(null);

            // License claims
            result.License.Claims.Should().NotBeNull();
            result.License.Quota.Should().NotBeNull();
            result.License.LicenseKey.Should().NotBeNull();

            // license key
            result.License.LicenseKey.Should().Be(TerminalIdentifiers.AirGappedKey);

            // plan, mode and usage
            result.License.Plan.Should().Be(TerminalLicensePlans.Corporate);
            result.License.Usage.Should().Be(TerminalIdentifiers.AirGappedUsage);

            // claims
            result.License.Claims.AcrValues.Should().BeNull();
            result.License.Claims.Audience.Should().BeNull();
            result.License.Claims.AuthorizedParty.Should().BeNull();
            result.License.Claims.TenantCountry.Should().BeNull();
            result.License.Claims.Custom.Should().BeNull();

            //result.License.Claims.Expiry.Should().BeNull();
            //result.License.Claims.IssuedAt.Should().BeNull();
            result.License.Claims.Issuer.Should().BeNull();
            result.License.Claims.Jti.Should().BeNull();
            result.License.Claims.TenantName.Should().BeNull();

            //result.License.Claims.NotBefore.Should().BeNull();
            result.License.Claims.Subject.Should().BeNull(); // subscription
            result.License.Claims.Id.Should().BeNull(); // Id
            result.License.Claims.Custom.Should().BeNull();

            // quota
            result.License.Quota.Plan.Should().Be(TerminalLicensePlans.Corporate);

            // After extract and Get should return the correct license
            licenseFromGet = await licenseExtractor.GetLicenseAsync();
            licenseFromGet.Should().NotBeNull();
            licenseFromGet.Should().BeSameAs(result.License);

            // Check on-prem-deployment license
            licenseChecker = new LicenseChecker(commandStore, textHandler, terminalOptions, new LoggerFactory().CreateLogger<LicenseChecker>());
            LicenseCheckerResult licResult = await licenseChecker.CheckLicenseAsync(result.License);
            licResult.Should().NotBeNull();
            licResult.CommandCount.Should().Be(11);
            licResult.License.Should().BeSameAs(result.License);
            licResult.InputCount.Should().Be(90);
            licResult.TerminalCount.Should().Be(1);

            AssertAirGappedDeploymentLicense(result.License);
        }

        public Task DisposeAsync()
        {
            if (File.Exists(testOfflineLicPath))
            {
                File.Delete(testOfflineLicPath);
            }

            return Task.CompletedTask;
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Extraction_Mode_Is_Offline_For_Standard_Deployment(bool isDebuggerAttached)
        {
            // On-prem license is processed only if debugger is attached
            licenseDebugger = new MockLicenseDebugger(isDebuggerAttached);
            terminalOptions.Licensing.Deployment = TerminalIdentifiers.StandardDeployment;

            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>());

            terminalOptions.Id = TerminalIdentifiers.TestApplicationId;
            terminalOptions.Licensing.LicenseFile = testOfflineLicPath;

            LicenseExtractorResult result = await licenseExtractor.ExtractLicenseAsync();
            result.License.Should().NotBeNull();

            // The license is not for on-prem and extraction was done offline
            result.ExtractionMode.Should().Be(TerminalIdentifiers.OfflineLicenseMode);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Extracts_From_AirGapped_Sets_Extraction_Mode_Based_On_Debugger(bool isDebuggerAttached)
        {
            // On-prem license is processed only if debugger is attached
            licenseDebugger = new MockLicenseDebugger(isDebuggerAttached);
            terminalOptions.Licensing.Deployment = TerminalIdentifiers.AirGappedDeployment;

            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>());

            terminalOptions.Id = TerminalIdentifiers.TestApplicationId;
            terminalOptions.Licensing.LicenseFile = testOfflineLicPath;

            LicenseExtractorResult result = await licenseExtractor.ExtractLicenseAsync();
            result.License.Should().NotBeNull();

            // The license is for on-prem and no extraction was done via online
            if (isDebuggerAttached)
            {
                result.ExtractionMode.Should().Be(TerminalIdentifiers.OfflineLicenseMode);
            }
            else
            {
                result.ExtractionMode.Should().Be(null);
                AssertAirGappedDeploymentLicense(result.License);
            }
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public async Task Invalid_Deployment_Extracts_From_Json()
        {
            // We always check for license if debugger is attached. If debugger is not attached then we check if
            // OnPremiseDeployment is set. Onprem license is processed only if debugger is attached
            licenseDebugger = new MockLicenseDebugger(false);
            terminalOptions.Licensing.Deployment = "invalid_deployment";

            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>());

            terminalOptions.Id = TerminalIdentifiers.TestApplicationId;
            terminalOptions.Licensing.LicenseFile = testOfflineLicPath;

            LicenseExtractorResult? result = await licenseExtractor.ExtractLicenseAsync();
            result.License.Should().NotBeNull();

            // The license is for on-prem but the extraction was done via online
            result.ExtractionMode.Should().Be(TerminalIdentifiers.OfflineLicenseMode);
        }

        private static string GetJsonLicenseFileForLocalHostGitHubSecretForCICD(string env)
        {
            // The demo json is too long for system env, so we use path for system env and json for github
            string? fileOrJson = Environment.GetEnvironmentVariable(env) ?? throw new TerminalException(TerminalErrors.InvalidConfiguration, "Environment variable with license key not found. env={0}", env);
            string json = fileOrJson;
            if (File.Exists(fileOrJson))
            {
                json = File.ReadAllText(fileOrJson);
            }

            string tempJsonLicPath = Path.Combine(AppContext.BaseDirectory, $"{Guid.NewGuid()}.json");
            File.WriteAllText(tempJsonLicPath, json);
            return tempJsonLicPath;
        }

        private void AssertAirGappedDeploymentLicense(License license)
        {
            license.Claims.Should().BeEquivalentTo(new LicenseClaims());
            license.LicenseKey.Should().Be(TerminalIdentifiers.AirGappedKey);
            license.Plan.Should().Be(terminalOptions.Licensing.LicensePlan);
            license.Usage.Should().Be(TerminalIdentifiers.AirGappedUsage);
            license.Quota.Should().BeEquivalentTo(LicenseQuota.Create(license.Plan, license.Claims.Custom));
            license.Failed.Should().BeNull();
        }

        private readonly ITerminalCommandStore commandStore;
        private readonly TerminalOptions terminalOptions;
        private readonly string testOfflineLicPath;
        private ILicenseChecker licenseChecker = null!;
        private ILicenseDebugger licenseDebugger = null!;
        private ILicenseExtractor licenseExtractor = null!;
        private TerminalTextHandler textHandler = null!;
    }
}
