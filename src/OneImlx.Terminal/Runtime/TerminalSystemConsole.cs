/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Shared.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// The default <see cref="ITerminalConsole"/> implementation that use system <see cref="Console"/>.
    /// </summary>
    public class TerminalSystemConsole : ITerminalConsole
    {
        /// <summary>
        /// The foreground color.
        /// </summary>
        public ConsoleColor ForegroundColor
        {
            get
            {
                return Console.ForegroundColor;
            }
            set
            {
                Console.ForegroundColor = value;
            }
        }

        /// <summary>
        /// The background color.
        /// </summary>
        public ConsoleColor BackgroundColor
        {
            get
            {
                return Console.BackgroundColor;
            }
            set
            {
                Console.BackgroundColor = value;
            }
        }

        /// <summary>
        /// Clears the <see cref="Console"/> buffer and the corresponding display information.
        /// </summary>
        public Task ClearAsync()
        {
            return Task.Run(Console.Clear);
        }

        /// <summary>
        /// Return <c>true</c> if the specified string value is ignored by the <see cref="ITerminalConsole"/>, otherwise <c>false</c>.
        /// </summary>
        /// <param name="value">The value to check.</param>
        public bool Ignore(string? value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// Prints the question to the <see cref="ITerminalConsole"/> standard output stream and waits for an answer asynchronously.
        /// </summary>
        /// <param name="question">The question to print. The <c>?</c> will be appended at the end.</param>
        /// <param name="answers">
        /// The allowed answers or <c>null</c> if all answers are allowed. It is recommended to keep the answers short
        /// for readability. If specified this method will print the answers with question in the format <c>{question} {answer1}/{answer2}/{answer3}?</c>
        /// </param>
        /// <returns>The answer for the question or <c>null</c> if canceled.</returns>
        public virtual async Task<string> ReadAnswerAsync(string question, params string[]? answers)
        {
            // Print the question
            if (answers != null)
            {
                Console.Write($"{question} ({string.Join("/", answers)})? ");
            }
            else
            {
                Console.Write($"{question}? ");
            }

            // Check answer
            string? answer = Console.ReadLine();
            if (answers != null)
            {
                if (answers.Contains(answer))
                {
                    return answer;
                }
                else
                {
                    Console.WriteLine($"The answer is not valid. answers={answers.JoinBySpace()}");
                    return await ReadAnswerAsync(question, answers);
                }
            }
            else
            {
                return answer;
            }
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