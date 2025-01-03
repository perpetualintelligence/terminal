﻿/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.Logging;
using OneImlx.Shared.Authorization;
using OneImlx.Shared.Json;
using OneImlx.Shared.Licensing;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Mocks;
using OneImlx.Test.FluentAssertions;
using System;
using System.IO;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Licensing
{
    [Collection("Sequential")]
    public class LicenseExtractorOnlineTests : IDisposable
    {
        public LicenseExtractorOnlineTests()
        {
            // Local Vs GitHub
            testOnlineLicPath = GetJsonLicenseFileForLocalHostGitHubSecretForCICD("PI_TERMINAL_TEST_ONLINE_LIC");
            testDemoLicPath = GetJsonLicenseFileForLocalHostGitHubSecretForCICD("PI_TERMINAL_TEST_DEMO_LIC");

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

            if (File.Exists(testOnlineLicPath))
            {
                File.Delete(testOnlineLicPath);
            }

            if (File.Exists(testDemoLicPath))
            {
                File.Delete(testDemoLicPath);
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
            terminalOptions.Licensing.LicenseFile = testOnlineLicPath;
            terminalOptions.Licensing.LicensePlan = "invalid_plan";

            Func<Task> func = async () => await licenseExtractor.ExtractLicenseAsync();
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The license plan is not valid. plan=invalid_plan");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_InvalidAuthApp_ShouldErrorAsync()
        {
            terminalOptions.Id = "invalid_auth_app";
            terminalOptions.Licensing.LicenseFile = testOnlineLicPath;
            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());

            Func<Task> func = async () => await licenseExtractor.ExtractLicenseAsync();
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.UnauthorizedAccess).WithErrorDescription("The application is not authorized. application_id=invalid_auth_app");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_InvalidKeyFilePath_ShouldErrorAsync()
        {
            terminalOptions.Id = TerminalIdentifiers.TestApplicationId;
            terminalOptions.Licensing.LicenseFile = "D:\\lic\\path_does_exist\\invalid.lic";

            Func<Task> func = async () => await licenseExtractor.ExtractLicenseAsync();
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The license file path is not valid. file=D:\\lic\\path_does_exist\\invalid.lic");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_MissingAuthApp_ShouldErrorAsync()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            terminalOptions.Id = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            terminalOptions.Licensing.LicenseFile = testOnlineLicPath;

            Func<Task> func = async () => await licenseExtractor.ExtractLicenseAsync();
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The authorized application is not configured as a terminal identifier.");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_NonJsonLic_ShouldErrorAsync()
        {
            terminalOptions.Id = TerminalIdentifiers.TestApplicationId;
            terminalOptions.Licensing.LicenseFile = nonJsonLicPath;

            try
            {
                await licenseExtractor.ExtractLicenseAsync();
            }
            catch (TerminalException ex)
            {
                ex.Message.Should().StartWith("The license file or contents are not valid. file=");
            }
        }

        [Fact]
        public async Task ExtractFromJsonAsync_OnlineMode_DemoKey_ShouldContainClaimsAsync()
        {
            // Before extract get should be null
            License? licenseFromGet = await licenseExtractor.GetLicenseAsync();
            licenseFromGet.Should().BeNull();

            terminalOptions.Id = TerminalIdentifiers.TestApplicationId;
            terminalOptions.Licensing.LicenseFile = testDemoLicPath;
            terminalOptions.Licensing.LicensePlan = TerminalLicensePlans.Demo;
            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());

            var result = await licenseExtractor.ExtractLicenseAsync();
            result.License.Should().NotBeNull();
            result.License.Claims.Should().NotBeNull();
            result.License.Limits.Should().NotBeNull();
            result.License.LicenseKey.Should().NotBeNull();

            // license key
            result.License.LicenseKey.Should().Be(testDemoLicPath);

            // plan, mode and usage
            result.License.Plan.Should().Be("urn:oneimlx:terminal:plan:demo");
            result.License.Usage.Should().Be("urn:oneimlx:lic:usage:rnd");

            // claims
            result.License.Claims.AcrValues.Should().Be("urn:oneimlx:terminal:plan:demo urn:oneimlx:lic:usage:rnd 91b7fb8e-3fd1-4a80-9978-99c6bfbe2d32");
            result.License.Claims.Audience.Should().Be(AuthEndpoints.PiB2CIssuer("21d818a5-935c-496f-9faf-d9ff9d9645d8"));
            result.License.Claims.AuthorizedParty.Should().Be("urn:oneimlx:terminal");
            result.License.Claims.TenantCountry.Should().Be("USA");
            result.License.Claims.Custom.Should().BeNull();

            //result.License.Claims.Expiry.Date.Should().Be(DateTimeOffset.UtcNow.AddYears(1).ToLocalTime().Date);
            //result.License.Claims.IssuedAt.Date.Should().Be(DateTimeOffset.UtcNow.ToLocalTime().Date);
            result.License.Claims.Issuer.Should().Be("https://api.perpetualintelligence.com");
            result.License.Claims.Jti.Should().NotBeNullOrWhiteSpace();
            result.License.Claims.TenantName.Should().Be("pi-test");

            //result.License.Claims.NotBefore.Date.Should().Be(DateTimeOffset.UtcNow.ToLocalTime().Date);
            result.License.Claims.Subject.Should().Be("3dbb973a-5296-4cec-abd8-6a6a1683086b"); // Graph user id
            result.License.Claims.Id.Should().Be("cac4b942-e362-490b-a7b1-c4f7f0daf08f"); // Id
            result.License.Claims.TenantId.Should().Be("21d818a5-935c-496f-9faf-d9ff9d9645d8");

            // Verify limits
            LicenseLimits limits = result.License.Limits;
            limits.Plan.Should().Be(TerminalLicensePlans.Demo);

            limits.TerminalLimit.Should().Be(1);
            limits.RedistributionLimit.Should().Be(0);
            limits.RootCommandLimit.Should().Be(1);
            limits.GroupedCommandLimit.Should().Be(5);
            limits.SubCommandLimit.Should().Be(25);
            limits.OptionLimit.Should().Be(500);

            limits.StrictDataType.Should().Be(true);

            // Price
            result.License.Price.Plan.Should().Be("urn:oneimlx:terminal:plan:demo");
            result.License.Price.Currency.Should().Be("USD");
            result.License.Price.Monthly.Should().Be(0.0);
            result.License.Price.Yearly.Should().Be(0.0);

            // After extract and Get should return the correct license
            licenseFromGet = await licenseExtractor.GetLicenseAsync();
            licenseFromGet.Should().NotBeNull();
            licenseFromGet.Should().BeSameAs(result.License);
        }

        [Fact]
        public async Task ExtractFromJsonAsync_OnlineMode_InvalidApplicationId_ShouldErrorAsync()
        {
            terminalOptions.Id = "invalid_app";
            terminalOptions.Licensing.LicenseFile = testOnlineLicPath;
            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());

            Func<Task> func = async () => await licenseExtractor.ExtractLicenseAsync();
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.UnauthorizedAccess).WithErrorDescription("The application is not authorized. application_id=invalid_app");
        }

        [Fact(Skip = "Need to add an jwt token with invalid license provider")]
        public async Task ExtractFromJsonAsync_OnlineMode_InvalidProviderTenant_ShouldErrorAsync()
        {
            terminalOptions.Id = TerminalIdentifiers.TestApplicationId;
            terminalOptions.Licensing.LicenseFile = testOnlineLicPath;
            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());

            Func<Task> func = async () => await licenseExtractor.ExtractLicenseAsync();
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The provider is not authorized. provider_id=invalid_provider");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_OnlineMode_NoHttpClientFactory_ShouldErrorAsync()
        {
            terminalOptions.Id = TerminalIdentifiers.TestApplicationId;
            terminalOptions.Licensing.LicenseFile = testOnlineLicPath;

            Func<Task> func = async () => await licenseExtractor.ExtractLicenseAsync();
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The IHttpClientFactory is not configured.");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_OnlineMode_ValidKey_ShouldContainClaimsAsync()
        {
            // Before extract get should be null
            License? licenseFromGet = await licenseExtractor.GetLicenseAsync();
            licenseFromGet.Should().BeNull();

            terminalOptions.Id = TerminalIdentifiers.TestApplicationId;
            terminalOptions.Licensing.LicenseFile = testOnlineLicPath;
            licenseExtractor = new LicenseExtractor(licenseDebugger, terminalOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());

            var result = await licenseExtractor.ExtractLicenseAsync();
            result.License.Should().NotBeNull();

            // ensure passed and extraction handler
            result.ExtractionMode.Should().Be(TerminalIdentifiers.OnlineLicenseMode);

            result.License.Should().NotBeNull();
            result.License.Claims.Should().NotBeNull();
            result.License.Limits.Should().NotBeNull();
            result.License.LicenseKey.Should().NotBeNull();

            // license key
            result.License.LicenseKey.Should().Be(testOnlineLicPath);

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
            result.License.Claims.Subject.Should().Be("eaf50a3b-2e60-4029-cf41-4f1b65fdf749"); // subscription
            result.License.Claims.Id.Should().Be("3626ec63-2d2d-44e5-85d9-7f115dfae1c3"); // subscription
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

            Func<Task> func = async () => await licenseExtractor.ExtractLicenseAsync();
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The license file is not configured.");
        }

        private static async Task<HttpResponseMessage> GetDemoLicenseAsync()
        {
            MockHttpClientFactory mockHttpClientFactory = new();
            HttpResponseMessage httpResponseMessage;
            try
            {
                HttpClient httpClient = mockHttpClientFactory.CreateClient("prod");
                httpResponseMessage = await httpClient.GetAsync("public/demolicense");
            }
            catch (HttpRequestException)
            {
                HttpClient httpClient = mockHttpClientFactory.CreateClient("prod_fallback");
                httpResponseMessage = await httpClient.GetAsync("public/demolicense");
            }
            return httpResponseMessage;
        }

        private static string GetJsonLicenseFileForLocalHostGitHubSecretForCICD(string env)
        {
            // The demo json is too long for system env, so we use path for system env and json for GitHub
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

        private readonly ILicenseDebugger licenseDebugger;
        private readonly string nonJsonLicPath;
        private readonly TerminalOptions terminalOptions;
        private readonly string testDemoLicPath;
        private readonly string testOnlineLicPath;
        private ILicenseExtractor licenseExtractor;
    }
}
