/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Mocks;
using System.ComponentModel.DataAnnotations;

namespace PerpetualIntelligence.Terminal.Commands.Declarative
{
    [CommandDescriptor("id4", "name4", "test grp cmd4", "description")]
    [CommandRunner(typeof(MockCommandRunner))]
    [CommandChecker(typeof(MockCommandChecker))]
    [CommandTags("tag1", "tag2", "tag3")]
    [OptionDescriptor("arg1", DataType.Text, "test arg desc1")]
    [OptionDescriptor("arg2", DataType.Text, "test arg desc2")]
    [OptionDescriptor("arg3", DataType.Text, "test arg desc3")]
    public class MockDeclarativeTarget4 : IDeclarativeTarget
    {
    }
}