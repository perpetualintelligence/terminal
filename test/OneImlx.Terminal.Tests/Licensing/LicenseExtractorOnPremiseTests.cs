﻿/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using OneImlx.Shared.Licensing;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Stores;
using OneImlx.Test.FluentAssertions;
using Xunit;
using OneImlx.Terminal.Shared;

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

            commandStore = new TerminalInMemoryCommandStore(MockCommands.LicensingCommands.TextHandler, MockCommands.LicensingCommands.Values);
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
        public async Task Extracts_From_Offline_If_OnPremiseDeployment_Not_Configured(bool isDebuggerAttached)
        {
            // On-prem license is processed only if debugger is attached
            licenseDebugger = new MockLicenseDebugger(isDebuggerAttached);
            terminalOptions.Licensing.Deployment = null;

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
        public async Task Extracts_From_OffLine_Sets_Extraction_Mode_Based_On_Debugger(bool isDebuggerAttached)
        {
            // On-prem license is processed only if debugger is attached
            licenseDebugger = new MockLicenseDebugger(isDebuggerAttached);
            terminalOptions.Licensing.Deployment = TerminalIdentifiers.OnPremiseIsolatedDeployment;

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
                AssertOnPremIsolatedDeploymentLicense(result.License);
            }
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        [Theory]
        [InlineData(true, "invalid_deployment")]
        [InlineData(true, "    ")]
        [InlineData(true, "")]
        [InlineData(true, null)]
        [InlineData(false, "invalid_deployment")]
        [InlineData(false, "    ")]
        [InlineData(false, "")]
        [InlineData(false, null)]
        public async Task Invalid_Or_Null_Deployment_Extracts_From_Offline(bool isDebuggerAttached, string? deployment)
        {
            // We always check for license if debugger is attached. If debugger is not attached then we check if
            // OnPremiseDeployment is set. Onprem license is processed only if debugger is attached
            licenseDebugger = new MockLicenseDebugger(isDebuggerAttached);
            terminalOptions.Licensing.Deployment = deployment;

            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>());

            terminalOptions.Id = TerminalIdentifiers.TestApplicationId;
            terminalOptions.Licensing.LicenseFile = testOfflineLicPath;

            LicenseExtractorResult? result = await licenseExtractor.ExtractLicenseAsync();
            result.License.Should().NotBeNull();

            // The license is for on-prem but the extraction was done via online
            result.ExtractionMode.Should().Be(TerminalIdentifiers.OfflineLicenseMode);
        }

        [Fact]
        public async Task OnPremDeployment_Enabled_No_Debugger_Grants_Claims_Based_On_Configured_License_Plan()
        {
            // Before extract get should be null
            licenseDebugger = new MockLicenseDebugger(false);
            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>());

            License? licenseFromGet = await licenseExtractor.GetLicenseAsync();
            licenseFromGet.Should().BeNull();

            terminalOptions.Id = TerminalIdentifiers.TestApplicationId;
            terminalOptions.Licensing.LicenseFile = testOfflineLicPath;
            terminalOptions.Licensing.Deployment = TerminalIdentifiers.OnPremiseIsolatedDeployment;
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
            result.License.LicenseKey.Should().Be("onprem-isolated-deployment");

            // plan, mode and usage
            result.License.Plan.Should().Be("urn:oneimlx:terminal:plan:corporate");
            result.License.Usage.Should().Be("onprem-isolated-deployment");

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
            result.License.Quota.Plan.Should().Be("urn:oneimlx:terminal:plan:corporate");

            // After extract and Get should return the correct license
            licenseFromGet = await licenseExtractor.GetLicenseAsync();
            licenseFromGet.Should().NotBeNull();
            licenseFromGet.Should().BeSameAs(result.License);

            // Check on-prem-deployment license
            licenseChecker = new LicenseChecker(commandStore, terminalOptions, new LoggerFactory().CreateLogger<LicenseChecker>());
            LicenseCheckerResult licResult = await licenseChecker.CheckLicenseAsync(result.License);
            licResult.Should().NotBeNull();
            licResult.CommandCount.Should().Be(11);
            licResult.License.Should().BeSameAs(result.License);
            licResult.InputCount.Should().Be(90);
            licResult.TerminalCount.Should().Be(1);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task OnPremiseDeployment_Does_Not_Throws_For_Offline_License(bool isDebuggerAttached)
        {
            // On-prem license is processed only if debugger is attached
            licenseDebugger = new MockLicenseDebugger(isDebuggerAttached);
            terminalOptions.Licensing.Deployment = TerminalIdentifiers.OnPremiseIsolatedDeployment;

            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>());

            terminalOptions.Id = TerminalIdentifiers.TestApplicationId;
            terminalOptions.Licensing.LicenseFile = testOfflineLicPath;

            await licenseExtractor.ExtractLicenseAsync();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Unset_LicensePlan_Throws(bool isDebuggerAttached)
        {
            licenseDebugger = new MockLicenseDebugger(isDebuggerAttached);
            terminalOptions.Licensing.Deployment = TerminalIdentifiers.OnPremiseIsolatedDeployment;

            terminalOptions.Id = TerminalIdentifiers.TestApplicationId;
            terminalOptions.Licensing.LicenseFile = testOfflineLicPath;
            terminalOptions.Licensing.LicensePlan = null!;

            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>());
            Func<Task> act = async () => await licenseExtractor.ExtractLicenseAsync();
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode(TerminalErrors.InvalidConfiguration)
                .WithErrorDescription("The license plan is not valid. plan=");
        }

        [Theory]
        [InlineData(TerminalLicensePlans.Enterprise)]
        [InlineData(TerminalLicensePlans.Corporate)]
        public async Task Valid_LicensePlan_Does_Not_Throws(string licPlan)
        {
            licenseDebugger = new MockLicenseDebugger(false);
            terminalOptions.Licensing.Deployment = TerminalIdentifiers.OnPremiseIsolatedDeployment;
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
        public async Task Valid_OnPrem_License_But_Invalid_LicensePlan_Throws(string licPlan)
        {
            licenseDebugger = new MockLicenseDebugger(false);
            terminalOptions.Licensing.Deployment = TerminalIdentifiers.OnPremiseIsolatedDeployment;
            terminalOptions.Licensing.LicensePlan = licPlan;

            terminalOptions.Id = TerminalIdentifiers.TestApplicationId;
            terminalOptions.Licensing.LicenseFile = testOfflineLicPath;

            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>());
            Func<Task> act = async () => await licenseExtractor.ExtractLicenseAsync();
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode(TerminalErrors.InvalidConfiguration)
                .WithErrorDescription($"The license plan is not authorized for on-premise isolated deployment. plan={licPlan}");
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

        private void AssertOnPremIsolatedDeploymentLicense(License license)
        {
            license.Claims.Should().BeEquivalentTo(new LicenseClaims());
            license.LicenseKey.Should().Be("onprem-isolated-deployment");
            license.Plan.Should().Be(terminalOptions.Licensing.LicensePlan);
            license.Usage.Should().Be("onprem-isolated-deployment");
            license.Quota.Should().BeEquivalentTo(LicenseQuota.Create(license.Plan, license.Claims.Custom));
        }

        private readonly ITerminalCommandStore commandStore;
        private readonly TerminalOptions terminalOptions;
        private readonly string testOfflineLicPath;
        private ILicenseChecker licenseChecker = null!;
        private ILicenseDebugger licenseDebugger = null!;
        private ILicenseExtractor licenseExtractor = null!;
    }
}
