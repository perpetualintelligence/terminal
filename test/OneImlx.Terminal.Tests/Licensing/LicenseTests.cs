/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using OneImlx.Shared.Infrastructure;
using OneImlx.Shared.Licensing;
using Xunit;

namespace OneImlx.Terminal.Licensing
{
    public class LicenseTests
    {
        [Fact]
        public void SetFailed_Ignores_When_Called_MultipleTimes()
        {
            // Arrange
            var license = new License("plan", "usage", "licenseKey", new LicenseClaims(), new LicenseQuota());
            var initialError = new Error("first_error", "This is the initial error.");
            var newError = new Error("second_error", "This is a new error.");

            // Act
            license.SetFailed(initialError);
            license.SetFailed(newError);

            // Assert
            license.Failed.Should().NotBeNull();
            license.Failed.Should().BeSameAs(initialError);
        }

        [Fact]
        public void SetFailed_Sets_FailedError_WhenCalled()
        {
            // Arrange
            var license = new License("plan", "usage", "licenseKey", new LicenseClaims(), new LicenseQuota());
            var error = new Error("test_error", "This is a test error.");

            // Act
            license.SetFailed(error);

            // Assert
            license.Failed.Should().NotBeNull();
            license.Failed.Should().Be(error);
        }
    }
}
