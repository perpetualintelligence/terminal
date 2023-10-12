/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Shared.Licensing;
using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Terminal.Mocks;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace PerpetualIntelligence.Terminal.Licensing
{
    public class LicenseExtractorOnPremiseTests : IDisposable
    {
        public LicenseExtractorOnPremiseTests()
        {
            // Read the lic file from Github secrets
            testOnlineLicPath = GetJsonLicenseFileForLocalHostGitHubSecretForCICD("PI_CLI_TEST_ONLINE_LIC");
            testOfflineLicPath = GetJsonLicenseFileForLocalHostGitHubSecretForCICD("PI_CLI_TEST_OFFLINE_LIC");

            terminalOptions = MockTerminalOptions.NewLegacyOptions();
            terminalOptions.Licensing.LicensePlan = TerminalLicensePlans.Unlimited;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            if (File.Exists(testOnlineLicPath))
            {
                File.Delete(testOnlineLicPath);
            }

            if (File.Exists(testOfflineLicPath))
            {
                File.Delete(testOfflineLicPath);
            }
        }

        [Theory]
        [InlineData(false, true)]
        public async Task Does_Not_Extracts_From_Online_If_Validation_Key_Is_Null_And_OnPremiseDeployment(bool isDebuggerAttached, bool? onPremDeployment)
        {
            // On-prem license is processed only if debugger is attached
            licenseDebugger = new MockLicenseDebugger(isDebuggerAttached);
            terminalOptions.Licensing.OnPremiseDeployment = onPremDeployment;

            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());

            terminalOptions.Licensing.LicenseKey = testOnlineLicPath;
            terminalOptions.Licensing.LicenseKeySource = LicenseSources.JsonFile;
            terminalOptions.Handler.LicenseHandler = TerminalHandlers.OnlineLicenseHandler;
            terminalOptions.Http.HttpClientName = httpClientName;
            terminalOptions.Licensing.ConsumerTenantId = "a8379958-ea19-4918-84dc-199bf012361e";
            terminalOptions.Licensing.Subject = "68d230be-cf83-49a6-c83f-42949fb40f46";
            terminalOptions.Licensing.AuthorizedApplicationId = "641e1dc1-7ff3-4510-a8e5-abb787fe0fe1";

            terminalOptions.Handler.LicenseHandler = TerminalHandlers.OnPremiseLicenseHandler;
            LicenseExtractorResult result = await licenseExtractor.ExtractLicenseAsync(new LicenseExtractorContext());
            result.License.Should().NotBeNull();

            // The license is for on-prem and no extraction was done via online
            result.License.Handler.Should().Be(TerminalHandlers.OnPremiseLicenseHandler);
            result.ExtractionHandler.Should().Be(TerminalHandlers.OnPremiseLicenseHandler);

            AssertOnPremiseDeploymentLicense(result.License);
        }

        [Theory]
        [InlineData(false, true)]
        public async Task Does_Not_Extracts_From_Offline_If_Validation_Key_Is_Not_Null_And_OnPremiseDeployment(bool isDebuggerAttached, bool? onPremDeployment)
        {
            // On-prem license is processed only if debugger is attached
            licenseDebugger = new MockLicenseDebugger(isDebuggerAttached);
            terminalOptions.Licensing.OnPremiseDeployment = onPremDeployment;

            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());

            terminalOptions.Licensing.LicenseKey = testOfflineLicPath;
            terminalOptions.Licensing.LicenseKeySource = LicenseSources.JsonFile;
            terminalOptions.Handler.LicenseHandler = TerminalHandlers.OfflineLicenseHandler;
            terminalOptions.Licensing.ConsumerTenantId = "a8379958-ea19-4918-84dc-199bf012361e";
            terminalOptions.Licensing.Subject = "68d230be-cf83-49a6-c83f-42949fb40f46";
            terminalOptions.Licensing.AuthorizedApplicationId = "641e1dc1-7ff3-4510-a8e5-abb787fe0fe1";

            terminalOptions.Handler.LicenseHandler = TerminalHandlers.OnPremiseLicenseHandler;
            LicenseExtractorResult? result = await licenseExtractor.ExtractLicenseAsync(new LicenseExtractorContext());
            result.License.Should().NotBeNull();

            // The license is for on-prem and no extraction was done via offline
            result.License.Handler.Should().Be(TerminalHandlers.OnPremiseLicenseHandler);
            result.ExtractionHandler.Should().Be(TerminalHandlers.OnPremiseLicenseHandler);

            AssertOnPremiseDeploymentLicense(result.License);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(true, null)]
        [InlineData(false, false)]
        [InlineData(false, null)]
        public async Task Extracts_From_Online_If_Validation_Key_Is_Null_When_Debugger_Is_Attached_Or_OnPrem_Deployment_Is_Enabled(bool isDebuggerAttached, bool? onPremDeployment)
        {
            // We always check for license if debugger is attached.
            // If debugger is not attached then we check if OnPremiseDeployment is set.

            // Onprem license is processed only if debugger is attached
            licenseDebugger = new MockLicenseDebugger(isDebuggerAttached);
            terminalOptions.Licensing.OnPremiseDeployment = onPremDeployment;

            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());

            terminalOptions.Licensing.LicenseKey = testOnlineLicPath;
            terminalOptions.Licensing.LicenseKeySource = LicenseSources.JsonFile;
            terminalOptions.Handler.LicenseHandler = TerminalHandlers.OnlineLicenseHandler;
            terminalOptions.Http.HttpClientName = httpClientName;
            terminalOptions.Licensing.ConsumerTenantId = "a8379958-ea19-4918-84dc-199bf012361e";
            terminalOptions.Licensing.Subject = "68d230be-cf83-49a6-c83f-42949fb40f46";
            terminalOptions.Licensing.AuthorizedApplicationId = "641e1dc1-7ff3-4510-a8e5-abb787fe0fe1";

            terminalOptions.Handler.LicenseHandler = TerminalHandlers.OnPremiseLicenseHandler;
            LicenseExtractorResult? result = await licenseExtractor.ExtractLicenseAsync(new LicenseExtractorContext());
            result.License.Should().NotBeNull();

            // The license is for on-prem but the extraction was done via online
            result.License.Handler.Should().Be(TerminalHandlers.OnPremiseLicenseHandler);
            result.ExtractionHandler.Should().Be(TerminalHandlers.OnlineLicenseHandler);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(true, null)]
        [InlineData(false, false)]
        [InlineData(false, null)]
        public async Task Extracts_From_Offline_If_Validation_Key_Is_Not_Null_And_Debugger_Is_Attached(bool isDebuggerAttached, bool? onPremDeployment)
        {
            // Onprem license is processed only if debugger is attached
            licenseDebugger = new MockLicenseDebugger(isDebuggerAttached);
            terminalOptions.Licensing.OnPremiseDeployment = onPremDeployment;

            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());

            terminalOptions.Licensing.LicenseKey = testOfflineLicPath;
            terminalOptions.Licensing.LicenseKeySource = LicenseSources.JsonFile;
            terminalOptions.Handler.LicenseHandler = TerminalHandlers.OfflineLicenseHandler;
            terminalOptions.Licensing.ConsumerTenantId = "a8379958-ea19-4918-84dc-199bf012361e";
            terminalOptions.Licensing.Subject = "68d230be-cf83-49a6-c83f-42949fb40f46";
            terminalOptions.Licensing.AuthorizedApplicationId = "641e1dc1-7ff3-4510-a8e5-abb787fe0fe1";

            terminalOptions.Handler.LicenseHandler = TerminalHandlers.OnPremiseLicenseHandler;
            LicenseExtractorResult? result = await licenseExtractor.ExtractLicenseAsync(new LicenseExtractorContext());
            result.License.Should().NotBeNull();

            // The license is for on-prem but the extraction was done via offline
            result.License.Handler.Should().Be(TerminalHandlers.OnPremiseLicenseHandler);
            result.ExtractionHandler.Should().Be(TerminalHandlers.OfflineLicenseHandler);
        }

        private static string GetJsonLicenseFileForLocalHostGitHubSecretForCICD(string env)
        {
            // The demo json is too long for system env, so we use path for system env and json for github
            string? fileOrJson = Environment.GetEnvironmentVariable(env);

            if (fileOrJson == null)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "Environment variable with license key not found. env={0}", env);
            }

            string json = fileOrJson;
            if (File.Exists(fileOrJson))
            {
                json = File.ReadAllText(fileOrJson);
            }

            string tempJsonLicPath = Path.Combine(AppContext.BaseDirectory, $"{Guid.NewGuid()}.json");
            File.WriteAllText(tempJsonLicPath, json);
            return tempJsonLicPath;
        }

        private void AssertOnPremiseDeploymentLicense(License license)
        {
            license.Claims.Should().BeEquivalentTo(new LicenseClaimsModel());
            license.Handler.Should().Be(TerminalHandlers.OnPremiseLicenseHandler);
            license.LicenseKey.Should().Be("on-premise-deployment");
            license.LicenseKeySource.Should().Be(LicenseSources.JsonFile);
            license.Plan.Should().Be(terminalOptions.Licensing.LicensePlan);
            license.Usage.Should().Be("on-premise-deployment");
        }

        private readonly TerminalOptions terminalOptions;
        private readonly string httpClientName = "prod";
        private ILicenseExtractor licenseExtractor = null!;
        private readonly string testOnlineLicPath;
        private readonly string testOfflineLicPath;
        private ILicenseDebugger licenseDebugger = null!;
    }
}