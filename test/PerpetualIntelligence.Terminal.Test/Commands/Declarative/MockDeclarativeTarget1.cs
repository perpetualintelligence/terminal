/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Shared.Attributes.Validation;
using PerpetualIntelligence.Terminal.Mocks;
using System.ComponentModel.DataAnnotations;

namespace PerpetualIntelligence.Terminal.Commands.Declarative
{
    [CommandDescriptor("id1", "name1", "test grp cmd1", "description")]
    [CommandRunner(typeof(MockCommandRunner))]
    [CommandChecker(typeof(MockCommandChecker))]
    [CommandTags("tag1", "tag2", "tag3")]
    [CommandCustomProperty("key1", "value1")]
    [CommandCustomProperty("key2", "value2")]
    [CommandCustomProperty("key3", "value3")]
    [OptionDescriptor("arg1", DataType.Text, "test arg desc1")]
    [OptionCustomProperty("arg1", "a1Key1", "a1Value1")]
    [OptionCustomProperty("arg1", "a1Key2", "a1Value2")]
    [OptionCustomProperty("arg1", "a1Key3", "a1Value3")]
    [OptionDescriptor("arg2", DataType.Text, "test arg desc2", Disabled = true, Alias = "arg2_alias")]
    [OptionValidation("arg2", typeof(RequiredAttribute))]
    [OptionValidation("arg2", typeof(OneOfAttribute), "test1", "test2", "test3")]
    [OptionCustomProperty("arg2", "a2Key1", "a2Value1")]
    [OptionCustomProperty("arg2", "a2Key2", "a2Value2")]
    [OptionDescriptor("arg3", nameof(System.Double), "test arg desc3", Required = true, Disabled = false, Obsolete = true)]
    [OptionValidation("arg3", typeof(RangeAttribute), 25.34, 40.56)]
    public class MockDeclarativeTarget1 : IDeclarativeTarget
    {
    }
}