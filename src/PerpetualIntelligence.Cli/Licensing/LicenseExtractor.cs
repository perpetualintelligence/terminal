/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Protocols.Authorization;
using PerpetualIntelligence.Protocols.Licensing;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Shared.Extensions;
using PerpetualIntelligence.Shared.Infrastructure;
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
        /// <param name="logger">The logger.</param>
        /// <param name="httpClientFactory">The optional HTTP client factory</param>
        public LicenseExtractor(CliOptions cliOptions, ILogger<LicenseExtractor> logger, IHttpClientFactory? httpClientFactory = null)
        {
            this.cliOptions = cliOptions;
            this.logger = logger;
            this.httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Extracts the <see cref="License"/> from the license keys.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<LicenseExtractorResult> ExtractAsync(LicenseExtractorContext context)
        {
            // For singleton DI service we don't extract license keys once extracted.
            if (license == null)
            {
                if (cliOptions.Licensing.KeySource == LicenseKeySources.JsonFile)
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

        /// <inheritdoc/>
        public Task<License?> GetLicenseAsync()
        {
            return Task.FromResult(license);
        }

        private async Task<HttpResponseMessage> CheckLicenseAsync(LicenseCheckModel checkModel)
        {
            // Setup the HTTP client
            HttpClient httpClient = EnsureHttpClient();

            // Primary and Secondary endpoints E.g. during certificate renewal the primary endpoints may fail so we fall
            // back to secondary endpoints.
            HttpResponseMessage httpResponseMessage;
            var checkContent = new StringContent(JsonSerializer.Serialize(checkModel), Encoding.UTF8, "application/json");
            try
            {
                httpResponseMessage = await httpClient.PostAsync(checkLicUrl, checkContent);
            }
            catch (HttpRequestException)
            {
                logger.LogWarning("The primary endpoint is not healthy. We are falling back to the secondary endpoint. Please contact the support team if you continue to see this warning after 24 hours.");
                httpResponseMessage = await httpClient.PostAsync(fallbackCheckLicUrl, checkContent);
            }
            return httpResponseMessage;
        }

        /// <summary>
        /// Should be called only for online check.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ErrorException"></exception>
        private HttpClient EnsureHttpClient()
        {
            if (httpClientFactory == null)
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The IHttpClientFactory is not configured. licensing_handler={0}", cliOptions.Handler.LicenseHandler);
            }

            // Make sure the HTTP client name is setup
            if (cliOptions.Http.HttpClientName == null)
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The HTTP client name is not configured, see licensing options. licensing_handler={0}", cliOptions.Handler.LicenseHandler);
            }

            // Setup the HTTP client
            HttpClient httpClient = httpClientFactory.CreateClient(cliOptions.Http.HttpClientName);
            return httpClient;
        }

        private async Task<License> ExtractFromJsonAsync()
        {
            // Missing app id
            if (string.IsNullOrWhiteSpace(cliOptions.Licensing.AuthorizedApplicationId))
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The authorized application is not configured, see licensing options.");
            }

            // Missing key
            if (string.IsNullOrWhiteSpace(cliOptions.Licensing.LicenseKey))
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The Json license file is not configured, see licensing options. key_source={0}", cliOptions.Licensing.KeySource);
            }

            // Key not a file
            if (!File.Exists(cliOptions.Licensing.LicenseKey))
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The Json license file path is not valid, see licensing options. key_file={0}", cliOptions.Licensing.LicenseKey);
            }

            // For now we only support the online check
            if (cliOptions.Handler.LicenseHandler != Handlers.OnlineHandler)
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The Json license file licensing handler mode is not valid, see hosting options. licensing_handler={0}", cliOptions.Handler.LicenseHandler);
            }

            // Read the json file
            LicenseKeyJsonFileModel? jsonFileModel;
            try
            {
                // Make sure the lic stream is disposed to avoid locking.
                using (Stream licStream = File.OpenRead(cliOptions.Licensing.LicenseKey))
                {
                    jsonFileModel = await JsonSerializer.DeserializeAsync<LicenseKeyJsonFileModel>(licStream);
                }
            }
            catch (JsonException ex)
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The Json license file is not valid, see licensing options. json_file={0} additional_info={1}", cliOptions.Licensing.LicenseKey, ex.Message);
            }

            // Make sure the model is valid Why ?
            if (jsonFileModel == null)
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The Json license file cannot be read, see licensing options. json_file={0}", cliOptions.Licensing.LicenseKey);
            }

            // Check JWS signed assertion (JWS key)
            LicenseCheckModel checkModel = new()
            {
                Issuer = Protocols.Constants.Issuer,
                Audience = MsalEndpoints.B2CIssuer("perpetualintelligenceb2c", jsonFileModel.ConsumerTenantId),
                AuthorizedApplicationId = cliOptions.Licensing.AuthorizedApplicationId!,
                AuthorizedParty = jsonFileModel.AuthorizedParty,
                ConsumerObjectId = jsonFileModel.ConsumerObjectId,
                ConsumerTenantId = jsonFileModel.ConsumerTenantId,
                Key = jsonFileModel.Key,
                KeyType = jsonFileModel.KeyType,
                BrokerId = jsonFileModel.BrokerId,
                Subject = jsonFileModel.Subject
            };

            // Make sure we use the full base address
            using (HttpResponseMessage response = await CheckLicenseAsync(checkModel))
            {
                if (!response.IsSuccessStatusCode)
                {
                    Error? error = await JsonSerializer.DeserializeAsync<Error>(await response.Content.ReadAsStreamAsync());
                    throw new ErrorException(error!);
                }

                LicenseClaimsModel? claims = await JsonSerializer.DeserializeAsync<LicenseClaimsModel>(await response.Content.ReadAsStreamAsync());
                if (claims == null)
                {
                    throw new ErrorException(Errors.InvalidLicense, "The license claims are invalid.");
                }

                // Check consumer with licensing options.
                if (claims.TenantId != cliOptions.Licensing.ConsumerTenantId)
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The consumer tenant is not authorized, see licensing options. consumer_tenant_id={0}", cliOptions.Licensing.ConsumerTenantId);
                }

                // Check subject with licensing options.
                if (claims.Subject != cliOptions.Licensing.Subject)
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The subject is not authorized, see licensing options. subject={0}", cliOptions.Licensing.Subject);
                }

                // Make sure the acr contains the
                string[] acrValues = claims.AcrValues.SplitBySpace();
                if (acrValues.Length < 3)
                {
                    throw new ErrorException(Errors.InvalidLicense, "The acr values are not valid. acr={0}", claims.AcrValues);
                }

                string plan = acrValues[0];
                string usage = acrValues[1];
                string providerId = acrValues[2];

                // Make sure the provider tenant id matches
                if (providerId != cliOptions.Licensing.ProviderId)
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The provider is not authorized, see licensing options. provider_id={0}", cliOptions.Licensing.ProviderId);
                }

                LicenseLimits licenseLimits = LicenseLimits.Create(plan, claims.Custom);
                LicensePrice licensePrice = LicensePrice.Create(plan, claims.Custom);
                return new License(providerId, cliOptions.Handler.LicenseHandler, plan, usage, cliOptions.Licensing.KeySource, cliOptions.Licensing.LicenseKey!, claims, licenseLimits, licensePrice);
            }
        }

        private readonly string checkLicUrl = "https://api.perpetualintelligence.com/public/checklicense";
        private readonly CliOptions cliOptions;
        private readonly string fallbackCheckLicUrl = "https://piapim.azure-api.net/public/checklicense";
        private readonly IHttpClientFactory? httpClientFactory;
        private readonly ILogger<LicenseExtractor> logger;
        private License? license;

        //private readonly string checkLicUrl = "http://localhost:7071/api/public/checklicense";
    }
}
