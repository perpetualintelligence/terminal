/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Runtime
{
    /// <summary>
    /// An abstraction of a console for <c>pi-cli</c> terminal framework.
    /// </summary>
    /// <remarks>
    /// <see cref="ITerminalConsole"/> allows application to define a standard or a custom console environment.
    /// </remarks>
    /// <see cref="ConsoleRouting"/>
    /// <seealso cref="TerminalSystemConsole"/>
    public interface ITerminalConsole
    {
        /// <summary>
        /// Reads the next line of characters from the <see cref="ITerminalConsole"/> input stream asynchronously.
        /// </summary>
        /// <returns>The next line of characters from the input stream, or <c>null</c> if no more lines are available.</returns>
        public Task<string?> ReadLineAsync();

        /// <summary>
        /// Reads the next line of characters to the <see cref="ITerminalConsole"/> input stream asynchronously.
        /// </summary>
        /// <returns>The next line of characters from the input stream, or <c>null</c> if no more lines are available.</returns>
        public Task WriteLineAsync();

        /// <summary>
        /// Reads the next line of characters to the <see cref="ITerminalConsole"/> input stream asynchronously.
        /// </summary>
        /// <returns>The next line of characters from the input stream, or <c>null</c> if no more lines are available.</returns>
        public Task WriteLineAsync(string value, params object[] args);

        /// <summary>
        /// Reads the next line of characters to the <see cref="ITerminalConsole"/> input stream asynchronously.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="value"></param>
        /// <param name="args"></param>
        /// <returns>The next line of characters from the input stream, or <c>null</c> if no more lines are available.</returns>
        public Task WriteLineColorAsync(ConsoleColor color, string value, params object[] args);

        /// <summary>
        /// Writes the specified string value to the <see cref="ITerminalConsole"/> standard output stream.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <param name="args"></param>
        public Task WriteAsync(string value, params object[] args);

        /// <summary>
        /// Return <c>true</c> if the specified string value is ignored by the <see cref="ITerminalConsole"/>, otherwise <c>false</c>.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns></returns>
        public bool Ignore(string? value);
    }
}