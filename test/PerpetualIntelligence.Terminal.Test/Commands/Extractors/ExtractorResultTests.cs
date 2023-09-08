/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

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
            TestHelper.AssertThrowsWithMessage<ArgumentNullException>(() => new CommandExtractorResult(new ParsedCommand(new CommandRoute("test_route", "test cmd"), new Command(null), null)), "Value cannot be null. (Parameter 'commandDescriptor')");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CA1806 // Do not ignore method results
        }

        [TestMethod]
        public void CommandExtractorResultNullCommandRouteShouldThrow()
        {
#pragma warning disable CA1806 // Do not ignore method results
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            TestHelper.AssertThrowsWithMessage<ArgumentNullException>(() => new CommandExtractorResult(new ParsedCommand(null, new Command(new CommandDescriptor("test_id", "test_name", "desc", CommandType.SubCommand, CommandFlags.None)), null)), "Value cannot be null. (Parameter 'commandRoute')");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CA1806 // Do not ignore method results
        }

        [TestMethod]
        public void CommandExtractorResultNullExtractedCommandShouldThrow()
        {
#pragma warning disable CA1806 // Do not ignore method results
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            TestHelper.AssertThrowsWithMessage<ArgumentNullException>(() => new CommandExtractorResult(null), "Value cannot be null. (Parameter 'parsedCommand')");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CA1806 // Do not ignore method results
        }
    }
}