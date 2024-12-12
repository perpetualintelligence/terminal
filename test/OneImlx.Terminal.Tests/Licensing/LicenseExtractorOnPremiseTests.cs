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
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Licensing
{
    public class LicenseExtractorOnPremiseTests : IAsyncLifetime
    {
        public LicenseExtractorOnPremiseTests()
        {
            // Read the lic file from Github secrets
            testOnlineLicPath = GetJsonLicenseFileForLocalHostGitHubSecretForCICD("PI_TERMINAL_TEST_ONLINE_LIC");
            testOfflineLicPath = GetJsonLicenseFileForLocalHostGitHubSecretForCICD("PI_TERMINAL_TEST_OFFLINE_LIC");

            terminalOptions = MockTerminalOptions.NewLegacyOptions();
            terminalOptions.Licensing.LicensePlan = TerminalLicensePlans.Unlimited;

            commandStore = new TerminalInMemoryCommandStore(MockCommands.LicensingCommands.TextHandler, MockCommands.LicensingCommands.Values);
        }

        public Task DisposeAsync()
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

            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());

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
        public async Task Extracts_From_Offline_Sets_Extraction_Mode_Based_On_Debugger(bool isDebuggerAttached)
        {
            // On-prem license is processed only if debugger is attached
            licenseDebugger = new MockLicenseDebugger(isDebuggerAttached);
            terminalOptions.Licensing.Deployment = TerminalIdentifiers.OnPremiseDeployment;

            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());

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
                AssertOnPremiseDeploymentLicense(result.License);
            }
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        [Theory]
        [InlineData(TerminalLicensePlans.Demo)]
        [InlineData(TerminalLicensePlans.Micro)]
        [InlineData(TerminalLicensePlans.SMB)]
        [InlineData(TerminalLicensePlans.Enterprise)]
        public async Task Invalid_LicensePlan_Throws(string licPlan)
        {
            licenseDebugger = new MockLicenseDebugger(false);
            terminalOptions.Licensing.Deployment = TerminalIdentifiers.OnPremiseDeployment;
            terminalOptions.Licensing.LicensePlan = licPlan;

            terminalOptions.Id = TerminalIdentifiers.TestApplicationId;
            terminalOptions.Licensing.LicenseFile = testOfflineLicPath;

            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());
            Func<Task> act = async () => await licenseExtractor.ExtractLicenseAsync();
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode(TerminalErrors.InvalidConfiguration)
                .WithErrorDescription($"The license plan is not authorized for on-premise deployment. plan={licPlan}");
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

            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());

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
            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());

            License? licenseFromGet = await licenseExtractor.GetLicenseAsync();
            licenseFromGet.Should().BeNull();

            terminalOptions.Id = TerminalIdentifiers.TestApplicationId;
            terminalOptions.Licensing.LicenseFile = testOfflineLicPath;
            terminalOptions.Licensing.Deployment = TerminalIdentifiers.OnPremiseDeployment;
            terminalOptions.Licensing.LicensePlan = TerminalLicensePlans.Unlimited;
            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());

            var result = await licenseExtractor.ExtractLicenseAsync();
            result.License.Should().NotBeNull();

            // ensure passed and extraction handler
            result.ExtractionMode.Should().Be(null);

            // License claims
            result.License.Claims.Should().NotBeNull();
            result.License.Limits.Should().NotBeNull();
            result.License.LicenseKey.Should().NotBeNull();

            // license key
            result.License.LicenseKey.Should().Be("on-premise-deployment");

            // plan, mode and usage
            result.License.Plan.Should().Be("urn:oneimlx:terminal:plan:unlimited");
            result.License.Usage.Should().Be("on-premise-deployment");

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

            // limits
            result.License.Limits.Plan.Should().Be("urn:oneimlx:terminal:plan:unlimited");

            // Price
            result.License.Price.Plan.Should().Be("urn:oneimlx:terminal:plan:unlimited");
            result.License.Price.Currency.Should().Be("USD");
            result.License.Price.Monthly.Should().Be(3299.0);
            result.License.Price.Yearly.Should().Be(35629.0);

            // After extract and Get should return the correct license
            licenseFromGet = await licenseExtractor.GetLicenseAsync();
            licenseFromGet.Should().NotBeNull();
            licenseFromGet.Should().BeSameAs(result.License);

            // Check on-prem-deployment license
            licenseChecker = new LicenseChecker(commandStore, terminalOptions, new LoggerFactory().CreateLogger<LicenseChecker>());
            LicenseCheckerResult licResult = await licenseChecker.CheckLicenseAsync(result.License);
            licResult.Should().NotBeNull();
            licResult.CommandGroupCount.Should().Be(3);
            licResult.License.Should().BeSameAs(result.License);
            licResult.OptionCount.Should().Be(90);
            licResult.RootCommandCount.Should().Be(3);
            licResult.SubCommandCount.Should().Be(5);
            licResult.TerminalCount.Should().Be(1);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task OnPremiseDeployment_Does_Not_Throws_For_Offline_License(bool isDebuggerAttached)
        {
            // On-prem license is processed only if debugger is attached
            licenseDebugger = new MockLicenseDebugger(isDebuggerAttached);
            terminalOptions.Licensing.Deployment = TerminalIdentifiers.OnPremiseDeployment;

            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());

            terminalOptions.Id = TerminalIdentifiers.TestApplicationId;
            terminalOptions.Licensing.LicenseFile = testOfflineLicPath;

            await licenseExtractor.ExtractLicenseAsync();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task OnPremiseDeployment_Throws_For_Online_License(bool isDebuggerAttached)
        {
            // On-prem license is processed only if debugger is attached
            licenseDebugger = new MockLicenseDebugger(isDebuggerAttached);
            terminalOptions.Licensing.Deployment = TerminalIdentifiers.OnPremiseDeployment;

            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());

            terminalOptions.Id = TerminalIdentifiers.TestApplicationId;
            terminalOptions.Licensing.LicenseFile = testOnlineLicPath;

            Func<Task> act = async () => await licenseExtractor.ExtractLicenseAsync();
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode(TerminalErrors.InvalidConfiguration)
                .WithErrorDescription("The on-premise deployment is not supported for online license.");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Unset_LicensePlan_Throws(bool isDebuggerAttached)
        {
            licenseDebugger = new MockLicenseDebugger(isDebuggerAttached);
            terminalOptions.Licensing.Deployment = TerminalIdentifiers.OnPremiseDeployment;

            terminalOptions.Id = TerminalIdentifiers.TestApplicationId;
            terminalOptions.Licensing.LicenseFile = testOfflineLicPath;
            terminalOptions.Licensing.LicensePlan = null!;

            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());
            Func<Task> act = async () => await licenseExtractor.ExtractLicenseAsync();
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode(TerminalErrors.InvalidConfiguration)
                .WithErrorDescription("The license plan is not valid. plan=");
        }

        [Theory]
        [InlineData(TerminalLicensePlans.OnPremise)]
        [InlineData(TerminalLicensePlans.Unlimited)]
        public async Task Valid_LicensePlan_Does_Not_Throws(string licPlan)
        {
            licenseDebugger = new MockLicenseDebugger(false);
            terminalOptions.Licensing.Deployment = TerminalIdentifiers.OnPremiseDeployment;
            terminalOptions.Licensing.LicensePlan = licPlan;

            terminalOptions.Id = TerminalIdentifiers.TestApplicationId;
            terminalOptions.Licensing.LicenseFile = testOfflineLicPath;

            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());
            await licenseExtractor.ExtractLicenseAsync();
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

        private void AssertOnPremiseDeploymentLicense(License license)
        {
            license.Claims.Should().BeEquivalentTo(new LicenseClaims());
            license.LicenseKey.Should().Be("on-premise-deployment");
            license.Plan.Should().Be(terminalOptions.Licensing.LicensePlan);
            license.Usage.Should().Be("on-premise-deployment");
            license.Limits.Should().BeEquivalentTo(LicenseLimits.Create(license.Plan, license.Claims.Custom));
        }

        private readonly ITerminalCommandStore commandStore;
        private readonly TerminalOptions terminalOptions;
        private readonly string testOfflineLicPath;
        private readonly string testOnlineLicPath;
        private ILicenseChecker licenseChecker = null!;
        private ILicenseDebugger licenseDebugger = null!;
        private ILicenseExtractor licenseExtractor = null!;
    }
}
