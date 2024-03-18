/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.Logging;
using OneImlx.Shared.Json;
using OneImlx.Shared.Licensing;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Mocks;
using OneImlx.Test.FluentAssertions;
using System;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Licensing
{
    public class LicenseExtractorOfflineTests : IDisposable
    {
        public LicenseExtractorOfflineTests()
        {
            // Read the lic file from Github secrets
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
        public async Task ExtractFromJsonAsync_Invalid_Plan_ShouldErrorAsync()
        {
            terminalOptions.Licensing.Application = "08c6925f-a734-4e24-8d84-e06737420766";
            terminalOptions.Licensing.LicenseFile = testLicPath;
            terminalOptions.Handler.LicenseHandler = TerminalHandlers.OfflineLicenseHandler;
            terminalOptions.Licensing.LicensePlan = "invalid_plan";

            Func<Task> func = async () => await licenseExtractor.ExtractLicenseAsync(new LicenseExtractorContext());
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The license plan is not valid, see licensing options. plan=invalid_plan");
        }

        [Fact]
        public void CustomClaims_ShouldBeDecoratedWith_JsonConverterAttribute()
        {
            typeof(LicenseClaims).GetProperty("Custom").Should().BeDecoratedWith<JsonConverterAttribute>(a => a.ConverterType == typeof(DictionaryStringObjectPrimitiveJsonConverter));
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
        public async Task ExtractFromJsonAsync_InvalidAuthApp_ShouldErrorAsync()
        {
            terminalOptions.Licensing.Application = "invalid_auth_app";
            terminalOptions.Licensing.LicenseFile = testLicPath;
            terminalOptions.Handler.LicenseHandler = TerminalHandlers.OfflineLicenseHandler;
            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());

            Func<Task> func = async () => await licenseExtractor.ExtractLicenseAsync(new LicenseExtractorContext());
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.UnauthorizedAccess).WithErrorDescription("The application is not authorized. app=invalid_auth_app");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_InvalidKeyFilePath_ShouldErrorAsync()
        {
            terminalOptions.Licensing.Application = "08c6925f-a734-4e24-8d84-e06737420766";
            terminalOptions.Licensing.LicenseFile = "D:\\lic\\path_does_exist\\invalid.lic";

            Func<Task> func = async () => await licenseExtractor.ExtractLicenseAsync(new LicenseExtractorContext());
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The license file path is not valid, see licensing options. key_file=D:\\lic\\path_does_exist\\invalid.lic");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_MissingAuthApp_ShouldErrorAsync()
        {
            terminalOptions.Licensing.Application = null;
            terminalOptions.Licensing.LicenseFile = testLicPath;
            terminalOptions.Handler.LicenseHandler = TerminalHandlers.OfflineLicenseHandler;

            Func<Task> func = async () => await licenseExtractor.ExtractLicenseAsync(new LicenseExtractorContext());
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The authorized application is not configured, see licensing options.");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_NonJsonLic_ShouldErrorAsync()
        {
            terminalOptions.Licensing.Application = "08c6925f-a734-4e24-8d84-e06737420766";
            terminalOptions.Licensing.LicenseFile = nonJsonLicPath;
            terminalOptions.Handler.LicenseHandler = TerminalHandlers.OfflineLicenseHandler;

            try
            {
                await licenseExtractor.ExtractLicenseAsync(new LicenseExtractorContext());
            }
            catch (TerminalException ex)
            {
                ex.Message.Should().StartWith("The license file is not valid, see licensing options. key_file=");
            }
        }

        [Fact]
        public async Task ExtractFromJsonAsync_InvalidMode_ShouldErrorAsync()
        {
            terminalOptions.Licensing.Application = "08c6925f-a734-4e24-8d84-e06737420766";
            terminalOptions.Licensing.LicenseFile = testLicPath;

            terminalOptions.Handler.LicenseHandler = "invalid_lic_mode";

            Func<Task> func = async () => await licenseExtractor.ExtractLicenseAsync(new LicenseExtractorContext());
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The license handler is not valid, see hosting options. licensing_handler=invalid_lic_mode");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_InvalidApplicationId_ShouldErrorAsync()
        {
            terminalOptions.Licensing.LicenseFile = testLicPath;
            terminalOptions.Handler.LicenseHandler = TerminalHandlers.OfflineLicenseHandler;
            terminalOptions.Licensing.Application = "invalid_app";
            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());

            Func<Task> func = async () => await licenseExtractor.ExtractLicenseAsync(new LicenseExtractorContext());
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.UnauthorizedAccess).WithErrorDescription("The application is not authorized. app=invalid_app");
        }

        [Fact(Skip = "Need to add an jwt token with invalid license provider")]
        public async Task ExtractFromJsonAsync_InvalidProviderTenant_ShouldErrorAsync()
        {
            terminalOptions.Licensing.LicenseFile = testLicPath;
            terminalOptions.Handler.LicenseHandler = TerminalHandlers.OfflineLicenseHandler;
            terminalOptions.Licensing.Application = "08c6925f-a734-4e24-8d84-e06737420766";
            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());

            Func<Task> func = async () => await licenseExtractor.ExtractLicenseAsync(new LicenseExtractorContext());
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The provider is not authorized, see licensing options. provider_id=invalid_provider");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_NoHttpClientFactory_ShouldNotErrorAsync()
        {
            terminalOptions.Licensing.Application = "08c6925f-a734-4e24-8d84-e06737420766";
            terminalOptions.Licensing.LicenseFile = testLicPath;
            terminalOptions.Handler.LicenseHandler = TerminalHandlers.OfflineLicenseHandler;

            await licenseExtractor.ExtractLicenseAsync(new LicenseExtractorContext());
        }

        [Fact]
        public async Task ExtractFromJsonAsync_NoHttpClientName_ShouldNotErrorAsync()
        {
            terminalOptions.Licensing.Application = "08c6925f-a734-4e24-8d84-e06737420766";
            terminalOptions.Licensing.LicenseFile = testLicPath;
            terminalOptions.Handler.LicenseHandler = TerminalHandlers.OfflineLicenseHandler;
            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());

            await licenseExtractor.ExtractLicenseAsync(new LicenseExtractorContext());
        }

        [Fact]
        public async Task ExtractFromJsonAsync_ValidKey_ShouldContainClaimsAsync()
        {
            // Before extract get should be null
            License? licenseFromGet = await licenseExtractor.GetLicenseAsync();
            licenseFromGet.Should().BeNull();

            terminalOptions.Licensing.LicenseFile = testLicPath;
            terminalOptions.Handler.LicenseHandler = TerminalHandlers.OfflineLicenseHandler;
            terminalOptions.Licensing.Application = "08c6925f-a734-4e24-8d84-e06737420766";
            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());

            var result = await licenseExtractor.ExtractLicenseAsync(new LicenseExtractorContext());
            result.License.Should().NotBeNull();

            // ensure passed and extraction handler
            result.License.Handler.Should().Be(TerminalHandlers.OfflineLicenseHandler);
            result.ExtractionHandler.Should().Be(TerminalHandlers.OfflineLicenseHandler);

            // License claims
            result.License.Claims.Should().NotBeNull();
            result.License.Limits.Should().NotBeNull();
            result.License.LicenseKey.Should().NotBeNull();

            // license key
            result.License.LicenseKey.Should().Be(testLicPath);

            // plan, mode and usage
            terminalOptions.Handler.LicenseHandler = TerminalHandlers.OfflineLicenseHandler;
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
            result.License.Claims.Id.Should().Be("48ee7ff0-9874-4098-b4fc-89717f7a9596"); // Test License Id
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
            terminalOptions.Licensing.Application = "08c6925f-a734-4e24-8d84-e06737420766";
            terminalOptions.Licensing.LicenseFile = null;
            terminalOptions.Handler.LicenseHandler = TerminalHandlers.OfflineLicenseHandler;

            Func<Task> func = async () => await licenseExtractor.ExtractLicenseAsync(new LicenseExtractorContext());
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The license file is not configured, see licensing options.");
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

        private readonly TerminalOptions terminalOptions;
        private ILicenseExtractor licenseExtractor;
        private readonly string nonJsonLicPath;
        private readonly string testLicPath;
        private readonly ILicenseDebugger licenseDebugger;
    }
}