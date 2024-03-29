﻿/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Shared.Attributes.Validation;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OneImlx.Terminal.Mocks
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
            TerminalUnicodeTextHandler unicodeTextHandler = new();

            TestOptionDescriptors = new(unicodeTextHandler, new List<OptionDescriptor>()
            {
                new OptionDescriptor("key1", nameof(String), "Key1 value text", OptionFlags.None),
                new OptionDescriptor("key2", nameof(String), "Key2 value text", OptionFlags.Required),
                new OptionDescriptor("key3", nameof(Int64), "Key3 value phone", OptionFlags.None),
                new OptionDescriptor("key4", nameof(String), "Key4 value email", OptionFlags.None),
                new OptionDescriptor("key5", nameof(String), "Key5 value url", OptionFlags.None),
                new OptionDescriptor("key6", nameof(Boolean), "Key6 no value", OptionFlags.None),
                new OptionDescriptor("key7", nameof(Int64), "Key7 value currency", OptionFlags.Required) { ValueCheckers = new[] { new DataValidationValueChecker<Option>(new OneOfAttribute("INR", "USD", "EUR")) } },
                new OptionDescriptor("key8", nameof(Int32), "Key8 value custom int", OptionFlags.None),
                new OptionDescriptor("key9", nameof(Double), "Key9 value custom double", OptionFlags.Required) {ValueCheckers = new []{ new DataValidationValueChecker<Option>(new RequiredAttribute()), new DataValidationValueChecker<Option>(new OneOfAttribute(2.36, 25.36, 3669566.36, 26.36, -36985.25, 0, -5)) } },
                new OptionDescriptor("key10", nameof(String), "Key10 value custom string", OptionFlags.Required)
            });

            TestOptionsDescriptors = new(unicodeTextHandler, new List<OptionDescriptor>()
            {
                new OptionDescriptor("key1", nameof(String), "Key1 value text", OptionFlags.None, "key1_alias"),
                new OptionDescriptor("key2-er", nameof(String), "Key2 value text", OptionFlags.Required),
                new OptionDescriptor("key3-a-z-d", nameof(Int64), "Key3 value phone", OptionFlags.None, "k3"),
                new OptionDescriptor("key4", nameof(String), "Key4 value email", OptionFlags.None),
                new OptionDescriptor("key5", nameof(String), "Key5 value url", OptionFlags.None),
                new OptionDescriptor("key6-a-s-xx-s", nameof(Boolean), "Key6 no value", OptionFlags.None),
                new OptionDescriptor("key7", nameof(Int64), "Key7 value currency", OptionFlags.Required) { ValueCheckers = new[] { new DataValidationValueChecker<Option>( new OneOfAttribute("INR", "USD", "EUR") )} },
                new OptionDescriptor("key8", nameof(Int32), "Key8 value int", OptionFlags.None),
                new OptionDescriptor("key9", nameof(Double), "Key9 invalid default value", OptionFlags.Required) {ValueCheckers = new[] { new DataValidationValueChecker<Option>( new OneOfAttribute(2.36, 25.36, 3669566.36, 26.36, -36985.25, 0, -5)) } },
                new OptionDescriptor("key10", nameof(String), "Key10 value custom string", OptionFlags.Required, "k10"),
                new OptionDescriptor("key11", nameof(Boolean), "Key11 value boolean", OptionFlags.Required, "k11"),
                new OptionDescriptor("key12", nameof(Boolean), "Key12 value default boolean", OptionFlags.Required, "k12")
            });

            TestHindiUnicodeOptionDescriptors = new(unicodeTextHandler, new List<OptionDescriptor>()
            {
                new OptionDescriptor("एक", nameof(String), "पहला तर्क", OptionFlags.None, "एकहै" ),
                new OptionDescriptor("दो", nameof(Boolean), "दूसरा तर्क", OptionFlags.Required) { },
                new OptionDescriptor("तीन", nameof(String), "तीसरा तर्क", OptionFlags.None, "तीनहै" ),
                new OptionDescriptor("चार", nameof(Double), "चौथा तर्क", OptionFlags.None, "चारहै"),
            });

            Commands = new(unicodeTextHandler, new List<CommandDescriptor>()
            {
                // Different name and prefix
                NewCommandDefinition("id1", "name1", "desc1", CommandType.SubCommand, CommandFlags.None, TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("id2", "name2", "desc2", CommandType.SubCommand, CommandFlags.None, TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,

                // Multiple prefix names
                NewCommandDefinition("id3", "name3", "desc3", CommandType.SubCommand, CommandFlags.None, TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,

                // Command with no args
                NewCommandDefinition("id4", "name4", "desc4", CommandType.SubCommand, CommandFlags.None).Item1,

                // Command with no default arg
                NewCommandDefinition("id5", "name5", "desc5", CommandType.SubCommand, CommandFlags.None, new OptionDescriptors(new TerminalUnicodeTextHandler()), typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,
            });

            GroupedCommands = new(unicodeTextHandler, new List<CommandDescriptor>()
            {
                // Different name and prefix
                NewCommandDefinition("orgid", "pi", "the top org grouped command", CommandType.SubCommand, CommandFlags.None, TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("orgid:authid", "auth", "the auth grouped command", CommandType.SubCommand, CommandFlags.None, TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("orgid:authid:loginid", "login", "the login command within the auth group", CommandType.SubCommand, CommandFlags.None, TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("orgid:authid:sloginid", "slogin", "the silent login command within the auth group", CommandType.SubCommand, CommandFlags.None, TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("orgid:authid:sloginid:oidc", "oidc", "the slient oidc login command within the slogin group", CommandType.SubCommand, CommandFlags.None, TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("orgid:authid:sloginid:oauth", "oauth", "the slient oauth login command within the slogin group", CommandType.SubCommand, CommandFlags.None, TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,
            });

            GroupedOptionsCommands = new(unicodeTextHandler, new List<CommandDescriptor>()
            {
                // Different name and prefix
                NewCommandDefinition("orgid", "pi", "top org cmd group", CommandType.SubCommand, CommandFlags.None, TestOptionsDescriptors, typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("orgid:authid", "auth", "org auth cmd group", CommandType.SubCommand, CommandFlags.None, TestOptionsDescriptors, typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("orgid:authid:loginid", "login", "org auth login cmd", CommandType.SubCommand, CommandFlags.None, TestOptionsDescriptors, typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("orgid:authid:sloginid", "slogin", "org auth slogin cmd", CommandType.SubCommand, CommandFlags.None, TestOptionsDescriptors, typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("orgid:authid:sloginid:oidc", "oidc", "org auth slogin oidc cmd", CommandType.SubCommand, CommandFlags.None, TestOptionsDescriptors, typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("orgid:authid:sloginid:oauth", "oauth", "org auth slogin oauth cmd", CommandType.SubCommand, CommandFlags.None, TestOptionsDescriptors, typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,
            });

            LicensingCommands = new(unicodeTextHandler, new List<CommandDescriptor>()
            {
                // Different name and prefix
                NewCommandDefinition("root1", "name1", "desc1", CommandType.Root, CommandFlags.None, TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,

                // Different name and prefix
                NewCommandDefinition("root2", "name2", "desc2", CommandType.Root, CommandFlags.None, TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,

                // Different name and prefix
                NewCommandDefinition("root3", "name3", "desc3", CommandType.Root, CommandFlags.None, TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,

                // Different name and prefix
                NewCommandDefinition("grp1", "name1", "desc1", CommandType.Group, CommandFlags.None, TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,

                // Different name and prefix
                NewCommandDefinition("grp2", "name2", "desc2", CommandType.Group, CommandFlags.None, TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,

                // Different name and prefix
                NewCommandDefinition("grp3", "name3", "desc3", CommandType.Group, CommandFlags.None, TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,

                // Different name and prefix
                NewCommandDefinition("id1", "name1", "desc1", CommandType.SubCommand, CommandFlags.None, TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("id2", "name2", "desc2", CommandType.SubCommand, CommandFlags.None, TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,

                // Multiple prefix names
                NewCommandDefinition("id3", "name3", "desc3", CommandType.SubCommand, CommandFlags.None, TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,

                // Command with no args
                NewCommandDefinition("id4", "name4", "desc4", CommandType.SubCommand, CommandFlags.None).Item1,

                // Command with no default arg
                NewCommandDefinition("id5", "name5", "desc5",CommandType.SubCommand, CommandFlags.None, new OptionDescriptors( new TerminalUnicodeTextHandler()), typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,
            });

            UnicodeCommands = new(unicodeTextHandler, new List<CommandDescriptor>()
            {
                // --- Hindi --- Root command
                NewCommandDefinition("यूनिकोड", "यूनिकोड नाम", "यूनिकोड रूट कमांड", CommandType.Root, CommandFlags.None, null, typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,

                // Grouped command
                NewCommandDefinition("परीक्षण", "परीक्षण नाम", "यूनिकोड समूहीकृत कमांड", CommandType.Group, CommandFlags.None, null, typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,

                // Subcommand
                NewCommandDefinition("प्रिंट", "प्रिंट नाम", "प्रिंट कमांड", CommandType.SubCommand, CommandFlags.None, TestHindiUnicodeOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,

                // Subcommand
                NewCommandDefinition("दूसरा", "दूसरा नाम", "दूसरा आदेश", CommandType.SubCommand, CommandFlags.None, TestHindiUnicodeOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,
            });
        }

        public static Tuple<CommandDescriptor, Command> NewCommandDefinition(string id, string name, string desc, CommandType commandType, CommandFlags commandFlags, OptionDescriptors? args = null, Type? checker = null, Type? runner = null, Options? options = null)
        {
            var cmd1 = new CommandDescriptor(id, name, desc, commandType, commandFlags, optionDescriptors: args)
            {
                Checker = checker,
                Runner = runner,
            };

            return new Tuple<CommandDescriptor, Command>(cmd1, new Command(cmd1, options: options));
        }

        public static CommandDescriptors Commands;
        public static CommandDescriptors GroupedCommands;
        public static CommandDescriptors GroupedOptionsCommands;
        public static CommandDescriptors LicensingCommands;
        public static CommandDescriptors UnicodeCommands;
        public static OptionDescriptors TestOptionDescriptors;
        public static OptionDescriptors TestHindiUnicodeOptionDescriptors;
        public static OptionDescriptors TestOptionsDescriptors;
    }
}