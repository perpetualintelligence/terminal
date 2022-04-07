/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using PerpetualIntelligence.Shared.Exceptions;

namespace PerpetualIntelligence.Cli.Licensing
{
    public class LicenseClaimsTests
    {
        [Fact]
        public void LicenseClaims_Create_ShouldSetPropsCorrectly()
        {
            long exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();
            long iat = DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeSeconds();
            long nbf = DateTimeOffset.UtcNow.AddHours(-0.5).ToUnixTimeSeconds();

            var claims = new Dictionary<string, object>()
            {
                {"name", "test_name" },
                {"country", "test_country" },
                {"aud", "test_audience" },
                {"iss", "test_issuer" },
                {"jti", "test_jti" },
                {"sub", "test_subject" },
                {"tid", "test_tenantid" },
                {"azp", "test_azp" },
                {"acr", "test_acr1 test_acr2" },
                {"exp", exp },
                {"iat", iat },
                {"nbf",  nbf },
            };

            LicenseClaims licClaims = LicenseClaims.Create(claims);

            licClaims.Name.Should().Be("test_name");
            licClaims.Country.Should().Be("test_country");
            licClaims.Audience.Should().Be("test_audience");
            licClaims.Issuer.Should().Be("test_issuer");
            licClaims.Jti.Should().Be("test_jti");
            licClaims.Subject.Should().Be("test_subject");
            licClaims.TenantId.Should().Be("test_tenantid");
            licClaims.AuthorizedParty.Should().Be("test_azp");
            licClaims.Acr.Should().Be("test_acr1 test_acr2");
            licClaims.Expiry.Should().Be(DateTimeOffset.FromUnixTimeSeconds(exp));
            licClaims.IssuedAt.Should().Be(DateTimeOffset.FromUnixTimeSeconds(iat));
            licClaims.NotBefore.Should().Be(DateTimeOffset.FromUnixTimeSeconds(nbf));

            // First acr
            licClaims.Plan.Should().Be("test_acr1");

            // Second acr
            licClaims.Usage.Should().Be("test_acr2");
        }

        [Fact]
        public void MissingClaim_ShouldThrowErrorException()
        {
            try
            {
                long exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();
                long iat = DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeSeconds();
                long nbf = DateTimeOffset.UtcNow.AddHours(-0.5).ToUnixTimeSeconds();

                var claims = new Dictionary<string, object>()
            {
                {"country", "test_country" },
                {"aud", "test_audience" },
                {"iss", "test_issuer" },
                {"jti", "test_jti" },
                {"sub", "test_subject" },
                {"tid", "test_tenantid" },
                {"azp", "test_azp" },
                {"acr", "test_acr1 test_acr2" },
                {"exp", exp },
                {"iat", iat },
                {"nbf",  nbf },
            };

                LicenseClaims licClaims = LicenseClaims.Create(claims);
            }
            catch (Exception ex)
            {
                ErrorException eex = (ErrorException)ex;
                eex.Error.ErrorCode.Should().Be(Errors.MissingClaim);
                eex.Error.FormatDescription().Should().Be("The claim is missing in the request. additional_info=The given key 'name' was not present in the dictionary.");
            }

        }
    }
}
