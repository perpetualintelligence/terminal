/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Shared.Attributes.Validation;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text;

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
            TerminalTextHandler unicodeTextHandler = new(StringComparison.OrdinalIgnoreCase, Encoding.ASCII);

            TestOptionDescriptors = new(unicodeTextHandler,
            [
                new("key1", nameof(String), "Key1 value text", OptionFlags.None),
                new("key2", nameof(String), "Key2 value text", OptionFlags.Required),
                new("key3", nameof(Int64), "Key3 value phone", OptionFlags.None),
                new("key4", nameof(String), "Key4 value email", OptionFlags.None),
                new("key5", nameof(String), "Key5 value url", OptionFlags.None),
                new("key6", nameof(Boolean), "Key6 no value", OptionFlags.None),
                new("key7", nameof(Int64), "Key7 value currency", OptionFlags.Required) { ValueCheckers = [new DataValidationValueChecker<Option>(new OneOfAttribute("INR", "USD", "EUR"))] },
                new("key8", nameof(Int32), "Key8 value custom int", OptionFlags.None),
                new("key9", nameof(Double), "Key9 value custom double", OptionFlags.Required) {ValueCheckers = [new DataValidationValueChecker<Option>(new RequiredAttribute()), new DataValidationValueChecker<Option>(new OneOfAttribute(2.36, 25.36, 3669566.36, 26.36, -36985.25, 0, -5))] },
                new("key10", nameof(String), "Key10 value custom string", OptionFlags.Required)
            ]);

            TestOptionsDescriptors = new(unicodeTextHandler,
            [
                new("key1", nameof(String), "Key1 value text", OptionFlags.None, "key1_alias"),
                new("key2-er", nameof(String), "Key2 value text", OptionFlags.Required),
                new("key3-a-z-d", nameof(Int64), "Key3 value phone", OptionFlags.None, "k3"),
                new("key4", nameof(String), "Key4 value email", OptionFlags.None),
                new("key5", nameof(String), "Key5 value url", OptionFlags.None),
                new("key6-a-s-xx-s", nameof(Boolean), "Key6 no value", OptionFlags.None),
                new("key7", nameof(Int64), "Key7 value currency", OptionFlags.Required) { ValueCheckers = [new DataValidationValueChecker<Option>( new OneOfAttribute("INR", "USD", "EUR") )] },
                new("key8", nameof(Int32), "Key8 value int", OptionFlags.None),
                new("key9", nameof(Double), "Key9 invalid default value", OptionFlags.Required) {ValueCheckers = [new DataValidationValueChecker<Option>( new OneOfAttribute(2.36, 25.36, 3669566.36, 26.36, -36985.25, 0, -5))] },
                new("key10", nameof(String), "Key10 value custom string", OptionFlags.Required, "k10"),
                new("key11", nameof(Boolean), "Key11 value boolean", OptionFlags.Required, "k11"),
                new("key12", nameof(Boolean), "Key12 value default boolean", OptionFlags.Required, "k12")
            ]);

            TestHindiUnicodeOptionDescriptors = new(unicodeTextHandler,
            [
                new("एक", nameof(String), "पहला तर्क", OptionFlags.None, "एकहै" ),
                new("दो", nameof(Boolean), "दूसरा तर्क", OptionFlags.Required) { },
                new("तीन", nameof(String), "तीसरा तर्क", OptionFlags.None, "तीनहै" ),
                new("चार", nameof(Double), "चौथा तर्क", OptionFlags.None, "चारहै"),
            ]);

            Commands = new(unicodeTextHandler,
            [

                // Different name and prefix
                NewCommandDefinition("id1", "name1", "desc1", CommandType.SubCommand, CommandFlags.None, TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("id2", "name2", "desc2", CommandType.SubCommand, CommandFlags.None, TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,

                // Multiple prefix names
                NewCommandDefinition("id3", "name3", "desc3", CommandType.SubCommand, CommandFlags.None, TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,

                // Command with no args
                NewCommandDefinition("id4", "name4", "desc4", CommandType.SubCommand, CommandFlags.None).Item1,

                // Command with no default arg
                NewCommandDefinition("id5", "name5", "desc5", CommandType.SubCommand, CommandFlags.None, new OptionDescriptors(new TerminalTextHandler( StringComparison.OrdinalIgnoreCase, Encoding.Unicode )), typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,
            ]);

            GroupedCommands = new(unicodeTextHandler,
            [

                // Different name and prefix
                NewCommandDefinition("pi", "pi", "the top org grouped command", CommandType.Root, CommandFlags.None, TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("auth", "pi:auth", "the auth grouped command", CommandType.Group, CommandFlags.None, TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("login", "pi:auth:login", "the login command within the auth group", CommandType.SubCommand, CommandFlags.None, TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("slogin", "pi:auth:slogin", "the silent login command within the auth group", CommandType.Group, CommandFlags.None, TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("oidc", "pi:auth:slogin:oidc", "the slient oidc login command within the slogin group", CommandType.SubCommand, CommandFlags.None, TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("outh", "pi:auth:slogin:oauth", "the slient oauth login command within the slogin group", CommandType.SubCommand, CommandFlags.None, TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,
            ]);

            GroupedOptionsCommands = new(unicodeTextHandler,
            [

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
            ]);

            LicensingCommands = new(unicodeTextHandler,
            [

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
                NewCommandDefinition("id5", "name5", "desc5",CommandType.SubCommand, CommandFlags.None, new OptionDescriptors( new TerminalTextHandler( StringComparison.OrdinalIgnoreCase, Encoding.Unicode )), typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,
            ]);

            UnicodeCommands = new(unicodeTextHandler,
            [

                // --- Hindi --- Root command
                NewCommandDefinition("यूनिकोड", "यूनिकोड नाम", "यूनिकोड रूट कमांड", CommandType.Root, CommandFlags.None, null, typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,

                // Grouped command
                NewCommandDefinition("परीक्षण", "परीक्षण नाम", "यूनिकोड समूहीकृत कमांड", CommandType.Group, CommandFlags.None, null, typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,

                // Subcommand
                NewCommandDefinition("प्रिंट", "प्रिंट नाम", "प्रिंट कमांड", CommandType.SubCommand, CommandFlags.None, TestHindiUnicodeOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,

                // Subcommand
                NewCommandDefinition("दूसरा", "दूसरा नाम", "दूसरा आदेश", CommandType.SubCommand, CommandFlags.None, TestHindiUnicodeOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner < CommandRunnerResult >)).Item1,
            ]);
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
        public static OptionDescriptors TestHindiUnicodeOptionDescriptors;
        public static OptionDescriptors TestOptionDescriptors;
        public static OptionDescriptors TestOptionsDescriptors;
        public static CommandDescriptors UnicodeCommands;
    }
}
