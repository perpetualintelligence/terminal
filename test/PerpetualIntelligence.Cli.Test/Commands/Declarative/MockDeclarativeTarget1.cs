/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Cli.Mocks;
using System.ComponentModel.DataAnnotations;

namespace PerpetualIntelligence.Cli.Commands.Declarative
{
    [CommandDescriptor("id1", "name1", "test grp cmd1", "description")]
    [CommandRunner(typeof(MockCommandRunner))]
    [CommandChecker(typeof(MockCommandChecker))]
    [TextHandler(typeof(UnicodeTextHandler))]
    [CommandTags("tag1", "tag2", "tag3")]
    [CommandCustomProperty("key1", "value1")]
    [CommandCustomProperty("key2", "value2")]
    [CommandCustomProperty("key3", "value3")]
    [ArgumentDescriptor("arg1", DataType.Text, "test arg desc1")]
    [ArgumentCustomProperty("arg1", "a1Key1", "a1Value1")]
    [ArgumentCustomProperty("arg1", "a1Key2", "a1Value2")]
    [ArgumentCustomProperty("arg1", "a1Key3", "a1Value3")]
    [ArgumentDescriptor("arg2", DataType.PhoneNumber, "test arg desc2", DefaultValue = 1111111111, Disabled = true, Alias = "arg2_alias")]
    [ArgumentValidation("arg2", typeof(RequiredAttribute))]
    [ArgumentCustomProperty("arg2", "a2Key1", "a2Value1")]
    [ArgumentCustomProperty("arg2", "a2Key2", "a2Value2")]
    [ArgumentDescriptor("arg3", nameof(System.Double), "test arg desc3", Required = true, Disabled = false, Obsolete = true)]
    [ArgumentValidation("arg3", typeof(RangeAttribute), 25, 40)]
    public class MockDeclarativeTarget1 : IDeclarativeTarget
    {
    }
}
