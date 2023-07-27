/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Shared.Exceptions;
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
            testOnlineLicPath = GetJsonLicenseFileForLocalHostGithubSecretForCICD("PI_CLI_TEST_ONLINE_LIC");
            testOfflineLicPath = GetJsonLicenseFileForLocalHostGithubSecretForCICD("PI_CLI_TEST_OFFLINE_LIC");

            terminalOptions = MockTerminalOptions.NewLegacyOptions();
            licenseExtractor = new LicenseExtractor(terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>());
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

        [Fact]
        public async Task Extracts_From_Online_If_Validation_Key_Is_Null()
        {
            if (!TerminalHelper.IsDevMode())
            {
                return;
            }

            terminalOptions.Licensing.LicenseKey = testOnlineLicPath;
            terminalOptions.Licensing.KeySource = LicenseSources.JsonFile;
            terminalOptions.Handler.LicenseHandler = TerminalHandlers.OnlineLicenseHandler;
            terminalOptions.Http.HttpClientName = httpClientName;
            terminalOptions.Licensing.ConsumerTenantId = "a8379958-ea19-4918-84dc-199bf012361e";
            terminalOptions.Licensing.Subject = "68d230be-cf83-49a6-c83f-42949fb40f46";
            terminalOptions.Licensing.AuthorizedApplicationId = "0c1a06c9-c0ee-476c-bf54-527bcf71ada2";
            terminalOptions.Licensing.ProviderId = LicenseProviders.PerpetualIntelligence;
            licenseExtractor = new LicenseExtractor(terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());

            terminalOptions.Handler.LicenseHandler = TerminalHandlers.OnPremiseLicenseHandler;
            LicenseExtractorResult? result = await licenseExtractor.ExtractAsync(new LicenseExtractorContext());
            result.License.Should().NotBeNull();

            // The license is for on-prem but the extraction was done via online
            result.License.Handler.Should().Be(TerminalHandlers.OnPremiseLicenseHandler);
            result.ExtractionHandler.Should().Be(TerminalHandlers.OnlineLicenseHandler);
        }

        [Fact]
        public async Task Extracts_From_Offline_If_Validation_Key_Is_Not_Null()
        {
            if (!TerminalHelper.IsDevMode())
            {
                return;
            }

            terminalOptions.Licensing.LicenseKey = testOfflineLicPath;
            terminalOptions.Licensing.KeySource = LicenseSources.JsonFile;
            terminalOptions.Handler.LicenseHandler = TerminalHandlers.OfflineLicenseHandler;
            terminalOptions.Licensing.ConsumerTenantId = "a8379958-ea19-4918-84dc-199bf012361e";
            terminalOptions.Licensing.Subject = "68d230be-cf83-49a6-c83f-42949fb40f46";
            terminalOptions.Licensing.AuthorizedApplicationId = "0c1a06c9-c0ee-476c-bf54-527bcf71ada2";
            terminalOptions.Licensing.ProviderId = LicenseProviders.PerpetualIntelligence;
            licenseExtractor = new LicenseExtractor(terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());

            terminalOptions.Handler.LicenseHandler = TerminalHandlers.OnPremiseLicenseHandler;
            LicenseExtractorResult? result = await licenseExtractor.ExtractAsync(new LicenseExtractorContext());
            result.License.Should().NotBeNull();

            // The license is for on-prem but the extraction was done via offline
            result.License.Handler.Should().Be(TerminalHandlers.OnPremiseLicenseHandler);
            result.ExtractionHandler.Should().Be(TerminalHandlers.OfflineLicenseHandler);
        }

        private static string GetJsonLicenseFileForLocalHostGithubSecretForCICD(string env)
        {
            // The demo json is too long for system env, so we use path for system env and json for github
            string? fileOrJson = Environment.GetEnvironmentVariable(env);

            if (fileOrJson == null)
            {
                throw new ErrorException(TerminalErrors.InvalidConfiguration, "Environment variable with license key not found. env={0}", env);
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

        private readonly TerminalOptions terminalOptions;
        private readonly string httpClientName = "prod";
        private ILicenseExtractor licenseExtractor;
        private readonly string testOnlineLicPath;
        private readonly string testOfflineLicPath;
    }
}