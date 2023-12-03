/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/
/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PerpetualIntelligence.Terminal.Mocks
{
    public class MockListLoggerFactory : ILoggerFactory
    {
        public List<string> AllLogMessages { get; } = new List<string>();

        private readonly ConcurrentDictionary<string, MockListLogger> _loggers = new ConcurrentDictionary<string, MockListLogger>();

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
    }
}