/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using FluentAssertions;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Mocks;
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
            licenseExtractor = new LicenseExtractor(cliOptions);
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
            cliOptions.Licensing.CheckMode = LicenseCheckMode.Online;

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
        public async Task ExtractFromJsonAsync_NonOnlineeMode_ShouldErrorAsync()
        {
            cliOptions.Licensing.LicenseKey = jsonLicPath;
            cliOptions.Licensing.KeySource = LicenseKeySource.JsonFile;

            cliOptions.Licensing.CheckMode = LicenseCheckMode.Offline;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseExtractor.ExtractAsync(new LicenseExtractorContext()), Errors.InvalidConfiguration, "The Json license file check mode is not valid, see licensing options. check_mode=Offline");

            cliOptions.Licensing.CheckMode = LicenseCheckMode.OnlineThenOffline;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseExtractor.ExtractAsync(new LicenseExtractorContext()), Errors.InvalidConfiguration, "The Json license file check mode is not valid, see licensing options. check_mode=OnlineThenOffline");

            cliOptions.Licensing.CheckMode = LicenseCheckMode.OfflineThenOnline;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseExtractor.ExtractAsync(new LicenseExtractorContext()), Errors.InvalidConfiguration, "The Json license file check mode is not valid, see licensing options. check_mode=OfflineThenOnline");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_OnlineMode_InvalidBaseAddress_ShouldErrorAsync()
        {
            cliOptions.Licensing.LicenseKey = jsonLicPath;
            cliOptions.Licensing.KeySource = LicenseKeySource.JsonFile;
            cliOptions.Licensing.CheckMode = LicenseCheckMode.Online;
            cliOptions.Licensing.HttpClientName = "invalid_based_address";
            licenseExtractor = new LicenseExtractor(cliOptions, new MockHttpClientFactory());

            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseExtractor.ExtractAsync(new LicenseExtractorContext()), Errors.InvalidConfiguration, "The HTTP client base address is not valid, see licensing options. check_mode=Online base_address=https://api.perpetualintelligence.com/invalid/");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_OnlineMode_NoHttpClientFactory_ShouldErrorAsync()
        {
            cliOptions.Licensing.LicenseKey = jsonLicPath;
            cliOptions.Licensing.KeySource = LicenseKeySource.JsonFile;
            cliOptions.Licensing.CheckMode = LicenseCheckMode.Online;

            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseExtractor.ExtractAsync(new LicenseExtractorContext()), Errors.InvalidConfiguration, "The HTTP client factory is not configured, see cli builder extensions. service=AddLicensingClient check_mode=Online");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_OnlineMode_NoHttpClientName_ShouldErrorAsync()
        {
            cliOptions.Licensing.LicenseKey = jsonLicPath;
            cliOptions.Licensing.KeySource = LicenseKeySource.JsonFile;
            cliOptions.Licensing.CheckMode = LicenseCheckMode.Online;
            licenseExtractor = new LicenseExtractor(cliOptions, new MockHttpClientFactory());

            await TestHelper.AssertThrowsErrorExceptionAsync(() => licenseExtractor.ExtractAsync(new LicenseExtractorContext()), Errors.InvalidConfiguration, "The HTTP client name is not configured, see licensing options. check_mode=Online");
        }

        [Fact]
        public async Task ExtractFromJsonAsync_OnlineMode_ValidKey_ShouldContainClaimsAsync()
        {
            cliOptions.Licensing.LicenseKey = jsonLicPath;
            cliOptions.Licensing.KeySource = LicenseKeySource.JsonFile;
            cliOptions.Licensing.CheckMode = LicenseCheckMode.Online;
            cliOptions.Licensing.HttpClientName = "test_client";
            licenseExtractor = new LicenseExtractor(cliOptions, new MockHttpClientFactory());

            var result = await licenseExtractor.ExtractAsync(new LicenseExtractorContext());
            result.License.Should().NotBeNull();
            result.License.Claims.Should().NotBeNull();
            result.License.Limits.Should().NotBeNull();
            result.License.LicenseKey.Should().NotBeNull();

            // license key
            result.License.LicenseKey.Should().Be(jsonLicPath);

            // claims
            result.License.Claims.Acr.Should().Be("urn:oneimlx:lic:saasplans:community urn:oneimlx:lic:saasusage:rnd");
            result.License.Claims.Audience.Should().Be("https://login.microsoftonline.com/a8379958-ea19-4918-84dc-199bf012361e/v2.0");
            result.License.Claims.AuthorizedParty.Should().Be("urn:oneimlx:cli");
            result.License.Claims.Country.Should().Be("USA");
            result.License.Claims.Custom.Should().BeNull();
            result.License.Claims.Expiry.Should().NotBeNull();
            result.License.Claims.IssuedAt.Should().NotBeNull();
            result.License.Claims.Issuer.Should().Be("https://api.perpetualintelligence.com/security/licensing");
            result.License.Claims.Jti.Should().NotBeNullOrWhiteSpace();
            result.License.Claims.Name.Should().Be("Perpetual Intelligence L.L.C.");
            result.License.Claims.NotBefore.Should().NotBeNull();
            result.License.Claims.ObjectId.Should().BeNull();
            result.License.Claims.Plan.Should().Be("urn:oneimlx:lic:saasplans:community");
            result.License.Claims.Subject.Should().Be("89607467-5bdb-4caf-d513-3cb6c830fa2f"); // Test Microsoft SaaS subscription
            result.License.Claims.TenantId.Should().Be("a8379958-ea19-4918-84dc-199bf012361e");
            result.License.Claims.Usage.Should().Be("urn:oneimlx:lic:saasusage:rnd");

            // limits
            result.License.Limits.Plan.Should().Be("urn:oneimlx:lic:saasplans:community");
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
        private ILicenseExtractor licenseExtractor;
        private string nonJsonLicPath;
    }
}
