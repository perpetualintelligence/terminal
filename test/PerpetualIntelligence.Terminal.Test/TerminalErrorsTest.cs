/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Test.Services;

namespace PerpetualIntelligence.Terminal
{
    [TestClass]
    public class TerminalErrorsTest
    {
        [TestMethod]
        public void AssertErrorsAreValid()
        {
            TestHelper.AssertConstantCount(typeof(TerminalErrors), 15);

            Assert.AreEqual("connection_closed", TerminalErrors.ConnectionClosed);
            Assert.AreEqual("invalid_command", TerminalErrors.InvalidCommand);
            Assert.AreEqual("invalid_configuration", TerminalErrors.InvalidConfiguration);
            Assert.AreEqual("invalid_option", TerminalErrors.InvalidOption);
            Assert.AreEqual("duplicate_option", TerminalErrors.DuplicateOption);
            Assert.AreEqual("invalid_request", TerminalErrors.InvalidRequest);
            Assert.AreEqual("unsupported_option", TerminalErrors.UnsupportedOption);
            Assert.AreEqual("unsupported_command", TerminalErrors.UnsupportedCommand);
            Assert.AreEqual("server_error", TerminalErrors.ServerError);
            Assert.AreEqual("missing_option", TerminalErrors.MissingOption);
            Assert.AreEqual("missing_claim", TerminalErrors.MissingClaim);
            Assert.AreEqual("request_canceled", TerminalErrors.RequestCanceled);
            Assert.AreEqual("invalid_license", TerminalErrors.InvalidLicense);
            Assert.AreEqual("unauthorized_access", TerminalErrors.UnauthorizedAccess);
            Assert.AreEqual("invalid_declaration", TerminalErrors.InvalidDeclaration);
        }
    }
}