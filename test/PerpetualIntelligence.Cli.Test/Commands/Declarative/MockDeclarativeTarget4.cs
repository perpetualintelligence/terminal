/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Mocks;
using System.ComponentModel.DataAnnotations;

namespace PerpetualIntelligence.Cli.Commands.Declarative
{
    [CommandDescriptor("id4", "name4", "test grp cmd4", "description")]
    [CommandRunner(typeof(MockCommandRunner))]
    [CommandChecker(typeof(MockCommandChecker))]
    [CommandTags("tag1", "tag2", "tag3")]
    [CommandCustomProperty("key1", "value1")]
    [CommandCustomProperty("key2", "value2")]
    [CommandCustomProperty("key3", "value3")]
    [ArgumentDescriptor("arg1", DataType.Text, "test arg desc1")]
    [ArgumentDescriptor("arg2", DataType.Text, "test arg desc2")]
    [ArgumentValidation("arg2", typeof(RequiredAttribute))]
    [ArgumentDescriptor("arg3", DataType.Text, "test arg desc3")]
    [ArgumentValidation("arg3", typeof(RangeAttribute), 25, 40)]
    public class MockDeclarativeTarget4 : IDeclarativeTarget
    {
    }
}