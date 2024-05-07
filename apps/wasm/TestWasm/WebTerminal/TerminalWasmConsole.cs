using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using OneImlx.Terminal.Runtime;

public class TerminalWasmConsole : ITerminalConsole
{
    private StringBuilder _outputBuffer = new StringBuilder();
    private StringBuilder _inputBuffer = new StringBuilder();
    private StringWriter _outputWriter;
    private StringReader _inputReader;

    public TerminalWasmConsole()
    {
        _outputWriter = new StringWriter(_outputBuffer);
        _inputReader = new StringReader(_inputBuffer.ToString());
    }

    public ConsoleColor BackgroundColor { get; set; }
    public ConsoleColor ForegroundColor { get; set; }

    public TextWriter Out => _outputWriter;
    public TextReader In => _inputReader;

    public Task ClearAsync()
    {
        _outputBuffer.Clear();
        _inputBuffer.Clear();
        // Reset the output and input streams to reflect cleared buffers
        _outputWriter = new StringWriter(_outputBuffer);
        _inputReader = new StringReader(_inputBuffer.ToString());
        return Task.CompletedTask;
    }

    public bool Ignore(string? value)
    {
        return value != null && value.StartsWith("IGNORE");
    }

    public Task<string> ReadAnswerAsync(string question, params string[]? answers)
    {
        throw new NotImplementedException();
    }

    public Task<string?> ReadLineAsync()
    {
        return _inputReader.ReadLineAsync();
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
}
