/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Shared.Attributes.Validation;
using OneImlx.Terminal.Mocks;
using System;
using System.ComponentModel.DataAnnotations;

namespace OneImlx.Terminal.Commands.Declarative
{
    [CommandOwners("oid1, oid2")]
    [CommandDescriptor("id1", "name1", "description", CommandType.SubCommand, CommandFlags.None)]
    [CommandRunner(typeof(MockCommandRunner))]
    [CommandChecker(typeof(MockCommandChecker))]
    [CommandTags("tag1", "tag2", "tag3")]
    [CommandCustomProperty("key1", "value1")]
    [CommandCustomProperty("key2", "value2")]
    [CommandCustomProperty("key3", "value3")]
    [OptionDescriptor("opt1", nameof(String), "test opt desc1", OptionFlags.None)]
    [OptionDescriptor("opt2", nameof(String), "test opt desc2", OptionFlags.Disabled, "opt2_alias")]
    [OptionValidation("opt2", typeof(RequiredAttribute))]
    [OptionValidation("opt2", typeof(OneOfAttribute), "test1", "test2", "test3")]
    [OptionDescriptor("opt3", nameof(System.Double), "test opt desc3", OptionFlags.Required | OptionFlags.Obsolete)]
    [OptionValidation("opt3", typeof(RangeAttribute), 25.34, 40.56)]
    [ArgumentDescriptor(1, "arg1", nameof(String), "test arg desc1", ArgumentFlags.None)]
    [ArgumentDescriptor(2, "arg2", nameof(String), "test arg desc2", ArgumentFlags.Disabled)]
    [ArgumentValidation("arg2", typeof(RequiredAttribute))]
    [ArgumentValidation("arg2", typeof(OneOfAttribute), "test1", "test2", "test3")]
    [ArgumentDescriptor(3, "arg3", nameof(System.Double), "test arg desc3", ArgumentFlags.Required | ArgumentFlags.Obsolete)]
    [ArgumentValidation("arg3", typeof(RangeAttribute), 25.34, 40.56)]
    public class MockDeclarativeTarget1 : IDeclarativeTarget
    {
    }
}