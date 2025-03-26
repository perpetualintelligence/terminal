/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using System;
using Xunit;

namespace OneImlx.Terminal
{
    public class TerminalServicesTests
    {
        public TerminalServicesTests()
        {
        }

        [Fact]
        public void DelimitBytes_NullOrEmptyByteArray_ThrowsArgumentException()
        {
            byte[]? bytes = null;
            byte delimiter = 255;
            Action act = () => TerminalServices.DelimitBytes(bytes!, delimiter);
            act.Should().Throw<ArgumentException>();

            bytes = [];
            act = () => TerminalServices.DelimitBytes(bytes, delimiter);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void DelimitBytes_ValidByteArray_ReturnsDelimitedByteArray()
        {
            byte[] bytes = [1, 2, 3];
            byte delimiter = 255;

            byte[] result = TerminalServices.DelimitBytes(bytes, delimiter);
            result.Should().Equal([1, 2, 3, 255]);
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

        [Fact]
        public void IsOption_ShouldReturnFalse_WhenTokenIsNotOption()
        {
            // Arrange
            string token = "notAnOption";

            // Act
            bool result = TerminalServices.IsOption(token, '-', out bool isAlias);

            // Assert
            result.Should().BeFalse();
            isAlias.Should().BeFalse();
        }

        [Fact]
        public void IsOption_ShouldReturnTrue_WhenTokenIsOption()
        {
            // Arrange
            string token = "--option";

            // Act
            bool result = TerminalServices.IsOption(token, '-', out bool isAlias);

            // Assert
            result.Should().BeTrue();
            isAlias.Should().BeFalse();
        }

        [Fact]
        public void IsOption_ShouldReturnTrueAndSetIsAlias_WhenTokenIsAlias()
        {
            // Arrange
            string token = "-alias";

            // Act
            bool result = TerminalServices.IsOption(token, '-', out bool isAlias);

            // Assert
            result.Should().BeTrue();
            isAlias.Should().BeTrue();
        }
    }
}
