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
        /// Writes the current newline terminator to the <see cref="ITerminalConsole"/> input stream asynchronously.
        /// </summary>
        public Task WriteLineAsync();

        /// <summary>
        /// Writes the specified string value followed by the current newline terminator to the <see cref="ITerminalConsole"/> standard output stream.
        /// </summary>
        /// <param name="value">The text to write.</param>
        /// <param name="args">The format arguments.</param>
        public Task WriteLineAsync(string value, params object[] args);

        /// <summary>
        /// Writes the specified string value in the foreground color followed by the current newline terminator to the <see cref="ITerminalConsole"/> standard output stream.
        /// </summary>
        /// <param name="foregroundColor">The foreground text color.</param>
        /// <param name="value">The text to write.</param>
        /// <param name="args">The format arguments.</param>
        public Task WriteLineColorAsync(ConsoleColor foregroundColor, string value, params object[] args);

        /// <summary>
        /// Writes the specified string value in the foreground color to the <see cref="ITerminalConsole"/> standard output stream.
        /// </summary>
        /// <param name="foregroundColor">The foreground text color.</param
        /// <param name="value">The text to write.</param>
        /// <param name="args">The format arguments.</param>
        public Task WriteColorAsync(ConsoleColor foregroundColor, string value, params object[] args);

        /// <summary>
        /// Writes the specified string value to the <see cref="ITerminalConsole"/> standard output stream.
        /// </summary>
        /// <param name="value">The text to write.</param>
        /// <param name="args">The format arguments.</param>
        public Task WriteAsync(string value, params object[] args);

        /// <summary>
        /// Return <c>true</c> if the specified string value is ignored by the <see cref="ITerminalConsole"/>, otherwise <c>false</c>.
        /// </summary>
        /// <param name="value">The value to check.</param>
        public bool Ignore(string? value);
    }
}