/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Shared.Authorization;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Shared.Extensions;
using PerpetualIntelligence.Shared.Infrastructure;
using PerpetualIntelligence.Shared.Licensing;
using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
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
                if (cliOptions.Licensing.KeySource == LicenseSources.JsonFile)
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

        private async Task<HttpResponseMessage> CheckOnlineLicenseAsync(LicenseOnlineCheckModel checkModel)
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

        private async Task<LicenseClaimsModel> CheckOfflineLicenseAsync(LicenseOfflineCheckModel checkModel)
        {
            // https://stackoverflow.com/questions/58102904/how-to-verify-a-jwt-token-signed-with-x509-with-only-a-public-key-in-aspnetcore
            X509Certificate2 x509Certificate = new(Convert.FromBase64String(checkModel.ValidationKey));
            X509SecurityKey validationKey = new(x509Certificate);

            // Validation key cannot be private
            if (validationKey.PrivateKeyStatus == PrivateKeyStatus.Exists)
            {
                throw new ErrorException(Error.Unauthorized, "License validation certificate cannot have private key.");
            }

            // Init token validation params
            TokenValidationParameters validationParameters = new()
            {
                ValidateAudience = true,
                ValidAudience = checkModel.Audience,

                ValidateIssuer = true,
                ValidIssuer = checkModel.Issuer,

                RequireSignedTokens = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = new[] { validationKey },

                RequireExpirationTime = true,
                ValidateLifetime = true,
            };

            // Validate the license key
            JsonWebTokenHandler jwtHandler = new();
            TokenValidationResult result = await jwtHandler.ValidateTokenAsync(checkModel.Key, validationParameters);
            if (!result.IsValid)
            {
                throw new ErrorException(Error.Unauthorized, "License key validation failed. info={0}", result.Exception.Message);
            }

            // Check Standard claims
            EnsureClaim(result, "sub", checkModel.Subject, new Error(Error.Unauthorized, "The subject or sub claim is not authorized. sub={0}", checkModel.Subject));
            EnsureClaim(result, "tid", checkModel.ConsumerTenantId, new Error(Error.Unauthorized, "The consumer tenant or tid claim is not authorized. tid={0}", checkModel.ConsumerTenantId));
            EnsureClaim(result, "oid", checkModel.ConsumerObjectId, new Error(Error.Unauthorized, "The consumer object or oid claim is not authorized. tid={0}", checkModel.ConsumerObjectId));
            EnsureClaim(result, "auth_apps", checkModel.AuthorizedApplicationId, new Error(Error.Unauthorized, "The application is not authorized. application_id={0}", checkModel.AuthorizedApplicationId));

            return LicenseClaimsModel.Create(result.Claims);
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

            // Read the json file
            LicenseFileModel? licenseFileModel;
            try
            {
                // Make sure the lic stream is disposed to avoid locking.
                using (Stream licStream = File.OpenRead(cliOptions.Licensing.LicenseKey))
                {
                    licenseFileModel = await JsonSerializer.DeserializeAsync<LicenseFileModel>(licStream);
                }
            }
            catch (JsonException ex)
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The Json license file is not valid, see licensing options. json_file={0} info={1}", cliOptions.Licensing.LicenseKey, ex.Message);
            }

            // Make sure the model is valid Why ?
            if (licenseFileModel == null)
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The Json license file cannot be read, see licensing options. json_file={0}", cliOptions.Licensing.LicenseKey);
            }

            // Online lic check
            if (cliOptions.Handler.LicenseHandler == Handlers.OnlineLicenseHandler)
            {
                return await EnsureOnlineLicenseAsync(licenseFileModel);
            }
            else if (cliOptions.Handler.LicenseHandler == Handlers.OfflineLicenseHandler)
            {
                return await EnsureOfflineLicenseAsync(licenseFileModel);
            }
            else
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The Json license file licensing handler mode is not valid, see hosting options. licensing_handler={0}", cliOptions.Handler.LicenseHandler);
            }
        }

        private async Task<License> EnsureOnlineLicenseAsync(LicenseFileModel licenseFileModel)
        {
            // Check JWS signed assertion (JWS key)
            LicenseOnlineCheckModel checkModel = new()
            {
                Issuer = Shared.Constants.Issuer,
                Audience = AuthEndpoints.PiB2CIssuer(licenseFileModel.ConsumerTenantId),
                AuthorizedApplicationId = cliOptions.Licensing.AuthorizedApplicationId!,
                AuthorizedParty = licenseFileModel.AuthorizedParty,
                ConsumerObjectId = licenseFileModel.ConsumerObjectId,
                ConsumerTenantId = licenseFileModel.ConsumerTenantId,
                Key = licenseFileModel.Key,
                KeyType = licenseFileModel.KeyType,
                BrokerId = licenseFileModel.BrokerId,
                Subject = licenseFileModel.Subject
            };

            // Make sure we use the full base address
            using (HttpResponseMessage response = await CheckOnlineLicenseAsync(checkModel))
            {
                if (!response.IsSuccessStatusCode)
                {
                    Error? error = await JsonSerializer.DeserializeAsync<Error>(await response.Content.ReadAsStreamAsync());
                    throw new ErrorException(error!);
                }

                LicenseClaimsModel? claims = await JsonSerializer.DeserializeAsync<LicenseClaimsModel>(await response.Content.ReadAsStreamAsync()) ?? throw new ErrorException(Errors.InvalidLicense, "The license claims are invalid.");

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

        private async Task<License> EnsureOfflineLicenseAsync(LicenseFileModel licenseFileModel)
        {
            // Check JWS signed assertion (JWS key)
            LicenseOfflineCheckModel checkModel = new()
            {
                Issuer = Shared.Constants.Issuer,
                Audience = AuthEndpoints.PiB2CIssuer(licenseFileModel.ConsumerTenantId),
                AuthorizedApplicationId = cliOptions.Licensing.AuthorizedApplicationId!,
                AuthorizedParty = licenseFileModel.AuthorizedParty,
                ConsumerTenantId = licenseFileModel.ConsumerTenantId,
                Key = licenseFileModel.Key,
                KeyType = licenseFileModel.KeyType,
                BrokerId = licenseFileModel.BrokerId,
                Subject = licenseFileModel.Subject,
                ValidationKey = licenseFileModel.ValidationKey
            };

            // Make sure we use the full base address
            LicenseClaimsModel claims = await CheckOfflineLicenseAsync(checkModel);

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

        private void EnsureClaim(TokenValidationResult result, string claim, object? expectedValue, Error error)
        {
            result.Claims.TryGetValue(claim, out object? jwtValue);

            // Both JSON claim and expected claim do not exist.
            // E.g. oid claim is optional, so the JWT token may not have it and if check still passes that then it is an error.
            if (jwtValue == null && expectedValue == null)
            {
                return;
            }

            if (jwtValue == null || !jwtValue.Equals(expectedValue))
            {
                throw new ErrorException(error);
            }
        }

        private readonly string checkLicUrl = "https://api.perpetualintelligence.com/public/checklicense";
        private readonly CliOptions cliOptions;
        private readonly string fallbackCheckLicUrl = "https://piapim.azure-api.net/public/checklicense";
        private readonly IHttpClientFactory? httpClientFactory;
        private readonly ILogger<LicenseExtractor> logger;
        private License? license;
    }
}