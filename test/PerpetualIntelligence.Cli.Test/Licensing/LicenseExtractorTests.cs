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
        public async Task ExtractFromJsonAsync_InvalidKeyFilePath_ShouldErrorAsync()
        {
            cliOptions.Licensing.LicenseKey = "D:\\lic\\path_does_exist\\invalid.lic";
            cliOptions.Licensing.KeySource = LicenseKeySource.JsonFile;

            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseExtractor.ExtractAsync(new LicenseExtractorContext()), Errors.InvalidConfiguration, "The Json license file path is not valid, see licensing options. key_source=JsonFile");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_NonJsonLic_ShouldErrorAsync()
        {
            cliOptions.Licensing.LicenseKey = nonJsonLicPath;
            cliOptions.Licensing.KeySource = LicenseKeySource.JsonFile;
            cliOptions.Licensing.CheckMode = SaaSCheckModes.Online;

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
            cliOptions.Licensing.LicenseKey = jsonLicPath;
            cliOptions.Licensing.KeySource = LicenseKeySource.JsonFile;

            cliOptions.Licensing.CheckMode = SaaSCheckModes.Offline;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseExtractor.ExtractAsync(new LicenseExtractorContext()), Errors.InvalidConfiguration, "The Json license file check mode is not valid, see licensing options. check_mode=urn:oneimlx:lic:saascmode:offline");

            cliOptions.Licensing.CheckMode = SaaSCheckModes.OnlineThenOffline;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseExtractor.ExtractAsync(new LicenseExtractorContext()), Errors.InvalidConfiguration, "The Json license file check mode is not valid, see licensing options. check_mode=urn:oneimlx:lic:saascmode:onthenoff");

            cliOptions.Licensing.CheckMode = SaaSCheckModes.OfflineThenOnline;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseExtractor.ExtractAsync(new LicenseExtractorContext()), Errors.InvalidConfiguration, "The Json license file check mode is not valid, see licensing options. check_mode=urn:oneimlx:lic:saascmode:offthenon");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_OnlineMode_InvalidConsumerTenant_ShouldErrorAsync()
        {
            cliOptions.Licensing.LicenseKey = jsonLicPath;
            cliOptions.Licensing.KeySource = LicenseKeySource.JsonFile;
            cliOptions.Licensing.CheckMode = SaaSCheckModes.Online;
            cliOptions.Licensing.HttpClientName = "test_client";
            cliOptions.Licensing.ConsumerTenantId = "invalid_consumer";
            licenseExtractor = new LicenseExtractor(licenseProviderResolver, cliOptions, new MockHttpClientFactory());

            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseExtractor.ExtractAsync(new LicenseExtractorContext()), Errors.InvalidConfiguration, "The consumer tenant is not authorized, see licensing options. consumer_tenant_id=invalid_consumer");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_OnlineMode_InvalidProviderTenant_ShouldErrorAsync()
        {
            cliOptions.Licensing.LicenseKey = jsonLicPath;
            cliOptions.Licensing.KeySource = LicenseKeySource.JsonFile;
            cliOptions.Licensing.CheckMode = SaaSCheckModes.Online;
            cliOptions.Licensing.HttpClientName = "test_client";
            cliOptions.Licensing.ConsumerTenantId = "a8379958-ea19-4918-84dc-199bf012361e";
            cliOptions.Licensing.Subject = "89607467-5bdb-4caf-d513-3cb6c830fa2f";
            cliOptions.Licensing.ProviderId = "invalid_provider";
            licenseExtractor = new LicenseExtractor(licenseProviderResolver, cliOptions, new MockHttpClientFactory());

            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseExtractor.ExtractAsync(new LicenseExtractorContext()), Errors.InvalidConfiguration, "The provider is not authorized, see licensing options. provider_id=invalid_provider");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_OnlineMode_InvalidSubject_ShouldErrorAsync()
        {
            cliOptions.Licensing.LicenseKey = jsonLicPath;
            cliOptions.Licensing.KeySource = LicenseKeySource.JsonFile;
            cliOptions.Licensing.CheckMode = SaaSCheckModes.Online;
            cliOptions.Licensing.HttpClientName = "test_client";
            cliOptions.Licensing.ConsumerTenantId = "a8379958-ea19-4918-84dc-199bf012361e";
            cliOptions.Licensing.Subject = "invalid_subject";
            licenseExtractor = new LicenseExtractor(licenseProviderResolver, cliOptions, new MockHttpClientFactory());

            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseExtractor.ExtractAsync(new LicenseExtractorContext()), Errors.InvalidConfiguration, "The subject is not authorized, see licensing options. subject=invalid_subject");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_OnlineMode_NoHttpClientFactory_ShouldErrorAsync()
        {
            cliOptions.Licensing.LicenseKey = jsonLicPath;
            cliOptions.Licensing.KeySource = LicenseKeySource.JsonFile;
            cliOptions.Licensing.CheckMode = SaaSCheckModes.Online;

            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseExtractor.ExtractAsync(new LicenseExtractorContext()), Errors.InvalidConfiguration, "The HTTP client factory is not configured, see cli builder extensions. service=AddLicensingClient check_mode=urn:oneimlx:lic:saascmode:online");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_OnlineMode_NoHttpClientName_ShouldErrorAsync()
        {
            cliOptions.Licensing.LicenseKey = jsonLicPath;
            cliOptions.Licensing.KeySource = LicenseKeySource.JsonFile;
            cliOptions.Licensing.CheckMode = SaaSCheckModes.Online;
            licenseExtractor = new LicenseExtractor(licenseProviderResolver, cliOptions, new MockHttpClientFactory());

            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseExtractor.ExtractAsync(new LicenseExtractorContext()), Errors.InvalidConfiguration, "The HTTP client name is not configured, see licensing options. check_mode=urn:oneimlx:lic:saascmode:online");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_OnlineMode_ValidKey_ShouldContainClaimsAsync()
        {
            cliOptions.Licensing.LicenseKey = jsonLicPath;
            cliOptions.Licensing.KeySource = LicenseKeySource.JsonFile;
            cliOptions.Licensing.CheckMode = SaaSCheckModes.Online;
            cliOptions.Licensing.HttpClientName = "test_client";
            cliOptions.Licensing.ConsumerTenantId = "a8379958-ea19-4918-84dc-199bf012361e";
            cliOptions.Licensing.Subject = "89607467-5bdb-4caf-d513-3cb6c830fa2f";
            cliOptions.Licensing.ProviderId = SaaSProviders.PerpetualIntelligence;
            licenseExtractor = new LicenseExtractor(licenseProviderResolver, cliOptions, new MockHttpClientFactory());

            var result = await licenseExtractor.ExtractAsync(new LicenseExtractorContext());
            result.License.Should().NotBeNull();
            result.License.Claims.Should().NotBeNull();
            result.License.Limits.Should().NotBeNull();
            result.License.LicenseKey.Should().NotBeNull();

            // license key
            result.License.LicenseKey.Should().Be(jsonLicPath);

            // plan, mode and usage
            result.License.ProviderTenantId.Should().Be("urn:oneimlx:lic:saaspvdr:pi");
            result.License.CheckMode.Should().Be(SaaSCheckModes.Online);
            result.License.Plan.Should().Be("urn:oneimlx:lic:saasplan:community");
            result.License.Usage.Should().Be("urn:oneimlx:lic:saasusage:rnd");

            // claims
            result.License.Claims.Acr.Should().Be("urn:oneimlx:lic:saasplan:community urn:oneimlx:lic:saasusage:rnd urn:oneimlx:lic:saaspvdr:pi");
            result.License.Claims.Audience.Should().Be("https://login.microsoftonline.com/a8379958-ea19-4918-84dc-199bf012361e/v2.0");
            result.License.Claims.AuthorizedParty.Should().Be("urn:oneimlx:cli");
            result.License.Claims.TenantCountry.Should().Be("USA");
            result.License.Claims.Custom.Should().BeNull();
            result.License.Claims.Expiry.Should().NotBeNull();
            result.License.Claims.IssuedAt.Should().NotBeNull();
            result.License.Claims.Issuer.Should().Be("https://api.perpetualintelligence.com/security/licensing");
            result.License.Claims.Jti.Should().NotBeNullOrWhiteSpace();
            result.License.Claims.Name.Should().Be("Perpetual Intelligence L.L.C.");
            result.License.Claims.NotBefore.Should().NotBeNull();
            result.License.Claims.ObjectId.Should().BeNull();
            result.License.Claims.ObjectCountry.Should().BeNull();
            result.License.Claims.Subject.Should().Be("89607467-5bdb-4caf-d513-3cb6c830fa2f"); // Test Microsoft SaaS subscription
            result.License.Claims.TenantId.Should().Be("a8379958-ea19-4918-84dc-199bf012361e");

            // limits
            result.License.Limits.Plan.Should().Be("urn:oneimlx:lic:saasplan:community");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_WithNoLicenseKey_ShouldErrorAsync()
        {
            cliOptions.Licensing.LicenseKey = null;
            cliOptions.Licensing.KeySource = LicenseKeySource.JsonFile;

            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseExtractor.ExtractAsync(new LicenseExtractorContext()), Errors.InvalidConfiguration, "The Json license file is not configured, see licensing options. key_source=JsonFile");
        }

        [Fact]
        public async Task UnsupportedKeySource_ShouldErrorAsync()
        {
            cliOptions.Licensing.KeySource = (LicenseKeySource)253;

            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseExtractor.ExtractAsync(new LicenseExtractorContext()), Errors.InvalidConfiguration, "The key source is not supported, see licensing options. key_source=253");
        }

        private CliOptions cliOptions;
        private string jsonLicPath;
        private ILicenseProviderResolver licenseProviderResolver;
        private ILicenseExtractor licenseExtractor;
        private string nonJsonLicPath;
    }
}
