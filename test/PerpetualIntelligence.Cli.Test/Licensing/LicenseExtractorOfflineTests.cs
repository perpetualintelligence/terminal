/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Mocks;
using PerpetualIntelligence.Shared.Licensing;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Shared.Json;
using PerpetualIntelligence.Test.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Xunit;

namespace PerpetualIntelligence.Cli.Licensing
{
    public class LicenseExtractorOfflineTests : IDisposable
    {
        public LicenseExtractorOfflineTests()
        {
            // Read the lic file from Github secrets
            testLicPath = GetJsonLicenseFIleForLocalHostGithubSecretForCICD("PI_CLI_TEST_OFFLINE_LIC");

            string nonJson = "non json document";
            nonJsonLicPath = Path.Combine(AppContext.BaseDirectory, $"{Guid.NewGuid()}.json");
            File.WriteAllText(nonJsonLicPath, nonJson);

            cliOptions = MockCliOptions.New();
            licenseExtractor = new LicenseExtractor(cliOptions, new LoggerFactory().CreateLogger<LicenseExtractor>());
        }

        [Fact]
        public void CustomAndStandardClaims_ShouldSerializerCorrectly()
        {
            LicenseOfflineProvisioningModel model = new()
            {
                AcrValues = new[] { "acr1", "acr2", "acr3", "custom" },
                Audience = "https://login.someone.com/hello-mello-jello/v2.0",
                AuthorizedApplicationIds = new[] { "app1", "app2" },
                AuthorizedParty = "authp1",
                ConsumerTenantCountry = "USA",
                ConsumerTenantId = "csmr1",
                ConsumerTenantName = "csmr name",
                Custom = LicenseLimits.DemoClaims(),
                ExpiresIn = 365,
                Issuer = "https://api.someone.com",
                BrokerTenantId = "pvdr1",
                PublisherTenantId = "pbsr1",
                Subject = "sub1",
                ConsumerObjectId = "coid",
                Status = "active",
                SigningKey = new LicenseSigningKeyModel(new byte[] { 1, 2, 3 }, "test_pwd")
            };

            string json = JsonSerializer.Serialize(model);

            LicenseOfflineProvisioningModel? fromJson = JsonSerializer.Deserialize<LicenseOfflineProvisioningModel>(json);
            fromJson.Should().NotBeNull();
            fromJson.Should().NotBeSameAs(model);

            fromJson!.AcrValues.Should().BeEquivalentTo(new[] { "acr1", "acr2", "acr3", "custom" });
            fromJson.Audience.Should().Be("https://login.someone.com/hello-mello-jello/v2.0");
            fromJson.AuthorizedApplicationIds.Should().BeEquivalentTo(new[] { "app1", "app2" });
            fromJson.AuthorizedParty.Should().Be("authp1");
            fromJson.ConsumerTenantCountry.Should().Be("USA");
            fromJson.ConsumerTenantId.Should().Be("csmr1");
            fromJson.ConsumerObjectId.Should().Be("coid");
            fromJson.ConsumerTenantName.Should().Be("csmr name");
            fromJson.ExpiresIn.Should().Be(365);
            fromJson.Issuer.Should().Be("https://api.someone.com");
            fromJson.BrokerTenantId.Should().Be("pvdr1");
            fromJson.PublisherTenantId.Should().Be("pbsr1");
            fromJson.Subject.Should().Be("sub1");
            fromJson.Status.Should().Be("active");
            fromJson.SigningKey.Key.Should().Equal(new byte[] { 1, 2, 3 });
            fromJson.SigningKey.Password.Should().Be("test_pwd");

            // custom claims
            fromJson.Custom.Should().HaveCount(19);

            fromJson.Custom.Should().Contain(new KeyValuePair<string, object>("terminal_limit", 1));
            fromJson.Custom.Should().Contain(new KeyValuePair<string, object>("redistribution_limit", 0));
            fromJson.Custom.Should().Contain(new KeyValuePair<string, object>("root_command_limit", 1));
            fromJson.Custom.Should().Contain(new KeyValuePair<string, object>("grouped_command_limit", 2));
            fromJson.Custom.Should().Contain(new KeyValuePair<string, object>("sub_command_limit", 15));
            fromJson.Custom.Should().Contain(new KeyValuePair<string, object>("argument_limit", 100));

            fromJson.Custom.Should().Contain(new KeyValuePair<string, object>("argument_alias", true));
            fromJson.Custom.Should().Contain(new KeyValuePair<string, object>("default_argument", true));
            fromJson.Custom.Should().Contain(new KeyValuePair<string, object>("default_argument_value", true));
            fromJson.Custom.Should().Contain(new KeyValuePair<string, object>("strict_data_type", true));

            fromJson.Custom.Should().Contain(new KeyValuePair<string, object>("data_type_handlers", "default"));
            fromJson.Custom.Should().Contain(new KeyValuePair<string, object>("text_handlers", "unicode ascii"));
            fromJson.Custom.Should().Contain(new KeyValuePair<string, object>("error_handlers", "default"));
            fromJson.Custom.Should().Contain(new KeyValuePair<string, object>("store_handlers", "in-memory"));
            fromJson.Custom.Should().Contain(new KeyValuePair<string, object>("service_handlers", "default"));
            fromJson.Custom.Should().Contain(new KeyValuePair<string, object>("license_handlers", "online"));

            fromJson.Custom.Should().Contain(new KeyValuePair<string, object>("currency", "USD"));
            fromJson.Custom.Should().Contain(new KeyValuePair<string, object>("monthly_price", 0.0));
            fromJson.Custom.Should().Contain(new KeyValuePair<string, object>("yearly_price", 0.0));
        }

        [Fact]
        public void CustomClaims_ShouldBeDecoratedWith_JsonConverterAttribute()
        {
            typeof(LicenseClaimsModel).GetProperty("Custom").Should().BeDecoratedWith<JsonConverterAttribute>(a => a.ConverterType == typeof(DictionaryStringObjectPrimitiveJsonConverter));
            typeof(LicenseOfflineProvisioningModel).GetProperty("Custom").Should().BeDecoratedWith<JsonConverterAttribute>(a => a.ConverterType == typeof(DictionaryStringObjectPrimitiveJsonConverter));
        }

        [Fact]
        public void CustomClaims_ShouldNotBeDecoratedWith_JsonExtensionDataAttribute()
        {
            typeof(LicenseClaimsModel).GetProperty("Custom").Should().NotBeDecoratedWith<JsonExtensionDataAttribute>();
            typeof(LicenseOfflineProvisioningModel).GetProperty("Custom").Should().NotBeDecoratedWith<JsonExtensionDataAttribute>();
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
            cliOptions.Licensing.AuthorizedApplicationId = "invalid_auth_app";
            cliOptions.Licensing.LicenseKey = testLicPath;
            cliOptions.Licensing.KeySource = LicenseSources.JsonFile;
            cliOptions.Handler.LicenseHandler = Handlers.OfflineHandler;
            cliOptions.Licensing.ConsumerTenantId = "a8379958-ea19-4918-84dc-199bf012361e";
            cliOptions.Licensing.Subject = "68d230be-cf83-49a6-c83f-42949fb40f46";
            cliOptions.Licensing.ProviderId = LicenseProviders.PerpetualIntelligence;
            licenseExtractor = new LicenseExtractor(cliOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());

            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseExtractor.ExtractAsync(new LicenseExtractorContext()), Errors.UnauthorizedAccess, "The application is not authorized. application_id=invalid_auth_app");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_InvalidKeyFilePath_ShouldErrorAsync()
        {
            cliOptions.Licensing.AuthorizedApplicationId = "0c1a06c9-c0ee-476c-bf54-527bcf71ada2";
            cliOptions.Licensing.LicenseKey = "D:\\lic\\path_does_exist\\invalid.lic";
            cliOptions.Licensing.KeySource = LicenseSources.JsonFile;

            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseExtractor.ExtractAsync(new LicenseExtractorContext()), Errors.InvalidConfiguration, "The Json license file path is not valid, see licensing options. key_file=D:\\lic\\path_does_exist\\invalid.lic");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_MissingAuthApp_ShouldErrorAsync()
        {
            cliOptions.Licensing.AuthorizedApplicationId = null;
            cliOptions.Licensing.LicenseKey = testLicPath;
            cliOptions.Licensing.KeySource = LicenseSources.JsonFile;
            cliOptions.Handler.LicenseHandler = Handlers.OfflineHandler;

            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseExtractor.ExtractAsync(new LicenseExtractorContext()), Errors.InvalidConfiguration, "The authorized application is not configured, see licensing options.");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_NonJsonLic_ShouldErrorAsync()
        {
            cliOptions.Licensing.AuthorizedApplicationId = "0c1a06c9-c0ee-476c-bf54-527bcf71ada2";
            cliOptions.Licensing.LicenseKey = nonJsonLicPath;
            cliOptions.Licensing.KeySource = LicenseSources.JsonFile;
            cliOptions.Handler.LicenseHandler = Handlers.OfflineHandler;

            try
            {
                await licenseExtractor.ExtractAsync(new LicenseExtractorContext());
            }
            catch (ErrorException ex)
            {
                ex.Message.Should().StartWith("The Json license file is not valid, see licensing options. json_file=");
            }
        }

        [Fact]
        public async Task ExtractFromJsonAsync_BoylMode_ShouldErrorAsync()
        {
            cliOptions.Licensing.AuthorizedApplicationId = "0c1a06c9-c0ee-476c-bf54-527bcf71ada2";
            cliOptions.Licensing.LicenseKey = testLicPath;
            cliOptions.Licensing.KeySource = LicenseSources.JsonFile;

            cliOptions.Handler.LicenseHandler = Handlers.BoylHandler;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseExtractor.ExtractAsync(new LicenseExtractorContext()), Errors.InvalidConfiguration, "The Json license file licensing handler mode is not valid, see hosting options. licensing_handler=boyl");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_InvalidApplicationId_ShouldErrorAsync()
        {
            cliOptions.Licensing.LicenseKey = testLicPath;
            cliOptions.Licensing.KeySource = LicenseSources.JsonFile;
            cliOptions.Handler.LicenseHandler = Handlers.OfflineHandler;
            cliOptions.Licensing.ConsumerTenantId = "a8379958-ea19-4918-84dc-199bf012361e";
            cliOptions.Licensing.AuthorizedApplicationId = "invalid_app";
            cliOptions.Licensing.Subject = "68d230be-cf83-49a6-c83f-42949fb40f46";
            licenseExtractor = new LicenseExtractor(cliOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());

            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseExtractor.ExtractAsync(new LicenseExtractorContext()), "unauthorized_access", "The application is not authorized. application_id=invalid_app");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_InvalidConsumerTenant_ShouldErrorAsync()
        {
            cliOptions.Licensing.LicenseKey = testLicPath;
            cliOptions.Licensing.KeySource = LicenseSources.JsonFile;
            cliOptions.Handler.LicenseHandler = Handlers.OfflineHandler;
            cliOptions.Licensing.ConsumerTenantId = "invalid_consumer";
            cliOptions.Licensing.AuthorizedApplicationId = "0c1a06c9-c0ee-476c-bf54-527bcf71ada2";
            licenseExtractor = new LicenseExtractor(cliOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());

            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseExtractor.ExtractAsync(new LicenseExtractorContext()), Errors.InvalidConfiguration, "The consumer tenant is not authorized, see licensing options. consumer_tenant_id=invalid_consumer");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_InvalidProviderTenant_ShouldErrorAsync()
        {
            cliOptions.Licensing.LicenseKey = testLicPath;
            cliOptions.Licensing.KeySource = LicenseSources.JsonFile;
            cliOptions.Handler.LicenseHandler = Handlers.OfflineHandler;
            cliOptions.Licensing.ConsumerTenantId = "a8379958-ea19-4918-84dc-199bf012361e";
            cliOptions.Licensing.AuthorizedApplicationId = "0c1a06c9-c0ee-476c-bf54-527bcf71ada2";
            cliOptions.Licensing.Subject = "68d230be-cf83-49a6-c83f-42949fb40f46";
            cliOptions.Licensing.ProviderId = "invalid_provider";
            licenseExtractor = new LicenseExtractor(cliOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());

            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseExtractor.ExtractAsync(new LicenseExtractorContext()), Errors.InvalidConfiguration, "The provider is not authorized, see licensing options. provider_id=invalid_provider");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_InvalidSubject_ShouldErrorAsync()
        {
            cliOptions.Licensing.LicenseKey = testLicPath;
            cliOptions.Licensing.KeySource = LicenseSources.JsonFile;
            cliOptions.Handler.LicenseHandler = Handlers.OfflineHandler;
            cliOptions.Licensing.ConsumerTenantId = "a8379958-ea19-4918-84dc-199bf012361e";
            cliOptions.Licensing.AuthorizedApplicationId = "0c1a06c9-c0ee-476c-bf54-527bcf71ada2";
            cliOptions.Licensing.Subject = "invalid_subject";
            licenseExtractor = new LicenseExtractor(cliOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());

            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseExtractor.ExtractAsync(new LicenseExtractorContext()), Errors.InvalidConfiguration, "The subject is not authorized, see licensing options. subject=invalid_subject");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_NoHttpClientFactory_ShouldNotErrorAsync()
        {
            cliOptions.Licensing.AuthorizedApplicationId = "0c1a06c9-c0ee-476c-bf54-527bcf71ada2";
            cliOptions.Licensing.LicenseKey = testLicPath;
            cliOptions.Licensing.KeySource = LicenseSources.JsonFile;
            cliOptions.Handler.LicenseHandler = Handlers.OfflineHandler;
            cliOptions.Licensing.ConsumerTenantId = "a8379958-ea19-4918-84dc-199bf012361e";
            cliOptions.Licensing.Subject = "68d230be-cf83-49a6-c83f-42949fb40f46";
            cliOptions.Licensing.ProviderId = LicenseProviders.PerpetualIntelligence;

            await licenseExtractor.ExtractAsync(new LicenseExtractorContext());
        }

        [Fact]
        public async Task ExtractFromJsonAsync_NoHttpClientName_ShouldNotErrorAsync()
        {
            cliOptions.Licensing.AuthorizedApplicationId = "0c1a06c9-c0ee-476c-bf54-527bcf71ada2";
            cliOptions.Licensing.LicenseKey = testLicPath;
            cliOptions.Licensing.KeySource = LicenseSources.JsonFile;
            cliOptions.Handler.LicenseHandler = Handlers.OfflineHandler;
            cliOptions.Licensing.ConsumerTenantId = "a8379958-ea19-4918-84dc-199bf012361e";
            cliOptions.Licensing.Subject = "68d230be-cf83-49a6-c83f-42949fb40f46";
            cliOptions.Licensing.ProviderId = LicenseProviders.PerpetualIntelligence;
            licenseExtractor = new LicenseExtractor(cliOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());

            await licenseExtractor.ExtractAsync(new LicenseExtractorContext());
        }

        [Fact]
        public async Task ExtractFromJsonAsync_ValidKey_ShouldContainClaimsAsync()
        {
            // Before extract get should be null
            License? licenseFromGet = await licenseExtractor.GetLicenseAsync();
            licenseFromGet.Should().BeNull();

            cliOptions.Licensing.LicenseKey = testLicPath;
            cliOptions.Licensing.KeySource = LicenseSources.JsonFile;
            cliOptions.Handler.LicenseHandler = Handlers.OfflineHandler;
            cliOptions.Licensing.ConsumerTenantId = "a8379958-ea19-4918-84dc-199bf012361e";
            cliOptions.Licensing.Subject = "68d230be-cf83-49a6-c83f-42949fb40f46";
            cliOptions.Licensing.AuthorizedApplicationId = "0c1a06c9-c0ee-476c-bf54-527bcf71ada2";
            cliOptions.Licensing.ProviderId = LicenseProviders.PerpetualIntelligence;
            licenseExtractor = new LicenseExtractor(cliOptions, new LoggerFactory().CreateLogger<LicenseExtractor>(), new MockHttpClientFactory());

            var result = await licenseExtractor.ExtractAsync(new LicenseExtractorContext());
            result.License.Should().NotBeNull();
            result.License.Claims.Should().NotBeNull();
            result.License.Limits.Should().NotBeNull();
            result.License.LicenseKey.Should().NotBeNull();

            // license key
            result.License.LicenseKeySource.Should().Be(LicenseSources.JsonFile);
            result.License.LicenseKey.Should().Be(testLicPath);

            // plan, mode and usage
            result.License.ProviderId.Should().Be("urn:oneimlx:lic:pvdr:pi");
            cliOptions.Handler.LicenseHandler = Handlers.OfflineHandler;
            result.License.Plan.Should().Be("urn:oneimlx:lic:plan:unlimited");
            result.License.Usage.Should().Be("urn:oneimlx:lic:usage:rnd");

            // claims
            result.License.Claims.AcrValues.Should().Be("urn:oneimlx:lic:plan:unlimited urn:oneimlx:lic:usage:rnd urn:oneimlx:lic:pvdr:pi");
            result.License.Claims.Audience.Should().Be("https://login.perpetualintelligence.com/a8379958-ea19-4918-84dc-199bf012361e/v2.0");
            result.License.Claims.AuthorizedParty.Should().Be("urn:oneimlx:cli");
            result.License.Claims.TenantCountry.Should().Be("USA");
            result.License.Claims.Custom.Should().BeNull();
            //result.License.Claims.Expiry.Should().NotBeNull();
            //result.License.Claims.IssuedAt.Should().NotBeNull();
            result.License.Claims.Issuer.Should().Be("https://api.perpetualintelligence.com");
            result.License.Claims.Jti.Should().NotBeNullOrWhiteSpace();
            result.License.Claims.Name.Should().Be("Perpetual Intelligence L.L.C. - Test");
            //result.License.Claims.NotBefore.Should().NotBeNull();
            result.License.Claims.ObjectId.Should().BeNull();
            result.License.Claims.ObjectCountry.Should().BeNull();
            result.License.Claims.Subject.Should().Be("68d230be-cf83-49a6-c83f-42949fb40f46"); // Test Microsoft SaaS subscription
            result.License.Claims.TenantId.Should().Be("a8379958-ea19-4918-84dc-199bf012361e");

            // no custom claims
            result.License.Claims.Custom.Should().BeNull();

            // limits
            result.License.Limits.Plan.Should().Be("urn:oneimlx:lic:plan:unlimited");

            // Price
            result.License.Price.Plan.Should().Be("urn:oneimlx:lic:plan:unlimited");
            result.License.Price.Currency.Should().Be("USD");
            result.License.Price.Monthly.Should().Be(1219.0);
            result.License.Price.Yearly.Should().Be(13109.0);

            // After extract and Get should return the correct license
            licenseFromGet = await licenseExtractor.GetLicenseAsync();
            licenseFromGet.Should().NotBeNull();
            licenseFromGet.Should().BeSameAs(result.License);
        }

        [Fact]
        public async Task ExtractFromJsonAsync_WithNoLicenseKey_ShouldErrorAsync()
        {
            cliOptions.Licensing.AuthorizedApplicationId = "0c1a06c9-c0ee-476c-bf54-527bcf71ada2";
            cliOptions.Licensing.LicenseKey = null;
            cliOptions.Licensing.KeySource = LicenseSources.JsonFile;
            cliOptions.Handler.LicenseHandler = Handlers.OfflineHandler;

            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseExtractor.ExtractAsync(new LicenseExtractorContext()), Errors.InvalidConfiguration, "The Json license file is not configured, see licensing options. key_source=urn:oneimlx:lic:source:jsonfile");
        }

        [Fact]
        public async Task UnsupportedKeySource_ShouldErrorAsync()
        {
            cliOptions.Licensing.KeySource = "253";
            cliOptions.Handler.LicenseHandler = Handlers.OfflineHandler;

            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseExtractor.ExtractAsync(new LicenseExtractorContext()), Errors.InvalidConfiguration, "The key source is not supported, see licensing options. key_source=253");
        }

        private static string GetJsonLicenseFIleForLocalHostGithubSecretForCICD(string env)
        {
            // The demo json is too long for system env, so we use path for system env and json for github
            string? fileOrJson = Environment.GetEnvironmentVariable(env);

            if (fileOrJson == null)
            {
                throw new ErrorException(Errors.InvalidConfiguration, "Environment variable with license key not found. env={0}", env);
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

        private readonly CliOptions cliOptions;
        private ILicenseExtractor licenseExtractor;
        private readonly string nonJsonLicPath;
        private readonly string testLicPath;
    }
}