/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace PerpetualIntelligence.Cli.Mocks
{
    public class MockLoggerFactory : ILoggerFactory
    {
        public StringWriter? StringWriter { get; set; }

        public void AddProvider(ILoggerProvider provider)
        {
            throw new NotImplementedException();
        }

        public ILogger CreateLogger(string categoryName)
        {
            Assert.IsNotNull(StringWriter);
            var logger = new MockLogger(StringWriter);
            return logger;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
