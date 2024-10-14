/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace OneImlx.Terminal.Mocks
{
    public class MockListLoggerFactory : ILoggerFactory
    {
        public List<string> AllLogMessages { get; } = [];

        public void AddProvider(ILoggerProvider provider)
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new MockListLogger(AllLogMessages));
        }

        public void Dispose()
        {
            _loggers.Clear();
        }

        private readonly ConcurrentDictionary<string, MockListLogger> _loggers = new();
    }
}
