/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace PerpetualIntelligence.Cli.Mocks
{
    public class MockLoggerFactory : ILoggerFactory
    {
        public StringWriter? StringWriter { get; set; }

        public void AddProvider(ILoggerProvider provider)
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            Assert.IsNotNull(StringWriter);
            var logger = new MockStringWriterLogger(StringWriter);
            return logger;
        }

        public void Dispose()
        {
        }
    }
}
