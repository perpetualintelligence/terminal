/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Extensions;
using PerpetualIntelligence.Cli.Licensing.Models;
using PerpetualIntelligence.Protocols.Licensing.Models;
using PerpetualIntelligence.Shared.Exceptions;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Licensing
{
    /// <summary>
    /// The default <see cref="ILicenseExtractor"/>
    /// </summary>
    public sealed class LicenseExtractor : ILicenseExtractor
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="cliOptions">The configuration options.</param>
        /// <param name="httpClientFactory">The optional HTTP client factory</param>
        public LicenseExtractor(CliOptions cliOptions, IHttpClientFactory? httpClientFactory = null)
        {
            this.cliOptions = cliOptions;
            this.httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Extracts the <see cref="License"/> from the license keys.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public async Task<LicenseExtractorResult> ExtractAsync(LicenseExtractorContext context)
        {
            // For singleton DI service we don't extract license keys once extracted.
            if (license == null)
            {
                if (cliOptions.Licensing.KeySource == LicenseKeySource.JsonFile)
                {
                    license = await ExtractFromJsonAsync();
                }
                else
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The key source is not supported, see licensing options. key_source={0}", cliOptions.Licensing.KeySource);
                }
            }

            return new LicenseExtractorResult(license);
        }

        /// <summary>
        /// Should be called only for online check.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ErrorException"></exception>
        private HttpClient EnsureHttpClient()
        {
            // Make sure the HTTP client is correctly configured.
            if (httpClientFactory == null)
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The HTTP client factory is not configured, see cli builder extensions. service={0} check_mode={1}", nameof(ICliBuilderExtensions.AddLicensingClient), cliOptions.Licensing.CheckMode);
            }

            // Make sure the Http client name is setup
            if (string.IsNullOrWhiteSpace(cliOptions.Licensing.HttpClientName))
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The HTTP client name is not configured, see licensing options. check_mode={0}", cliOptions.Licensing.CheckMode);
            }

            // Setup the HTTP client
            HttpClient httpClient = httpClientFactory.CreateClient(cliOptions.Licensing.HttpClientName!);

            // Make sure the base address is correct
            if (httpClient.BaseAddress != new Uri(GetBaseAddress()))
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The HTTP client base address is not valid, see licensing options. check_mode={0} base_address={1}", cliOptions.Licensing.CheckMode, httpClient.BaseAddress);
            }

            return httpClient;
        }

        private async Task<License> ExtractFromJsonAsync()
        {
            // Missing key
            if (string.IsNullOrWhiteSpace(cliOptions.Licensing.LicenseKey))
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The Json license file is not configured, see licensing options. key_source={0}", cliOptions.Licensing.KeySource);
            }

            // Key not a file
            if (!File.Exists(cliOptions.Licensing.LicenseKey))
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The Json license file path is not valid, see licensing options. key_source={0}", cliOptions.Licensing.KeySource);
            }

            // For now we only support the online check
            if (cliOptions.Licensing.CheckMode != LicenseCheckMode.Online)
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The Json license file check mode is not valid, see licensing options. check_mode={0}", cliOptions.Licensing.CheckMode);
            }

            // Read the json file
            LicenseKeyJsonFileModel? model;
            try
            {
                model = await JsonSerializer.DeserializeAsync<LicenseKeyJsonFileModel>(File.OpenRead(cliOptions.Licensing.LicenseKey));
            }
            catch (JsonException ex)
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The Json license file is not valid, see licensing options. json_file={0} additional_info={1}", cliOptions.Licensing.LicenseKey, ex.Message);
            }

            // Make sure the model is valid Why ?
            if (model == null)
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The Json license file cannot be read, see licensing options. json_file={0}", cliOptions.Licensing.LicenseKey);
            }

            // Setup the HTTP client
            HttpClient httpClient = EnsureHttpClient();

            // Check JWS signed assertion (JWS key)
            var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            using (HttpResponseMessage response = await httpClient.PostAsync("licensing/b2bjwskeycheck", content))
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw new ErrorException(Errors.InvalidLicense, "The license check failed. status_code={0} additional_info={1}", response.StatusCode, await response.Content.ReadAsStringAsync());
                }

                LicenseClaimsModel? claims = await JsonSerializer.DeserializeAsync<LicenseClaimsModel>( await response.Content.ReadAsStreamAsync());
                if (claims == null)
                {
                    throw new ErrorException(Errors.InvalidLicense, "The license claims are invalid.");
                }

                LicenseLimits licenseLimits = LicenseLimits.Create(claims.Plan);
                return new License(cliOptions.Licensing.LicenseKey!, claims, licenseLimits);
            }
        }

        private string GetBaseAddress()
        {
#if DEBUG
            return "http://localhost:7071/api/";
#else
            return "https://api.perpetualintelligence.com/security/";
#endif
        }

        private readonly CliOptions cliOptions;
        private readonly IHttpClientFactory? httpClientFactory;
        private License? license;
    }
}
