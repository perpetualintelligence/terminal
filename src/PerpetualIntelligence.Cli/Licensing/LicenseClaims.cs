/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Shared.Extensions;
using System;
using System.Collections.Generic;

namespace PerpetualIntelligence.Cli.Licensing
{
    /// <summary>
    /// Defines the licensing claims.
    /// </summary>
    public sealed class LicenseClaims
    {
        /// <summary>
        /// The <c>acr</c> claim values.
        /// </summary>
        public string Acr { get; private set; } = null!;

        /// <summary>
        /// The <c>aud</c> claim.
        /// </summary>
        public string Audience { get; private set; } = null!;

        /// <summary>
        /// The <c>azp</c> claim.
        /// </summary>
        public string AuthorizedParty { get; private set; } = null!;

        /// <summary>
        /// The <c>country</c> claim.
        /// </summary>
        public string Country { get; private set; } = null!;

        /// <summary>
        /// The <c>exp</c> claim.
        /// </summary>
        public DateTimeOffset? Expiry { get; private set; } = null!;

        /// <summary>
        /// The <c>iat</c> claim.
        /// </summary>
        public DateTimeOffset? IssuedAt { get; private set; }

        /// <summary>
        /// The <c>iss</c> claim.
        /// </summary>
        public string Issuer { get; private set; } = null!;

        /// <summary>
        /// The <c>jti</c> claim.
        /// </summary>
        public string Jti { get; private set; } = null!;

        /// <summary>
        /// The <c>name</c> claim.
        /// </summary>
        public string Name { get; private set; } = null!;

        /// <summary>
        /// The <c>nbf</c> claim.
        /// </summary>
        public DateTimeOffset? NotBefore { get; private set; } = null!;

        /// <summary>
        /// The pricing plan read from the <see cref="Acr"/>.
        /// </summary>
        public string Plan { get; private set; } = null!;

        /// <summary>
        /// The <c>sub</c> claim.
        /// </summary>
        public string Subject { get; private set; } = null!;

        /// <summary>
        /// The <c>tid</c> claim.
        /// </summary>
        public string TenantId { get; private set; } = null!;

        /// <summary>
        /// The usage read from the <see cref="Acr"/>.
        /// </summary>
        public string Usage { get; private set; } = null!;

        /// <summary>
        /// Creates a new instance of <see cref="LicenseClaims"/> based on the specified claims dictionary.
        /// </summary>
        /// <param name="claims">The source claims.</param>
        public static LicenseClaims Create(IDictionary<string, object> claims)
        {
            try
            {
                LicenseClaims fromClaims = new LicenseClaims()
                {
                    Name = claims["name"].ToString(),
                    Country = claims["country"].ToString(),
                    Audience = claims["aud"].ToString(),
                    Issuer = claims["iss"].ToString(),
                    Jti = claims["jti"].ToString(),
                    Subject = claims["sub"].ToString(),
                    TenantId = claims["tid"].ToString(),
                    AuthorizedParty = claims["azp"].ToString(),
                    Acr = claims["acr"].ToString(),
                    Expiry = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(claims["exp"])),
                    IssuedAt = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(claims["iat"])),
                    NotBefore = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(claims["nbf"])),
                };

                // Read from acr
                string[] acrValues = fromClaims.Acr.SplitBySpace();
                fromClaims.Plan = acrValues[0];
                fromClaims.Usage = acrValues[1];

                return fromClaims;
            }
            catch (Exception ex)
            {
                throw new ErrorException(Errors.MissingClaim, "The claim is missing in the request. additional_info={0}", ex.Message);
            }
        }
    }
}
