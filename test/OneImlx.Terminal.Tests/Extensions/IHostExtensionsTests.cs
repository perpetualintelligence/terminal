/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using OneImlx.Terminal.Runtime;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Extensions
{
    public class IHostExtensionsTests
    {
        [Fact]
        public async Task RunTerminalRouterAsync_Should_Run_TerminalRouter_And_Log_Start_And_End()
        {
            // Arrange
            var mockHost = new Mock<IHost>();
            var mockLogger = new Mock<ILogger<ITerminalRouter<TerminalRouterContext>>>();
            var mockTerminalRouter = new Mock<ITerminalRouter<TerminalRouterContext>>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var terminalRouterContext = new TerminalConsoleRouterContext(new TerminalStartContext(TerminalStartMode.Custom, CancellationToken.None, CancellationToken.None, null));

            // Set up the service provider to return the logger and terminal router when requested
            mockServiceProvider.Setup(x => x.GetService(typeof(ILogger<ITerminalRouter<TerminalRouterContext>>)))
                .Returns(mockLogger.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(ITerminalRouter<TerminalRouterContext>)))
                .Returns(mockTerminalRouter.Object);

            // Set up the host to return the mock service provider
            mockHost.Setup(x => x.Services).Returns(mockServiceProvider.Object);

            // Act
            await mockHost.Object.RunTerminalRouterAsync<ITerminalRouter<TerminalRouterContext>, TerminalRouterContext>(terminalRouterContext);

            // Verify
            mockTerminalRouter.Verify(x => x.RunAsync(terminalRouterContext), Times.Once);
        }
    }
}
