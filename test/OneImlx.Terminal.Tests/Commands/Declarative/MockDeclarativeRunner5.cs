/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Mocks;

namespace OneImlx.Terminal.Commands.Declarative
{
    [CommandOwners("oid1, oid2")]
    [CommandDescriptor("id5", "name5", "description", CommandType.SubCommand, CommandFlags.None)]
    [CommandChecker(typeof(MockCommandChecker))]
    public class MockDeclarativeRunner5 : IDeclarativeRunner
    {
    }
}