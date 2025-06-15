/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using OneImlx.Shared.Extensions;
using OneImlx.Shared.Infrastructure;
using OneImlx.Shared.Licensing;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Shared;

namespace OneImlx.Terminal.Licensing
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
        public LicenseExtractor(ILicenseDebugger licenseDebugger, TerminalOptions terminalOptions, ILogger<LicenseExtractor> logger)
        {
            this._licenseDebugger = licenseDebugger;
            this._terminalOptions = terminalOptions;
            this._logger = logger;
        }

        /// <summary>
        /// Extracts the <see cref="License"/> from the license keys.
        /// </summary>
        public async Task<LicenseExtractorResult> ExtractLicenseAsync()
        {
            // Ensure license plan is valid.
            if (!ProductCatalog.IsValidPlan(ProductCatalog.TerminalFramework, _terminalOptions.Licensing.LicensePlan))
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The license plan is not valid. plan={0}", _terminalOptions.Licensing.LicensePlan);
            }

            // In on-prem or air-gapped deployments with locked-down environments, skip license check if no debugger is
            // attached. Always perform license validation when a debugger is attached. Grant claims based on the
            // license plan when skipping.
            if (IsAirGappedDeployment(_terminalOptions.Licensing.Deployment) && !_licenseDebugger.IsDebuggerAttached())
            {
                _licenseExtractorResult = ExtractAirGapped();
            }
            else
            {
                _licenseExtractorResult = await ExtractJsonAsync();
            }

            return _licenseExtractorResult;
        }

        /// <inheritdoc/>
        public Task<License?> GetLicenseAsync()
        {
            return Task.FromResult(_licenseExtractorResult?.License);
        }

        private static async Task<LicenseClaims> CheckLicenseAsync(LicenseCheck checkModel)
        {
            // Make sure validation key is not null
            if (checkModel.ValidationKey == null)
            {
                throw new TerminalException(TerminalErrors.MissingClaim, "The validation key is missing in the request.");
            }

            // https://stackoverflow.com/questions/58102904/how-to-verify-a-jwt-token-signed-with-x509-with-only-a-public-key-in-aspnetcore
            _ = LicenseX509Service.FromJwtValidationKey(checkModel.ValidationKey, out X509Certificate2 x509Certificate);
            if (!x509Certificate.Thumbprint.Equals(TerminalIdentifiers.ValidationThumbprint, StringComparison.OrdinalIgnoreCase))
            {
                throw new TerminalException(TerminalErrors.UnauthorizedAccess, "The validation certificate is not valid. thumbprint={0}", x509Certificate.Thumbprint);
            }

            X509SecurityKey validationKey = new(x509Certificate);

            // Init token validation params
            TokenValidationParameters validationParameters = new()
            {
                ValidateAudience = true,
                ValidAudience = checkModel.Audience,

                ValidateIssuer = true,
                ValidIssuer = checkModel.Issuer,

                RequireSignedTokens = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = [validationKey],

                RequireExpirationTime = true,
                ValidateLifetime = true,
            };

            // Validate the license key
            JsonWebTokenHandler jwtHandler = new();
            TokenValidationResult result = await jwtHandler.ValidateTokenAsync(checkModel.LicenseKey, validationParameters);
            if (!result.IsValid)
            {
                throw new TerminalException(TerminalErrors.UnauthorizedAccess, "License key validation failed. info={0}", result.Exception.Message);
            }

            // Check Standard claims
            EnsureClaim(result, "tid", checkModel.TenantId, new Error(TerminalErrors.UnauthorizedAccess, "The tenant is not authorized. tid={0}", checkModel.TenantId));
            EnsureClaim(result, "apps", checkModel.Application, new Error(TerminalErrors.UnauthorizedAccess, "The application is not authorized. app={0}", checkModel.Application));

            return LicenseClaims.Create(result.Claims);
        }

        private static void EnsureClaim(TokenValidationResult result, string claim, object? expectedValue, Error error)
        {
            result.Claims.TryGetValue(claim, out object? jwtValue);

            // Both JSON claim and expected claim do not exist. E.g. oid claim is optional, so the JWT token may not
            // have it and if check still passes that then it is an error.
            if (jwtValue == null && expectedValue == null)
            {
                return;
            }

            if (jwtValue == null || !jwtValue.Equals(expectedValue))
            {
                throw new TerminalException(error);
            }
        }

        private static bool IsAirGappedDeployment(string deployment)
        {
            return (deployment == TerminalIdentifiers.AirGappedDeployment);
        }

        /// <summary>
        /// Creates a license for on-premise production deployment.
        /// </summary>
        /// <seealso cref="LicensingOptions.Deployment"/>
        private License AirGappedLicense()
        {
            return new License(
                _terminalOptions.Licensing.LicensePlan,
                TerminalIdentifiers.AirGappedUsage,
                TerminalIdentifiers.AirGappedKey,
                new LicenseClaims(),
                LicenseQuota.Create(_terminalOptions.Licensing.LicensePlan));
        }

        private async Task<LicenseExtractorResult> EnsureLicenseAsync(LicenseFile licenseFile)
        {
            LicenseCheck checkModel = new()
            {
                Issuer = TerminalIdentifiers.Issuer,
                Audience = GetAudience(licenseFile.TenantId),
                Application = _terminalOptions.Id,
                AuthorizedParty = ProductCatalog.TerminalFramework,
                TenantId = licenseFile.TenantId,
                LicenseKey = licenseFile.LicenseKey,
                Id = licenseFile.Id,
                ValidationKey = licenseFile.ValidationKey,
                Mode = TerminalIdentifiers.OfflineLicenseMode
            };

            // Check the license key
            LicenseClaims claims = await CheckLicenseAsync(checkModel);

            // Make sure the acr contains the
            string[] acrValues = claims.AcrValues.SplitBySpace();
            if (acrValues.Length < 3)
            {
                throw new TerminalException(TerminalErrors.InvalidLicense, "The acr values are not valid. acr={0}", claims.AcrValues);
            }

            string plan = acrValues[0];
            string usage = acrValues[1];
            string brokerTenantId = acrValues[2];

            // Mismatch in license plan
            if (plan != _terminalOptions.Licensing.LicensePlan)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The license plan is not authorized. expected={0} actual={1}", plan, _terminalOptions.Licensing.LicensePlan);
            }

            // Mismatch in air gapped license usage
            if (IsAirGappedDeployment(_terminalOptions.Licensing.Deployment) && !IsAirGappedPlan(_terminalOptions.Licensing.LicensePlan))
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The license plan is not authorized for air gapped deployment. plan={0}", _terminalOptions.Licensing.LicensePlan);
            }

            // We just check the broker tenant to be present in offline mode
            if (string.IsNullOrWhiteSpace(brokerTenantId))
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The broker tenant is missing.");
            }

            LicenseQuota licenseQuota = LicenseQuota.Create(plan, claims.Custom);
            return new LicenseExtractorResult
            (
                new License(plan, usage, _terminalOptions.Licensing.LicenseFile!, claims, licenseQuota),
                TerminalIdentifiers.OfflineLicenseMode
            );
        }

        private LicenseExtractorResult ExtractAirGapped()
        {
            // Air gapped deployment is only supported for Enterprise and Corporate license plans.
            LicenseExtractorResult? licenseExtractorResult = null;
            if (IsAirGappedPlan(_terminalOptions.Licensing.LicensePlan))
            {
                licenseExtractorResult = new LicenseExtractorResult(AirGappedLicense(), null);
            }
            else
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The license plan is not authorized for air gapped deployment. plan={0}", _terminalOptions.Licensing.LicensePlan);
            }

            _logger.LogDebug("Extract air gapped license. plan={0} usage={1} sub={2} tenant={3}", licenseExtractorResult.License.Plan, licenseExtractorResult.License.Usage, licenseExtractorResult.License.Claims.Subject, licenseExtractorResult.License.Claims.TenantId);
            return licenseExtractorResult;
        }

        private async Task<LicenseExtractorResult> ExtractJsonAsync()
        {
            // Missing app id
            if (string.IsNullOrWhiteSpace(_terminalOptions.Id))
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The authorized application is not configured as a terminal identifier.");
            }

            // Missing key
            if (string.IsNullOrWhiteSpace(_terminalOptions.Licensing.LicenseFile))
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The license file is not configured.");
            }

            // License file needs to be a valid path if license contents is not set.
            if (_terminalOptions.Licensing.LicenseContents == null && !File.Exists(_terminalOptions.Licensing.LicenseFile))
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The license file path is not valid. file={0}", _terminalOptions.Licensing.LicenseFile);
            }

            // Avoid file locking for multiple threads.
            LicenseFile? licenseFile;
            await _semaphoreSlim.WaitAsync();
            try
            {
                // Decode the license contents if set.
                if (_terminalOptions.Licensing.LicenseContents != null)
                {
                    licenseFile = JsonSerializer.Deserialize<LicenseFile>(TerminalServices.DecodeLicenseContents(_terminalOptions.Licensing.LicenseContents));
                }
                else
                {
                    // Make sure the lic stream is disposed to avoid locking.
                    using (Stream licStream = File.OpenRead(_terminalOptions.Licensing.LicenseFile))
                    {
                        licenseFile = await JsonSerializer.DeserializeAsync<LicenseFile>(licStream);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The license file or contents are not valid. file={0} info={1}", _terminalOptions.Licensing.LicenseFile, ex.Message);
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            // Make sure the model is valid.
            if (licenseFile == null)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The license file cannot be read. file={0}", _terminalOptions.Licensing.LicenseFile);
            }

            // License check based on configured license handler.
            LicenseExtractorResult? licenseExtractorResult;
            if (licenseFile.Mode == TerminalIdentifiers.OfflineLicenseMode)
            {
                licenseExtractorResult = await EnsureLicenseAsync(licenseFile);
            }
            else
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The license mode is not valid. mode={0}", licenseFile.Mode);
            }

            _logger.LogDebug("Extract standard license. plan={0} usage={1} sub={2} tenant={3}", licenseExtractorResult.License.Plan, licenseExtractorResult.License.Usage, licenseExtractorResult.License.Claims.Subject, licenseExtractorResult.License.Claims.TenantId);
            return licenseExtractorResult;
        }

        private string GetAudience(string tenantId)
        {
            return string.Format("https://login.perpetualintelligence.com/{0}/v2.0", tenantId);
        }

        private bool IsAirGappedPlan(string licensePlan)
        {
            return licensePlan == ProductCatalog.TerminalPlanEnterprise ||
                   licensePlan == ProductCatalog.TerminalPlanCorporate;
        }

        private readonly ILicenseDebugger _licenseDebugger;
        private readonly ILogger<LicenseExtractor> _logger;
        private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
        private readonly TerminalOptions _terminalOptions;
        private LicenseExtractorResult? _licenseExtractorResult;
    }
}
