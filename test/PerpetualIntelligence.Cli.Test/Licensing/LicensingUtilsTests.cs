/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Protocols.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace PerpetualIntelligence.Cli.Licensing
{
    [TestClass]
    public class LicensingUtilsTests
    {
        [TestMethod]
        public void CreateLicenseKey()
        {
            JsonWebTokenHandler handler = new();

            RSA rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(File.ReadAllBytes("cli_private_test"), out int bytesRead);

            string keyId = File.ReadAllLines("cli_keyid_test").First();
            RsaSecurityKey securityPrivateKey = new(rsa)
            {
                KeyId = keyId
            };

            SigningCredentials signingCredentials = new(securityPrivateKey, SecurityAlgorithms.RsaSha256);

            SecurityTokenDescriptor securityTokenDescriptor = new()
            {
                Issuer = "https://perpetualintelligence.com",
                Audience = Protocols.Constants.CliUrn,
                SigningCredentials = signingCredentials,
                Expires = DateTime.MaxValue.ToUniversalTime(),
                IssuedAt = DateTime.Now.ToUniversalTime(),
                Claims = new Dictionary<string, object>()
                {
                    { "license_plan" , "community" }
                }
            };

            string licKey = handler.CreateToken(securityTokenDescriptor);
            File.WriteAllText("cli_licKey_test", licKey);
        }

        [TestMethod]
        public void CreateRSAKeys()
        {
            // https://stackoverflow.com/questions/56471898/given-rsa-public-key-of-size-2048-how-do-i-get-its-modulus-and-exponent-using-b
            RSA rsa = RSA.Create();
            RSAParameters parameters = rsa.ExportParameters(true);
            byte[] privateKey = rsa.ExportPkcs8PrivateKey();
            File.WriteAllBytes("cli_private_test", rsa.ExportRSAPrivateKey());
            File.WriteAllBytes("cli_public_test", rsa.ExportRSAPublicKey());
            File.WriteAllText("cli_keyid_test", $"{Protocols.Constants.CliUrn}{":skey:"}{IdGenerator.NewLongId()}");
        }

        [TestMethod]
        public void GetExponentAndModulus()
        {
            RSA rsa = RSA.Create();
            rsa.ImportRSAPublicKey(File.ReadAllBytes("cli_public_test"), out int bytesRead);
            var rsaParams = rsa.ExportParameters(false);

            string[] em = new[]
            {
                Convert.ToBase64String(rsaParams.Exponent),
                Convert.ToBase64String(rsaParams.Modulus)
            };

            File.WriteAllLines("cli_expmod_test", em);
        }
    }
}
