/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Linq;

namespace PerpetualIntelligence.Terminal.Authentication.Msal
{
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using Microsoft.Identity.Client;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Xunit;

    public class MsalPublicClientAccessTokenProviderTests
    {
        private readonly Mock<IMsalTokenAcquisition> _msalTokenAcquisitionMock;
        private readonly Mock<ILogger<MsalPublicClientAccessTokenProvider>> _loggerMock;
        private readonly IEnumerable<string> _defaultScopes = new[] { "User.Read" };
        private readonly IEnumerable<string> _validHosts = new[] { "graph.microsoft.com" };

        public MsalPublicClientAccessTokenProviderTests()
        {
            _msalTokenAcquisitionMock = new Mock<IMsalTokenAcquisition>();
            _loggerMock = new Mock<ILogger<MsalPublicClientAccessTokenProvider>>();
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
        public async Task GetAuthorizationTokenAsync_DoesNot_Attempts_InteractiveAcquisition_WhenSilentFails()
        {
            var silentException = new MsalUiRequiredException("error_code", "A problem occurred.");
            SetupMockTokenAcquisitionForUiRequiredException(silentException);

            var provider = CreateProvider();

            Func<Task> task = () => provider.GetAuthorizationTokenAsync(new Uri("https://graph.microsoft.com"), null);
            await task.Should().ThrowAsync<MsalUiRequiredException>();
        }

        [Fact]
        public async Task GetAuthorizationTokenAsync_Throws_ForNoAccounts()
        {
            SetupMockTokenAcquisitionForNoAccounts();
            var provider = CreateProvider();

            Func<Task> action = async () => await provider.GetAuthorizationTokenAsync(new Uri("https://graph.microsoft.com"), null);

            await action.Should().ThrowAsync<TerminalException>().WithMessage("The MSAL account is missing in the request.");
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
            usedScopes.Should().BeEquivalentTo(_defaultScopes.Concat(additionalScopes));

            // Act: Call the method without additional scopes.
            await provider.GetAuthorizationTokenAsync(new Uri("https://graph.microsoft.com"), null);

            // Assert: Check if the used scopes are the default scopes.
            usedScopes.Should().BeEquivalentTo(_defaultScopes);
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
                                        scopes: _defaultScopes.ToArray(),
                                        Guid.NewGuid()));

            var provider = CreateProvider();

            // Act: Call the method under test.
            await provider.GetAuthorizationTokenAsync(new Uri("https://graph.microsoft.com"), null);

            // Assert: Check if the first account from the list was used for token acquisition.
            usedAccount.Should().BeEquivalentTo(mockAccounts.First(), options => options.ComparingByMembers<IAccount>());
            usedAccount!.Username.Should().Be("user1@example.com");
        }

        private MsalPublicClientAccessTokenProvider CreateProvider()
        {
            return new MsalPublicClientAccessTokenProvider(_msalTokenAcquisitionMock.Object, _loggerMock.Object, _defaultScopes, _validHosts);
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
                scopes: _defaultScopes.ToArray(),
                Guid.NewGuid());

            _msalTokenAcquisitionMock.Setup(x => x.AcquireTokenSilentAsync(It.IsAny<IEnumerable<string>>(), account))
                                     .ReturnsAsync(authenticationResult);
        }

        private void SetupMockTokenAcquisitionForUiRequiredException(MsalUiRequiredException exception)
        {
            _msalTokenAcquisitionMock.Setup(x => x.GetAccountsAsync(null)).ReturnsAsync(new[] { new Mock<IAccount>().Object });
            _msalTokenAcquisitionMock.Setup(x => x.AcquireTokenSilentAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<IAccount>()))
                                     .ThrowsAsync(exception);
        }

        private void SetupMockTokenAcquisitionForNoAccounts()
        {
            _msalTokenAcquisitionMock.Setup(x => x.GetAccountsAsync(null)).ReturnsAsync(Enumerable.Empty<IAccount>());
        }
    }
}