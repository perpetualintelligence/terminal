/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using FluentAssertions;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Mocks;
using PerpetualIntelligence.Protocols.Licensing;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Test.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace PerpetualIntelligence.Cli.Licensing
{
    public class LicenseExtractorTests
    {
        public LicenseExtractorTests()
        {
            cliOptions = MockCliOptions.New();
            licenseProviderResolver = new LicenseProviderResolver();
            licenseExtractor = new LicenseExtractor(licenseProviderResolver, cliOptions);
            jsonLicPath = Path.Combine(AppContext.BaseDirectory, "TestInfra", "test_lic.json");
            nonJsonLicPath = Path.Combine(AppContext.BaseDirectory, "TestInfra", "test_lic_non_json.json");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_MissingAuthApp_ShouldErrorAsync()
        {
            cliOptions.Licensing.AuthorizedApplicationId = null;
            cliOptions.Licensing.LicenseKey = jsonLicPath;
            cliOptions.Licensing.KeySource = SaaSKeySources.JsonFile;
            cliOptions.Handler.LicenseHandler = Handlers.OnlineHandler;

            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseExtractor.ExtractAsync(new LicenseExtractorContext()), Errors.InvalidConfiguration, "The authorized application is not configured, see licensing options.");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_InvalidAuthApp_ShouldErrorAsync()
        {
            cliOptions.Licensing.AuthorizedApplicationId = "invalid_auth_app";
            cliOptions.Licensing.LicenseKey = jsonLicPath;
            cliOptions.Licensing.KeySource = SaaSKeySources.JsonFile;
            cliOptions.Handler.LicenseHandler = Handlers.OnlineHandler;
            cliOptions.Licensing.HttpClientName = "test_client";
            cliOptions.Licensing.ConsumerTenantId = "a8379958-ea19-4918-84dc-199bf012361e";
            cliOptions.Licensing.Subject = "68d230be-cf83-49a6-c83f-42949fb40f46";            
            cliOptions.Licensing.ProviderId = SaaSProviders.PerpetualIntelligence;
            licenseExtractor = new LicenseExtractor(licenseProviderResolver, cliOptions, new MockHttpClientFactory());

            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseExtractor.ExtractAsync(new LicenseExtractorContext()), Errors.UnauthorizedAccess, "The application is not authorized. application_id=invalid_auth_app");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_InvalidKeyFilePath_ShouldErrorAsync()
        {
            cliOptions.Licensing.AuthorizedApplicationId = "0c1a06c9-c0ee-476c-bf54-527bcf71ada2";
            cliOptions.Licensing.LicenseKey = "D:\\lic\\path_does_exist\\invalid.lic";
            cliOptions.Licensing.KeySource = SaaSKeySources.JsonFile;

            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseExtractor.ExtractAsync(new LicenseExtractorContext()), Errors.InvalidConfiguration, "The Json license file path is not valid, see licensing options. key_file=D:\\lic\\path_does_exist\\invalid.lic");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_NonJsonLic_ShouldErrorAsync()
        {
            cliOptions.Licensing.AuthorizedApplicationId = "0c1a06c9-c0ee-476c-bf54-527bcf71ada2";
            cliOptions.Licensing.LicenseKey = nonJsonLicPath;
            cliOptions.Licensing.KeySource = SaaSKeySources.JsonFile;
            cliOptions.Handler.LicenseHandler = Handlers.OnlineHandler;

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
        public async Task ExtractFromJsonAsync_NonOnlineMode_ShouldErrorAsync()
        {
            cliOptions.Licensing.AuthorizedApplicationId = "0c1a06c9-c0ee-476c-bf54-527bcf71ada2";
            cliOptions.Licensing.LicenseKey = jsonLicPath;
            cliOptions.Licensing.KeySource = SaaSKeySources.JsonFile;

            cliOptions.Handler.LicenseHandler = Handlers.OfflineHandler;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseExtractor.ExtractAsync(new LicenseExtractorContext()), Errors.InvalidConfiguration, "The Json license file licensing handler mode is not valid, see hosting options. licensing_handler=offline");

            cliOptions.Handler.LicenseHandler = Handlers.BoylHandler;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseExtractor.ExtractAsync(new LicenseExtractorContext()), Errors.InvalidConfiguration, "The Json license file licensing handler mode is not valid, see hosting options. licensing_handler=boyl");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_OnlineMode_InvalidConsumerTenant_ShouldErrorAsync()
        {
            cliOptions.Licensing.LicenseKey = jsonLicPath;
            cliOptions.Licensing.KeySource = SaaSKeySources.JsonFile;
            cliOptions.Handler.LicenseHandler = Handlers.OnlineHandler;
            cliOptions.Licensing.HttpClientName = "test_client";
            cliOptions.Licensing.ConsumerTenantId = "invalid_consumer";
            cliOptions.Licensing.AuthorizedApplicationId = "0c1a06c9-c0ee-476c-bf54-527bcf71ada2";
            licenseExtractor = new LicenseExtractor(licenseProviderResolver, cliOptions, new MockHttpClientFactory());

            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseExtractor.ExtractAsync(new LicenseExtractorContext()), Errors.InvalidConfiguration, "The consumer tenant is not authorized, see licensing options. consumer_tenant_id=invalid_consumer");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_OnlineMode_InvalidProviderTenant_ShouldErrorAsync()
        {
            cliOptions.Licensing.LicenseKey = jsonLicPath;
            cliOptions.Licensing.KeySource = SaaSKeySources.JsonFile;
            cliOptions.Handler.LicenseHandler = Handlers.OnlineHandler;
            cliOptions.Licensing.HttpClientName = "test_client";
            cliOptions.Licensing.ConsumerTenantId = "a8379958-ea19-4918-84dc-199bf012361e";
            cliOptions.Licensing.AuthorizedApplicationId = "0c1a06c9-c0ee-476c-bf54-527bcf71ada2";
            cliOptions.Licensing.Subject = "68d230be-cf83-49a6-c83f-42949fb40f46";
            cliOptions.Licensing.ProviderId = "invalid_provider";
            licenseExtractor = new LicenseExtractor(licenseProviderResolver, cliOptions, new MockHttpClientFactory());

            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseExtractor.ExtractAsync(new LicenseExtractorContext()), Errors.InvalidConfiguration, "The provider is not authorized, see licensing options. provider_id=invalid_provider");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_OnlineMode_InvalidSubject_ShouldErrorAsync()
        {
            cliOptions.Licensing.LicenseKey = jsonLicPath;
            cliOptions.Licensing.KeySource = SaaSKeySources.JsonFile;
            cliOptions.Handler.LicenseHandler = Handlers.OnlineHandler;
            cliOptions.Licensing.HttpClientName = "test_client";
            cliOptions.Licensing.ConsumerTenantId = "a8379958-ea19-4918-84dc-199bf012361e";
            cliOptions.Licensing.AuthorizedApplicationId = "0c1a06c9-c0ee-476c-bf54-527bcf71ada2";
            cliOptions.Licensing.Subject = "invalid_subject";
            licenseExtractor = new LicenseExtractor(licenseProviderResolver, cliOptions, new MockHttpClientFactory());

            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseExtractor.ExtractAsync(new LicenseExtractorContext()), Errors.InvalidConfiguration, "The subject is not authorized, see licensing options. subject=invalid_subject");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_OnlineMode_InvalidApplicationId_ShouldErrorAsync()
        {
            cliOptions.Licensing.LicenseKey = jsonLicPath;
            cliOptions.Licensing.KeySource = SaaSKeySources.JsonFile;
            cliOptions.Handler.LicenseHandler = Handlers.OnlineHandler;
            cliOptions.Licensing.HttpClientName = "test_client";
            cliOptions.Licensing.ConsumerTenantId = "a8379958-ea19-4918-84dc-199bf012361e";
            cliOptions.Licensing.AuthorizedApplicationId = "invalid_app";
            cliOptions.Licensing.Subject = "68d230be-cf83-49a6-c83f-42949fb40f46";
            licenseExtractor = new LicenseExtractor(licenseProviderResolver, cliOptions, new MockHttpClientFactory());

            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseExtractor.ExtractAsync(new LicenseExtractorContext()), "unauthorized_access", "The application is not authorized. application_id=invalid_app");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_OnlineMode_NoHttpClientFactory_ShouldErrorAsync()
        {
            cliOptions.Licensing.AuthorizedApplicationId = "0c1a06c9-c0ee-476c-bf54-527bcf71ada2";
            cliOptions.Licensing.LicenseKey = jsonLicPath;
            cliOptions.Licensing.KeySource = SaaSKeySources.JsonFile;
            cliOptions.Handler.LicenseHandler = Handlers.OnlineHandler;

            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseExtractor.ExtractAsync(new LicenseExtractorContext()), Errors.InvalidConfiguration, "The HTTP client factory is not configured, see cli builder extensions. service=AddLicensingClient licensing_handler=online");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_OnlineMode_NoHttpClientName_ShouldErrorAsync()
        {
            cliOptions.Licensing.AuthorizedApplicationId = "0c1a06c9-c0ee-476c-bf54-527bcf71ada2";
            cliOptions.Licensing.LicenseKey = jsonLicPath;
            cliOptions.Licensing.KeySource = SaaSKeySources.JsonFile;
            cliOptions.Handler.LicenseHandler = Handlers.OnlineHandler;
            licenseExtractor = new LicenseExtractor(licenseProviderResolver, cliOptions, new MockHttpClientFactory());

            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseExtractor.ExtractAsync(new LicenseExtractorContext()), Errors.InvalidConfiguration, "The HTTP client name is not configured, see licensing options. licensing_handler=online");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_OnlineMode_ValidKey_ShouldContainClaimsAsync()
        {
            // Before extract get should be null
            License? licenseFromGet = await licenseExtractor.GetLicenseAsync();
            licenseFromGet.Should().BeNull();

            cliOptions.Licensing.LicenseKey = jsonLicPath;
            cliOptions.Licensing.KeySource = SaaSKeySources.JsonFile;
            cliOptions.Handler.LicenseHandler = Handlers.OnlineHandler;
            cliOptions.Licensing.HttpClientName = "test_client";
            cliOptions.Licensing.ConsumerTenantId = "a8379958-ea19-4918-84dc-199bf012361e";
            cliOptions.Licensing.Subject = "68d230be-cf83-49a6-c83f-42949fb40f46";
            cliOptions.Licensing.AuthorizedApplicationId = "0c1a06c9-c0ee-476c-bf54-527bcf71ada2";
            cliOptions.Licensing.ProviderId = SaaSProviders.PerpetualIntelligence;
            licenseExtractor = new LicenseExtractor(licenseProviderResolver, cliOptions, new MockHttpClientFactory());

            var result = await licenseExtractor.ExtractAsync(new LicenseExtractorContext());
            result.License.Should().NotBeNull();
            result.License.Claims.Should().NotBeNull();
            result.License.Limits.Should().NotBeNull();
            result.License.LicenseKey.Should().NotBeNull();

            // license key
            result.License.LicenseKeySource.Should().Be(SaaSKeySources.JsonFile);
            result.License.LicenseKey.Should().Be(jsonLicPath);

            // plan, mode and usage
            result.License.ProviderId.Should().Be("urn:oneimlx:lic:saaspvdr:pi");
            cliOptions.Handler.LicenseHandler = Handlers.OnlineHandler;
            result.License.Plan.Should().Be("urn:oneimlx:lic:saasplan:isvu");
            result.License.Usage.Should().Be("urn:oneimlx:lic:saasusage:rnd");

            // claims
            result.License.Claims.AcrValues.Should().Be("urn:oneimlx:lic:saasplan:isvu urn:oneimlx:lic:saasusage:rnd urn:oneimlx:lic:saaspvdr:pi");
            result.License.Claims.Audience.Should().Be("https://login.microsoftonline.com/a8379958-ea19-4918-84dc-199bf012361e/v2.0");
            result.License.Claims.AuthorizedParty.Should().Be("urn:oneimlx:cli");
            result.License.Claims.TenantCountry.Should().Be("USA");
            result.License.Claims.Custom.Should().BeNull();
            result.License.Claims.Expiry.Should().NotBeNull();
            result.License.Claims.IssuedAt.Should().NotBeNull();
            result.License.Claims.Issuer.Should().Be("https://api.perpetualintelligence.com/security/licensing");
            result.License.Claims.Jti.Should().NotBeNullOrWhiteSpace();
            result.License.Claims.Name.Should().Be("Perpetual Intelligence L.L.C. - Test");
            result.License.Claims.NotBefore.Should().NotBeNull();
            result.License.Claims.ObjectId.Should().BeNull();
            result.License.Claims.ObjectCountry.Should().BeNull();
            result.License.Claims.Subject.Should().Be("68d230be-cf83-49a6-c83f-42949fb40f46"); // Test Microsoft SaaS subscription
            result.License.Claims.TenantId.Should().Be("a8379958-ea19-4918-84dc-199bf012361e");

            // limits
            result.License.Limits.Plan.Should().Be("urn:oneimlx:lic:saasplan:isvu");

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
            cliOptions.Licensing.KeySource = SaaSKeySources.JsonFile;

            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseExtractor.ExtractAsync(new LicenseExtractorContext()), Errors.InvalidConfiguration, "The Json license file is not configured, see licensing options. key_source=urn:oneimlx:lic:ksource:jsonfile");
        }

        [Fact]
        public async Task UnsupportedKeySource_ShouldErrorAsync()
        {
            cliOptions.Licensing.KeySource = "253";

            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseExtractor.ExtractAsync(new LicenseExtractorContext()), Errors.InvalidConfiguration, "The key source is not supported, see licensing options. key_source=253");
        }

        private CliOptions cliOptions;
        private string jsonLicPath;
        private ILicenseProviderResolver licenseProviderResolver;
        private ILicenseExtractor licenseExtractor;
        private string nonJsonLicPath;
    }
}
