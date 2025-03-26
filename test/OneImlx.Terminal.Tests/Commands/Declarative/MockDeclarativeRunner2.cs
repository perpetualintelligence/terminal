﻿/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Mocks;
using System;
using System.ComponentModel.DataAnnotations;

namespace OneImlx.Terminal.Commands.Declarative
{
    [CommandOwners("oid1, oid2")]
    [CommandDescriptor("id2", "name2", "description", CommandType.SubCommand, CommandFlags.None)]
    [CommandChecker(typeof(MockCommandChecker))]
    [OptionDescriptor("opt1", nameof(String), "test arg desc1", OptionFlags.None)]
    [OptionDescriptor("opt2", nameof(String), "test arg desc2", OptionFlags.None)]
    [OptionValidation("opt2", typeof(RequiredAttribute))]
    [OptionDescriptor("ar3", nameof(String), "test arg desc3", OptionFlags.None)]
    [OptionValidation("opt3", typeof(RangeAttribute), 25, 40)]
    public class MockDeclarativeRunner2 : IDeclarativeRunner
    {
    }
}