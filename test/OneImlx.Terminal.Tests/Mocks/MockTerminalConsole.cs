/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Runtime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Mocks
{
    public class MockTerminalConsole : ITerminalConsole
    {
        public ConsoleColor ForegroundColor { get; set; }
        public ConsoleColor BackgroundColor { get; set; }

        public List<string> Messages { get; }

        public MockTerminalConsole()
        {
            Messages = [];
        }

        public Task ClearAsync()
        {
            // Optionally, handle the clear operation by clearing the history or by marking a clear point
            Messages.Add("[Console Cleared]");
            return Task.CompletedTask;
        }

        public bool Ignore(string? value)
        {
            // Implement logic to ignore specific values if necessary
            // For simplicity, this example does not ignore any values
            return false;
        }

        public Task<string?> ReadLineAsync()
        {
            // Simulate reading a line by returning a predetermined or mock value
            string mockInput = "mock input";
            Messages.Add($"ReadLine: {mockInput}");
            return Task.FromResult<string?>(mockInput);
        }

        public Task<string> ReadAnswerAsync(string question, params string[]? answers)
        {
            // Simulate reading an answer by returning a predetermined or mock value
            string mockAnswer = "mock answer";
            Messages.Add($"Question: {question}, Answer: {mockAnswer}");
            return Task.FromResult(mockAnswer);
        }

        public Task WriteLineAsync()
        {
            // Simulate writing an empty line
            Messages.Add("");
            return Task.CompletedTask;
        }

        public Task WriteLineAsync(string value, params object[] args)
        {
            // Simulate writing a line with formatting
            string formattedValue = string.Format(value, args);
            Messages.Add(formattedValue);
            return Task.CompletedTask;
        }

        public Task WriteLineColorAsync(ConsoleColor foregroundColor, string value, params object[] args)
        {
            // Simulate writing a line with color and formatting
            string formattedValue = string.Format(value, args);
            Messages.Add($"[Color: {foregroundColor}] {formattedValue}");
            return Task.CompletedTask;
        }

        public Task WriteColorAsync(ConsoleColor foregroundColor, string value, params object[] args)
        {
            // Simulate writing with color and formatting, without a newline
            string formattedValue = string.Format(value, args);
            Messages.Add($"[Color: {foregroundColor}] {formattedValue}");
            return Task.CompletedTask;
        }

        public Task WriteAsync(string value, params object[] args)
        {
            // Simulate writing without a newline
            string formattedValue = string.Format(value, args);
            Messages.Add(formattedValue);
            return Task.CompletedTask;
        }

        // Optionally, provide a method to retrieve the console history for verification or debugging
        public List<string> GetConsoleHistory()
        {
            return Messages;
        }
    }
}