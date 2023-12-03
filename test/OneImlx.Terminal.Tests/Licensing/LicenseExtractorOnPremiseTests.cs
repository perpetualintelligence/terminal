/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Shared.Licensing;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Stores;
using OneImlx.Terminal.Stores.InMemory;
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
            testOnlineLicPath = GetJsonLicenseFileForLocalHostGitHubSecretForCICD("PI_CLI_TEST_ONLINE_LIC");
            testOfflineLicPath = GetJsonLicenseFileForLocalHostGitHubSecretForCICD("PI_CLI_TEST_OFFLINE_LIC");

            terminalOptions = MockTerminalOptions.NewLegacyOptions();
            terminalOptions.Licensing.LicensePlan = TerminalLicensePlans.Unlimited;

            commandStore = new InMemoryImmutableCommandStore(MockCommands.LicensingCommands.TextHandler, MockCommands.LicensingCommands.Values);
        }

        [Theory]
        [InlineData(TerminalLicensePlans.Demo)]
        [InlineData(TerminalLicensePlans.Micro)]
        [InlineData(TerminalLicensePlans.SMB)]
        [InlineData(TerminalLicensePlans.Enterprise)]
        public async Task Invalid_LicensePlan_Throws(string licPlan)
        {
            licenseDebugger = new MockLicenseDebugger(false);
            terminalOptions.Licensing.OnPremiseDeployment = true;
            terminalOptions.Licensing.LicensePlan = licPlan;

            terminalOptions.Licensing.LicenseKey = testOnlineLicPath;
            terminalOptions.Licensing.LicenseKeySource = LicenseSources.JsonFile;
            terminalOptions.Licensing.HttpClientName = httpClientName;
            terminalOptions.Licensing.ConsumerTenantId = "a8379958-ea19-4918-84dc-199bf012361e";
            terminalOptions.Licensing.Subject = "68d230be-cf83-49a6-c83f-42949fb40f46";
            terminalOptions.Licensing.AuthorizedApplicationId = "641e1dc1-7ff3-4510-a8e5-abb787fe0fe1";

            terminalOptions.Handler.LicenseHandler = TerminalHandlers.OnPremiseLicenseHandler;

            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());
            Func<Task> act = async () => await licenseExtractor.ExtractLicenseAsync(new LicenseExtractorContext());
            await act.Should().ThrowAsync<TerminalException>().WithMessage($"The license plan is not authorized for on-premise deployment, see licensing options. plan={licPlan}");
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
            terminalOptions.Licensing.HttpClientName = httpClientName;
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
            terminalOptions.Licensing.HttpClientName = httpClientName;
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

        [Fact]
        public async Task OnPremDeployment_Enabled_Offline_ValidKey_Is_Ignored_And_License_Passes()
        {
            // Before extract get should be null
            licenseDebugger = new MockLicenseDebugger(false);
            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());

            License? licenseFromGet = await licenseExtractor.GetLicenseAsync();
            licenseFromGet.Should().BeNull();

            terminalOptions.Licensing.LicenseKey = testOfflineLicPath;
            terminalOptions.Licensing.LicenseKeySource = LicenseSources.JsonFile;
            terminalOptions.Licensing.OnPremiseDeployment = true;
            terminalOptions.Licensing.LicensePlan = TerminalLicensePlans.Unlimited;
            terminalOptions.Licensing.ConsumerTenantId = "a8379958-ea19-4918-84dc-199bf012361e";
            terminalOptions.Licensing.Subject = "68d230be-cf83-49a6-c83f-42949fb40f46";
            terminalOptions.Licensing.AuthorizedApplicationId = "641e1dc1-7ff3-4510-a8e5-abb787fe0fe1";
            terminalOptions.Handler.LicenseHandler = TerminalHandlers.OnPremiseLicenseHandler;
            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());

            var result = await licenseExtractor.ExtractLicenseAsync(new LicenseExtractorContext());
            result.License.Should().NotBeNull();

            // ensure passed and extraction handler
            result.License.Handler.Should().Be(TerminalHandlers.OnPremiseLicenseHandler);
            result.ExtractionHandler.Should().Be(TerminalHandlers.OnPremiseLicenseHandler);

            // License claims
            result.License.Claims.Should().NotBeNull();
            result.License.Limits.Should().NotBeNull();
            result.License.LicenseKey.Should().NotBeNull();

            // license key
            result.License.LicenseKeySource.Should().Be(LicenseSources.JsonFile);
            result.License.LicenseKey.Should().Be("on-premise-deployment");

            // plan, mode and usage
            result.License.ProviderId.Should().Be("urn:oneimlx:lic:pvdr:pi");
            terminalOptions.Handler.LicenseHandler = TerminalHandlers.OfflineLicenseHandler;
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
            result.License.Claims.Name.Should().BeNull();
            //result.License.Claims.NotBefore.Should().BeNull();
            result.License.Claims.ObjectId.Should().BeNull();
            result.License.Claims.ObjectCountry.Should().BeNull();
            result.License.Claims.Subject.Should().BeNull(); // Test Microsoft SaaS subscription
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
            LicenseCheckerResult licResult = await licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(result.License));
            licResult.Should().NotBeNull();
            licResult.CommandGroupCount.Should().Be(3);
            licResult.License.Should().BeSameAs(result.License);
            licResult.OptionCount.Should().Be(90);
            licResult.RootCommandCount.Should().Be(3);
            licResult.SubCommandCount.Should().Be(5);
            licResult.TerminalCount.Should().Be(1);
        }

        [Fact]
        public async Task OnPremDeployment_Enabled_Online_ValidKey_Is_Ignored_And_License_Passes()
        {
            // Before extract get should be null
            licenseDebugger = new MockLicenseDebugger(false);
            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());

            License? licenseFromGet = await licenseExtractor.GetLicenseAsync();
            licenseFromGet.Should().BeNull();

            terminalOptions.Licensing.LicenseKey = testOnlineLicPath;
            terminalOptions.Licensing.LicenseKeySource = LicenseSources.JsonFile;
            terminalOptions.Licensing.OnPremiseDeployment = true;
            terminalOptions.Licensing.LicensePlan = TerminalLicensePlans.Unlimited;
            terminalOptions.Licensing.ConsumerTenantId = "a8379958-ea19-4918-84dc-199bf012361e";
            terminalOptions.Licensing.Subject = "68d230be-cf83-49a6-c83f-42949fb40f46";
            terminalOptions.Licensing.AuthorizedApplicationId = "641e1dc1-7ff3-4510-a8e5-abb787fe0fe1";
            terminalOptions.Handler.LicenseHandler = TerminalHandlers.OnPremiseLicenseHandler;
            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());

            var result = await licenseExtractor.ExtractLicenseAsync(new LicenseExtractorContext());
            result.License.Should().NotBeNull();

            // ensure passed and extraction handler
            result.License.Handler.Should().Be(TerminalHandlers.OnPremiseLicenseHandler);
            result.ExtractionHandler.Should().Be(TerminalHandlers.OnPremiseLicenseHandler);

            // License claims
            result.License.Claims.Should().NotBeNull();
            result.License.Limits.Should().NotBeNull();
            result.License.LicenseKey.Should().NotBeNull();

            // license key
            result.License.LicenseKeySource.Should().Be(LicenseSources.JsonFile);
            result.License.LicenseKey.Should().Be("on-premise-deployment");

            // plan, mode and usage
            result.License.ProviderId.Should().Be("urn:oneimlx:lic:pvdr:pi");
            terminalOptions.Handler.LicenseHandler = TerminalHandlers.OfflineLicenseHandler;
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
            result.License.Claims.Name.Should().BeNull();
            //result.License.Claims.NotBefore.Should().BeNull();
            result.License.Claims.ObjectId.Should().BeNull();
            result.License.Claims.ObjectCountry.Should().BeNull();
            result.License.Claims.Subject.Should().BeNull(); // Test Microsoft SaaS subscription
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
            LicenseCheckerResult licResult = await licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(result.License));
            licResult.Should().NotBeNull();
            licResult.CommandGroupCount.Should().Be(3);
            licResult.License.Should().BeSameAs(result.License);
            licResult.OptionCount.Should().Be(90);
            licResult.RootCommandCount.Should().Be(3);
            licResult.SubCommandCount.Should().Be(5);
            licResult.TerminalCount.Should().Be(1);
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
            license.Claims.Should().BeEquivalentTo(new LicenseClaimsModel());
            license.Handler.Should().Be(TerminalHandlers.OnPremiseLicenseHandler);
            license.LicenseKey.Should().Be("on-premise-deployment");
            license.LicenseKeySource.Should().Be(LicenseSources.JsonFile);
            license.Plan.Should().Be(terminalOptions.Licensing.LicensePlan);
            license.Usage.Should().Be("on-premise-deployment");
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
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

        private readonly TerminalOptions terminalOptions;
        private readonly string httpClientName = "prod";
        private ILicenseExtractor licenseExtractor = null!;
        private readonly string testOnlineLicPath;
        private readonly string testOfflineLicPath;
        private ILicenseDebugger licenseDebugger = null!;
        private ILicenseChecker licenseChecker = null!;
        private IImmutableCommandStore commandStore;
    }
}