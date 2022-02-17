/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands;
using PerpetualIntelligence.Cli.Commands.Checkers;
using PerpetualIntelligence.Cli.Commands.Comparers;
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
            TestArgumentDescriptors = new(new StringComparisonComparer(StringComparison.Ordinal))
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

            TestDefaultArgumentDescriptors = new(new StringComparisonComparer(StringComparison.Ordinal))
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

            TestDefaultArgumentValueDescriptors = new(new StringComparisonComparer(StringComparison.Ordinal))
            {
                new ArgumentDescriptor("key1", DataType.Text, false, "Key1 value text", defaultValue: "key1 default value"),
                new ArgumentDescriptor("key2", DataType.Text, true, "Key2 value text"),
                new ArgumentDescriptor("key3", DataType.PhoneNumber, false, "Key3 value phone"),
                new ArgumentDescriptor("key4", DataType.EmailAddress, false, "Key4 value email"),
            };

            TestAliasArgumentDescriptors = new(new StringComparisonComparer(StringComparison.Ordinal))
            {
                new ArgumentDescriptor("key1", DataType.Text, false, "Key1 value text", defaultValue: "key1 default value") { Alias = "key1_alias" },
                new ArgumentDescriptor("key2", DataType.Text, true, "Key2 value text") { },
                new ArgumentDescriptor("key3", DataType.PhoneNumber, false, "Key3 value phone") { Alias = "key3_alias" },
                new ArgumentDescriptor("key4", nameof(Double), false, "Key4 value number") { Alias = "key4_alias" },
            };

            TestOptionsDescriptors = new(new StringComparisonComparer(StringComparison.Ordinal))
            {
                new ArgumentDescriptor("key1", DataType.Text, false, "Key1 value text", defaultValue: "key1 default value") { Alias = "key1_alias" },
                new ArgumentDescriptor("key2-er", DataType.Text, true, "Key2 value text", defaultValue: "key2 default value"),
                new ArgumentDescriptor("key3-a-z-d", DataType.PhoneNumber, false, "Key3 value phone") { Alias = "k3" },
                new ArgumentDescriptor("key4", DataType.EmailAddress, false, "Key4 value email"),
                new ArgumentDescriptor("key5", DataType.Url, false, "Key5 value url"),
                new ArgumentDescriptor("key6-a-s-xx-s", nameof(Boolean), false, "Key6 no value"),
                new ArgumentDescriptor("key7", DataType.Currency, true, "Key7 value currency", new[] { new OneOfAttribute("INR", "USD", "EUR") }, defaultValue: "INR"),
                new ArgumentDescriptor("key8", nameof(Int32), false, "Key8 value int"),
                new ArgumentDescriptor("key9", nameof(Double), true, "Key9 invalid default value", new[] { new OneOfAttribute(2.36, 25.36, 3669566.36, 26.36, -36985.25, 0, -5) }, defaultValue: 89568.36),
                new ArgumentDescriptor("key10", nameof(String), true, "Key10 value custom string") { Alias = "k10" },
                new ArgumentDescriptor("key11", nameof(Boolean), true, "Key11 value boolean") { Alias = "k11" },
                new ArgumentDescriptor("key12", nameof(Boolean), true, "Key12 value default boolean") { Alias = "k12", DefaultValue = true }
            };

            Commands = new()
            {
                // Different name and prefix
                NewCommandDefinition("id1", "name1", "prefix1", "desc1", TestArgumentDescriptors, typeof(CommandChecker), typeof(CommandRunner)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("id2", "name2", "name2", "desc2", TestArgumentDescriptors, typeof(CommandChecker), typeof(CommandRunner)).Item1,

                // Multiple prefix names
                NewCommandDefinition("id3", "name3", "prefix3 sub3 name3", "desc3", TestArgumentDescriptors, typeof(CommandChecker), typeof(CommandRunner)).Item1,

                // Command with no args
                NewCommandDefinition("id4", "name4", "prefix4_noargs", "desc4").Item1,

                // Command with no default arg
                NewCommandDefinition("id5", "name5", "prefix5_default", "desc5", TestDefaultArgumentDescriptors, typeof(CommandChecker), typeof(CommandRunner)).Item1,

                // Command with no default arg
                NewCommandDefinition("id6", "name6", "prefix6_empty_args", "desc6", new ArgumentDescriptors(new StringComparisonComparer(StringComparison.Ordinal)), typeof(CommandChecker), typeof(CommandRunner)).Item1,

                // Command with default arg
                NewCommandDefinition("id7", "name7", "prefix7_defaultarg", "desc7", TestDefaultArgumentDescriptors, typeof(CommandChecker), typeof(CommandRunner), defaultArg: "key1").Item1,

                // Command with default arg
                NewCommandDefinition("id8", "name8", "prefix8_defaultarg_defaultvalue", "desc8", TestDefaultArgumentValueDescriptors, typeof(CommandChecker), typeof(CommandRunner), defaultArg: "key1").Item1,
            };

            GroupedCommands = new()
            {
                // Different name and prefix
                NewCommandDefinition("orgid", "pi", "pi", "the top org command group", TestArgumentDescriptors, typeof(CommandChecker), typeof(CommandRunner)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("orgid:authid", "auth", "pi auth", "the auth command group", TestArgumentDescriptors, typeof(CommandChecker), typeof(CommandRunner)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("orgid:authid:loginid", "login", "pi auth login", "the login command within the auth group", TestArgumentDescriptors, typeof(CommandChecker), typeof(CommandRunner)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("orgid:authid:sloginid", "slogin", "pi auth slogin", "the silent login command within the auth group", TestArgumentDescriptors, typeof(CommandChecker), typeof(CommandRunner), defaultArg: "key2").Item1,

                // Same name and prefix with args
                NewCommandDefinition("orgid:authid:sloginid:oidc", "oidc", "pi auth slogin oidc", "the slient oidc login command within the slogin group", TestArgumentDescriptors, typeof(CommandChecker), typeof(CommandRunner)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("orgid:authid:sloginid:oauth", "oauth", "pi auth slogin oauth", "the slient oauth login command within the slogin group", TestArgumentDescriptors, typeof(CommandChecker), typeof(CommandRunner), defaultArg: "key1").Item1,
            };

            AliasCommands = new()
            {
                // Different name and prefix
                NewCommandDefinition("orgid", "pi", "pi", "the top org command group", TestAliasArgumentDescriptors, typeof(CommandChecker), typeof(CommandRunner), defaultArg: "key1").Item1,

                // Same name and prefix with args
                NewCommandDefinition("orgid:authid", "auth", "pi auth", "the auth command group", TestAliasArgumentDescriptors, typeof(CommandChecker), typeof(CommandRunner)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("orgid:authid:loginid", "login", "pi auth login", "the login command within the auth group", TestAliasArgumentDescriptors, typeof(CommandChecker), typeof(CommandRunner)).Item1,
            };

            GroupedOptionsCommands = new()
            {
                // Different name and prefix
                NewCommandDefinition("orgid", "pi", "pi", "top org cmd group", TestOptionsDescriptors, typeof(CommandChecker), typeof(CommandRunner)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("orgid:authid", "auth", "pi auth", "org auth cmd group", TestOptionsDescriptors, typeof(CommandChecker), typeof(CommandRunner)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("orgid:authid:loginid", "login", "pi auth login", "org auth login cmd", TestOptionsDescriptors, typeof(CommandChecker), typeof(CommandRunner)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("orgid:authid:sloginid", "slogin", "pi auth slogin", "org auth slogin cmd", TestOptionsDescriptors, typeof(CommandChecker), typeof(CommandRunner), defaultArg: "key2").Item1,

                // Same name and prefix with args
                NewCommandDefinition("orgid:authid:sloginid:oidc", "oidc", "pi auth slogin oidc", "org auth slogin oidc cmd", TestOptionsDescriptors, typeof(CommandChecker), typeof(CommandRunner)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("orgid:authid:sloginid:oauth", "oauth", "pi auth slogin oauth", "org auth slogin oauth cmd", TestOptionsDescriptors, typeof(CommandChecker), typeof(CommandRunner), defaultArg: "key1").Item1,
            };
        }

        public static Tuple<CommandDescriptor, Command> NewCommandDefinition(string id, string name, string prefix, string desc, ArgumentDescriptors? args = null, Type? checker = null, Type? runner = null, string? defaultArg = null)
        {
            var cmd1 = new CommandDescriptor(id, name, prefix, desc, args, defaultArgument: defaultArg);

            // Internal set, in prod apps this will be set by DI Addxxx methods
            cmd1._checker = checker;
            cmd1._runner = runner;

            return new Tuple<CommandDescriptor, Command>(cmd1, new Command(id, name, desc));
        }

        public static List<CommandDescriptor> AliasCommands;
        public static List<CommandDescriptor> Commands;
        public static List<CommandDescriptor> GroupedCommands;
        public static List<CommandDescriptor> GroupedOptionsCommands;
        public static ArgumentDescriptors TestAliasArgumentDescriptors;
        public static ArgumentDescriptors TestArgumentDescriptors;
        public static ArgumentDescriptors TestDefaultArgumentDescriptors;
        public static ArgumentDescriptors TestDefaultArgumentValueDescriptors;
        public static ArgumentDescriptors TestOptionsDescriptors;
    }
}
