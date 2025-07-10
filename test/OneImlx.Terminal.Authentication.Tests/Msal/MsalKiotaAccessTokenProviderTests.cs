/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

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
    using OneImlx.Terminal.Authentication;
    using OneImlx.Terminal.Configuration.Options;
    using OneImlx.Terminal.Shared;
    using OneImlx.Test.FluentAssertions;
    using Xunit;

    public class MsalKiotaAccessTokenProviderTests
    {
        public MsalKiotaAccessTokenProviderTests()
        {
            _msalTokenAcquisitionMock = new Mock<ITokenAcquisition>();
            _loggerMock = new Mock<ILogger<MsalKiotaAuthProvider>>();
            _terminalOptions = new TerminalOptions
            {
                Authentication = new AuthenticationOptions
                {
                    Provider = "msal",
                    DefaultScopes = ["User.Read"],
                    ValidHosts = ["graph.microsoft.com"]
                }
            };
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
            var exception = new MsalUiRequiredException("error_code", "A problem occurred.");
            SetupMockTokenAcquisitionForUiRequiredException(exception);

            string expectedToken = "interactive_token";
            SetupMockTokenInteractiveAcquisition(expectedToken);

            var provider = CreateProvider();

            string token = await provider.GetAuthorizationTokenAsync(new Uri("https://graph.microsoft.com"), null);

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
        public async Task GetAuthorizationTokenAsync_Throws_If_Authentication_Is_Not_Enabled()
        {
            _terminalOptions.Authentication.Provider = "none";

            var provider = CreateProvider();
            Func<Task> func = async () => await provider.GetAuthorizationTokenAsync(new Uri("https://graph.microsoft.com"), null);
            await func.Should().ThrowAsync<TerminalException>()
                .WithErrorCode(TerminalErrors.InvalidConfiguration)
                .WithErrorDescription("The terminal MSAL authentication is not enabled.");
        }

        [Fact]
        public async Task GetAuthorizationTokenAsync_UsesCorrectScopes_ForTokenAcquisition()
        {
            var account = new Mock<IAccount>().Object;
            var identity = new TokenAcquisitionIdentity { Raw = account };
            _msalTokenAcquisitionMock.Setup(x => x.GetIdentitiesAsync(null)).ReturnsAsync([identity]);

            TokenAcquisitionSilentInput? capturedInput = null;
            _msalTokenAcquisitionMock.Setup(x => x.AcquireTokenSilentAsync(It.IsAny<TokenAcquisitionSilentInput>()))
                .Callback<TokenAcquisitionSilentInput>(input => capturedInput = input)
                .ReturnsAsync(new TokenAcquisitionResult { AccessToken = "expected_token" });

            var provider = CreateProvider();

            var additionalScopes = new[] { "email", "test", "test2" };
            await provider.GetAuthorizationTokenAsync(new Uri("https://graph.microsoft.com"), new Dictionary<string, object> { { "scopes", additionalScopes } });
            capturedInput!.Scopes.Should().BeEquivalentTo(_terminalOptions.Authentication.DefaultScopes!.Concat(additionalScopes));

            await provider.GetAuthorizationTokenAsync(new Uri("https://graph.microsoft.com"), null);
            capturedInput!.Scopes.Should().BeEquivalentTo(_terminalOptions.Authentication.DefaultScopes);
        }

        [Fact]
        public async Task GetAuthorizationTokenAsync_UsesFirstAccount_ForTokenAcquisition()
        {
            var account = new Mock<IAccount>().Object;
            var identity = new TokenAcquisitionIdentity { Raw = account };
            _msalTokenAcquisitionMock.Setup(x => x.GetIdentitiesAsync(null)).ReturnsAsync([identity]);

            TokenAcquisitionSilentInput? capturedInput = null;
            _msalTokenAcquisitionMock.Setup(x => x.AcquireTokenSilentAsync(It.IsAny<TokenAcquisitionSilentInput>()))
                .Callback<TokenAcquisitionSilentInput>(input => capturedInput = input)
                .ReturnsAsync(new TokenAcquisitionResult { AccessToken = "expected_token" });

            var provider = CreateProvider();

            await provider.GetAuthorizationTokenAsync(new Uri("https://graph.microsoft.com"), null);

            ReferenceEquals(capturedInput!.Identity!.Raw, account).Should().BeTrue();
        }

        private MsalKiotaAuthProvider CreateProvider()
        {
            return new MsalKiotaAuthProvider(_terminalOptions, _msalTokenAcquisitionMock.Object, _loggerMock.Object);
        }

        private void SetupMockTokenAcquisition(string token)
        {
            var account = new Mock<IAccount>().Object;
            var identity = new TokenAcquisitionIdentity { Raw = account };

            _msalTokenAcquisitionMock
                .Setup(x => x.GetIdentitiesAsync(null))
                .ReturnsAsync([identity]);

            _msalTokenAcquisitionMock
                .Setup(x => x.AcquireTokenSilentAsync(It.Is<TokenAcquisitionSilentInput>(input =>
                    input.Identity != null &&
                    input.Identity.Raw != null &&
                    input.Identity.Raw == account)))
                .ReturnsAsync(new TokenAcquisitionResult { AccessToken = token });
        }

        private void SetupMockTokenAcquisitionForNoAccounts()
        {
            _msalTokenAcquisitionMock.Setup(x => x.GetIdentitiesAsync(null)).ReturnsAsync([]);
            _msalTokenAcquisitionMock.Setup(x => x.AcquireTokenSilentAsync(It.IsAny<TokenAcquisitionSilentInput>()))
                .ThrowsAsync(new MsalUiRequiredException("test_error", "test_error_message"));
        }

        private void SetupMockTokenAcquisitionForUiRequiredException(MsalUiRequiredException exception)
        {
            var account = new Mock<IAccount>().Object;
            var identity = new TokenAcquisitionIdentity { Raw = account };
            _msalTokenAcquisitionMock.Setup(x => x.GetIdentitiesAsync(null)).ReturnsAsync([identity]);
            _msalTokenAcquisitionMock.Setup(x => x.AcquireTokenSilentAsync(It.IsAny<TokenAcquisitionSilentInput>()))
                .ThrowsAsync(exception);
        }

        private void SetupMockTokenInteractiveAcquisition(string token = "interactive_token")
        {
            _msalTokenAcquisitionMock.Setup(x => x.AcquireTokenInteractiveAsync(It.IsAny<TokenAcquisitionInteractiveInput>()))
                .ReturnsAsync(new TokenAcquisitionResult { AccessToken = token });
        }

        private readonly Mock<ILogger<MsalKiotaAuthProvider>> _loggerMock;
        private readonly Mock<ITokenAcquisition> _msalTokenAcquisitionMock;
        private readonly TerminalOptions _terminalOptions;
    }
}
