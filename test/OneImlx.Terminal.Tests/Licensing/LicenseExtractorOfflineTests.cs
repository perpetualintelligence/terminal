/*
    Copyright 2024 (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using OneImlx.Shared.Json;
using OneImlx.Shared.Licensing;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Mocks;
using OneImlx.Test.FluentAssertions;
using Xunit;

namespace OneImlx.Terminal.Licensing
{
    public class LicenseExtractorOfflineTests : IDisposable
    {
        public LicenseExtractorOfflineTests()
        {
            // Read the lic file from GitHub secrets
            testLicPath = GetJsonLicenseFileForLocalHostGitHubSecretForCICD("PI_TERMINAL_TEST_OFFLINE_LIC");

            string nonJson = "non json document";
            nonJsonLicPath = Path.Combine(AppContext.BaseDirectory, $"{Guid.NewGuid()}.json");
            File.WriteAllText(nonJsonLicPath, nonJson);

            terminalOptions = MockTerminalOptions.NewLegacyOptions();
            terminalOptions.Licensing.LicensePlan = TerminalLicensePlans.Unlimited;

            licenseDebugger = new MockLicenseDebugger(isDebuggerAttached: false);
            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>());
        }

        [Fact]
        public void CustomClaims_ShouldBeDecoratedWith_JsonConverterAttribute()
        {
            typeof(LicenseClaims).GetProperty("Custom").Should().BeDecoratedWith<JsonConverterAttribute>(static a => a.ConverterType == typeof(DictionaryStringObjectPrimitiveJsonConverter));
        }

        [Fact]
        public void CustomClaims_ShouldNotBeDecoratedWith_JsonExtensionDataAttribute()
        {
            typeof(LicenseClaims).GetProperty("Custom").Should().NotBeDecoratedWith<JsonExtensionDataAttribute>();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            if (File.Exists(testLicPath))
            {
                File.Delete(testLicPath);
            }

            if (File.Exists(nonJsonLicPath))
            {
                File.Delete(nonJsonLicPath);
            }
        }

        [Fact]
        public async Task ExtractFromJsonAsync_Invalid_Plan_ShouldErrorAsync()
        {
            terminalOptions.Id = TerminalIdentifiers.TestApplicationId;
            terminalOptions.Licensing.LicenseFile = testLicPath;
            terminalOptions.Licensing.LicensePlan = "invalid_plan";

            Func<Task> func = async () => await licenseExtractor.ExtractLicenseAsync(new LicenseExtractorContext());
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The license plan is not valid. plan=invalid_plan");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_InvalidApplicationId_ShouldErrorAsync()
        {
            terminalOptions.Id = "invalid_app";
            terminalOptions.Licensing.LicenseFile = testLicPath;
            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());

            Func<Task> func = async () => await licenseExtractor.ExtractLicenseAsync(new LicenseExtractorContext());
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.UnauthorizedAccess).WithErrorDescription("The application is not authorized. app=invalid_app");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_InvalidAuthApp_ShouldErrorAsync()
        {
            terminalOptions.Id = "invalid_auth_app";
            terminalOptions.Licensing.LicenseFile = testLicPath;
            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());

            Func<Task> func = async () => await licenseExtractor.ExtractLicenseAsync(new LicenseExtractorContext());
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.UnauthorizedAccess).WithErrorDescription("The application is not authorized. app=invalid_auth_app");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("invalid_license_contents")]
        public async Task ExtractFromJsonAsync_InvalidKeyFilePath_ShouldErrorAsync(string? licenseContents)
        {
            terminalOptions.Id = TerminalIdentifiers.TestApplicationId;
            terminalOptions.Licensing.LicenseFile = "D:\\lic\\path_does_exist\\invalid.lic";
            terminalOptions.Licensing.LicenseContents = licenseContents;

            if (licenseContents != null)
            {
                Func<Task> func = async () => await licenseExtractor.ExtractLicenseAsync(new LicenseExtractorContext());
                await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The license file or contents are not valid. file=D:\\lic\\path_does_exist\\invalid.lic info=The input is not a valid Base-64 string as it contains a non-base 64 character, more than two padding characters, or an illegal character among the padding characters.");
            }
            else
            {
                Func<Task> func = async () => await licenseExtractor.ExtractLicenseAsync(new LicenseExtractorContext());
                await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The license file path is not valid. file=D:\\lic\\path_does_exist\\invalid.lic");
            }
        }

        [Fact]
        public async Task ExtractFromJsonAsync_InvalidMode_ShouldErrorAsync()
        {
            terminalOptions.Id = TerminalIdentifiers.TestApplicationId;

            string? tempJsonLicPath = null;

            try
            {
                // Read testLicPath and replace the mode JSON property with invalid_mode so the code can throw exception
                string json = File.ReadAllText(testLicPath);
                json = json.Replace("\"mode\": \"offline\"", "\"mode\": \"invalid_mode\"");
                tempJsonLicPath = Path.Combine(AppContext.BaseDirectory, $"{Guid.NewGuid()}.json");
                File.WriteAllText(tempJsonLicPath, json);
                terminalOptions.Licensing.LicenseFile = tempJsonLicPath;

                Func<Task> func = async () => await licenseExtractor.ExtractLicenseAsync(new LicenseExtractorContext());
                await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The license mode is not valid. mode=invalid_mode");
            }
            finally
            {
                if (tempJsonLicPath != null && File.Exists(tempJsonLicPath))
                {
                    File.Delete(tempJsonLicPath);
                }
            }
        }

        [Fact(Skip = "Need to add an jwt token with invalid license provider")]
        public async Task ExtractFromJsonAsync_InvalidProviderTenant_ShouldErrorAsync()
        {
            terminalOptions.Id = TerminalIdentifiers.TestApplicationId;
            terminalOptions.Licensing.LicenseFile = testLicPath;
            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());

            Func<Task> func = async () => await licenseExtractor.ExtractLicenseAsync(new LicenseExtractorContext());
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The provider is not authorized. provider_id=invalid_provider");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_MissingAuthApp_ShouldErrorAsync()
        {
            terminalOptions.Id = null!;
            terminalOptions.Licensing.LicenseFile = testLicPath;

            Func<Task> func = async () => await licenseExtractor.ExtractLicenseAsync(new LicenseExtractorContext());
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The authorized application is not configured as a terminal identifier.");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_NoHttpClientFactory_ShouldNotErrorAsync()
        {
            terminalOptions.Id = TerminalIdentifiers.TestApplicationId;
            terminalOptions.Licensing.LicenseFile = testLicPath;

            await licenseExtractor.ExtractLicenseAsync(new LicenseExtractorContext());
        }

        [Fact]
        public async Task ExtractFromJsonAsync_NoHttpClientName_ShouldNotErrorAsync()
        {
            terminalOptions.Id = TerminalIdentifiers.TestApplicationId;
            terminalOptions.Licensing.LicenseFile = testLicPath;
            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());

            await licenseExtractor.ExtractLicenseAsync(new LicenseExtractorContext());
        }

        [Fact]
        public async Task ExtractFromJsonAsync_NonJsonLic_ShouldErrorAsync()
        {
            terminalOptions.Id = TerminalIdentifiers.TestApplicationId;
            terminalOptions.Licensing.LicenseFile = nonJsonLicPath;

            try
            {
                await licenseExtractor.ExtractLicenseAsync(new LicenseExtractorContext());
            }
            catch (TerminalException ex)
            {
                ex.Message.Should().StartWith("The license file or contents are not valid. file=");
            }
        }

        [Fact]
        public async Task ExtractFromJsonAsync_ValidKey_ShouldContainClaimsAsync()
        {
            // Before extract get should be null
            License? licenseFromGet = await licenseExtractor.GetLicenseAsync();
            licenseFromGet.Should().BeNull();

            terminalOptions.Id = TerminalIdentifiers.TestApplicationId;
            terminalOptions.Licensing.LicenseFile = testLicPath;
            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());

            var result = await licenseExtractor.ExtractLicenseAsync(new LicenseExtractorContext());
            result.License.Should().NotBeNull();

            // ensure passed and extraction handler
            result.ExtractionMode.Should().Be(TerminalIdentifiers.OfflineLicenseMode);

            // License claims
            result.License.Claims.Should().NotBeNull();
            result.License.Limits.Should().NotBeNull();
            result.License.LicenseKey.Should().NotBeNull();

            // license key
            result.License.LicenseKey.Should().Be(testLicPath);

            // plan, mode and usage
            result.License.Plan.Should().Be("urn:oneimlx:terminal:plan:unlimited");
            result.License.Usage.Should().Be("urn:oneimlx:lic:usage:org");

            // claims
            result.License.Claims.AcrValues.Should().Be("urn:oneimlx:terminal:plan:unlimited urn:oneimlx:lic:usage:org 91b7fb8e-3fd1-4a80-9978-99c6bfbe2d32");
            result.License.Claims.Audience.Should().Be("https://login.perpetualintelligence.com/21d818a5-935c-496f-9faf-d9ff9d9645d8/v2.0");
            result.License.Claims.AuthorizedParty.Should().Be("urn:oneimlx:terminal");
            result.License.Claims.TenantCountry.Should().Be("USA");
            result.License.Claims.Custom.Should().BeNull();

            //result.License.Claims.Expiry.Should().NotBeNull();
            //result.License.Claims.IssuedAt.Should().NotBeNull();
            result.License.Claims.Issuer.Should().Be("https://api.perpetualintelligence.com");
            result.License.Claims.Jti.Should().NotBeNullOrWhiteSpace();
            result.License.Claims.TenantName.Should().Be("pi-test");

            //result.License.Claims.NotBefore.Should().NotBeNull();
            result.License.Claims.Id.Should().Be("bb8969c6-b1a9-42ef-986b-a56db4b155f1"); // Test License Id
            result.License.Claims.Subject.Should().Be("eaf50a3b-2e60-4029-cf41-4f1b65fdf749"); // Test subscription
            result.License.Claims.TenantId.Should().Be("21d818a5-935c-496f-9faf-d9ff9d9645d8");

            // no custom claims
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
        }

        [Fact]
        public async Task ExtractFromJsonAsync_ValidKey_Via_Contents_ShouldContainClaimsAsync()
        {
            // Before extract get should be null
            License? licenseFromGet = await licenseExtractor.GetLicenseAsync();
            licenseFromGet.Should().BeNull();

            terminalOptions.Id = TerminalIdentifiers.TestApplicationId;
            terminalOptions.Licensing.LicenseContents = TerminalServices.EncodeLicenseContents(File.ReadAllText(testLicPath));
            terminalOptions.Licensing.LicenseFile = "license-with-contents";
            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());

            var result = await licenseExtractor.ExtractLicenseAsync(new LicenseExtractorContext());
            result.License.Should().NotBeNull();

            // ensure passed and extraction handler
            result.ExtractionMode.Should().Be(TerminalIdentifiers.OfflineLicenseMode);

            // License claims
            result.License.Claims.Should().NotBeNull();
            result.License.Limits.Should().NotBeNull();
            result.License.LicenseKey.Should().NotBeNull();

            // license key
            result.License.LicenseKey.Should().Be("license-with-contents");

            // plan, mode and usage
            result.License.Plan.Should().Be("urn:oneimlx:terminal:plan:unlimited");
            result.License.Usage.Should().Be("urn:oneimlx:lic:usage:org");

            // claims
            result.License.Claims.AcrValues.Should().Be("urn:oneimlx:terminal:plan:unlimited urn:oneimlx:lic:usage:org 91b7fb8e-3fd1-4a80-9978-99c6bfbe2d32");
            result.License.Claims.Audience.Should().Be("https://login.perpetualintelligence.com/21d818a5-935c-496f-9faf-d9ff9d9645d8/v2.0");
            result.License.Claims.AuthorizedParty.Should().Be("urn:oneimlx:terminal");
            result.License.Claims.TenantCountry.Should().Be("USA");
            result.License.Claims.Custom.Should().BeNull();

            //result.License.Claims.Expiry.Should().NotBeNull();
            //result.License.Claims.IssuedAt.Should().NotBeNull();
            result.License.Claims.Issuer.Should().Be("https://api.perpetualintelligence.com");
            result.License.Claims.Jti.Should().NotBeNullOrWhiteSpace();
            result.License.Claims.TenantName.Should().Be("pi-test");

            //result.License.Claims.NotBefore.Should().NotBeNull();
            result.License.Claims.Id.Should().Be("bb8969c6-b1a9-42ef-986b-a56db4b155f1"); // Test License Id
            result.License.Claims.Subject.Should().Be("eaf50a3b-2e60-4029-cf41-4f1b65fdf749"); // Test subscription
            result.License.Claims.TenantId.Should().Be("21d818a5-935c-496f-9faf-d9ff9d9645d8");

            // no custom claims
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
        }

        [Fact]
        public async Task ExtractFromJsonAsync_WithNoLicenseKey_ShouldErrorAsync()
        {
            terminalOptions.Id = TerminalIdentifiers.TestApplicationId;
            terminalOptions.Licensing.LicenseFile = null!;

            Func<Task> func = async () => await licenseExtractor.ExtractLicenseAsync(new LicenseExtractorContext());
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The license file is not configured.");
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

        private readonly ILicenseDebugger licenseDebugger;
        private readonly string nonJsonLicPath;
        private readonly TerminalOptions terminalOptions;
        private readonly string testLicPath;
        private ILicenseExtractor licenseExtractor;
    }
}
