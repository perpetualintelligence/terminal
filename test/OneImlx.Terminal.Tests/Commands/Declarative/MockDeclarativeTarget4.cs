/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Mocks;
using System;

namespace OneImlx.Terminal.Commands.Declarative
{
    [CommandOwners("oid1, oid2")]
    [CommandDescriptor("id4", "name4", "description", CommandType.SubCommand, CommandFlags.None)]
    [CommandRunner(typeof(MockCommandRunner))]
    [CommandChecker(typeof(MockCommandChecker))]
    [CommandTags("tag1", "tag2", "tag3")]
    [OptionDescriptor("opt1", nameof(String), "test arg desc1", OptionFlags.None)]
    [OptionDescriptor("opt2", nameof(String), "test arg desc2", OptionFlags.None)]
    [OptionDescriptor("opt3", nameof(String), "test arg desc3", OptionFlags.None)]
    [ArgumentDescriptor(1, "arg1", nameof(String), "test arg desc1", ArgumentFlags.None)]
    [ArgumentDescriptor(2, "arg2", nameof(String), "test arg desc2", ArgumentFlags.None)]
    [ArgumentDescriptor(3, "arg3", nameof(System.Double), "test arg desc3", ArgumentFlags.None)]
    public class MockDeclarativeTarget4 : IDeclarativeTarget
    {
    }
}