/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Xunit;

namespace PerpetualIntelligence.Terminal.Commands.Extractors
{
    [TestClass]
    public class CommandExtractorResultTests
    {
        [Fact]
        public void CommandExtractorResultNullCommandDescriptorShouldThrow()
        {
#pragma warning disable CA1806 // Do not ignore method results
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action act = () => new CommandExtractorResult(new ParsedCommand(new CommandRoute("test_route", "test cmd"), new Command(null), null));
            act.Should().Throw<ArgumentNullException>().WithMessage("Value cannot be null. (Parameter 'commandDescriptor')");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CA1806 // Do not ignore method results
        }

        [Fact]
        public void CommandExtractorResultNullCommandRouteShouldThrow()
        {
#pragma warning disable CA1806 // Do not ignore method results
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action act = () => new CommandExtractorResult(new ParsedCommand(null, new Command(new CommandDescriptor("test_id", "test_name", "desc", CommandType.SubCommand, CommandFlags.None)), null));
            act.Should().Throw<ArgumentNullException>().WithMessage("Value cannot be null. (Parameter 'commandRoute')");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CA1806 // Do not ignore method results
        }

        [Fact]
        public void CommandExtractorResultNullParsedCommandShouldThrow()
        {
#pragma warning disable CA1806 // Do not ignore method results
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action act = () => new CommandExtractorResult(null);
            act.Should().Throw<ArgumentNullException>().WithMessage("Value cannot be null. (Parameter 'parsedCommand')");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CA1806 // Do not ignore method results
        }
    }
}