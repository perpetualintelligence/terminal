/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OneImlx.Shared.Extensions;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// The default implementation of <see cref="ITerminalConsole"/> that uses the system <see cref="Console"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="TerminalSystemConsole"/> is thread safe and allows multiple tasks to write to the console concurrently.
    /// </remarks>
    public class TerminalSystemConsole : ITerminalConsole
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TerminalSystemConsole"/> class.
        /// </summary>
        public TerminalSystemConsole()
        {
            oneThread = new SemaphoreSlim(1, 1); // Initialize with a single permit
        }

        /// <summary>
        /// Gets or sets the background color of the console.
        /// </summary>
        public ConsoleColor BackgroundColor
        {
            get => Console.BackgroundColor;
            set => Console.BackgroundColor = value;
        }

        /// <summary>
        /// Gets or sets the foreground color of the console.
        /// </summary>
        public ConsoleColor ForegroundColor
        {
            get => Console.ForegroundColor;
            set => Console.ForegroundColor = value;
        }

        /// <summary>
        /// Clears the console buffer and display information asynchronously.
        /// </summary>
        public async Task ClearAsync()
        {
            await oneThread.WaitAsync();
            try
            {
                // Clear the console.
                Console.Clear();

                // 12/15/2024 10:26 PM
                // ChatGPT: Suggestion for clearing the scrollback
                Console.Write("\u001b[3J");

                // 12/15/2024 10:26 PM
                // ChatGPT: Suggestion to move cursor to the top-left
                Console.Write("\u001b[H");
            }
            finally
            {
                oneThread.Release();
            }
        }

        /// <summary>
        /// Determines whether the specified string value should be ignored by the <see cref="ITerminalConsole"/>.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns><c>true</c> if the value is null, empty, or whitespace; otherwise, <c>false</c>.</returns>
        public bool Ignore(string? value) => string.IsNullOrWhiteSpace(value);

        /// <summary>
        /// Prints a question to the console and waits for an answer asynchronously.
        /// </summary>
        /// <param name="question">The question to print. A <c>?</c> will be appended at the end.</param>
        /// <param name="answers">
        /// The allowed answers, or <c>null</c> if all answers are allowed. If specified, this method prints the answers
        /// in the format <c>{question} ({answer1}/{answer2}/{answer3})?</c>.
        /// </param>
        /// <returns>The user's answer, or <c>null</c> if canceled.</returns>
        public async Task<string> ReadAnswerAsync(string question, params string[]? answers)
        {
            await oneThread.WaitAsync();
            try
            {
                Console.Write(answers != null
                    ? $"{question} ({string.Join("/", answers)})? "
                    : $"{question}? ");

                return Console.ReadLine();
            }
            finally
            {
                oneThread.Release();
            }
        }

        /// <summary>
        /// Reads the next line of input from the console asynchronously.
        /// </summary>
        /// <returns>The next line of input, or <c>null</c> if no more lines are available.</returns>
        public async Task<string?> ReadLineAsync()
        {
            await oneThread.WaitAsync();
            try
            {
                return Console.ReadLine();
            }
            finally
            {
                oneThread.Release();
            }
        }

        /// <summary>
        /// Writes a formatted string to the console asynchronously.
        /// </summary>
        /// <param name="value">The string to write.</param>
        /// <param name="args">The format arguments.</param>
        public async Task WriteAsync(string value, params object[] args)
        {
            await oneThread.WaitAsync();
            try
            {
                Console.Write(value, args);
            }
            finally
            {
                oneThread.Release();
            }
        }

        /// <summary>
        /// Writes a formatted string to the console in the specified foreground color asynchronously.
        /// </summary>
        /// <param name="foregroundColor">The foreground color.</param>
        /// <param name="value">The string to write.</param>
        /// <param name="args">The format arguments.</param>
        public async Task WriteColorAsync(ConsoleColor foregroundColor, string value, params object[] args)
        {
            await oneThread.WaitAsync();
            try
            {
                Console.ForegroundColor = foregroundColor;
                Console.Write(value, args);
            }
            finally
            {
                Console.ResetColor();
                oneThread.Release();
            }
        }

        /// <summary>
        /// Writes a formatted string followed by a newline to the console asynchronously.
        /// </summary>
        /// <param name="value">The string to write.</param>
        /// <param name="args">The format arguments.</param>
        public async Task WriteLineAsync(string value, params object[] args)
        {
            await oneThread.WaitAsync();
            try
            {
                Console.WriteLine(value, args);
            }
            finally
            {
                oneThread.Release();
            }
        }

        /// <summary>
        /// Writes a newline to the console asynchronously.
        /// </summary>
        public async Task WriteLineAsync()
        {
            await oneThread.WaitAsync();
            try
            {
                Console.WriteLine();
            }
            finally
            {
                oneThread.Release();
            }
        }

        /// <summary>
        /// Writes a formatted string followed by a newline in the specified foreground color to the console asynchronously.
        /// </summary>
        /// <param name="foregroundColor">The foreground color.</param>
        /// <param name="value">The string to write.</param>
        /// <param name="args">The format arguments.</param>
        public async Task WriteLineColorAsync(ConsoleColor foregroundColor, string value, params object[] args)
        {
            await oneThread.WaitAsync();
            try
            {
                Console.ForegroundColor = foregroundColor;
                Console.WriteLine(value, args);
            }
            finally
            {
                Console.ResetColor();
                oneThread.Release();
            }
        }

        private readonly SemaphoreSlim oneThread;
    }
}
