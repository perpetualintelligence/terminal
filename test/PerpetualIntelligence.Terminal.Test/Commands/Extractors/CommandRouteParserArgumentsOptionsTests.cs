/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Terminal.Mocks;
using PerpetualIntelligence.Terminal.Stores;
using PerpetualIntelligence.Terminal.Stores.InMemory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace PerpetualIntelligence.Terminal.Commands.Extractors
{
    public class CommandRouteParserArgumentsOptionsTests
    {
        public CommandRouteParserArgumentsOptionsTests()
        {
            argumentDescriptors = new ArgumentDescriptors(new AsciiTextHandler(), new List<ArgumentDescriptor>() {
                new ArgumentDescriptor(1, "arg1", nameof(String), "Argument1", ArgumentFlags.None),
                new ArgumentDescriptor(2, "arg2", nameof(Int32), "Argument2", ArgumentFlags.Required),
                new ArgumentDescriptor(3, "arg3", nameof(Boolean), "Argument3", ArgumentFlags.None),
                new ArgumentDescriptor(4, "arg4", nameof(Double), "Argument4", ArgumentFlags.None),
                new ArgumentDescriptor(5, "arg5", nameof(Int64), "Argument5", ArgumentFlags.None),
                new ArgumentDescriptor(6, "arg6", nameof(String), "Argument6", ArgumentFlags.None),
                new ArgumentDescriptor(7, "arg7", nameof(DateOnly), "Argument7", ArgumentFlags.None),
                new ArgumentDescriptor(8, "arg8", nameof(DateTime), "Argument8", ArgumentFlags.None),
                new ArgumentDescriptor(9, "arg9", nameof(String), "Argument9", ArgumentFlags.None),
            });

            optionDescriptors = new OptionDescriptors(new AsciiTextHandler(), new List<OptionDescriptor>() {
                new OptionDescriptor( "opt1", nameof(Int32), "Option1", OptionFlags.None),
                new OptionDescriptor( "opt2", nameof(String), "Option2", OptionFlags.Required),
                new OptionDescriptor( "opt3", nameof(String), "Option3", OptionFlags.Disabled),
                new OptionDescriptor( "opt4", nameof(String), "Option4", OptionFlags.Obsolete),
                new OptionDescriptor( "opt5", nameof(Double), "Option5", OptionFlags.None, alias:"opt5_a"),
                new OptionDescriptor( "opt6", nameof(DateTime), "Option6", OptionFlags.Required, alias:"opt6_a"),
                new OptionDescriptor( "opt7", nameof(Boolean), "Option7", OptionFlags.None, alias:"opt7_a"),
                new OptionDescriptor( "opt8", nameof(Boolean), "Option8", OptionFlags.Required, alias:"opt8_a"),
            });

            terminalOptions = MockTerminalOptions.NewAliasOptions();
            textHandler = new AsciiTextHandler();
            commandDescriptors = new Dictionary<string, CommandDescriptor>()
            {
               { "root1", new CommandDescriptor("root1", "root1_name", "root1_desc", CommandType.Root, CommandFlags.None, argumentDescriptors: argumentDescriptors, optionDescriptors: optionDescriptors) },
               { "grp1", new CommandDescriptor("grp1", "grp1_name", "grp1_desc", CommandType.Group, CommandFlags.None, new OwnerCollection("root1"),  argumentDescriptors: argumentDescriptors, optionDescriptors: optionDescriptors) },
               { "cmd1", new CommandDescriptor("cmd1", "cmd1_name", "cmd1_desc", CommandType.SubCommand, CommandFlags.None, new OwnerCollection("grp1"),  argumentDescriptors: argumentDescriptors, optionDescriptors: optionDescriptors) },
               { "cmd_nr2", new CommandDescriptor("cmd_nr2", "cmd_nr2_name", "cmd_nr2_desc", CommandType.SubCommand, CommandFlags.None,  argumentDescriptors: argumentDescriptors, optionDescriptors: optionDescriptors) }
            };
            commandStoreHandler = new InMemoryCommandStore(commandDescriptors);
            logger = new NullLogger<CommandRouteParser>();

            commandRouteParser = new CommandRouteParser(textHandler, commandStoreHandler, terminalOptions, logger);
        }

        [Fact]
        public async Task Delimited_Stays_As_Is()
        {
            string cmdStr = "root1 grp1 cmd1 \"  -- - -arg1 asd&&&*ASD**ASD&A* value  --\" --opt3 \"--option    -delimited   ^#%^@#&*&* JJKJKASD value3    \"";
            var result = await commandRouteParser.ParseAsync(new CommandRoute("id1", cmdStr));

            // Command
            result.Command.Id.Should().Be("cmd1");
            result.Command.Arguments.Should().NotBeNull();
            result.Command.Options.Should().NotBeNull();

            // Arguments
            result.Command.Arguments!.Count.Should().Be(1);
            result.Command.Arguments[0].Id.Should().Be("arg1");
            result.Command.Arguments[0].Value.Should().Be("  -- - -arg1 asd&&&*ASD**ASD&A* value  --");

            // Options
            result.Command.Options!.Count.Should().Be(1);
            Option opt1 = result.Command.Options["opt3"];
            opt1.Value.Should().Be("--option    -delimited   ^#%^@#&*&* JJKJKASD value3    ");
        }

        [Fact]
        public async Task Arguments_And_Options_Are_Processed_Correctly()
        {
            string cmdStr = "root1 grp1 cmd1 \"arg1 value\" 32 true 35.987 3435345345 arg6value 12/23/2023 12/23/2022:12:23:22 \"arg9 value\" -opt7_a --opt3 \"option delimited value3\" --opt4 option value4    with multiple  spaces --opt5 35.987 --opt1 34 -opt6_a 12/23/2023 --opt2 option value2 -opt8_a true";
            var result = await commandRouteParser.ParseAsync(new CommandRoute("id1", cmdStr));

            // Command
            result.Command.Id.Should().Be("cmd1");
            result.Command.Arguments.Should().NotBeNull();
            result.Command.Options.Should().NotBeNull();

            // Arguments
            result.Command.Arguments!.Count.Should().Be(9);

            result.Command.Arguments[0].Id.Should().Be("arg1");
            result.Command.Arguments[0].Value.Should().Be("arg1 value");

            result.Command.Arguments[1].Id.Should().Be("arg2");
            result.Command.Arguments[1].Value.Should().Be("32");

            result.Command.Arguments[2].Id.Should().Be("arg3");
            result.Command.Arguments[2].Value.Should().Be("true");

            result.Command.Arguments[3].Id.Should().Be("arg4");
            result.Command.Arguments[3].Value.Should().Be("35.987");

            result.Command.Arguments[4].Id.Should().Be("arg5");
            result.Command.Arguments[4].Value.Should().Be("3435345345");

            result.Command.Arguments[5].Id.Should().Be("arg6");
            result.Command.Arguments[5].Value.Should().Be("arg6value");

            result.Command.Arguments[6].Id.Should().Be("arg7");
            result.Command.Arguments[6].Value.Should().Be("12/23/2023");

            result.Command.Arguments[7].Id.Should().Be("arg8");
            result.Command.Arguments[7].Value.Should().Be("12/23/2022:12:23:22");

            result.Command.Arguments[8].Id.Should().Be("arg9");
            result.Command.Arguments[8].Value.Should().Be("arg9 value");

            // 8 options + 4 aliases
            result.Command.Options!.Count.Should().Be(12);

            Option opt1 = result.Command.Options["opt1"];
            opt1.Value.Should().Be("34");

            Option opt2 = result.Command.Options["opt2"];
            opt2.Value.Should().Be("option value2");

            Option opt3 = result.Command.Options["opt3"];
            opt3.Value.Should().Be("option delimited value3");

            Option opt4 = result.Command.Options["opt4"];
            opt4.Value.Should().Be("option value4    with multiple  spaces");

            Option opt5 = result.Command.Options["opt5"];
            opt5.Value.Should().Be("35.987");
            Option opt5Alias = result.Command.Options["opt5_a"];
            opt5Alias.Value.Should().Be("35.987");
            result.Command.Options["opt5"].Should().BeSameAs(result.Command.Options["opt5_a"]);

            Option opt6 = result.Command.Options["opt6"];
            opt6.Value.Should().Be("12/23/2023");
            Option opt6Alias = result.Command.Options["opt6_a"];
            opt6Alias.Value.Should().Be("12/23/2023");
            result.Command.Options["opt6"].Should().BeSameAs(result.Command.Options["opt6_a"]);

            Option opt7 = result.Command.Options["opt7"];
            opt7.Value.Should().Be(true.ToString()); // Value is not provided so it is set to true (true.ToString())
            Option opt7Alias = result.Command.Options["opt7_a"];
            opt7Alias.Value.Should().Be(true.ToString());
            result.Command.Options["opt7"].Should().BeSameAs(result.Command.Options["opt7_a"]);

            Option opt8 = result.Command.Options["opt8"];
            opt8.Value.Should().Be("true");
            Option opt8Alias = result.Command.Options["opt8_a"];
            opt8Alias.Value.Should().Be("true");
            result.Command.Options["opt8"].Should().BeSameAs(result.Command.Options["opt8_a"]);
        }

        [Fact]
        public async Task Argument_Without_Closing_Delimiter_Throws()
        {
            Func<Task> act = async () => await commandRouteParser.ParseAsync(new CommandRoute("id1", "root1 grp1 cmd1 32 \"arg2 value true 35.987 3435345345 2312.123123 12/23/2023 12/23/2022:12:23:22"));
            await act.Should().ThrowAsync<ErrorException>().WithMessage("The argument value is missing the closing delimiter. argument=\"arg2 value true 35.987 3435345345 2312.123123 12/23/2023 12/23/2022:12:23:22");
        }

        [Fact]
        public async Task Options_Are_Processed_Correctly_In_Randomized_Order()
        {
            var options = "--opt7 --opt3 \"option delimited value3\" --opt4 option value4    with multiple  spaces --opt5 35.987 --opt1 34 --opt6 12/23/2023 --opt2 option value2 --opt8 false";
            var result = await commandRouteParser.ParseAsync(new CommandRoute("id1", "root1 grp1 cmd1 " + options));

            result.Command.Id.Should().Be("cmd1");
            result.Command.Options.Should().NotBeNull();

            // 8 options + 4 aliases
            result.Command.Options!.Count.Should().Be(12);

            Option opt1 = result.Command.Options["opt1"];
            opt1.Value.Should().Be("34");

            Option opt2 = result.Command.Options["opt2"];
            opt2.Value.Should().Be("option value2");

            Option opt3 = result.Command.Options["opt3"];
            opt3.Value.Should().Be("option delimited value3");

            Option opt4 = result.Command.Options["opt4"];
            opt4.Value.Should().Be("option value4    with multiple  spaces");

            Option opt5 = result.Command.Options["opt5"];
            opt5.Value.Should().Be("35.987");
            Option opt5Alias = result.Command.Options["opt5_a"];
            opt5Alias.Value.Should().Be("35.987");
            result.Command.Options["opt5"].Should().BeSameAs(result.Command.Options["opt5_a"]);

            Option opt6 = result.Command.Options["opt6"];
            opt6.Value.Should().Be("12/23/2023");
            Option opt6Alias = result.Command.Options["opt6_a"];
            opt6Alias.Value.Should().Be("12/23/2023");
            result.Command.Options["opt6"].Should().BeSameAs(result.Command.Options["opt6_a"]);

            Option opt7 = result.Command.Options["opt7"];
            opt7.Value.Should().Be(true.ToString()); // Value is not provided so it is set to true (true.ToString())
            Option opt7Alias = result.Command.Options["opt7_a"];
            opt7Alias.Value.Should().Be(true.ToString());
            result.Command.Options["opt7"].Should().BeSameAs(result.Command.Options["opt7_a"]);

            Option opt8 = result.Command.Options["opt8"];
            opt8.Value.Should().Be("false");
            Option opt8Alias = result.Command.Options["opt8_a"];
            opt8Alias.Value.Should().Be("false");
            result.Command.Options["opt8"].Should().BeSameAs(result.Command.Options["opt8_a"]);

            result.Command.Arguments.Should().BeNull();
        }

        [Theory]
        [InlineData("--optinvalid", "optinvalid")]
        [InlineData("-optinvalid_alias", "optinvalid_alias")]
        [InlineData("--opt-in-valid", "opt-in-valid")]
        [InlineData("-optinvalid", "optinvalid")]
        public async Task Invalid_Option_Throws(string invalidOpt, string errOpt)
        {
            Func<Task> act = async () => await commandRouteParser.ParseAsync(new CommandRoute("id1", "root1 grp1 cmd1 " + invalidOpt));
            await act.Should().ThrowAsync<ErrorException>().WithMessage($"The command does not support an option or its alias. command=cmd1 option={errOpt}");
        }

        [Fact]
        public async Task Alias_With_Option_Prefix_Throws()
        {
            // opt7_a is an alias for opt7 so we cannot use -- prefix we have to use - alias prefix
            var options = "--opt7_a";
            Func<Task> act = async () => await commandRouteParser.ParseAsync(new CommandRoute("id1", "root1 grp1 cmd1 " + options));
            await act.Should().ThrowAsync<ErrorException>().WithMessage("The option prefix is not valid for an alias. option=opt7_a");
        }

        [Fact]
        public async Task Option_With_Alias_Prefix_Throws()
        {
            // opt7 is an option so we cannot use - alias prefix we have to use -- option prefix
            var options = "-opt7";
            Func<Task> act = async () => await commandRouteParser.ParseAsync(new CommandRoute("id1", "root1 grp1 cmd1 " + options));
            await act.Should().ThrowAsync<ErrorException>().WithMessage("The alias prefix is not valid for an option. option=-opt7");
        }

        [Fact]
        public async Task Option_With_Single_Delimiter_Value_Works()
        {
            var result = await commandRouteParser.ParseAsync(new CommandRoute("id1", "root1 grp1 cmd1 --opt3 \"  option    delimited  value3  \""));

            result.Command.Id.Should().Be("cmd1");
            result.Command.Options.Should().NotBeNull();
            result.Command.Arguments.Should().BeNull();

            result.Command.Options!.Count.Should().Be(1);
            Option opt1 = result.Command.Options["opt3"];
            opt1.Value.Should().Be("  option    delimited  value3  ");
        }

        [Fact]
        public async Task Option_With_Single_Delimiter_Multiple_Separator_Value_Works()
        {
            var result = await commandRouteParser.ParseAsync(new CommandRoute("id1", "root1 grp1 cmd1 --opt3       \"  option    delimited  value3  \"      "));

            result.Command.Id.Should().Be("cmd1");
            result.Command.Options.Should().NotBeNull();
            result.Command.Arguments.Should().BeNull();

            result.Command.Options!.Count.Should().Be(1);
            Option opt1 = result.Command.Options["opt3"];
            opt1.Value.Should().Be("  option    delimited  value3  ");
        }


        [Fact]
        public async Task Argument_With_Single_Delimiter_Value_Works()
        {
            var result = await commandRouteParser.ParseAsync(new CommandRoute("id1", "root1 grp1 cmd1 \"  argument    delimited  value3  \""));

            result.Command.Id.Should().Be("cmd1");
            result.Command.Options.Should().BeNull();
            result.Command.Arguments.Should().NotBeNull();

            result.Command.Arguments!.Count.Should().Be(1);
            result.Command.Arguments![0].Value.Should().Be("  argument    delimited  value3  ");
        }

        [Fact]
        public async Task Option_With_Single_Boolean_Works_Without_Value()
        {
            var result = await commandRouteParser.ParseAsync(new CommandRoute("id1", "root1 grp1 cmd1 --opt8"));

            result.Command.Id.Should().Be("cmd1");
            result.Command.Options.Should().NotBeNull();

            // 1 option + 1 alias
            result.Command.Options!.Count.Should().Be(2);
            result.Command.Arguments.Should().BeNull();

            Option opt1 = result.Command.Options["opt8"];
            opt1.Value.Should().Be(true.ToString());
            Option opt1Alias = result.Command.Options["opt8_a"];
            opt1Alias.Value.Should().Be(true.ToString());
        }

        [Fact]
        public async Task Option_With_Single_Boolean_Works_With_Value()
        {
            var result = await commandRouteParser.ParseAsync(new CommandRoute("id1", "root1 grp1 cmd1 --opt8 false"));

            result.Command.Id.Should().Be("cmd1");
            result.Command.Options.Should().NotBeNull();

            // 1 option + 1 alias
            result.Command.Options!.Count.Should().Be(2);
            result.Command.Arguments.Should().BeNull();

            Option opt1 = result.Command.Options["opt8"];
            opt1.Value.Should().Be("false");
            Option opt1Alias = result.Command.Options["opt8_a"];
            opt1Alias.Value.Should().Be("false");
        }

        [Fact]
        public async Task Options_Are_Processed_Correctly_With_Alias()
        {
            var options = "-opt7_a --opt3 \"option delimited value3\" --opt4 option value4    with multiple  spaces --opt5 35.987 --opt1 34 -opt6_a 12/23/2023 --opt2 option value2 -opt8_a false";
            var result = await commandRouteParser.ParseAsync(new CommandRoute("id1", "root1 grp1 cmd1 " + options));

            result.Command.Id.Should().Be("cmd1");
            result.Command.Options.Should().NotBeNull();

            // 8 options + 4 aliases
            result.Command.Options!.Count.Should().Be(12);

            Option opt1 = result.Command.Options["opt1"];
            opt1.Value.Should().Be("34");

            Option opt2 = result.Command.Options["opt2"];
            opt2.Value.Should().Be("option value2");

            Option opt3 = result.Command.Options["opt3"];
            opt3.Value.Should().Be("option delimited value3");

            Option opt4 = result.Command.Options["opt4"];
            opt4.Value.Should().Be("option value4    with multiple  spaces");

            Option opt5 = result.Command.Options["opt5"];
            opt5.Value.Should().Be("35.987");
            Option opt5Alias = result.Command.Options["opt5_a"];
            opt5Alias.Value.Should().Be("35.987");
            result.Command.Options["opt5"].Should().BeSameAs(result.Command.Options["opt5_a"]);

            Option opt6 = result.Command.Options["opt6"];
            opt6.Value.Should().Be("12/23/2023");
            Option opt6Alias = result.Command.Options["opt6_a"];
            opt6Alias.Value.Should().Be("12/23/2023");
            result.Command.Options["opt6"].Should().BeSameAs(result.Command.Options["opt6_a"]);

            Option opt7 = result.Command.Options["opt7"];
            opt7.Value.Should().Be(true.ToString()); // Value is not provided so it is set to true (true.ToString())
            Option opt7Alias = result.Command.Options["opt7_a"];
            opt7Alias.Value.Should().Be(true.ToString());
            result.Command.Options["opt7"].Should().BeSameAs(result.Command.Options["opt7_a"]);

            Option opt8 = result.Command.Options["opt8"];
            opt8.Value.Should().Be("false");
            Option opt8Alias = result.Command.Options["opt8_a"];
            opt8Alias.Value.Should().Be("false");
            result.Command.Options["opt8"].Should().BeSameAs(result.Command.Options["opt8_a"]);

            result.Command.Arguments.Should().BeNull();
        }

        [Fact]
        public async Task Options_With_Multiple_Separators_Are_Processed_Correctly()
        {
            string cmdStr = "root1 grp1 cmd1   --opt1    34 --opt2  option    value2 --opt3     \"   option    delimited    value3    \"     --opt4    option value4       with multiple  spaces    --opt5  35.987 --opt6 12/23/2023 --opt7                --opt8 false    ";
            var result = await commandRouteParser.ParseAsync(new CommandRoute("id1", cmdStr));

            result.Command.Id.Should().Be("cmd1");
            result.Command.Options.Should().NotBeNull();

            // 8 options + 4 aliases
            result.Command.Options!.Count.Should().Be(12);

            Option opt1 = result.Command.Options["opt1"];
            opt1.Value.Should().Be("34");

            Option opt2 = result.Command.Options["opt2"];
            opt2.Value.Should().Be("option    value2");

            Option opt3 = result.Command.Options["opt3"];
            opt3.Value.Should().Be("   option    delimited    value3    ");

            Option opt4 = result.Command.Options["opt4"];
            opt4.Value.Should().Be("option value4       with multiple  spaces");

            Option opt5 = result.Command.Options["opt5"];
            opt5.Value.Should().Be("35.987");
            Option opt5Alias = result.Command.Options["opt5_a"];
            opt5Alias.Value.Should().Be("35.987");
            result.Command.Options["opt5"].Should().BeSameAs(result.Command.Options["opt5_a"]);

            Option opt6 = result.Command.Options["opt6"];
            opt6.Value.Should().Be("12/23/2023");
            Option opt6Alias = result.Command.Options["opt6_a"];
            opt6Alias.Value.Should().Be("12/23/2023");
            result.Command.Options["opt6"].Should().BeSameAs(result.Command.Options["opt6_a"]);

            Option opt7 = result.Command.Options["opt7"];
            opt7.Value.Should().Be(true.ToString()); // Value is not provided so it is set to true (true.ToString())
            Option opt7Alias = result.Command.Options["opt7_a"];
            opt7Alias.Value.Should().Be(true.ToString());
            result.Command.Options["opt7"].Should().BeSameAs(result.Command.Options["opt7_a"]);

            Option opt8 = result.Command.Options["opt8"];
            opt8.Value.Should().Be("false");
            Option opt8Alias = result.Command.Options["opt8_a"];
            opt8Alias.Value.Should().Be("false");
            result.Command.Options["opt8"].Should().BeSameAs(result.Command.Options["opt8_a"]);

            result.Command.Arguments.Should().BeNull();
        }

        [Fact]
        public async Task Options_Are_Processed_Correctly()
        {
            string cmdStr = "root1 grp1 cmd1 --opt1 34 --opt2 option value2 --opt3 \"option delimited value3\" --opt4 option value4    with multiple  spaces --opt5 35.987 --opt6 12/23/2023 --opt7 --opt8 false";
            var result = await commandRouteParser.ParseAsync(new CommandRoute("id1", cmdStr));

            result.Command.Id.Should().Be("cmd1");
            result.Command.Options.Should().NotBeNull();

            // 8 options + 4 aliases
            result.Command.Options!.Count.Should().Be(12);

            Option opt1 = result.Command.Options["opt1"];
            opt1.Value.Should().Be("34");

            Option opt2 = result.Command.Options["opt2"];
            opt2.Value.Should().Be("option value2");

            Option opt3 = result.Command.Options["opt3"];
            opt3.Value.Should().Be("option delimited value3");

            Option opt4 = result.Command.Options["opt4"];
            opt4.Value.Should().Be("option value4    with multiple  spaces");

            Option opt5 = result.Command.Options["opt5"];
            opt5.Value.Should().Be("35.987");
            Option opt5Alias = result.Command.Options["opt5_a"];
            opt5Alias.Value.Should().Be("35.987");
            result.Command.Options["opt5"].Should().BeSameAs(result.Command.Options["opt5_a"]);

            Option opt6 = result.Command.Options["opt6"];
            opt6.Value.Should().Be("12/23/2023");
            Option opt6Alias = result.Command.Options["opt6_a"];
            opt6Alias.Value.Should().Be("12/23/2023");
            result.Command.Options["opt6"].Should().BeSameAs(result.Command.Options["opt6_a"]);

            Option opt7 = result.Command.Options["opt7"];
            opt7.Value.Should().Be(true.ToString()); // Value is not provided so it is set to true (true.ToString())
            Option opt7Alias = result.Command.Options["opt7_a"];
            opt7Alias.Value.Should().Be(true.ToString());
            result.Command.Options["opt7"].Should().BeSameAs(result.Command.Options["opt7_a"]);

            Option opt8 = result.Command.Options["opt8"];
            opt8.Value.Should().Be("false");
            Option opt8Alias = result.Command.Options["opt8_a"];
            opt8Alias.Value.Should().Be("false");
            result.Command.Options["opt8"].Should().BeSameAs(result.Command.Options["opt8_a"]);

            result.Command.Arguments.Should().BeNull();
        }

        [Fact]
        public async Task Arguments_Are_Processed_In_Order()
        {
            var result = await commandRouteParser.ParseAsync(new CommandRoute("id1", "root1 grp1 cmd1 \"arg1 value\" 32 true 35.987 3435345345 arg6value 12/23/2023 12/23/2022:12:23:22 \"arg9 value\""));

            result.Command.Id.Should().Be("cmd1");

            result.Command.Arguments.Should().NotBeNull();
            result.Command.Arguments!.Count.Should().Be(9);

            result.Command.Arguments[0].Id.Should().Be("arg1");
            result.Command.Arguments[0].Value.Should().Be("arg1 value");

            result.Command.Arguments[1].Id.Should().Be("arg2");
            result.Command.Arguments[1].Value.Should().Be("32");

            result.Command.Arguments[2].Id.Should().Be("arg3");
            result.Command.Arguments[2].Value.Should().Be("true");

            result.Command.Arguments[3].Id.Should().Be("arg4");
            result.Command.Arguments[3].Value.Should().Be("35.987");

            result.Command.Arguments[4].Id.Should().Be("arg5");
            result.Command.Arguments[4].Value.Should().Be("3435345345");

            result.Command.Arguments[5].Id.Should().Be("arg6");
            result.Command.Arguments[5].Value.Should().Be("arg6value");

            result.Command.Arguments[6].Id.Should().Be("arg7");
            result.Command.Arguments[6].Value.Should().Be("12/23/2023");

            result.Command.Arguments[7].Id.Should().Be("arg8");
            result.Command.Arguments[7].Value.Should().Be("12/23/2022:12:23:22");

            result.Command.Arguments[8].Id.Should().Be("arg9");
            result.Command.Arguments[8].Value.Should().Be("arg9 value");

            result.Command.Options.Should().BeNull();
        }

        [Fact]
        public async Task Arguments_With_Multiple_Separator_Not_Delimited_Exceed_Argument_Limit_Throws()
        {
            // Here arg9 and value are not delimited so they are considered as two arguments and exceed the limit of 9 arguments
            Func<Task> act = async () => await commandRouteParser.ParseAsync(new CommandRoute("id1", " root1   grp1  cmd1           \"arg1    value\"   32   true       35.987 3435345345       arg6value      12/23/2023 12/23/2022:12:23:22     arg9   value   "));
            await act.Should().ThrowAsync<ErrorException>().WithMessage("The command does not support 10 arguments. command=cmd1 arguments=arg1    value,32,true,35.987,3435345345,arg6value,12/23/2023,12/23/2022:12:23:22,arg9,value");
        }

        [Fact]
        public async Task Arguments_With_Multiple_Separator_Are_Processed_In_Order()
        {
            var result = await commandRouteParser.ParseAsync(new CommandRoute("id1", " root1   grp1  cmd1           \"arg1    value\"   32   true       35.987 3435345345       arg6value      12/23/2023 12/23/2022:12:23:22  \"   arg9   value   \""));

            result.Command.Id.Should().Be("cmd1");

            result.Command.Arguments.Should().NotBeNull();
            result.Command.Arguments!.Count.Should().Be(9);

            result.Command.Arguments[0].Id.Should().Be("arg1");
            result.Command.Arguments[0].Value.Should().Be("arg1    value");

            result.Command.Arguments[1].Id.Should().Be("arg2");
            result.Command.Arguments[1].Value.Should().Be("32");

            result.Command.Arguments[2].Id.Should().Be("arg3");
            result.Command.Arguments[2].Value.Should().Be("true");

            result.Command.Arguments[3].Id.Should().Be("arg4");
            result.Command.Arguments[3].Value.Should().Be("35.987");

            result.Command.Arguments[4].Id.Should().Be("arg5");
            result.Command.Arguments[4].Value.Should().Be("3435345345");

            result.Command.Arguments[5].Id.Should().Be("arg6");
            result.Command.Arguments[5].Value.Should().Be("arg6value");

            result.Command.Arguments[6].Id.Should().Be("arg7");
            result.Command.Arguments[6].Value.Should().Be("12/23/2023");

            result.Command.Arguments[7].Id.Should().Be("arg8");
            result.Command.Arguments[7].Value.Should().Be("12/23/2022:12:23:22");

            result.Command.Arguments[8].Id.Should().Be("arg9");
            result.Command.Arguments[8].Value.Should().Be("   arg9   value   ");

            result.Command.Options.Should().BeNull();
        }

        [Fact]
        public async Task Delimited_Argument_Processes_String_As_Is()
        {
            var result = await commandRouteParser.ParseAsync(new CommandRoute("id1", "root1 grp1 cmd1 \"cmd2 'cmd2_arg1' 'cmd2_arg2 value' cmd2_arg3 --opt1 val1 --opt2 val2 -opt3 --opt4 'c:\\\\somedir\\somepath\\somefile.txt'\""));

            result.Command.Id.Should().Be("cmd1");
            result.Command.Arguments.Should().NotBeNull();
            result.Command.Arguments!.Count.Should().Be(1);

            result.Command.Arguments[0].Id.Should().Be("arg1");
            result.Command.Arguments[0].Value.Should().Be("cmd2 'cmd2_arg1' 'cmd2_arg2 value' cmd2_arg3 --opt1 val1 --opt2 val2 -opt3 --opt4 'c:\\\\somedir\\somepath\\somefile.txt'");
        }

        [Fact]
        public async Task Multiple_Delimited_Strips_First_Delimited_Pair()
        {
            var result = await commandRouteParser.ParseAsync(new CommandRoute("id1", "root1 grp1 cmd1 \"\"\"arg1 delimited value\"\"\""));

            result.Command.Id.Should().Be("cmd1");
            result.Command.Arguments.Should().NotBeNull();
            result.Command.Arguments!.Count.Should().Be(1);

            result.Command.Arguments[0].Id.Should().Be("arg1");
            result.Command.Arguments[0].Value.Should().Be("\"\"arg1 delimited value\"\"");
        }

        [Theory]
        [InlineData("\"   arg1  delimited     value     \"", "   arg1  delimited     value     ")]
        [InlineData("\"arg1  delimited     value     \"", "arg1  delimited     value     ")]
        [InlineData("\"   arg1  delimited     value\"", "   arg1  delimited     value")]
        [InlineData("\"arg1 delimited value\"", "arg1 delimited value")]
        [InlineData("\"arg1   delimited         value\"", "arg1   delimited         value")]
        public async Task Separators_In_Delimited_Argument_Value_Are_Preserved(string arg, string preserved)
        {
            var result = await commandRouteParser.ParseAsync(new CommandRoute("id1", $"root1 grp1 cmd1 {arg}"));

            result.Command.Id.Should().Be("cmd1");
            result.Command.Arguments.Should().NotBeNull();
            result.Command.Arguments!.Count.Should().Be(1);

            result.Command.Arguments[0].Id.Should().Be("arg1");
            result.Command.Arguments[0].Value.Should().Be(preserved);
        }

        [Fact]
        public async Task Nested_Delimited_Gives_Inconsistent_Result()
        {
            var result = await commandRouteParser.ParseAsync(new CommandRoute("id1", "root1 grp1 cmd1 \"arg1 \"nested delimited value\" value\""));

            result.Command.Id.Should().Be("cmd1");
            result.Command.Arguments.Should().NotBeNull();
            result.Command.Arguments!.Count.Should().Be(2);

            result.Command.Arguments[0].Id.Should().Be("arg1");
            result.Command.Arguments[0].Value.Should().Be("arg1 \"nested delimited value");

            result.Command.Arguments[1].Id.Should().Be("arg2");
            result.Command.Arguments[1].Value.Should().Be("value");
        }

        [Fact]
        public async Task Multiple_Delimited_Arguments_Are_Processed_In_Order()
        {
            var result = await commandRouteParser.ParseAsync(new CommandRoute("id1", "root1 grp1 cmd1 \"arg1 value\" \"32\" \"true\" \"35.987\" 3435345345 \"arg6value\" 12/23/2023 12/23/2022:12:23:22 \"arg9 value\""));
            result.Command.Id.Should().Be("cmd1");

            result.Command.Arguments.Should().NotBeNull();
            result.Command.Arguments!.Count.Should().Be(9);

            result.Command.Arguments[0].Id.Should().Be("arg1");
            result.Command.Arguments[0].Value.Should().Be("arg1 value");

            result.Command.Arguments[1].Id.Should().Be("arg2");
            result.Command.Arguments[1].Value.Should().Be("32");

            result.Command.Arguments[2].Id.Should().Be("arg3");
            result.Command.Arguments[2].Value.Should().Be("true");

            result.Command.Arguments[3].Id.Should().Be("arg4");
            result.Command.Arguments[3].Value.Should().Be("35.987");

            result.Command.Arguments[4].Id.Should().Be("arg5");
            result.Command.Arguments[4].Value.Should().Be("3435345345");

            result.Command.Arguments[5].Id.Should().Be("arg6");
            result.Command.Arguments[5].Value.Should().Be("arg6value");

            result.Command.Arguments[6].Id.Should().Be("arg7");
            result.Command.Arguments[6].Value.Should().Be("12/23/2023");

            result.Command.Arguments[7].Id.Should().Be("arg8");
            result.Command.Arguments[7].Value.Should().Be("12/23/2022:12:23:22");

            result.Command.Arguments[8].Id.Should().Be("arg9");
            result.Command.Arguments[8].Value.Should().Be("arg9 value");

            result.Command.Options.Should().BeNull();
        }

        private readonly TerminalOptions terminalOptions;
        private ITextHandler textHandler;
        private ICommandStoreHandler commandStoreHandler;
        private Dictionary<string, CommandDescriptor> commandDescriptors;
        private ArgumentDescriptors argumentDescriptors;
        private OptionDescriptors optionDescriptors;
        private ICommandRouteParser commandRouteParser;
        private ILogger<CommandRouteParser> logger;
    }
}