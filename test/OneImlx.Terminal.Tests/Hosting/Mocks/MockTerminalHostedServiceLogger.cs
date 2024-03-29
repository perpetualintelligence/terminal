﻿/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace OneImlx.Terminal.Hosting.Mocks
{
    internal class MockTerminalHostedServiceLogger : ILogger<TerminalHostedService>
    {
        public MockTerminalHostedServiceLogger()
        {
            Messages = new List<string>();
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            throw new NotImplementedException();
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter.Invoke(state, exception));
        }

        public List<string> Messages { get; private set; }
    }
}