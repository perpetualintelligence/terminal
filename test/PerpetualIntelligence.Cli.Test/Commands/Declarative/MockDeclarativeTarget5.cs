/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Mocks;

namespace PerpetualIntelligence.Cli.Commands.Declarative
{
    [CommandDescriptor("id5", "name5", "test grp cmd5", "description")]
    [CommandRunner(typeof(MockCommandRunner))]
    [CommandChecker(typeof(MockCommandChecker))]
    public class MockDeclarativeTarget5 : IDeclarativeTarget
    {
    }
}