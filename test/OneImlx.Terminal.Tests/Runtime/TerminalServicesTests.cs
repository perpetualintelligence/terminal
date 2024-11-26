/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using OneImlx.Terminal.Configuration.Options;
using Xunit;

namespace OneImlx.Terminal.Runtime.Tests
{
    public class TerminalServicesTests
    {
        public TerminalServicesTests()
        {
            _terminalOptions = new TerminalOptions
            {
                Router = new RouterOptions
                {
                }
            };
        }


        [Theory]
        [InlineData("dGVzdCBsaWNlbnNlY29uZGVudHM=")]
        [InlineData("[{\"prop1\": \"value1\", \"prop2\": 42, \"prop3\": [\"string\", 3.14, true, {\"nested\": \"object\"}]}]")]
        public void EncodeLicenseContents_ShouldReturn_ExpectedString(string licenseContents)
        {
            // Encode
            string result = TerminalServices.EncodeLicenseContents(licenseContents);
            result.Should().NotBe(licenseContents);

            // Decode
            string decodedContents = TerminalServices.DecodeLicenseContents(result);
            decodedContents.Should().Be(licenseContents);
        }

        private readonly TerminalOptions _terminalOptions;
    }
}
