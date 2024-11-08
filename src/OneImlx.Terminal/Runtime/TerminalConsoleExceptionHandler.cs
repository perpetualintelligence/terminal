/*
    Copyright 2024 (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OneImlx.Shared.Infrastructure;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// The default <see cref="ITerminalExceptionHandler"/> to handle an <see cref="Exception"/> and log the error
    /// message to <see cref="ITerminalConsole"/>.
    /// </summary>
    public sealed class TerminalConsoleExceptionHandler : ITerminalExceptionHandler
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="terminalConsole"></param>
        public TerminalConsoleExceptionHandler(ITerminalConsole terminalConsole)
        {
            this.terminalConsole = terminalConsole;
        }

        /// <summary>
        /// Publish the <see cref="Exception"/> asynchronously to the logger.
        /// </summary>
        /// <param name="context">The error to publish.</param>
        /// <returns>The string representation.</returns>
        public Task HandleExceptionAsync(TerminalExceptionHandlerContext context)
        {
            if (context.Exception is TerminalException ee)
            {
                object[] args = ee.Error.Args != null ? ee.Error.Args.Select(e => e ?? "").ToArray() : [];
                terminalConsole.WriteLineAsync(ee.Error.ErrorDescription, args);
            }
            else if (context.Exception is MultiErrorException me)
            {
                foreach (Error err in me.Errors)
                {
                    object[] args = err.Args != null ? err.Args.Select(e => e ?? "").ToArray() : [];
                    terminalConsole.WriteLineAsync(err.ErrorDescription, args);
                }
            }
            else if (context.Exception is OperationCanceledException)
            {
                if (context.Request != null)
                {
                    terminalConsole.WriteLineAsync("The request was canceled. request={0} command={1}", context.Request.Id, context.Request.Raw);
                }
                else
                {
                    terminalConsole.WriteLineAsync("The request was canceled.");
                }
            }
            else
            {
                if (context.Request != null)
                {
                    terminalConsole.WriteLineAsync("The request failed. request={0} command={1} info={2}", context.Request.Id, context.Request.Raw, context.Exception.Message);
                }
                else
                {
                    terminalConsole.WriteLineAsync("The request failed.");
                }
            }

            return Task.CompletedTask;
        }

        private readonly ITerminalConsole terminalConsole;
    }
}
