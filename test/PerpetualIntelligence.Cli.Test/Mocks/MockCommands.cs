/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands;
using PerpetualIntelligence.Cli.Commands.Checkers;
using PerpetualIntelligence.Cli.Commands.Runners;
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
            new ArgumentIdentity("aid1", "key1", DataType.Text, false, "Key1 value text"),
            new ArgumentIdentity("aid2", "key2", DataType.Text, true, "Key2 value text"),
            new ArgumentIdentity("aid3", "key3", DataType.PhoneNumber, false, "Key3 value phone"),
            new ArgumentIdentity("aid4", "key4", DataType.EmailAddress, false, "Key4 value email"),
            new ArgumentIdentity("aid5", "key5", DataType.Url, false, "Key5 value url"),
            new ArgumentIdentity("aid6", "key6", nameof(Boolean), false, "Key6 no value"),
            new ArgumentIdentity("aid7", "key7", DataType.Currency, true, "Key7 value currency", new[] { "INR", "USD", "EUR" }),
            new ArgumentIdentity("aid8", "key8", nameof(Int32), false, "Key8 value custom int"),
            new ArgumentIdentity("aid9", "key9", nameof(Double), true, "Key9 value custom double", new object[] { 2.36, 25.36, 3669566.36, 26.36, -36985.25, 0, -5 })
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
