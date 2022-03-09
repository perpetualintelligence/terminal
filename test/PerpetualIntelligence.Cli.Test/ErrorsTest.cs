/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Test.Services;

namespace PerpetualIntelligence.Cli
{
    [TestClass]
    public class ErrorsTest
    {
        [TestMethod]
        public void AssertErrorsAreValid()
        {
            TestHelper.AssertConstantCount(typeof(Errors), 11);

            Assert.AreEqual("invalid_command", Errors.InvalidCommand);
            Assert.AreEqual("invalid_configuration", Errors.InvalidConfiguration);
            Assert.AreEqual("invalid_argument", Errors.InvalidArgument);
            Assert.AreEqual("duplicate_argument", Errors.DuplicateArgument);
            Assert.AreEqual("invalid_request", Errors.InvalidRequest);
            Assert.AreEqual("unsupported_argument", Errors.UnsupportedArgument);
            Assert.AreEqual("unsupported_command", Errors.UnsupportedCommand);
            Assert.AreEqual("server_error", Errors.ServerError);
            Assert.AreEqual("missing_argument", Errors.MissingArgument);
            Assert.AreEqual("request_canceled", Errors.RequestCanceled);
            Assert.AreEqual("invalid_license", Errors.InvalidLicense);
        }
    }
}
