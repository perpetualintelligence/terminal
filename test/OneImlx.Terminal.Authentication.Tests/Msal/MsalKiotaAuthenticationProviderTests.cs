/*
    Copyright 2024 (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.Kiota.Abstractions;
using Moq;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Test.FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Authentication.Msal
{
    public class MsalKiotaAuthenticationProviderTests
    {
        public MsalKiotaAuthenticationProviderTests()
        {
            _msalTokenAcquisitionMock = new Mock<IMsalTokenAcquisition>();
            _loggerMock = new Mock<ILogger<MsalKiotaAuthProvider>>();
            _terminalOptions = new TerminalOptions
            {
                Authentication = new AuthenticationOptions
                {
                    Enabled = true,
                    DefaultScopes = ["User.Read"],
                    ValidHosts = ["graph.microsoft.com"]
                }
            };
        }

        [Fact]
        public async Task AuthenticateRequestAsync_AuthenticatesInteractive_ForNoAccounts()
        {
            // Arrange: Set up the token acquisition mock to simulate no accounts available that throws interactive
            // required exception.
            SetupMockTokenAcquisitionForNoAccounts();

            string expectedToken = "interactive_token_no_accounts";
            SetupMockTokenInteractiveAcquisition(expectedToken);

            var provider = CreateProvider();
            var requestInfo = new RequestInformation { URI = new Uri("https://graph.microsoft.com") };

            // Act & Assert: Ensure that the method throws a TerminalException when no accounts are available.
            await provider.AuthenticateRequestAsync(requestInfo);

            // Assert
            requestInfo.Headers["Authorization"].Should().Contain($"Bearer {expectedToken}");
        }

        [Fact]
        public async Task AuthenticateRequestAsync_ShouldAcquireTokenInteractively_WhenSilentFails()
        {
            // Arrange
            var exception = new MsalUiRequiredException("error_code", "A problem occurred.");
            SetupMockTokenAcquisitionForUiRequiredException(exception);

            string expectedToken = "interactive_token";
            SetupMockTokenInteractiveAcquisition(expectedToken);

            var provider = CreateProvider();
            var requestInfo = new RequestInformation { URI = new Uri("https://graph.microsoft.com") };

            // Act
            await provider.AuthenticateRequestAsync(requestInfo);

            // Assert
            requestInfo.Headers["Authorization"].Should().Contain($"Bearer {expectedToken}");
        }

        [Fact]
        public async Task AuthenticateRequestAsync_ShouldAcquireTokenSilently_WhenAccountExists()
        {
            // Arrange
            string expectedToken = "expected_token";
            SetupMockTokenAcquisition(expectedToken);
            var provider = CreateProvider();

            var requestInfo = new RequestInformation { URI = new Uri("https://graph.microsoft.com") };

            // Act
            await provider.AuthenticateRequestAsync(requestInfo);

            // Assert
            requestInfo.Headers["Authorization"].Should().Contain($"Bearer {expectedToken}");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(null)]
        public async Task AuthenticateRequestAsync_Throws_If_Authentication_Is_Not_Enabled(bool? enabled)
        {
            // Not enabled
            _terminalOptions.Authentication.Enabled = enabled;

            var requestInfo = new RequestInformation { URI = new Uri("https://graph.microsoft.com") };

            var provider = CreateProvider();
            Func<Task> func = async () => await provider.AuthenticateRequestAsync(requestInfo);
            await func.Should().ThrowAsync<TerminalException>()
                .WithErrorCode(TerminalErrors.InvalidConfiguration)
                .WithErrorDescription("The terminal authentication is not enabled.");
        }

        [Fact]
        public async Task AuthenticateRequestAsync_UsesCorrectScopes_ForTokenAcquisition()
        {
            // Arrange: Set up a list of mock accounts.
            var mockAccounts = new List<IAccount> { Mock.Of<IAccount>(acc => acc.Username == "user@example.com") };
            _msalTokenAcquisitionMock.Setup(x => x.GetAccountsAsync(null)).ReturnsAsync(mockAccounts);

            // Arrange: Set up the token acquisition mock to capture the scopes used for token acquisition.
            IEnumerable<string>? usedScopes = null;
            _msalTokenAcquisitionMock.Setup(x => x.AcquireTokenSilentAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<IAccount>()))
                                     .Callback<IEnumerable<string>, IAccount>((scopes, account) => usedScopes = scopes)
                                     .ReturnsAsync(new AuthenticationResult(
                                         accessToken: "expected_token",
                                         isExtendedLifeTimeToken: false,
                                         uniqueId: string.Empty,
                                         expiresOn: DateTimeOffset.UtcNow.AddHours(1),
                                         extendedExpiresOn: DateTimeOffset.UtcNow.AddHours(2),
                                         tenantId: string.Empty,
                                         account: Mock.Of<IAccount>(acc => acc.Username == "user@example.com"),
                                         idToken: string.Empty,
                                         scopes: _terminalOptions.Authentication.DefaultScopes!.ToArray(),
                                         Guid.NewGuid()));

            var provider = CreateProvider();
            var requestInfo = new RequestInformation { URI = new Uri("https://graph.microsoft.com") };
            var additionalScopes = new[] { "email", "openid", "profile" };
            var additionalAuthenticationContext = new Dictionary<string, object> { { "scopes", additionalScopes } };

            // Act & Assert: Call the method with additional scopes.
            await provider.AuthenticateRequestAsync(requestInfo, additionalAuthenticationContext);
            usedScopes.Should().BeEquivalentTo(_terminalOptions.Authentication.DefaultScopes!.Concat(additionalScopes));

            // Act & Assert: Call the method without additional scopes and assert default scopes are used.
            await provider.AuthenticateRequestAsync(requestInfo);
            usedScopes.Should().BeEquivalentTo(_terminalOptions.Authentication.DefaultScopes);
        }

        [Fact]
        public async Task AuthenticateRequestAsync_UsesFirstAccount_ForTokenAcquisition()
        {
            // Arrange: Set up a list of mock accounts.
            var mockAccounts = new List<IAccount>
            {
                Mock.Of<IAccount>(acc => acc.Username == "user1@example.com"),
                Mock.Of<IAccount>(acc => acc.Username == "user2@example.com")
            };
            _msalTokenAcquisitionMock.Setup(x => x.GetAccountsAsync(null)).ReturnsAsync(mockAccounts);

            // Arrange: Set up the token acquisition mock to capture the account used for token acquisition.
            IAccount? usedAccount = null;
            _msalTokenAcquisitionMock.Setup(x => x.AcquireTokenSilentAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<IAccount>()))
                                     .Callback<IEnumerable<string>, IAccount>((scopes, account) => usedAccount = account) // Capturing the account used.
                                     .ReturnsAsync(new AuthenticationResult(
                                         accessToken: "expected_token",
                                         isExtendedLifeTimeToken: false,
                                         uniqueId: string.Empty,
                                         expiresOn: DateTimeOffset.UtcNow.AddHours(1),
                                         extendedExpiresOn: DateTimeOffset.UtcNow.AddHours(2),
                                         tenantId: string.Empty,
                                         account: mockAccounts.First(),
                                         idToken: string.Empty,
                                         scopes: _terminalOptions.Authentication.DefaultScopes!.ToArray(),
                                         Guid.NewGuid()));

            var provider = CreateProvider();
            var requestInfo = new RequestInformation { URI = new Uri("https://graph.microsoft.com") };

            // Act: Call the method under test.
            await provider.AuthenticateRequestAsync(requestInfo);

            // Assert: Check if the first account from the list was used for token acquisition.
            usedAccount.Should().BeEquivalentTo(mockAccounts.First(), options => options.ComparingByMembers<IAccount>());
            usedAccount!.Username.Should().Be("user1@example.com");
        }

        private MsalKiotaAuthProvider CreateProvider()
        {
            return new MsalKiotaAuthProvider(_terminalOptions, _msalTokenAcquisitionMock.Object, _loggerMock.Object);
        }

        private void SetupMockTokenAcquisition(string token)
        {
            var account = new Mock<IAccount>().Object;
            _msalTokenAcquisitionMock.Setup(x => x.GetAccountsAsync(null)).ReturnsAsync([account]);

            var authenticationResult = new AuthenticationResult(
                accessToken: token,
                isExtendedLifeTimeToken: false,
                uniqueId: string.Empty,
                expiresOn: DateTimeOffset.UtcNow.AddHours(1),
                extendedExpiresOn: DateTimeOffset.UtcNow.AddHours(2),
                tenantId: string.Empty,
                account: account,
                idToken: string.Empty,
                scopes: _terminalOptions.Authentication.DefaultScopes!.ToArray(),
                Guid.NewGuid());

            _msalTokenAcquisitionMock.Setup(x => x.AcquireTokenSilentAsync(It.IsAny<IEnumerable<string>>(), account))
                                     .ReturnsAsync(authenticationResult);
        }

        private void SetupMockTokenAcquisitionForNoAccounts()
        {
            _msalTokenAcquisitionMock.Setup(static x => x.GetAccountsAsync(null)).ReturnsAsync([]);
            _msalTokenAcquisitionMock.Setup(static x => x.AcquireTokenSilentAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<IAccount>()))
                                     .ThrowsAsync(new MsalUiRequiredException("test_error", "test_error_message"));
        }

        private void SetupMockTokenAcquisitionForUiRequiredException(MsalUiRequiredException exception)
        {
            _msalTokenAcquisitionMock.Setup(static x => x.GetAccountsAsync(null)).ReturnsAsync([new Mock<IAccount>().Object]);
            _msalTokenAcquisitionMock.Setup(static x => x.AcquireTokenSilentAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<IAccount>()))
                                     .ThrowsAsync(exception);
        }

        private void SetupMockTokenInteractiveAcquisition(string token = "interactive_token")
        {
            var authenticationResult = new AuthenticationResult(
                accessToken: token,
                isExtendedLifeTimeToken: false,
                uniqueId: string.Empty,
                expiresOn: DateTimeOffset.UtcNow.AddHours(1),
                extendedExpiresOn: DateTimeOffset.UtcNow.AddHours(2),
                tenantId: string.Empty,
                account: new Mock<IAccount>().Object,
                idToken: string.Empty,
                scopes: _terminalOptions.Authentication.DefaultScopes!.ToArray(),
                Guid.NewGuid());

            _msalTokenAcquisitionMock.Setup(static x => x.AcquireTokenInteractiveAsync(It.IsAny<IEnumerable<string>>()))
                                     .ReturnsAsync(authenticationResult);
        }

        private readonly Mock<ILogger<MsalKiotaAuthProvider>> _loggerMock;
        private readonly Mock<IMsalTokenAcquisition> _msalTokenAcquisitionMock;
        private readonly TerminalOptions _terminalOptions;
    }
}
