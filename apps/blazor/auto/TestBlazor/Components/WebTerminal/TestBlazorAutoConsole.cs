using OneImlx.Terminal.Runtime;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Apps.TestBlazor.Components.WebTerminal
{
    /// <summary>
    /// Represents a test Blazor server console. This is a test implementation, you can customize this as per your
    /// application requirement.
    /// </summary>
    public class TestBlazorAutoConsole : ITerminalConsole
    {
        /// <summary>
        /// Gets or sets the background color of the console.
        /// </summary>
        public ConsoleColor BackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the foreground color of the console.
        /// </summary>
        public ConsoleColor ForegroundColor { get; set; }

        /// <summary>
        /// Asynchronously clears the console.
        /// </summary>
        /// <returns>A task representing the clear operation.</returns>
        public Task ClearAsync()
        {
            _outputWriter.GetStringBuilder().Clear();
            _inputTaskSource = new TaskCompletionSource<string?>();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Determines whether the specified value should be ignored.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns><c>true</c> if the value should be ignored; otherwise, <c>false</c>.</returns>
        public bool Ignore(string? value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// Reads an answer asynchronously from the console.
        /// </summary>
        /// <param name="question">The question to ask.</param>
        /// <param name="answers">The possible answers.</param>
        /// <returns>A task representing the read operation.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<string> ReadAnswerAsync(string question, params string[]? answers)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads a line asynchronously from the console.
        /// </summary>
        /// <returns>A task representing the read operation.</returns>
        public async Task<string?> ReadLineAsync()
        {
            // Await directly on the TaskCompletionSource that will be set with input
            string? result = await _inputTaskSource.Task;

            // Reset for next input
            _inputTaskSource = new TaskCompletionSource<string?>();

            return result;
        }

        /// <summary>
        /// Writes a formatted string asynchronously to the console.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <param name="args">The formatting arguments.</param>
        /// <returns>A task representing the write operation.</returns>
        public Task WriteAsync(string value, params object[] args)
        {
            _outputWriter.Write(string.Format(value, args));
            return Task.CompletedTask;
        }

        /// <summary>
        /// Writes a formatted string with specified foreground color asynchronously to the console.
        /// </summary>
        /// <param name="foregroundColor">The foreground color.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="args">The formatting arguments.</param>
        /// <returns>A task representing the write operation.</returns>
        public Task WriteColorAsync(ConsoleColor foregroundColor, string value, params object[] args)
        {
            _outputWriter.Write(string.Format(value, args));
            return Task.CompletedTask;
        }

        /// <summary>
        /// Writes a formatted string followed by a line terminator asynchronously to the console.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <param name="args">The formatting arguments.</param>
        /// <returns>A task representing the write operation.</returns>
        public Task WriteLineAsync(string value, params object[] args)
        {
            _outputWriter.WriteLine(string.Format(value, args));
            return Task.CompletedTask;
        }

        /// <summary>
        /// Writes a line terminator asynchronously to the console.
        /// </summary>
        /// <returns>A task representing the write operation.</returns>
        public Task WriteLineAsync()
        {
            _outputWriter.WriteLine();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Writes a formatted string with specified foreground color followed by a line terminator asynchronously to
        /// the console.
        /// </summary>
        /// <param name="foregroundColor">The foreground color.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="args">The formatting arguments.</param>
        /// <returns>A task representing the write operation.</returns>
        public Task WriteLineColorAsync(ConsoleColor foregroundColor, string value, params object[] args)
        {
            _outputWriter.WriteLine(string.Format(value, args));
            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns the <see cref="StringBuilder"/> representing the current console output.
        /// </summary>
        /// <returns>The console output as a <see cref="StringBuilder"/>.</returns>
        internal StringBuilder GetConsoleOutput()
        {
            return _outputWriter.GetStringBuilder();
        }

        /// <summary>
        /// Notifies the console that user input is received.
        /// </summary>
        /// <param name="input">The user input.</param>
        internal void SetConsoleInput(string input)
        {
            _inputTaskSource.SetResult(input);
        }

        private TaskCompletionSource<string?> _inputTaskSource = new();
        private readonly StringWriter _outputWriter = new();
    }
}
