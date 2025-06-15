/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Linq;
using System.Threading.Tasks;
using OneImlx.Shared.Infrastructure;
using OneImlx.Terminal.Shared;

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
                object[] args = ee.Error.Args != null ? ee.Error.Args.Select(static e => e ?? "").ToArray() : [];
                terminalConsole.WriteLineColorAsync(ConsoleColor.Red, ee.Error.ErrorDescription, args);
            }
            else if (context.Exception is OperationCanceledException)
            {
                if (context.Request != null)
                {
                    terminalConsole.WriteLineColorAsync(ConsoleColor.Red, "The terminal request was canceled. request={0} command={1}", context.Request.Id, context.Request.Raw);
                }
                else
                {
                    terminalConsole.WriteLineColorAsync(ConsoleColor.Red, "The terminal request was canceled.");
                }
            }
            else
            {
                if (context.Request != null)
                {
                    terminalConsole.WriteLineColorAsync(ConsoleColor.Red, "The terminal request failed. request={0} command={1} info={2}", context.Request.Id, context.Request.Raw, context.Exception.Message);
                }
                else
                {
                    terminalConsole.WriteLineColorAsync(ConsoleColor.Red, "The terminal request failed. info={0}", context.Exception.Message);
                }
            }

            return Task.CompletedTask;
        }

        private readonly ITerminalConsole terminalConsole;
    }
}
