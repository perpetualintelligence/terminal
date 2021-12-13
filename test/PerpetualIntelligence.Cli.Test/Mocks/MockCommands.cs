/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands;
using PerpetualIntelligence.Cli.Commands.Checkers;
using PerpetualIntelligence.Cli.Commands.Runners;
using PerpetualIntelligence.Shared.Attributes.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PerpetualIntelligence.Cli.Mocks
{
    internal class MockCommands
    {
        public static Tuple<CommandIdentity, Command> NewCommand(string id, string name, string prefix, string? desc = null, Type? checker = null, Type? runner = null, ArgumentIdentities? args = null)
        {
            var cmd1 = new CommandIdentity(id, name, prefix, args, desc, checker, runner);
            return new Tuple<CommandIdentity, Command>(cmd1, new Command(cmd1));
        }

        public static ArgumentIdentities ArgumentIdentities = new()
        {
            new ArgumentIdentity("key1", DataType.Text, false, "Key1 value text"),
            new ArgumentIdentity("key2", DataType.Text, true, "Key2 value text"),
            new ArgumentIdentity("key3", DataType.PhoneNumber, false, "Key3 value phone"),
            new ArgumentIdentity("key4", DataType.EmailAddress, false, "Key4 value email"),
            new ArgumentIdentity("key5", DataType.Url, false, "Key5 value url"),
            new ArgumentIdentity("key6", nameof(Boolean), false, "Key6 no value"),
            new ArgumentIdentity("key7", DataType.Currency, true, "Key7 value currency", new[] { new OneOfAttribute("INR", "USD", "EUR") }),
            new ArgumentIdentity("key8", nameof(Int32), false, "Key8 value custom int"),
            new ArgumentIdentity("key9", nameof(Double), true, "Key9 value custom double", new[] { new OneOfAttribute(2.36, 25.36, 3669566.36, 26.36, -36985.25, 0, -5) }),
            new ArgumentIdentity("key10", nameof(String), true, "Key10 value custom string")
        };

        public static List<CommandIdentity> Commands = new()
        {
            // Different name and prefix
            NewCommand("id1", "name1", "prefix1", "desc1", typeof(CommandChecker), typeof(CommandRunner), ArgumentIdentities).Item1,

            // Same name and prefix with args
            NewCommand("id2", "name2", "name2", "desc2", typeof(CommandChecker), typeof(CommandRunner), ArgumentIdentities).Item1,

            // Multiple prefix names
            NewCommand("id3", "name3", "prefix3 sub3 name3", "desc3", typeof(CommandChecker), typeof(CommandRunner), ArgumentIdentities).Item1,

            // Command with no args
            NewCommand("id4", "name4", "prefix4_noargs").Item1,
        };
    }
}
