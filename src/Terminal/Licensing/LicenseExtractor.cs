﻿/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using PerpetualIntelligence.Shared.Authorization;
using PerpetualIntelligence.Shared.Extensions;
using PerpetualIntelligence.Shared.Infrastructure;
using PerpetualIntelligence.Shared.Licensing;
using PerpetualIntelligence.Terminal.Configuration.Options;
using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Licensing
{
    /// <summary>
    /// The default <see cref="ILicenseExtractor"/>
    /// </summary>
    public sealed class LicenseExtractor : ILicenseExtractor
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="licenseDebugger">The license debugger.</param>
        /// <param name="terminalOptions">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="httpClientFactory">The optional HTTP client factory</param>
        public LicenseExtractor(ILicenseDebugger licenseDebugger, TerminalOptions terminalOptions, ILogger<LicenseExtractor> logger, IHttpClientFactory? httpClientFactory = null)
        {
            this.licenseDebugger = licenseDebugger;
            this.terminalOptions = terminalOptions;
            this.logger = logger;
            this.httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Extracts the <see cref="License"/> from the license keys.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<LicenseExtractorResult> ExtractLicenseAsync(LicenseExtractorContext context)
        {
            logger.LogDebug("Extract license.");

            // For singleton DI service we don't extract license keys once extracted.
            if (licenseExtractorResult == null)
            {
                if (terminalOptions.Licensing.LicenseKeySource == LicenseSources.JsonFile)
                {
                    licenseExtractorResult = await ExtractFromJsonAsync();
                }
                else
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The license key source is not supported, see licensing options. key_source={0}", terminalOptions.Licensing.LicenseKeySource);
                }
            }

            return licenseExtractorResult;
        }

        /// <inheritdoc/>
        public Task<License?> GetLicenseAsync()
        {
            logger.LogDebug("Get license.");
            return Task.FromResult(licenseExtractorResult?.License);
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
                throw new TerminalException(TerminalErrors.UnauthorizedAccess, "License validation certificate cannot have private key.");
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
                throw new TerminalException(TerminalErrors.UnauthorizedAccess, "License key validation failed. info={0}", result.Exception.Message);
            }

            // Check Standard claims
            EnsureClaim(result, "sub", checkModel.Subject, new Error(TerminalErrors.UnauthorizedAccess, "The subject or sub claim is not authorized. sub={0}", checkModel.Subject));
            EnsureClaim(result, "tid", checkModel.ConsumerTenantId, new Error(TerminalErrors.UnauthorizedAccess, "The consumer tenant or tid claim is not authorized. tid={0}", checkModel.ConsumerTenantId));
            EnsureClaim(result, "oid", checkModel.ConsumerObjectId, new Error(TerminalErrors.UnauthorizedAccess, "The consumer object or oid claim is not authorized. tid={0}", checkModel.ConsumerObjectId));
            EnsureClaim(result, "auth_apps", checkModel.AuthorizedApplicationId, new Error(TerminalErrors.UnauthorizedAccess, "The application is not authorized. application_id={0}", checkModel.AuthorizedApplicationId));

            return LicenseClaimsModel.Create(result.Claims);
        }

        /// <summary>
        /// Should be called only for online check.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="TerminalException"></exception>
        private HttpClient EnsureHttpClient()
        {
            if (httpClientFactory == null)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The IHttpClientFactory is not configured. licensing_handler={0}", terminalOptions.Handler.LicenseHandler);
            }

            // Make sure the HTTP client name is setup
            if (terminalOptions.Http.HttpClientName == null)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The HTTP client name is not configured, see licensing options. licensing_handler={0}", terminalOptions.Handler.LicenseHandler);
            }

            // Setup the HTTP client
            HttpClient httpClient = httpClientFactory.CreateClient(terminalOptions.Http.HttpClientName);
            return httpClient;
        }

        private async Task<LicenseExtractorResult> ExtractFromJsonAsync()
        {
            logger.LogDebug("Extract from JSON license key.");

            // Missing app id
            if (string.IsNullOrWhiteSpace(terminalOptions.Licensing.AuthorizedApplicationId))
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The authorized application is not configured, see licensing options.");
            }

            // Missing key
            if (string.IsNullOrWhiteSpace(terminalOptions.Licensing.LicenseKey))
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The license file is not configured, see licensing options. key_source={0}", terminalOptions.Licensing.LicenseKeySource);
            }

            // Key not a file
            if (!File.Exists(terminalOptions.Licensing.LicenseKey))
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The license file path is not valid, see licensing options. key_file={0}", terminalOptions.Licensing.LicenseKey);
            }

            // Missing or invalid license plan.
            if (!TerminalLicensePlans.IsValidPlan(terminalOptions.Licensing.LicensePlan))
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The license plan is not valid, see licensing options. plan={0}", terminalOptions.Licensing.LicensePlan);
            }

            // Read the json file, avoid file locking for multiple threads.
            LicenseFileModel? licenseFileModel;
            await semaphoreSlim.WaitAsync();
            try
            {
                // Make sure the lic stream is disposed to avoid locking.
                using (Stream licStream = File.OpenRead(terminalOptions.Licensing.LicenseKey))
                {
                    licenseFileModel = await JsonSerializer.DeserializeAsync<LicenseFileModel>(licStream);
                }
            }
            catch (JsonException ex)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The license file is not valid, see licensing options. key_file={0} info={1}", terminalOptions.Licensing.LicenseKey, ex.Message);
            }
            finally
            {
                semaphoreSlim.Release();
            }

            // Make sure the model is valid.
            if (licenseFileModel == null)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The license file cannot be read, see licensing options. key_file={0}", terminalOptions.Licensing.LicenseKey);
            }

            // License check based on configured license handler.
            LicenseExtractorResult licenseExtractorResult;
            if (terminalOptions.Handler.LicenseHandler == TerminalHandlers.OnlineLicenseHandler)
            {
                licenseExtractorResult = await EnsureOnlineLicenseAsync(licenseFileModel);
            }
            else if (terminalOptions.Handler.LicenseHandler == TerminalHandlers.OfflineLicenseHandler)
            {
                licenseExtractorResult = await EnsureOfflineLicenseAsync(licenseFileModel);
            }
            else if (terminalOptions.Handler.LicenseHandler == TerminalHandlers.OnPremiseLicenseHandler)
            {
                licenseExtractorResult = await EnsureOnPremiseLicenseAsync(licenseFileModel);
            }
            else
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The license handler is not valid, see hosting options. licensing_handler={0}", terminalOptions.Handler.LicenseHandler);
            }

            logger.LogDebug("Extracted license. plan={0} usage={1} subject={2} tenant={3}", licenseExtractorResult.License.Plan, licenseExtractorResult.License.Usage, licenseExtractorResult.License.Claims.Subject, licenseExtractorResult.License.Claims.TenantId);
            return licenseExtractorResult;
        }

        private async Task<LicenseExtractorResult> EnsureOnPremiseLicenseAsync(LicenseFileModel licenseFileModel)
        {
            logger.LogDebug("Extract on-premise license. subject={0} tenant={1}", licenseFileModel.Subject, licenseFileModel.ConsumerTenantId);

            if (terminalOptions.Licensing.LicensePlan == TerminalLicensePlans.OnPremise ||
                terminalOptions.Licensing.LicensePlan == TerminalLicensePlans.Unlimited)
            {
                // Check if debugger is attached for on-premise mode.
                // On-Premise mode we allow developers to work online and offline.
                if (!licenseDebugger.IsDebuggerAttached() && terminalOptions.Licensing.OnPremiseDeployment.GetValueOrDefault())
                {
                    logger.LogDebug("On-premise deployment enabled. Skipping license check. plan={0} subject={1} tenant={2}", terminalOptions.Licensing.LicensePlan, licenseFileModel.Subject, licenseFileModel.ConsumerTenantId);
                    return new LicenseExtractorResult(OnPremiseDeploymentLicense(), TerminalHandlers.OnPremiseLicenseHandler);
                }
                else
                {
                    if (licenseFileModel.ValidationKey != null)
                    {
                        return await EnsureOfflineLicenseAsync(licenseFileModel);
                    }
                    else
                    {
                        return await EnsureOnlineLicenseAsync(licenseFileModel);
                    }
                }
            }
            else
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The license plan is not authorized for on-premise deployment, see licensing options. plan={0}", terminalOptions.Licensing.LicensePlan);
            }
        }

        /// <summary>
        /// Creates a license for on-premise production deployment.
        /// </summary>
        /// <seealso cref="LicensingOptions.OnPremiseDeployment"/>
        private License OnPremiseDeploymentLicense()
        {
            return new License(
                LicenseProviders.PerpetualIntelligence,
                TerminalHandlers.OnPremiseLicenseHandler,
                terminalOptions.Licensing.LicensePlan,
                "on-premise-deployment",
                terminalOptions.Licensing.LicenseKeySource,
                "on-premise-deployment",
                new LicenseClaimsModel(),
                LicenseLimits.Create(terminalOptions.Licensing.LicensePlan),
                LicensePrice.Create(terminalOptions.Licensing.LicensePlan));
        }

        private async Task<LicenseExtractorResult> EnsureOnlineLicenseAsync(LicenseFileModel licenseFileModel)
        {
            logger.LogDebug("Extract online license. subject={0} tenant={1}", licenseFileModel.Subject, licenseFileModel.ConsumerTenantId);

            // Check JWS signed assertion (JWS key)
            LicenseOnlineCheckModel checkModel = new()
            {
                Issuer = Shared.Constants.Issuer,
                Audience = AuthEndpoints.PiB2CIssuer(licenseFileModel.ConsumerTenantId),
                AuthorizedApplicationId = terminalOptions.Licensing.AuthorizedApplicationId!,
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
                    throw new TerminalException(error!);
                }

                LicenseClaimsModel? claims = await JsonSerializer.DeserializeAsync<LicenseClaimsModel>(await response.Content.ReadAsStreamAsync()) ?? throw new TerminalException(TerminalErrors.InvalidLicense, "The license claims are invalid.");

                // Check consumer with licensing options.
                if (claims.TenantId != terminalOptions.Licensing.ConsumerTenantId)
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The consumer tenant is not authorized, see licensing options. consumer_tenant_id={0}", terminalOptions.Licensing.ConsumerTenantId);
                }

                // Check subject with licensing options.
                if (claims.Subject != terminalOptions.Licensing.Subject)
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The subject is not authorized, see licensing options. subject={0}", terminalOptions.Licensing.Subject);
                }

                // Make sure the acr contains the
                string[] acrValues = claims.AcrValues.SplitBySpace();
                if (acrValues.Length < 3)
                {
                    throw new TerminalException(TerminalErrors.InvalidLicense, "The acr values are not valid. acr={0}", claims.AcrValues);
                }

                string plan = acrValues[0];
                string usage = acrValues[1];
                string providerId = acrValues[2];

                // If the on-prem-deployment plan is set then check
                if (plan != terminalOptions.Licensing.LicensePlan)
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The license plan is not authorized. expected={0} actual={1}", plan, terminalOptions.Licensing.LicensePlan);
                }

                // Make sure the provider tenant id matches
                if (providerId != LicenseProviders.PerpetualIntelligence)
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The provider is not authorized. expected={0} actual={1}", providerId, LicenseProviders.PerpetualIntelligence);
                }

                LicenseLimits licenseLimits = LicenseLimits.Create(plan, claims.Custom);
                LicensePrice licensePrice = LicensePrice.Create(plan, claims.Custom);
                return new LicenseExtractorResult
                (
                    new License(providerId, terminalOptions.Handler.LicenseHandler, plan, usage, terminalOptions.Licensing.LicenseKeySource, terminalOptions.Licensing.LicenseKey!, claims, licenseLimits, licensePrice),
                    TerminalHandlers.OnlineLicenseHandler
                );
            }
        }

        private async Task<LicenseExtractorResult> EnsureOfflineLicenseAsync(LicenseFileModel licenseFileModel)
        {
            logger.LogDebug("Extract offline license. subject={0} tenant={1}", licenseFileModel.Subject, licenseFileModel.ConsumerTenantId);

            // Check JWS signed assertion (JWS key)
            LicenseOfflineCheckModel checkModel = new()
            {
                Issuer = Shared.Constants.Issuer,
                Audience = AuthEndpoints.PiB2CIssuer(licenseFileModel.ConsumerTenantId),
                AuthorizedApplicationId = terminalOptions.Licensing.AuthorizedApplicationId!,
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
            if (claims.TenantId != terminalOptions.Licensing.ConsumerTenantId)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The consumer tenant is not authorized, see licensing options. consumer_tenant_id={0}", terminalOptions.Licensing.ConsumerTenantId);
            }

            // Check subject with licensing options.
            if (claims.Subject != terminalOptions.Licensing.Subject)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The subject is not authorized, see licensing options. subject={0}", terminalOptions.Licensing.Subject);
            }

            // Make sure the acr contains the
            string[] acrValues = claims.AcrValues.SplitBySpace();
            if (acrValues.Length < 3)
            {
                throw new TerminalException(TerminalErrors.InvalidLicense, "The acr values are not valid. acr={0}", claims.AcrValues);
            }

            string plan = acrValues[0];
            string usage = acrValues[1];
            string providerId = acrValues[2];

            // If the on-prem-deployment plan is set then check
            if (plan != terminalOptions.Licensing.LicensePlan)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The license plan is not authorized. expected={0} actual={1}", plan, terminalOptions.Licensing.LicensePlan);
            }

            // Make sure the provider tenant id matches
            if (providerId != LicenseProviders.PerpetualIntelligence)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The provider is not authorized. expected={0} actual={1}", providerId, LicenseProviders.PerpetualIntelligence);
            }

            LicenseLimits licenseLimits = LicenseLimits.Create(plan, claims.Custom);
            LicensePrice licensePrice = LicensePrice.Create(plan, claims.Custom);
            return new LicenseExtractorResult
            (
                new License(providerId, terminalOptions.Handler.LicenseHandler, plan, usage, terminalOptions.Licensing.LicenseKeySource, terminalOptions.Licensing.LicenseKey!, claims, licenseLimits, licensePrice),
                TerminalHandlers.OfflineLicenseHandler
            );
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
                throw new TerminalException(error);
            }
        }

        private readonly string checkLicUrl = "https://api.perpetualintelligence.com/public/checklicense";
        private readonly ILicenseDebugger licenseDebugger;
        private readonly TerminalOptions terminalOptions;
        private readonly string fallbackCheckLicUrl = "https://piapim.azure-api.net/public/checklicense";
        private readonly IHttpClientFactory? httpClientFactory;
        private readonly ILogger<LicenseExtractor> logger;
        private LicenseExtractorResult? licenseExtractorResult;
        private SemaphoreSlim semaphoreSlim = new(1, 1);
    }
}