/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Shared.Attributes.Validation;
using PerpetualIntelligence.Terminal.Mocks;
using System;
using System.ComponentModel.DataAnnotations;

namespace PerpetualIntelligence.Terminal.Commands.Declarative
{
    [CommandDescriptor("id1", "name1", "description", CommandType.SubCommand, CommandFlags.None)]
    [CommandRunner(typeof(MockCommandRunner))]
    [CommandChecker(typeof(MockCommandChecker))]
    [CommandTags("tag1", "tag2", "tag3")]
    [CommandCustomProperty("key1", "value1")]
    [CommandCustomProperty("key2", "value2")]
    [CommandCustomProperty("key3", "value3")]
    [OptionDescriptor("arg1", nameof(String), "test arg desc1", OptionFlags.None)]
    [OptionCustomProperty("arg1", "a1Key1", "a1Value1")]
    [OptionCustomProperty("arg1", "a1Key2", "a1Value2")]
    [OptionCustomProperty("arg1", "a1Key3", "a1Value3")]
    [OptionDescriptor("arg2", nameof(String), "test arg desc2", OptionFlags.Disabled, "arg2_alias")]
    [OptionValidation("arg2", typeof(RequiredAttribute))]
    [OptionValidation("arg2", typeof(OneOfAttribute), "test1", "test2", "test3")]
    [OptionCustomProperty("arg2", "a2Key1", "a2Value1")]
    [OptionCustomProperty("arg2", "a2Key2", "a2Value2")]
    [OptionDescriptor("arg3", nameof(System.Double), "test arg desc3", OptionFlags.Required | OptionFlags.Obsolete)]
    [OptionValidation("arg3", typeof(RangeAttribute), 25.34, 40.56)]
    public class MockDeclarativeTarget1 : IDeclarativeTarget
    {
    }
}