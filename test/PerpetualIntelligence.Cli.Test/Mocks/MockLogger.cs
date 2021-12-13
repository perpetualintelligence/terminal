/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace PerpetualIntelligence.Cli.Mocks
{
    public class MockLogger : ILogger
    {
        public MockLogger(StringWriter output)
        {
            Output = output;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Output.Write(state?.ToString());
        }

        private StringWriter Output { get; set; }
    }
}
