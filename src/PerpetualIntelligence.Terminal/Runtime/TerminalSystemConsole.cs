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
    /// The default <see cref="ITerminalConsole"/> implementation that use system <see cref="Console"/>.
    /// </summary>
    public sealed class TerminalSystemConsole : ITerminalConsole
    {
        /// <summary>
        /// Return <c>true</c> if the specified string value is ignored by the <see cref="ITerminalConsole"/>, otherwise <c>false</c>.
        /// </summary>
        /// <param name="value">The value to check.</param>
        public bool Ignore(string? value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// Reads the next line of characters from the <see cref="Console"/> input stream asynchronously.
        /// </summary>
        /// <returns>The next line of characters from the input stream, or <c>null</c> if no more lines are available.</returns>
        public Task<string?> ReadLineAsync()
        {
            return Task.Run(() => { return (string?)Console.ReadLine(); });
        }

        /// <summary>
        /// Writes the specified string value to the <see cref="Console"/> standard output stream.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <param name="args">The format arguments.</param>
        public Task WriteAsync(string value, params object[] args)
        {
            return Task.Run(() => { Console.Write(value, args); });
        }

        /// <summary>
        /// Writes the specified string value in the foreground color to the <see cref="Console"/> standard output stream.
        /// </summary>
        /// <param name="foregroundColor">The foreground text color.</param>
        /// <param name="value">The text to write.</param>
        /// <param name="args">The format arguments.</param>
        /// <returns></returns>
        public Task WriteColorAsync(ConsoleColor foregroundColor, string value, params object[] args)
        {
            return Task.Run(() =>
            {
                try
                {
                    Console.ForegroundColor = foregroundColor;
                    Console.Write(value, args);
                }
                finally
                {
                    Console.ResetColor();
                }
            });
        }

        /// <summary>
        /// Writes the specified string value followed by the current newline terminator to the <see cref="Console"/> input stream asynchronously.
        /// </summary>
        /// <param name="value">The text to write.</param>
        /// <param name="args">The format arguments.</param>
        public Task WriteLineAsync(string value, params object[] args)
        {
            return Task.Run(() => { Console.WriteLine(value, args); });
        }

        /// <summary>
        /// Writes the current newline terminator to the <see cref="Console"/> input stream asynchronously.
        /// </summary>
        public Task WriteLineAsync()
        {
            return Task.Run(Console.WriteLine);
        }

        /// <summary>
        /// Writes the specified string value followed by the current newline terminator to the <see cref="Console"/> input stream asynchronously.
        /// </summary>
        /// <param name="foregroundColor">The foreground text color.</param>
        /// <param name="value">The text to write.</param>
        /// <param name="args">The format arguments.</param>
        public Task WriteLineColorAsync(ConsoleColor foregroundColor, string value, params object[] args)
        {
            return Task.Run(() =>
            {
                try
                {
                    Console.ForegroundColor = foregroundColor;
                    Console.WriteLine(value, args);
                }
                finally
                {
                    Console.ResetColor();
                }
            });
        }
    }
}