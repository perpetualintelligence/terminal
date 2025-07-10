/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Identity.Client;
using Microsoft.Kiota.Abstractions.Authentication;
using Moq;
using OneImlx.Terminal.Authentication.Msal;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Hosting;
using Xunit;

namespace OneImlx.Terminal.Authentication.Extensions
{
    public class ITerminalBuilderExtensionsTests
    {
        [Fact]
        public void AddMsalAuthentication_RegistersRequiredServicesWithCorrectLifetimes()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.AddSingleton(new TerminalOptions());

            var builder = new Mock<ITerminalBuilder>();
            builder.Setup(static b => b.Services).Returns(services);
            var publicClientApplication = Mock.Of<IPublicClientApplication>();

            ITerminalBuilderExtensions.AddMsalAuthentication<MsalKiotaAuthProvider, MsalKiotaAuthProvider, TestHandler>(builder.Object, publicClientApplication);

            var serviceProvider = services.BuildServiceProvider();

            serviceProvider.GetService<IPublicClientApplication>().Should().NotBeNull();
            serviceProvider.GetService<ITokenAcquisition>().Should().NotBeNull();
            serviceProvider.GetService<IAuthenticationProvider>().Should().BeOfType<MsalKiotaAuthProvider>();
            serviceProvider.GetService<IAccessTokenProvider>().Should().BeOfType<MsalKiotaAuthProvider>();
            serviceProvider.GetService<TestHandler>().Should().NotBeNull();

            services.Should().Contain(static descriptor =>
                descriptor.ServiceType == typeof(IPublicClientApplication) && descriptor.Lifetime == ServiceLifetime.Singleton)
                .And.Contain(static descriptor =>
                descriptor.ServiceType == typeof(ITokenAcquisition) && descriptor.Lifetime == ServiceLifetime.Scoped)
                .And.Contain(static descriptor =>
                descriptor.ServiceType == typeof(IAuthenticationProvider) && descriptor.Lifetime == ServiceLifetime.Scoped)
                .And.Contain(static descriptor =>
                descriptor.ServiceType == typeof(IAccessTokenProvider) && descriptor.Lifetime == ServiceLifetime.Scoped)
                .And.Contain(static descriptor =>
                descriptor.ServiceType == typeof(TestHandler) && descriptor.Lifetime == ServiceLifetime.Scoped);
        }
    }
}