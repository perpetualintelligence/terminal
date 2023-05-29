/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Cli.Commands;
using PerpetualIntelligence.Cli.Commands.Checkers;
using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Cli.Commands.Routers;
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
            TestOptionDescriptors = new(new UnicodeTextHandler())
            {
                new OptionDescriptor("key1", DataType.Text, "Key1 value text", false),
                new OptionDescriptor("key2", DataType.Text, "Key2 value text", true),
                new OptionDescriptor("key3", DataType.PhoneNumber, "Key3 value phone", false),
                new OptionDescriptor("key4", DataType.EmailAddress, "Key4 value email", false),
                new OptionDescriptor("key5", DataType.Url, "Key5 value url", false),
                new OptionDescriptor("key6", nameof(Boolean), "Key6 no value", false),
                new OptionDescriptor("key7", DataType.Currency, "Key7 value currency", true) { ValueCheckers = new[] { new DataValidationOptionValueChecker(new OneOfAttribute("INR", "USD", "EUR")) } },
                new OptionDescriptor("key8", nameof(Int32), "Key8 value custom int", false),
                new OptionDescriptor("key9", nameof(Double), "Key9 value custom double", true) {ValueCheckers = new []{ new DataValidationOptionValueChecker(new RequiredAttribute()), new DataValidationOptionValueChecker(new OneOfAttribute(2.36, 25.36, 3669566.36, 26.36, -36985.25, 0, -5)) } },
                new OptionDescriptor("key10", nameof(String), "Key10 value custom string", true)
            };

            TestDefaultOptionDescriptors = new(new UnicodeTextHandler())
            {
                new OptionDescriptor("key1", DataType.Text, "Key1 value text", false),
                new OptionDescriptor("key2", DataType.Text, "Key2 value text", true),
                new OptionDescriptor("key3", DataType.PhoneNumber, "Key3 value phone", false, defaultValue: "44444444444"),
                new OptionDescriptor("key4", DataType.EmailAddress, "Key4 value email", false),
                new OptionDescriptor("key5", DataType.Url, "Key5 value url", false),
                new OptionDescriptor("key6", nameof(Boolean), "Key6 no value", false, defaultValue: false),
                new OptionDescriptor("key7", DataType.Currency, "Key7 value currency", true) {ValueCheckers = new[] { new DataValidationOptionValueChecker(new OneOfAttribute("INR", "USD", "EUR")) } },
                new OptionDescriptor("key8", nameof(Int32), "Key8 value custom int", false),
                new OptionDescriptor("key9", nameof(Double), "Key9 value custom double", true, defaultValue: 25.36) { ValueCheckers = new[] { new DataValidationOptionValueChecker(new OneOfAttribute(2.36, 25.36, 3669566.36, 26.36, -36985.25, 0, -5)) } },
                new OptionDescriptor("key10", nameof(String), "Key10 value custom string", true, defaultValue: "mello default")
            };

            TestDefaultOptionValueDescriptors = new(new UnicodeTextHandler())
            {
                new OptionDescriptor("key1", DataType.Text, "Key1 value text", false, defaultValue: "key1 default value"),
                new OptionDescriptor("key2", DataType.Text, "Key2 value text", true),
                new OptionDescriptor("key3", DataType.PhoneNumber, "Key3 value phone", false),
                new OptionDescriptor("key4", DataType.EmailAddress, "Key4 value email", false),
            };

            TestAliasOptionDescriptors = new(new UnicodeTextHandler())
            {
                new OptionDescriptor("key1", DataType.Text, "Key1 value text", false, defaultValue: "key1 default value") { Alias = "key1_alias" },
                new OptionDescriptor("key2", DataType.Text, "Key2 value text", true) { },
                new OptionDescriptor("key3", DataType.PhoneNumber, "Key3 value phone", false) { Alias = "key3_alias" },
                new OptionDescriptor("key4", nameof(Double), "Key4 value number", false) { Alias = "key4_alias" },
            };

            TestOptionsDescriptors = new(new UnicodeTextHandler())
            {
                new OptionDescriptor("key1", DataType.Text, "Key1 value text", false, defaultValue: "key1 default value") { Alias = "key1_alias" },
                new OptionDescriptor("key2-er", DataType.Text, "Key2 value text", true, defaultValue: "key2 default value"),
                new OptionDescriptor("key3-a-z-d", DataType.PhoneNumber, "Key3 value phone", false) { Alias = "k3" },
                new OptionDescriptor("key4", DataType.EmailAddress, "Key4 value email", false),
                new OptionDescriptor("key5", DataType.Url, "Key5 value url", false),
                new OptionDescriptor("key6-a-s-xx-s", nameof(Boolean), "Key6 no value", false),
                new OptionDescriptor("key7", DataType.Currency, "Key7 value currency", true, defaultValue: "INR") { ValueCheckers = new[] { new DataValidationOptionValueChecker( new OneOfAttribute("INR", "USD", "EUR") )} },
                new OptionDescriptor("key8", nameof(Int32), "Key8 value int", false),
                new OptionDescriptor("key9", nameof(Double), "Key9 invalid default value", true, defaultValue: 89568.36) {ValueCheckers = new[] { new DataValidationOptionValueChecker( new OneOfAttribute(2.36, 25.36, 3669566.36, 26.36, -36985.25, 0, -5)) } },
                new OptionDescriptor("key10", nameof(String), "Key10 value custom string", true) { Alias = "k10" },
                new OptionDescriptor("key11", nameof(Boolean), "Key11 value boolean", true) { Alias = "k11" },
                new OptionDescriptor("key12", nameof(Boolean), "Key12 value default boolean", true) { Alias = "k12", DefaultValue = true }
            };

            TestHindiUnicodeOptionDescriptors = new(new UnicodeTextHandler())
            {
                new OptionDescriptor("एक", DataType.Text, "पहला तर्क", false, defaultValue: "डिफ़ॉल्ट मान") { Alias = "एकहै" },
                new OptionDescriptor("दो", nameof(Boolean), "दूसरा तर्क", true) { },
                new OptionDescriptor("तीन", DataType.Text, "तीसरा तर्क", false) { Alias = "तीनहै" },
                new OptionDescriptor("चार", nameof(Double), "चौथा तर्क", false) { Alias = "चारहै" },
            };

            TestChineseUnicodeOptionDescriptors = new(new UnicodeTextHandler())
            {
                new OptionDescriptor("第一的", DataType.Text, "第一個命令參數", false, defaultValue: "默認值") { Alias = "第一" },
                new OptionDescriptor("第二", nameof(Boolean), "第二個命令參數", true) { },
                new OptionDescriptor("第三", DataType.Text, "第三個命令參數", false),
                new OptionDescriptor("第四", nameof(Double), "第四個命令參數", false)
            };

            Commands = new()
            {
                // Different name and prefix
                NewCommandDefinition("id1", "name1", "prefix1", "desc1", TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("id2", "name2", "name2", "desc2", TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>)).Item1,

                // Multiple prefix names
                NewCommandDefinition("id3", "name3", "prefix3 sub3 name3", "desc3", TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>)).Item1,

                // Command with no args
                NewCommandDefinition("id4", "name4", "prefix4_noargs", "desc4").Item1,

                // Command with no default arg
                NewCommandDefinition("id5", "name5", "prefix5_default", "desc5", TestDefaultOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>)).Item1,

                // Command with no default arg
                NewCommandDefinition("id6", "name6", "prefix6_empty_args", "desc6", new OptionDescriptors(new UnicodeTextHandler()), typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>)).Item1,

                // Command with default arg
                NewCommandDefinition("id7", "name7", "prefix7_defaultarg", "desc7", TestDefaultOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>), defaultArg: "key1").Item1,

                // Command with default arg
                NewCommandDefinition("id8", "name8", "prefix8_defaultarg_defaultvalue", "desc8", TestDefaultOptionValueDescriptors, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>), defaultArg: "key1").Item1,
            };

            GroupedCommands = new()
            {
                // Different name and prefix
                NewCommandDefinition("orgid", "pi", "pi", "the top org grouped command", TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("orgid:authid", "auth", "pi auth", "the auth grouped command", TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("orgid:authid:loginid", "login", "pi auth login", "the login command within the auth group", TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("orgid:authid:sloginid", "slogin", "pi auth slogin", "the silent login command within the auth group", TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>), defaultArg: "key2").Item1,

                // Same name and prefix with args
                NewCommandDefinition("orgid:authid:sloginid:oidc", "oidc", "pi auth slogin oidc", "the slient oidc login command within the slogin group", TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("orgid:authid:sloginid:oauth", "oauth", "pi auth slogin oauth", "the slient oauth login command within the slogin group", TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>), defaultArg: "key1").Item1,
            };

            AliasCommands = new()
            {
                // Different name and prefix
                NewCommandDefinition("orgid", "pi", "pi", "the top org grouped command", TestAliasOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>), defaultArg: "key1").Item1,

                // Same name and prefix with args
                NewCommandDefinition("orgid:authid", "auth", "pi auth", "the auth grouped command", TestAliasOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("orgid:authid:loginid", "login", "pi auth login", "the login command within the auth group", TestAliasOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>)).Item1,
            };

            GroupedOptionsCommands = new()
            {
                // Different name and prefix
                NewCommandDefinition("orgid", "pi", "pi", "top org cmd group", TestOptionsDescriptors, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("orgid:authid", "auth", "pi auth", "org auth cmd group", TestOptionsDescriptors, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("orgid:authid:loginid", "login", "pi auth login", "org auth login cmd", TestOptionsDescriptors, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("orgid:authid:sloginid", "slogin", "pi auth slogin", "org auth slogin cmd", TestOptionsDescriptors, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>), defaultArg: "key2").Item1,

                // Same name and prefix with args
                NewCommandDefinition("orgid:authid:sloginid:oidc", "oidc", "pi auth slogin oidc", "org auth slogin oidc cmd", TestOptionsDescriptors, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("orgid:authid:sloginid:oauth", "oauth", "pi auth slogin oauth", "org auth slogin oauth cmd", TestOptionsDescriptors, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>), defaultArg: "key1").Item1,
            };

            LicensingCommands = new()
            {
                // Different name and prefix
                NewCommandDefinition("root1", "name1", "prefix1", "desc1", TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>), isRoot: true).Item1,

                // Different name and prefix
                NewCommandDefinition("root2", "name2", "prefix2", "desc2", TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>), isRoot: true).Item1,

                // Different name and prefix
                NewCommandDefinition("root3", "name3", "prefix3", "desc3", TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>), isRoot: true).Item1,

                // Different name and prefix
                NewCommandDefinition("grp1", "name1", "prefix1", "desc1", TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>), isGroup: true).Item1,

                // Different name and prefix
                NewCommandDefinition("grp2", "name2", "prefix2", "desc2", TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>), isGroup: true).Item1,

                // Different name and prefix
                NewCommandDefinition("grp3", "name3", "prefix3", "desc3", TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>), isGroup: true).Item1,

                // Different name and prefix
                NewCommandDefinition("id1", "name1", "prefix1", "desc1", TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>)).Item1,

                // Same name and prefix with args
                NewCommandDefinition("id2", "name2", "name2", "desc2", TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>)).Item1,

                // Multiple prefix names
                NewCommandDefinition("id3", "name3", "prefix3 sub3 name3", "desc3", TestOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>)).Item1,

                // Command with no args
                NewCommandDefinition("id4", "name4", "prefix4_noargs", "desc4").Item1,

                // Command with no default arg
                NewCommandDefinition("id5", "name5", "prefix5_default", "desc5", TestDefaultOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>)).Item1,

                // Command with no default arg
                NewCommandDefinition("id6", "name6", "prefix6_empty_args", "desc6", new OptionDescriptors(new UnicodeTextHandler()), typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>)).Item1,

                // Command with default arg
                NewCommandDefinition("id7", "name7", "prefix7_defaultarg", "desc7", TestDefaultOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>), defaultArg: "key1").Item1,

                // Command with default arg
                NewCommandDefinition("id8", "name8", "prefix8_defaultarg_defaultvalue", "desc8", TestDefaultOptionValueDescriptors, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>), defaultArg: "key1").Item1,
            };

            UnicodeCommands = new()
            {
                // --- Hindi --- Root command
                NewCommandDefinition("uc1", "यूनिकोड", "यूनिकोड", "यूनिकोड रूट कमांड", null, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>), isRoot: true).Item1,

                // Grouped command
                NewCommandDefinition("uc2", "परीक्षण", "यूनिकोड परीक्षण", "यूनिकोड समूहीकृत कमांड", null, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>), isGroup: true).Item1,

                // Subcommand
                NewCommandDefinition("uc3", "प्रिंट", "यूनिकोड परीक्षण प्रिंट", "प्रिंट कमांड", TestHindiUnicodeOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>)).Item1,

                // subcommand with default option
                NewCommandDefinition("uc4", "दूसरा", "यूनिकोड परीक्षण दूसरा", "दूसरा आदेश", TestHindiUnicodeOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>), defaultArg: "एक").Item1,

                // --- Chinese --- Root command
                NewCommandDefinition("uc5", "統一碼", "統一碼", "示例根命令描述", null, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>), isRoot: true).Item1,

                // Grouped command
                NewCommandDefinition("uc6", "測試", "統一碼 測試", "示例分組命令", null, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>), isGroup: true).Item1,

                // Subcommand
                NewCommandDefinition("uc7", "打印", "統一碼 測試 打印", "測試命令", TestChineseUnicodeOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>)).Item1,

                // subcommand with default option
                NewCommandDefinition("uc8", "備用", "統一碼 測試 備用", "替代描述", TestChineseUnicodeOptionDescriptors, typeof(CommandChecker), typeof(CommandRunner <CommandRunnerResult>), defaultArg: "第一的").Item1,
            };
        }

        public static Tuple<CommandDescriptor, Command> NewCommandDefinition(string id, string name, string prefix, string desc, OptionDescriptors? args = null, Type? checker = null, Type? runner = null, string? defaultArg = null, bool? isRoot = false, bool? isGroup = false, Options? options = null)
        {
            var cmd1 = new CommandDescriptor(id, name, prefix, desc, args, defaultOption: defaultArg)
            {
                // Internal set, in prod apps this will be set by DI Addxxx methods
                Checker = checker,
                Runner = runner,
                IsGroup = isGroup.GetValueOrDefault(),
                IsRoot = isRoot.GetValueOrDefault()
            };

            return new Tuple<CommandDescriptor, Command>(cmd1, new Command(new CommandRoute("id1", "test"), cmd1, options));
        }

        public static List<CommandDescriptor> AliasCommands;
        public static List<CommandDescriptor> Commands;
        public static List<CommandDescriptor> GroupedCommands;
        public static List<CommandDescriptor> GroupedOptionsCommands;
        public static List<CommandDescriptor> LicensingCommands;
        public static OptionDescriptors TestAliasOptionDescriptors;
        public static OptionDescriptors TestOptionDescriptors;
        public static OptionDescriptors TestChineseUnicodeOptionDescriptors;
        public static OptionDescriptors TestDefaultOptionDescriptors;
        public static OptionDescriptors TestDefaultOptionValueDescriptors;
        public static OptionDescriptors TestHindiUnicodeOptionDescriptors;
        public static OptionDescriptors TestOptionsDescriptors;
        public static List<CommandDescriptor> UnicodeCommands;
    }
}