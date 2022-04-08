/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using PerpetualIntelligence.Cli.Configuration.Options;

using PerpetualIntelligence.Shared.Exceptions;
using System;
using System.Security.Cryptography;
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
        public LicenseExtractor(CliOptions cliOptions)
        {
            this.cliOptions = cliOptions;
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
                    throw new ErrorException(Errors.InvalidConfiguration, "The key source is not valid. {0}", cliOptions.Licensing.KeySource);
                }
            }

            return new LicenseExtractorResult(license);
        }

        private Task<License> ExtractFromJsonAsync()
        {
            // Missing key
            if (string.IsNullOrWhiteSpace(cliOptions.Licensing.LicenseKey))
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The license key is not configured, see licensing options. options={0}", typeof(LicensingOptions).FullName);
            }

            // Key not a file
            if (System.IO.File.Exists(cliOptions.Licensing.LicenseKey))
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The license key is not a valid json file path, see licensing options. options={0} key_source={1}", typeof(LicensingOptions).FullName, cliOptions.Licensing.KeySource);
            }

            // Not a valid Json

            throw new NotImplementedException();
        }

        private License ExtractFromKey(string licenseKey)
        {
            JsonWebTokenHandler handler = new();

            RSAParameters rsa = new()
            {
                Exponent = Convert.FromBase64String("AQAB"),
                Modulus = Convert.FromBase64String(
                    "pY85D8HC9MjdDQKwVM4wko/E4oTHWA7koXDvQ63B6BkClJCi4F922A3/3qnA1kWiK2N1cTFlbm5ThLbis+AxY68eP8qijQLrfnvukXY/c60Qr6brJmGCCqPdHHx5sUf8XAxPN/DzV75eHivrrzqgDwUXFlRLB16uVxCfIHD+o4thG5dlGfXpqa8pLGroVzhxQKkCmV7XE7aydakrziKZYzPUfjh5Y4PsH63LHpGym//J+bvJGeSZjFcu0UooAErCB2YbmpAALDzKzeIos+NL888IadqPqDvqoWdL46HE9K567xVRF3gYXR+3Xo19U2OjE30ls5SvFS0UZmWhe/GcAQ=="),
            };

            RsaSecurityKey securityKey = new(rsa)
            {
                KeyId = "urn:oneimlx:cli:skey:4766771d212d445db29e2d901f338c38"
            };

            TokenValidationParameters parms = new()
            {
                ValidIssuer = "https://perpetualintelligence.com",
                ValidAudience = Protocols.Constants.CliUrn,
                IssuerSigningKey = securityKey,
                ValidateLifetime = false
            };

            var validateResult = handler.ValidateToken(licenseKey, parms);
            if (validateResult.IsValid)
            {
                LicenseClaims claims = LicenseClaims.Create(validateResult.Claims);
                LicenseLimits limits = LicenseLimits.Create(claims.Plan);
                return new License(licenseKey, claims, limits);
            }
            else
            {
                throw new ErrorException(Errors.InvalidLicense, "The license key is not valid. additional_info={0}", validateResult.Exception.Message);
            }
        }

        private readonly CliOptions cliOptions;
        private License? license;
    }
}
