/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Runtime;
using System;
using System.Collections.Generic;

namespace PerpetualIntelligence.Cli.Mocks
{
    internal class MockTerminalLogger : TerminalLogger
    {
        public MockTerminalLogger(string categoryName)
        {
        }

        public override IDisposable? BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public override bool IsEnabled(LogLevel logLevel)
        {
            throw new NotImplementedException();
        }

        public override void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            StaticMessages.Add(formatter.Invoke(state, exception));
        }

        public static List<string> StaticMessages { get; private set; } = new List<string>();
    }
}