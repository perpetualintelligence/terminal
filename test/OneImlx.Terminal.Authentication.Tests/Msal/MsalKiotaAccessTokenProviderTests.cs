/*
    Copyright 2024 (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Linq;

namespace OneImlx.Terminal.Authentication.Msal
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Identity.Client;
    using FluentAssertions;
    using Moq;
    using OneImlx.Terminal.Configuration.Options;
    using Xunit;
    using Microsoft.Kiota.Abstractions;
    using OneImlx.Test.FluentAssertions;

    public class MsalKiotaAccessTokenProviderTests
    {
        public MsalKiotaAccessTokenProviderTests()
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

        [Theory]
        [InlineData(false)]
        [InlineData(null)]
        public async Task GetAuthorizationTokenAsync_Throws_If_Authentication_Is_Not_Enabled(bool? enabled)
        {
            // Not enabled
            _terminalOptions.Authentication.Enabled = enabled;

            var provider = CreateProvider();
            Func<Task> func = async () => await provider.GetAuthorizationTokenAsync(new Uri("https://graph.microsoft.com"), null); ;
            await func.Should().ThrowAsync<TerminalException>()
                .WithErrorCode(TerminalErrors.InvalidConfiguration)
                .WithErrorDescription("The terminal authentication is not enabled.");
        }

        [Fact]
        public async Task GetAuthorizationTokenAsync_AcquiresTokenInteractively_ForNoAccounts()
        {
            SetupMockTokenAcquisitionForNoAccounts();

            string expectedToken = "interactive_token_no_accounts";
            SetupMockTokenInteractiveAcquisition(expectedToken);

            var provider = CreateProvider();

            string token = await provider.GetAuthorizationTokenAsync(new Uri("https://graph.microsoft.com"), null);

            token.Should().Be(expectedToken);
        }

        [Fact]
        public async Task GetAuthorizationTokenAsync_Does_Attempts_InteractiveAcquisition_WhenSilentFails()
        {
            // Arrange
            var exception = new MsalUiRequiredException("error_code", "A problem occurred.");
            SetupMockTokenAcquisitionForUiRequiredException(exception);

            string expectedToken = "interactive_token";
            SetupMockTokenInteractiveAcquisition(expectedToken);

            var provider = CreateProvider();

            // Act
            string token = await provider.GetAuthorizationTokenAsync(new Uri("https://graph.microsoft.com"), null);

            // Assert
            expectedToken.Should().Be(token);
        }

        [Fact]
        public async Task GetAuthorizationTokenAsync_ReturnsToken_ForValidHostAndScopes()
        {
            string expectedToken = "expected_token";
            SetupMockTokenAcquisition(expectedToken);
            var provider = CreateProvider();

            string token = await provider.GetAuthorizationTokenAsync(new Uri("https://graph.microsoft.com"), null);

            token.Should().Be(expectedToken);
        }

        [Fact]
        public async Task GetAuthorizationTokenAsync_Throws_ForInvalidHost()
        {
            var provider = CreateProvider();

            Func<Task> action = async () => await provider.GetAuthorizationTokenAsync(new Uri("https://invalid.com"), null);

            await action.Should().ThrowAsync<TerminalException>().WithMessage("The host is not authorized. uri=https://invalid.com/");
        }

        [Fact]
        public async Task GetAuthorizationTokenAsync_UsesCorrectScopes_ForTokenAcquisition()
        {
            // Arrange: Set up a list of mock accounts.
            var mockAccounts = new List<IAccount> { Mock.Of<IAccount>(acc => acc.Username == "user@example.com") };
            _msalTokenAcquisitionMock.Setup(x => x.GetAccountsAsync(null)).ReturnsAsync(mockAccounts);

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
                                         account: mockAccounts.First(),
                                         idToken: string.Empty,
                                         scopes: Array.Empty<string>(),
                                         Guid.NewGuid()));

            var provider = CreateProvider();

            // Act: Call the method with additional scopes.
            var additionalScopes = new[] { "email", "test", "test2" };
            await provider.GetAuthorizationTokenAsync(new Uri("https://graph.microsoft.com"), new Dictionary<string, object> { { "scopes", additionalScopes } });

            // Assert: Check if the used scopes are a combination of default scopes and additional scopes.
            usedScopes.Should().BeEquivalentTo(_terminalOptions.Authentication.DefaultScopes!.Concat(additionalScopes));

            // Act: Call the method without additional scopes.
            await provider.GetAuthorizationTokenAsync(new Uri("https://graph.microsoft.com"), null);

            // Assert: Check if the used scopes are the default scopes from TerminalOptions.
            usedScopes.Should().BeEquivalentTo(_terminalOptions.Authentication.DefaultScopes);
        }

        [Fact]
        public async Task GetAuthorizationTokenAsync_UsesFirstAccount_ForTokenAcquisition()
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

            // Act: Call the method under test.
            await provider.GetAuthorizationTokenAsync(new Uri("https://graph.microsoft.com"), null);

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
            _msalTokenAcquisitionMock.Setup(x => x.GetAccountsAsync(null)).ReturnsAsync(new[] { account });

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
            _msalTokenAcquisitionMock.Setup(static x => x.GetAccountsAsync(null)).ReturnsAsync(new[] { new Mock<IAccount>().Object });
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
