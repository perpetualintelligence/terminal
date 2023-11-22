﻿/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Xunit;

namespace PerpetualIntelligence.Terminal.Commands.Parsers
{
    [TestClass]
    public class CommandParserResultTests
    {
        [Fact]
        public void CommandParserResultNullCommandDescriptorShouldThrow()
        {
#pragma warning disable CA1806 // Do not ignore method results
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action act = () => new CommandParserResult(new ParsedCommand(new CommandRoute("test_route", "test cmd"), new Command(null), null));
            act.Should().Throw<ArgumentNullException>().WithMessage("Value cannot be null. (Parameter 'commandDescriptor')");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CA1806 // Do not ignore method results
        }

        [Fact]
        public void CommandParserResultNullCommandRouteShouldThrow()
        {
#pragma warning disable CA1806 // Do not ignore method results
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action act = () => new CommandParserResult(new ParsedCommand(null, new Command(new CommandDescriptor("test_id", "test_name", "desc", CommandType.SubCommand, CommandFlags.None)), null));
            act.Should().Throw<ArgumentNullException>().WithMessage("Value cannot be null. (Parameter 'commandRoute')");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CA1806 // Do not ignore method results
        }

        [Fact]
        public void CommandParserResultNullParsedCommandShouldThrow()
        {
#pragma warning disable CA1806 // Do not ignore method results
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action act = () => new CommandParserResult(null);
            act.Should().Throw<ArgumentNullException>().WithMessage("Value cannot be null. (Parameter 'parsedCommand')");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CA1806 // Do not ignore method results
        }
    }
}