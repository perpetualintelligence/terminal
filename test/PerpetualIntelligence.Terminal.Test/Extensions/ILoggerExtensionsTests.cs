/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;

namespace PerpetualIntelligence.Terminal.Extensions
{
    [TestClass]
    public class ILoggerExtensionsTests : InitializerTests
    {
        public ILoggerExtensionsTests() : base(TestLogger.Create<ILoggerExtensionsTests>())
        {
        }

        [TestMethod]
        public void FormatAndLogDefaultShouldObsureArgumentsArguments()
        {
            LoggingOptions loggingOptions = new();

            Assert.IsNotNull(Logger);
            string? message = Logger.FormatAndLog(Microsoft.Extensions.Logging.LogLevel.Warning, loggingOptions, "Test message. client_id={0} scope={1} test={2}", "test_client_id", "test_scope", "test_value");
            Assert.AreEqual("Test message. client_id=**** scope=**** test=****", message);
        }

        [TestMethod]
        public void FormatAndLogShouldNotObsureArguments()
        {
            LoggingOptions loggingOptions = new()
            {
                ObsureInvalidOptions = false
            };

            Assert.IsNotNull(Logger);
            string? message = Logger.FormatAndLog(Microsoft.Extensions.Logging.LogLevel.Error, loggingOptions, "Test message. client_id={0} scope={1} test={2}", "test_client_id", "test_scope", "test_value");
            Assert.AreEqual("Test message. client_id=test_client_id scope=test_scope test=test_value", message);
        }

        [TestMethod]
        public void FormatAndLogShouldObsureArguments()
        {
            LoggingOptions loggingOptions = new()
            {
                ObsureInvalidOptions = true
            };

            Assert.IsNotNull(Logger);
            string? message = Logger.FormatAndLog(Microsoft.Extensions.Logging.LogLevel.Debug, loggingOptions, "Test message. client_id={0} scope={1} test={2}", "test_client_id", "test_scope", "test_value");
            Assert.AreEqual("Test message. client_id=**** scope=**** test=****", message);
        }
    }
}