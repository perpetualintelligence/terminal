/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Mocks;
using System.ComponentModel.DataAnnotations;

namespace PerpetualIntelligence.Terminal.Commands.Declarative
{
    [CommandDescriptor("id3", "name3", "test grp cmd3", "description")]
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
    public class MockDeclarativeTarget3 : IDeclarativeTarget
    {
    }
}
