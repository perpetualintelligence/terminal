/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands.Runners;

namespace OneImlx.Terminal.Apps.TestServer.Runners
{
    /// <summary>
    /// A custom command runner result that sends the command to a file based on the sender_endpoint.
    /// </summary>
    public class CommandRunnerResultSendToFile : CommandRunnerResult
    {
        /// <summary>
        /// Processes the command and writes it to a file based on the sender_endpoint.
        /// </summary>
        /// <param name="context">The command runner context containing the command and properties.</param>
        /// <param name="logger">An optional logger instance for logging.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override async Task ProcessAsync(CommandRunnerContext context, ILogger? logger = null)
        {
            // Retrieve properties from context
            if (context.HandlerContext.RouterContext.Properties != null)
            {
                Dictionary<string, object> properties = context.HandlerContext.RouterContext.Properties!;
                string senderEndpoint = (string)properties[TerminalIdentifiers.SenderEndpointToken];

                // Normalize the sender_endpoint to replace ':' and '.' with '_'
                string normalizedSenderEndpoint = senderEndpoint.ToString()!.Replace(':', '_').Replace('.', '_').Replace('[', '_').Replace(']', '_');

                // Create a file name based on the normalized sender_endpoint
                string fileName = $"{normalizedSenderEndpoint}.txt";

                // Retrieve the command string from context
                string commandString = context.HandlerContext.RouterContext.Request.Raw ?? string.Empty;

                // Append the command string to the file in a thread-safe manner
                await Semaphore.WaitAsync();
                try
                {
                    await File.AppendAllTextAsync(fileName, commandString + Environment.NewLine);
                }
                finally
                {
                    Semaphore.Release();
                }
            }
        }

        private static readonly SemaphoreSlim Semaphore = new(1, 1);
    }
}
