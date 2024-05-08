/*
    Copyright 2024 (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using OneImlx.Terminal.Runtime;

public class TerminalWasmConsole : ITerminalConsole
{
    public TerminalWasmConsole()
    {
        Out = _outputWriter;
    }

    public ConsoleColor BackgroundColor { get; set; }

    public ConsoleColor ForegroundColor { get; set; }

    public TextReader In { get; }

    // Not actively used unless required for compatibility
    public TextWriter Out { get; private set; }

    public Task ClearAsync()
    {
        _outputWriter.GetStringBuilder().Clear();
        _inputTaskSource = new TaskCompletionSource<string?>();
        return Task.CompletedTask;
    }

    public bool Ignore(string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    public Task<string> ReadAnswerAsync(string question, params string[]? answers)
    {
        throw new NotImplementedException();
    }

    public async Task<string?> ReadLineAsync()
    {
        // Await directly on the TaskCompletionSource that will be set with input
        string? result = await _inputTaskSource.Task;

        // Reset for next input
        _inputTaskSource = new TaskCompletionSource<string?>();

        return result;
    }

    // No additional methods exposed; handling of input must be done internally or by design
    public void SimulateInput(string input)
    {
        // Set the result of the task source when input is received
        _inputTaskSource.SetResult(input);
    }

    public Task WriteAsync(string value, params object[] args)
    {
        _outputWriter.Write(string.Format(value, args));
        return Task.CompletedTask;
    }

    public Task WriteColorAsync(ConsoleColor foregroundColor, string value, params object[] args)
    {
        throw new NotImplementedException();
    }

    public Task WriteLineAsync(string value, params object[] args)
    {
        _outputWriter.WriteLine(string.Format(value, args));
        return Task.CompletedTask;
    }

    public Task WriteLineAsync()
    {
        _outputWriter.WriteLine();
        return Task.CompletedTask;
    }

    public Task WriteLineColorAsync(ConsoleColor foregroundColor, string value, params object[] args)
    {
        throw new NotImplementedException();
    }

    private TaskCompletionSource<string?> _inputTaskSource = new TaskCompletionSource<string?>();
    private StringWriter _outputWriter = new ();
}
