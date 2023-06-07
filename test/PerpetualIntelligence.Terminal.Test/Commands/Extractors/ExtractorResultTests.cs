/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Test.Services;
using System;

namespace PerpetualIntelligence.Terminal.Commands.Extractors
{
    [TestClass]
    public class ExtractorResultTests
    {
        [TestMethod]
        public void ArgumentExtractorResultNullArgumentShouldThrow()
        {
#pragma warning disable CA1806 // Do not ignore method results
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            TestHelper.AssertThrowsWithMessage<ArgumentNullException>(() => new OptionExtractorResult(null), "Value cannot be null. (Parameter 'option')");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CA1806 // Do not ignore method results
        }

        [TestMethod]
        public void CommandExtractorResultNullCommandDescriptorShouldThrow()
        {
#pragma warning disable CA1806 // Do not ignore method results
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            TestHelper.AssertThrowsWithMessage<ArgumentNullException>(() => new CommandExtractorResult(new Command(new CommandRoute("test_route", "test cmd"), null, null)), "Value cannot be null. (Parameter 'commandDescriptor')");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CA1806 // Do not ignore method results
        }

        [TestMethod]
        public void CommandExtractorResultNullCommandRouteShouldThrow()
        {
#pragma warning disable CA1806 // Do not ignore method results
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            TestHelper.AssertThrowsWithMessage<ArgumentNullException>(() => new CommandExtractorResult(new Command(null, new CommandDescriptor("test_id", "test_name", "test_prefix", "desc"), null)), "Value cannot be null. (Parameter 'commandRoute')");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CA1806 // Do not ignore method results
        }

        [TestMethod]
        public void CommandExtractorResultNullCommandShouldThrow()
        {
#pragma warning disable CA1806 // Do not ignore method results
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            TestHelper.AssertThrowsWithMessage<ArgumentNullException>(() => new CommandExtractorResult(null), "Value cannot be null. (Parameter 'command')");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CA1806 // Do not ignore method results
        }
    }
}