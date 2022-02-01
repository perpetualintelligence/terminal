/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
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
    /// <summary>
    /// The mock test commands.
    /// </summary>
    internal static class MockCommands
    {
        /// <summary>
        /// Init the test commands.
        /// </summary>
        static MockCommands()
        {
            TestArgumentDescriptors = new()
            {
                new ArgumentDescriptor("key1", DataType.Text, false, "Key1 value text"),
                new ArgumentDescriptor("key2", DataType.Text, true, "Key2 value text"),
                new ArgumentDescriptor("key3", DataType.PhoneNumber, false, "Key3 value phone"),
                new ArgumentDescriptor("key4", DataType.EmailAddress, false, "Key4 value email"),
                new ArgumentDescriptor("key5", DataType.Url, false, "Key5 value url"),
                new ArgumentDescriptor("key6", nameof(Boolean), false, "Key6 no value"),
                new ArgumentDescriptor("key7", DataType.Currency, true, "Key7 value currency", new[] { new OneOfAttribute("INR", "USD", "EUR") }),
                new ArgumentDescriptor("key8", nameof(Int32), false, "Key8 value custom int"),
                new ArgumentDescriptor("key9", nameof(Double), true, "Key9 value custom double", new[] { new OneOfAttribute(2.36, 25.36, 3669566.36, 26.36, -36985.25, 0, -5) }),
                new ArgumentDescriptor("key10", nameof(String), true, "Key10 value custom string")
            };

            TestDefaultArgumentDescriptors = new()
            {
                new ArgumentDescriptor("key1", DataType.Text, false, "Key1 value text"),
                new ArgumentDescriptor("key2", DataType.Text, true, "Key2 value text"),
                new ArgumentDescriptor("key3", DataType.PhoneNumber, false, "Key3 value phone", defaultValue: "44444444444"),
                new ArgumentDescriptor("key4", DataType.EmailAddress, false, "Key4 value email"),
                new ArgumentDescriptor("key5", DataType.Url, false, "Key5 value url"),
                new ArgumentDescriptor("key6", nameof(Boolean), false, "Key6 no value", defaultValue: false),
                new ArgumentDescriptor("key7", DataType.Currency, true, "Key7 value currency", new[] { new OneOfAttribute("INR", "USD", "EUR") }),
                new ArgumentDescriptor("key8", nameof(Int32), false, "Key8 value custom int"),
                new ArgumentDescriptor("key9", nameof(Double), true, "Key9 value custom double", new[] { new OneOfAttribute(2.36, 25.36, 3669566.36, 26.36, -36985.25, 0, -5) }, defaultValue: 25.36),
                new ArgumentDescriptor("key10", nameof(String), true, "Key10 value custom string", defaultValue: "mello default")
            };

            Commands = new()
            {
                // Different name and prefix
                NewCommandDefinition("id1", "name1", "prefix1", TestArgumentDescriptors, "desc1", typeof(CommandChecker), typeof(CommandRunner)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("id2", "name2", "name2", TestArgumentDescriptors, "desc2", typeof(CommandChecker), typeof(CommandRunner)).Item1,

                // Multiple prefix names
                NewCommandDefinition("id3", "name3", "prefix3 sub3 name3", TestArgumentDescriptors, "desc3", typeof(CommandChecker), typeof(CommandRunner)).Item1,

                // Command with no args
                NewCommandDefinition("id4", "name4", "prefix4_noargs").Item1,

                // Command with no default args
                NewCommandDefinition("id5", "name5", "prefix5_default", TestDefaultArgumentDescriptors, "desc5", typeof(CommandChecker), typeof(CommandRunner)).Item1,

                // Command with no default args
                NewCommandDefinition("id5", "name5", "prefix6_empty_args", new(), "desc5", typeof(CommandChecker), typeof(CommandRunner)).Item1,

                // Command with no default args
                NewCommandDefinition("id5", "name5", "prefix6_empty_args", new(), "desc5", typeof(CommandChecker), typeof(CommandRunner)).Item1,
            };

            GroupedCommands = new()
            {
                // Different name and prefix
                NewCommandDefinition("orgid", "pi", "pi", TestArgumentDescriptors, "the top org command group", typeof(CommandChecker), typeof(CommandRunner)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("orgid:authid", "auth", "pi auth", TestArgumentDescriptors, "the auth command group", typeof(CommandChecker), typeof(CommandRunner)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("orgid:authid:loginid", "login", "pi auth login", TestArgumentDescriptors, "the login command within the auth group", typeof(CommandChecker), typeof(CommandRunner)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("orgid:authid:sloginid", "slogin", "pi auth slogin", TestArgumentDescriptors, "the silent login command within the auth group", typeof(CommandChecker), typeof(CommandRunner), defaultArg: "key2").Item1,

                // Same name and prefix with args
                NewCommandDefinition("orgid:authid:sloginid:oidc", "oidc", "pi auth slogin oidc", TestArgumentDescriptors, "the slient oidc login command within the slogin group", typeof(CommandChecker), typeof(CommandRunner)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("orgid:authid:sloginid:oauth", "oauth", "pi auth slogin oauth", TestArgumentDescriptors, "the slient oauth login command within the slogin group", typeof(CommandChecker), typeof(CommandRunner), defaultArg: "key1").Item1,
            };
        }

        public static Tuple<CommandDescriptor, Command> NewCommandDefinition(string id, string name, string prefix, ArgumentDescriptors? args = null, string? desc = null, Type? checker = null, Type? runner = null, string? defaultArg = null)
        {
            var cmd1 = new CommandDescriptor(id, name, prefix, args, desc, checker, runner, defaultArgument: defaultArg);
            return new Tuple<CommandDescriptor, Command>(cmd1, new Command(cmd1));
        }

        public static List<CommandDescriptor> Commands;
        public static List<CommandDescriptor> GroupedCommands;
        public static ArgumentDescriptors TestArgumentDescriptors;
        public static ArgumentDescriptors TestDefaultArgumentDescriptors;
    }
}
