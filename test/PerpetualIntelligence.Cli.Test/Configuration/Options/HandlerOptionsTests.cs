/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;

namespace PerpetualIntelligence.Cli.Configuration.Options
{
    [TestClass]
    public class HandlerOptionsTests : InitializerTests
    {
        public HandlerOptionsTests() : base(TestLogger.Create<HandlerOptionsTests>())
        {
        }

        [TestMethod]
        public void HostingOptionsShouldHaveCorrectDefaultValues()
        {
            HandlerOptions options = new();

            options.DataTypeHandler.Should().BeNull();
            options.ErrorHandler.Should().Be(Handlers.DefaultHandler);
            options.LicenseHandler.Should().Be(Handlers.OnlineHandler);
            options.ServiceHandler.Should().Be(Handlers.DefaultHandler);
            options.StoreHandler.Should().Be(Handlers.InMemoryHandler);
            options.TextHandler.Should().Be(Handlers.UnicodeHandler);
        }
    }
}
