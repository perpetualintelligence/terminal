﻿/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;

namespace PerpetualIntelligence.Terminal.Configuration.Options
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
            options.ErrorHandler.Should().Be(TerminalHandlers.DefaultHandler);
            options.LicenseHandler.Should().Be(TerminalHandlers.OnlineLicenseHandler);
            options.ServiceHandler.Should().Be(TerminalHandlers.DefaultHandler);
            options.StoreHandler.Should().Be(TerminalHandlers.InMemoryHandler);
            options.TextHandler.Should().Be(TerminalHandlers.UnicodeHandler);
        }
    }
}
