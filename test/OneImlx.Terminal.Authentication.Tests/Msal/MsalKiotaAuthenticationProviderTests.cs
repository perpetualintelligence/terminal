/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.Kiota.Abstractions;
using FluentAssertions;
using Moq;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Shared;
using OneImlx.Test.FluentAssertions;
using Xunit;

namespace OneImlx.Terminal.Authentication.Msal
{
    public class MsalKiotaAuthenticationProviderTests
    {
        public MsalKiotaAuthenticationProviderTests()
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
        public async Task AuthenticateRequestAsync_AuthenticatesInteractive_ForNoAccounts()
        {
            SetupMockTokenAcquisitionForNoAccounts();
            string expectedToken = "interactive_token_no_accounts";
            SetupMockTokenInteractiveAcquisition(expectedToken);

            var provider = CreateProvider();
            var requestInfo = new RequestInformation { URI = new Uri("https://graph.microsoft.com") };

            await provider.AuthenticateRequestAsync(requestInfo);

            requestInfo.Headers["Authorization"].Should().Contain($"Bearer {expectedToken}");
        }

        [Fact]
        public async Task AuthenticateRequestAsync_ShouldAcquireTokenInteractively_WhenSilentFails()
        {
            var exception = new MsalUiRequiredException("error_code", "A problem occurred.");
            SetupMockTokenAcquisitionForUiRequiredException(exception);

            string expectedToken = "interactive_token";
            SetupMockTokenInteractiveAcquisition(expectedToken);

            var provider = CreateProvider();
            var requestInfo = new RequestInformation { URI = new Uri("https://graph.microsoft.com") };

            await provider.AuthenticateRequestAsync(requestInfo);

            requestInfo.Headers["Authorization"].Should().Contain($"Bearer {expectedToken}");
        }

        [Fact]
        public async Task AuthenticateRequestAsync_ShouldAcquireTokenSilently_WhenAccountExists()
        {
            string expectedToken = "expected_token";
            SetupMockTokenAcquisition(expectedToken);
            var provider = CreateProvider();

            var requestInfo = new RequestInformation { URI = new Uri("https://graph.microsoft.com") };

            await provider.AuthenticateRequestAsync(requestInfo);

            requestInfo.Headers["Authorization"].Should().Contain($"Bearer {expectedToken}");
        }

        [Fact]
        public async Task AuthenticateRequestAsync_Throws_If_Authentication_Is_Not_Enabled()
        {
            _terminalOptions.Authentication.Provider = "none";
            var requestInfo = new RequestInformation { URI = new Uri("https://graph.microsoft.com") };

            var provider = CreateProvider();
            Func<Task> func = async () => await provider.AuthenticateRequestAsync(requestInfo);
            await func.Should().ThrowAsync<TerminalException>()
                .WithErrorCode(TerminalErrors.InvalidConfiguration)
                .WithErrorDescription("The terminal MSAL authentication is not enabled.");
        }

        [Fact]
        public async Task AuthenticateRequestAsync_UsesCorrectScopes_ForTokenAcquisition()
        {
            var account = new Mock<IAccount>().Object;
            var identity = new TokenAcquisitionIdentity { Raw = account };
            _msalTokenAcquisitionMock.Setup(x => x.GetIdentitiesAsync(null)).ReturnsAsync([identity]);

            TokenAcquisitionSilentInput? capturedInput = null;
            _msalTokenAcquisitionMock.Setup(x => x.AcquireTokenSilentAsync(It.IsAny<TokenAcquisitionSilentInput>()))
                .Callback<TokenAcquisitionSilentInput>(input => capturedInput = input)
                .ReturnsAsync(new TokenAcquisitionResult { AccessToken = "expected_token" });

            var provider = CreateProvider();
            var requestInfo = new RequestInformation { URI = new Uri("https://graph.microsoft.com") };
            var additionalScopes = new[] { "email", "openid", "profile" };
            var additionalAuthenticationContext = new Dictionary<string, object> { { "scopes", additionalScopes } };

            await provider.AuthenticateRequestAsync(requestInfo, additionalAuthenticationContext);
            capturedInput!.Scopes.Should().BeEquivalentTo(_terminalOptions.Authentication.DefaultScopes!.Concat(additionalScopes));

            await provider.AuthenticateRequestAsync(requestInfo);
            capturedInput!.Scopes.Should().BeEquivalentTo(_terminalOptions.Authentication.DefaultScopes);
        }

        [Fact]
        public async Task AuthenticateRequestAsync_UsesFirstAccount_ForTokenAcquisition()
        {
            var account1 = new Mock<IAccount>().Object;
            var account2 = new Mock<IAccount>().Object;

            var identity = new TokenAcquisitionIdentity { Raw = account1 };
            _msalTokenAcquisitionMock.Setup(x => x.GetIdentitiesAsync(null)).ReturnsAsync([identity]);

            TokenAcquisitionSilentInput? capturedInput = null;
            _msalTokenAcquisitionMock.Setup(x => x.AcquireTokenSilentAsync(It.IsAny<TokenAcquisitionSilentInput>()))
                .Callback<TokenAcquisitionSilentInput>(input => capturedInput = input)
                .ReturnsAsync(new TokenAcquisitionResult { AccessToken = "expected_token" });

            var provider = CreateProvider();
            var requestInfo = new RequestInformation { URI = new Uri("https://graph.microsoft.com") };

            await provider.AuthenticateRequestAsync(requestInfo);

            capturedInput!.Identity!.Raw.Should().Be(account1);
        }

        private MsalKiotaAuthProvider CreateProvider()
        {
            return new MsalKiotaAuthProvider(_terminalOptions, _msalTokenAcquisitionMock.Object, _loggerMock.Object);
        }

        private void SetupMockTokenAcquisition(string token)
        {
            var account = new Mock<IAccount>().Object;
            var identity = new TokenAcquisitionIdentity { Raw = account };

            // With the following fix:
            _msalTokenAcquisitionMock
                .Setup(x => x.AcquireTokenSilentAsync(It.Is<TokenAcquisitionSilentInput>(
                    i => i.Identity != null && ReferenceEquals(i.Identity.Raw, account))))
                .ReturnsAsync(new TokenAcquisitionResult
                {
                    AccessToken = token
                });

            _msalTokenAcquisitionMock
                .Setup(x => x.GetIdentitiesAsync(null))
                .ReturnsAsync([identity]);
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
                .ReturnsAsync(new TokenAcquisitionResult
                {
                    AccessToken = token
                });
        }

        private readonly Mock<ILogger<MsalKiotaAuthProvider>> _loggerMock;
        private readonly Mock<ITokenAcquisition> _msalTokenAcquisitionMock;
        private readonly TerminalOptions _terminalOptions;
    }
}
