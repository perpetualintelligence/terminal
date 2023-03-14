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
    [CommandRunner(typeof(MockCommandRunner))]
    [CommandChecker(typeof(MockCommandChecker))]
    [CommandTags("tag1", "tag2", "tag3")]
    [CommandCustomProperty("key1", "value1")]
    [CommandCustomProperty("key2", "value2")]
    [CommandCustomProperty("key3", "value3")]
    [OptionDescriptor("arg1", DataType.Text, "test arg desc1")]
    [OptionCustomProperty("arg1", "key1", "value1")]
    [OptionCustomProperty("arg1", "key2", "value2")]
    [OptionCustomProperty("arg1", "key3", "value3")]
    [OptionDescriptor("arg2", DataType.Text, "test arg desc2")]
    [OptionValidation("arg2", typeof(RequiredAttribute))]
    [OptionCustomProperty("arg2", "key1", "value1")]
    [OptionCustomProperty("arg2", "key2", "value2")]
    [OptionDescriptor("ar3", DataType.Text, "test arg desc3")]
    [OptionValidation("arg3", typeof(RangeAttribute), 25, 40)]
    public class MockDeclarativeTargetNoCommandDescriptor : IDeclarativeTarget
    {
    }
}
