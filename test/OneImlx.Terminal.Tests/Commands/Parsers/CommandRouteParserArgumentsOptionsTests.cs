/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OneImlx.Terminal.Commands.Handlers;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Stores;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Commands.Parsers
{
    public class CommandRouteParserArgumentsOptionsTests
    {
        public CommandRouteParserArgumentsOptionsTests()
        {
            argumentDescriptors = new ArgumentDescriptors(new TerminalAsciiTextHandler(), [
                new(1, "arg1", nameof(String), "Argument1", ArgumentFlags.None),
                new(2, "arg2", nameof(Int32), "Argument2", ArgumentFlags.Required),
                new(3, "arg3", nameof(Boolean), "Argument3", ArgumentFlags.None),
                new(4, "arg4", nameof(Double), "Argument4", ArgumentFlags.None),
                new(5, "arg5", nameof(Int64), "Argument5", ArgumentFlags.None),
                new(6, "arg6", nameof(String), "Argument6", ArgumentFlags.None),
                new(7, "arg7", nameof(DateTimeOffset), "Argument7", ArgumentFlags.None),
                new(8, "arg8", nameof(DateTime), "Argument8", ArgumentFlags.None),
                new(9, "arg9", nameof(String), "Argument9", ArgumentFlags.None),
            ]);

            optionDescriptors = new OptionDescriptors(new TerminalAsciiTextHandler(), [
                new( "opt1", nameof(Int32), "Option1", OptionFlags.None),
                new( "opt2", nameof(String), "Option2", OptionFlags.Required),
                new( "opt3", nameof(String), "Option3", OptionFlags.Disabled),
                new( "opt4", nameof(String), "Option4", OptionFlags.Obsolete),
                new( "opt5", nameof(Double), "Option5", OptionFlags.None, alias:"opt5_a"),
                new( "opt6", nameof(DateTime), "Option6", OptionFlags.Required, alias:"opt6_a"),
                new( "opt7", nameof(Boolean), "Option7", OptionFlags.None, alias:"opt7_a"),
                new( "opt8", nameof(Boolean), "Option8", OptionFlags.Required, alias:"opt8_a"),
            ]);

            terminalOptions = MockTerminalOptions.NewAliasOptions();
            textHandler = new TerminalAsciiTextHandler();
            commandDescriptors = new(textHandler,
            [
                new("root1", "root1_name", "root1_desc", CommandType.Root, CommandFlags.None, argumentDescriptors: argumentDescriptors, optionDescriptors: optionDescriptors),
                new("grp1", "grp1_name", "grp1_desc", CommandType.Group, CommandFlags.None, new OwnerIdCollection("root1"),  argumentDescriptors: argumentDescriptors, optionDescriptors: optionDescriptors),
                new("cmd1", "cmd1_name", "cmd1_desc", CommandType.SubCommand, CommandFlags.None, new OwnerIdCollection("grp1"),  argumentDescriptors: argumentDescriptors, optionDescriptors: optionDescriptors),
                new("cmd_nr2", "cmd_nr2_name", "cmd_nr2_desc", CommandType.SubCommand, CommandFlags.None,  argumentDescriptors: argumentDescriptors, optionDescriptors: optionDescriptors)
            ]);
            commandStore = new TerminalInMemoryCommandStore(textHandler, commandDescriptors.Values);
            logger = new NullLogger<CommandRouteParser>();

            commandRouteParser = new CommandRouteParser(textHandler, commandStore, terminalOptions, logger);
        }

        [Fact]
        public async Task Alias_With_Option_Prefix_Throws()
        {
            // opt7_a is an alias for opt7 so we cannot use -- prefix we have to use - alias prefix
            var options = "--opt7_a";
            Func<Task> act = async () => await commandRouteParser.ParseRouteAsync(new TerminalProcessorRequest("id1", "root1 grp1 cmd1 " + options));
            await act.Should().ThrowAsync<TerminalException>().WithMessage("The option prefix is not valid for an alias. option=opt7_a");
        }

        [Fact]
        public async Task Argument_With_Single_Delimiter_Value_Works()
        {
            var result = await commandRouteParser.ParseRouteAsync(new TerminalProcessorRequest("id1", "root1 grp1 cmd1 \"  argument    delimited  value3  \""));

            result.Command.Id.Should().Be("cmd1");
            result.Command.Options.Should().BeNull();
            result.Command.Arguments.Should().NotBeNull();

            result.Command.Arguments!.Count.Should().Be(1);
            result.Command.Arguments![0].Value.Should().Be("  argument    delimited  value3  ");
        }

        [Fact]
        public async Task Argument_Without_Closing_Delimiter_Throws()
        {
            Func<Task> act = async () => await commandRouteParser.ParseRouteAsync(new TerminalProcessorRequest("id1", "root1 grp1 cmd1 32 \"arg2 value true 35.987 3435345345 2312.123123 12/23/2023 12/23/2022:12:23:22"));
            await act.Should().ThrowAsync<TerminalException>().WithMessage("The argument value is missing the closing delimiter. argument=\"arg2 value true 35.987 3435345345 2312.123123 12/23/2023 12/23/2022:12:23:22");
        }

        [Fact]
        public async Task Arguments_And_Options_Are_Processed_Correctly()
        {
            string cmdStr = "root1 grp1 cmd1 \"arg1 value\" 32 true 35.987 3435345345 arg6value 12/23/2023 12/23/2022:12:23:22 \"arg9 value\" -opt7_a --opt3 \"option delimited value3\" --opt4 option value4    with multiple  spaces --opt5 35.987 --opt1 34 -opt6_a 12/23/2023 --opt2 option value2 -opt8_a true";
            var result = await commandRouteParser.ParseRouteAsync(new TerminalProcessorRequest("id1", cmdStr));

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
        public async Task Arguments_And_Options_With_Different_Separator_Are_Processed_Correctly()
        {
            terminalOptions.Parser.OptionValueSeparator = "=";

            string cmdStr = "root1 grp1 cmd1 \"arg1 value\" 32 true 35.987 3435345345 arg6value 12/23/2023 12/23/2022:12:23:22 \"arg9 value\" -opt7_a --opt3=\"option delimited value3\" --opt4=option value4    with multiple  spaces --opt5=35.987 --opt1=34 -opt6_a 12/23/2023 --opt2=option value2 -opt8_a=true";
            var result = await commandRouteParser.ParseRouteAsync(new TerminalProcessorRequest("id1", cmdStr));

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
        public async Task Arguments_Are_Processed_In_Order()
        {
            var result = await commandRouteParser.ParseRouteAsync(new TerminalProcessorRequest("id1", "root1 grp1 cmd1 \"arg1 value\" 32 true 35.987 3435345345 arg6value 12/23/2023 12/23/2022:12:23:22 \"arg9 value\""));

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

        [Theory]
        [InlineData(" ")]
        [InlineData("~")]
        [InlineData("#")]
        [InlineData("sp")]
        [InlineData("öö")]
        [InlineData("माणूस")]
        [InlineData("女性")]
        public async Task Arguments_Options_Are_Processed_With_Diverse_Separators_Correctly(string sep)
        {
            terminalOptions.Parser.Separator = sep;

            string cmdStr = $"root1{sep}grp1{sep}cmd1{sep}\"arg1{sep}value\"{sep}32{sep}true{sep}35.987{sep}3435345345{sep}arg6value{sep}12/23/2023{sep}12/23/2022:12:23:22{sep}\"arg9{sep}value\"{sep}-opt7_a{sep}--opt3{sep}\"option{sep}delimited{sep}value3\"{sep}--opt4{sep}option{sep}value4{sep}{sep}{sep}{sep}with{sep}multiple{sep}{sep}spaces{sep}--opt5{sep}35.987{sep}--opt1{sep}34{sep}-opt6_a{sep}12/23/2023{sep}--opt2{sep}option{sep}value2{sep}-opt8_a{sep}false";
            var result = await commandRouteParser.ParseRouteAsync(new TerminalProcessorRequest("id1", cmdStr));

            result.Command.Id.Should().Be("cmd1");
            result.Command.Options.Should().NotBeNull();

            // Arguments
            result.Command.Arguments!.Count.Should().Be(9);

            result.Command.Arguments[0].Id.Should().Be("arg1");
            result.Command.Arguments[0].Value.Should().Be($"arg1{sep}value");

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
            result.Command.Arguments[8].Value.Should().Be($"arg9{sep}value");

            // 8 options + 4 aliases
            result.Command.Options!.Count.Should().Be(12);

            Option opt1 = result.Command.Options["opt1"];
            opt1.Value.Should().Be("34");

            Option opt2 = result.Command.Options["opt2"];
            opt2.Value.Should().Be($"option{sep}value2");

            Option opt3 = result.Command.Options["opt3"];
            opt3.Value.Should().Be($"option{sep}delimited{sep}value3");

            Option opt4 = result.Command.Options["opt4"];
            opt4.Value.Should().Be($"option{sep}value4{sep}{sep}{sep}{sep}with{sep}multiple{sep}{sep}spaces");

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
        }

        [Fact]
        public async Task Arguments_With_Multiple_Separator_Are_Processed_In_Order()
        {
            var result = await commandRouteParser.ParseRouteAsync(new TerminalProcessorRequest("id1", " root1   grp1  cmd1           \"arg1    value\"   32   true       35.987 3435345345       arg6value      12/23/2023 12/23/2022:12:23:22  \"   arg9   value   \""));

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
        public async Task Arguments_With_Multiple_Separator_Not_Delimited_Exceed_Argument_Limit_Throws()
        {
            // Here arg9 and value are not delimited so they are considered as two arguments and exceed the limit of 9 arguments
            Func<Task> act = async () => await commandRouteParser.ParseRouteAsync(new TerminalProcessorRequest("id1", " root1   grp1  cmd1           \"arg1    value\"   32   true       35.987 3435345345       arg6value      12/23/2023 12/23/2022:12:23:22     arg9   value   "));
            await act.Should().ThrowAsync<TerminalException>().WithMessage("The command does not support 10 arguments. command=cmd1 arguments=arg1    value,32,true,35.987,3435345345,arg6value,12/23/2023,12/23/2022:12:23:22,arg9,value");
        }

        [Fact]
        public async Task Delimited_Argument_Processes_String_As_Is()
        {
            var result = await commandRouteParser.ParseRouteAsync(new TerminalProcessorRequest("id1", "root1 grp1 cmd1 \"cmd2 'cmd2_arg1' 'cmd2_arg2 value' cmd2_arg3 --opt1 val1 --opt2 val2 -opt3 --opt4 'c:\\\\somedir\\somepath\\somefile.txt'\""));

            result.Command.Id.Should().Be("cmd1");
            result.Command.Arguments.Should().NotBeNull();
            result.Command.Arguments!.Count.Should().Be(1);

            result.Command.Arguments[0].Id.Should().Be("arg1");
            result.Command.Arguments[0].Value.Should().Be("cmd2 'cmd2_arg1' 'cmd2_arg2 value' cmd2_arg3 --opt1 val1 --opt2 val2 -opt3 --opt4 'c:\\\\somedir\\somepath\\somefile.txt'");
        }

        [Fact]
        public async Task Delimited_Stays_As_Is()
        {
            string cmdStr = "root1 grp1 cmd1 \"  -- - -arg1 asd&&&*ASD**ASD&A* value  --\" --opt3 \"--option    -delimited   ^#%^@#&*&* JJKJKASD value3    \"";
            var result = await commandRouteParser.ParseRouteAsync(new TerminalProcessorRequest("id1", cmdStr));

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
        public async Task Duplicate_Custom_Value_Separators_Are_Ignored()
        {
            terminalOptions.Parser.OptionValueSeparator = "=";

            string cmdStr = "root1 grp1 cmd1 --opt3=============\"option delimited value3\"";
            var result = await commandRouteParser.ParseRouteAsync(new TerminalProcessorRequest("id1", cmdStr));

            // Command
            result.Command.Id.Should().Be("cmd1");
            result.Command.Arguments.Should().BeNull();
            result.Command.Options.Should().NotBeNull();

            result.Command.Options!.Count.Should().Be(1);
            result.Command.Options["opt3"].Value.Should().Be("option delimited value3");
        }

        [Fact]
        public async Task Duplicate_Delimited_Custom_Mixed_Separator_Are_Not_Ignored()
        {
            terminalOptions.Parser.OptionValueSeparator = "=";

            string cmdStr = "root1 grp1 cmd1 --opt3=\"=====       =======option delimited value3===     =====\"";
            var result = await commandRouteParser.ParseRouteAsync(new TerminalProcessorRequest("id1", cmdStr));

            // Command
            result.Command.Id.Should().Be("cmd1");
            result.Command.Arguments.Should().BeNull();
            result.Command.Options.Should().NotBeNull();

            result.Command.Options!.Count.Should().Be(1);
            result.Command.Options["opt3"].Value.Should().Be("=====       =======option delimited value3===     =====");
        }

        [Fact]
        public async Task Duplicate_Delimited_Custom_Value_Separator_Are_Not_Ignored()
        {
            terminalOptions.Parser.OptionValueSeparator = "=";

            string cmdStr = "root1 grp1 cmd1 --opt3=\"============option delimited value3\"";
            var result = await commandRouteParser.ParseRouteAsync(new TerminalProcessorRequest("id1", cmdStr));

            // Command
            result.Command.Id.Should().Be("cmd1");
            result.Command.Arguments.Should().BeNull();
            result.Command.Options.Should().NotBeNull();

            result.Command.Options!.Count.Should().Be(1);
            result.Command.Options["opt3"].Value.Should().Be("============option delimited value3");
        }

        [Fact]
        public async Task Duplicate_Value_Separators_In_Between_Are_Ignored()
        {
            terminalOptions.Parser.OptionValueSeparator = "=";

            string cmdStr = "root1======grp1=======cmd1 --opt3=\"option delimited value3\"";
            var result = await commandRouteParser.ParseRouteAsync(new TerminalProcessorRequest("id1", cmdStr));

            // Command
            result.Command.Id.Should().Be("cmd1");
            result.Command.Arguments.Should().BeNull();
            result.Command.Options.Should().NotBeNull();

            result.Command.Options!.Count.Should().Be(1);
            result.Command.Options["opt3"].Value.Should().Be("option delimited value3");
        }

        [Theory]
        [InlineData("--optinvalid", "optinvalid")]
        [InlineData("-optinvalid_alias", "optinvalid_alias")]
        [InlineData("--opt-in-valid", "opt-in-valid")]
        [InlineData("-optinvalid", "optinvalid")]
        public async Task Invalid_Option_Throws(string invalidOpt, string errOpt)
        {
            Func<Task> act = async () => await commandRouteParser.ParseRouteAsync(new TerminalProcessorRequest("id1", "root1 grp1 cmd1 " + invalidOpt));
            await act.Should().ThrowAsync<TerminalException>().WithMessage($"The command does not support option or its alias. command=cmd1 option={errOpt}");
        }

        [Fact]
        public async Task Mixed_Duplicate_Value_Separators_In_Between_Are_Ignored()
        {
            terminalOptions.Parser.OptionValueSeparator = "=";

            string cmdStr = "root1=     =====grp1=======      cmd1 --opt3= \"option delimited value3\"";
            var result = await commandRouteParser.ParseRouteAsync(new TerminalProcessorRequest("id1", cmdStr));

            // Command
            result.Command.Id.Should().Be("cmd1");
            result.Command.Arguments.Should().BeNull();
            result.Command.Options.Should().NotBeNull();

            result.Command.Options!.Count.Should().Be(1);
            result.Command.Options["opt3"].Value.Should().Be("option delimited value3");
        }

        [Fact]
        public async Task More_Arguments_Throws()
        {
            // We support 9 arguments but passed 11
            Func<Task> act = async () => await commandRouteParser.ParseRouteAsync(new TerminalProcessorRequest("id1", "root1 grp1 cmd1 val1 val2 val3    \"  val4   \" val5 val6 val7 val8 val9 val10 val11 "));
            await act.Should().ThrowAsync<TerminalException>().WithMessage("The command does not support 11 arguments. command=cmd1 arguments=val1,val2,val3,  val4   ,val5,val6,val7,val8,val9,val10,val11");
        }

        [Fact]
        public async Task More_Options_Throws()
        {
            // We support 9 arguments but passed 11
            string cmdStr = "root1 grp1 cmd1 --opt1 34 --opt2 option value2 --opt3 \"option delimited value3\" --opt4 option value4    with multiple  spaces --opt5 35.987 --opt6 12/23/2023 --opt7 --opt8 false --opt9 -opt10 val10 --opt11 val11 --opt12 -opt13 val13 --opt14 val14 -opt15";
            Func<Task> act = async () => await commandRouteParser.ParseRouteAsync(new TerminalProcessorRequest("id1", cmdStr));
            await act.Should().ThrowAsync<TerminalException>().WithMessage("The command does not support 15 options. command=cmd1 options=--opt1,--opt2,--opt3,--opt4,--opt5,--opt6,--opt7,--opt8,--opt9,-opt10,--opt11,--opt12,-opt13,--opt14,-opt15");
        }

        [Fact]
        public async Task Multiple_Delimited_Arguments_Are_Processed_In_Order()
        {
            var result = await commandRouteParser.ParseRouteAsync(new TerminalProcessorRequest("id1", "root1 grp1 cmd1 \"arg1 value\" \"32\" \"true\" \"35.987\" 3435345345 \"arg6value\" 12/23/2023 12/23/2022:12:23:22 \"arg9 value\""));
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
        public async Task Multiple_Delimited_Strips_First_Delimited_Pair()
        {
            var result = await commandRouteParser.ParseRouteAsync(new TerminalProcessorRequest("id1", "root1 grp1 cmd1 \"\"\"arg1 delimited value\"\"\""));

            result.Command.Id.Should().Be("cmd1");
            result.Command.Arguments.Should().NotBeNull();
            result.Command.Arguments!.Count.Should().Be(1);

            result.Command.Arguments[0].Id.Should().Be("arg1");
            result.Command.Arguments[0].Value.Should().Be("\"\"arg1 delimited value\"\"");
        }

        [Fact]
        public async Task Nested_Delimited_Gives_Inconsistent_Result()
        {
            var result = await commandRouteParser.ParseRouteAsync(new TerminalProcessorRequest("id1", "root1 grp1 cmd1 \"arg1 \"nested delimited value\" value\""));

            result.Command.Id.Should().Be("cmd1");
            result.Command.Arguments.Should().NotBeNull();
            result.Command.Arguments!.Count.Should().Be(2);

            result.Command.Arguments[0].Id.Should().Be("arg1");
            result.Command.Arguments[0].Value.Should().Be("arg1 \"nested delimited value");

            result.Command.Arguments[1].Id.Should().Be("arg2");
            result.Command.Arguments[1].Value.Should().Be("value");
        }

        [Fact]
        public async Task Option_With_Alias_Prefix_Throws()
        {
            // opt7 is an option so we cannot use - alias prefix we have to use -- option prefix
            var options = "-opt7";
            Func<Task> act = async () => await commandRouteParser.ParseRouteAsync(new TerminalProcessorRequest("id1", "root1 grp1 cmd1 " + options));
            await act.Should().ThrowAsync<TerminalException>().WithMessage("The alias prefix is not valid for an option. option=-opt7");
        }

        [Fact]
        public async Task Option_With_Nested_Delimiter_Should_Not_Error()
        {
            var result = await commandRouteParser.ParseRouteAsync(new TerminalProcessorRequest("id1", "root1 grp1 cmd1 --opt1 \"\"\"val1\"\"\""));

            result.Command.Id.Should().Be("cmd1");
            result.Command.Options.Should().NotBeNull();

            // 1 option + 1 alias
            result.Command.Options!.Count.Should().Be(1);
            result.Command.Arguments.Should().BeNull();

            Option opt1 = result.Command.Options["opt1"];
            opt1.Value.Should().Be("\"\"val1\"\"");
        }

        [Fact]
        public async Task Option_With_Single_Boolean_Works_With_Value()
        {
            var result = await commandRouteParser.ParseRouteAsync(new TerminalProcessorRequest("id1", "root1 grp1 cmd1 --opt8 false"));

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
        public async Task Option_With_Single_Boolean_Works_Without_Value()
        {
            var result = await commandRouteParser.ParseRouteAsync(new TerminalProcessorRequest("id1", "root1 grp1 cmd1 --opt8"));

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
        public async Task Option_With_Single_Delimiter_Multiple_Separator_Value_Works()
        {
            var result = await commandRouteParser.ParseRouteAsync(new TerminalProcessorRequest("id1", "root1 grp1 cmd1 --opt3       \"  option    delimited  value3  \"      "));

            result.Command.Id.Should().Be("cmd1");
            result.Command.Options.Should().NotBeNull();
            result.Command.Arguments.Should().BeNull();

            result.Command.Options!.Count.Should().Be(1);
            Option opt1 = result.Command.Options["opt3"];
            opt1.Value.Should().Be("  option    delimited  value3  ");
        }

        [Fact]
        public async Task Option_With_Single_Delimiter_Value_Works()
        {
            var result = await commandRouteParser.ParseRouteAsync(new TerminalProcessorRequest("id1", "root1 grp1 cmd1 --opt3 \"  option    delimited  value3  \""));

            result.Command.Id.Should().Be("cmd1");
            result.Command.Options.Should().NotBeNull();
            result.Command.Arguments.Should().BeNull();

            result.Command.Options!.Count.Should().Be(1);
            Option opt1 = result.Command.Options["opt3"];
            opt1.Value.Should().Be("  option    delimited  value3  ");
        }

        [Fact]
        public async Task Options_Are_Processed_Correctly()
        {
            string cmdStr = "root1 grp1 cmd1 --opt1 34 --opt2 option value2 --opt3 \"option delimited value3\" --opt4 option value4    with multiple  spaces --opt5 35.987 --opt6 12/23/2023 --opt7 --opt8 false";
            var result = await commandRouteParser.ParseRouteAsync(new TerminalProcessorRequest("id1", cmdStr));

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
        public async Task Options_Are_Processed_Correctly_In_Randomized_Order()
        {
            var options = "--opt7 --opt3 \"option delimited value3\" --opt4 option value4    with multiple  spaces --opt5 35.987 --opt1 34 --opt6 12/23/2023 --opt2 option value2 --opt8 false";
            var result = await commandRouteParser.ParseRouteAsync(new TerminalProcessorRequest("id1", "root1 grp1 cmd1 " + options));

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
        public async Task Options_Are_Processed_Correctly_With_Alias()
        {
            var options = "-opt7_a --opt3 \"option delimited value3\" --opt4 option value4    with multiple  spaces --opt5 35.987 --opt1 34 -opt6_a 12/23/2023 --opt2 option value2 -opt8_a false";
            var result = await commandRouteParser.ParseRouteAsync(new TerminalProcessorRequest("id1", "root1 grp1 cmd1 " + options));

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
        public async Task Options_Can_Have_Prefix_In_Delimited_Values()
        {
            string cmdStr = "root1 grp1 cmd1 --opt2 \"--option -value2\" --opt3 \"option --delimited value3-\" --opt4 \"------option -value4    with multiple  spaces--------\"";
            var result = await commandRouteParser.ParseRouteAsync(new TerminalProcessorRequest("id1", cmdStr));

            result.Command.Id.Should().Be("cmd1");
            result.Command.Options.Should().NotBeNull();

            // 8 options + 4 aliases
            result.Command.Options!.Count.Should().Be(3);

            Option opt1 = result.Command.Options["opt2"];
            opt1.Value.Should().Be("--option -value2");

            Option opt2 = result.Command.Options["opt3"];
            opt2.Value.Should().Be("option --delimited value3-");

            Option opt3 = result.Command.Options["opt4"];
            opt3.Value.Should().Be("------option -value4    with multiple  spaces--------");

            result.Command.Arguments.Should().BeNull();
        }

        [Fact]
        public async Task Options_With_Multiple_Separators_Are_Processed_Correctly()
        {
            string cmdStr = "root1 grp1 cmd1   --opt1    34 --opt2  option    value2 --opt3     \"   option    delimited    value3    \"     --opt4    option value4       with multiple  spaces    --opt5  35.987 --opt6 12/23/2023 --opt7                --opt8 false    ";
            var result = await commandRouteParser.ParseRouteAsync(new TerminalProcessorRequest("id1", cmdStr));

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

        [Theory]
        [InlineData("\"   arg1  delimited     value     \"", "   arg1  delimited     value     ")]
        [InlineData("\"arg1  delimited     value     \"", "arg1  delimited     value     ")]
        [InlineData("\"   arg1  delimited     value\"", "   arg1  delimited     value")]
        [InlineData("\"arg1 delimited value\"", "arg1 delimited value")]
        [InlineData("\"arg1   delimited         value\"", "arg1   delimited         value")]
        public async Task Separators_In_Delimited_Argument_Value_Are_Preserved(string arg, string preserved)
        {
            var result = await commandRouteParser.ParseRouteAsync(new TerminalProcessorRequest("id1", $"root1 grp1 cmd1 {arg}"));

            result.Command.Id.Should().Be("cmd1");
            result.Command.Arguments.Should().NotBeNull();
            result.Command.Arguments!.Count.Should().Be(1);

            result.Command.Arguments[0].Id.Should().Be("arg1");
            result.Command.Arguments[0].Value.Should().Be(preserved);
        }

        [Theory]
        [InlineData(" ")]
        [InlineData("~")]
        [InlineData("#")]
        [InlineData("sp")]
        [InlineData("öö")]
        [InlineData("माणूस")]
        [InlineData("女性")]
        public async Task Strips_Argument_Separator_At_The_End(string sep)
        {
            terminalOptions.Parser.Separator = sep;
            var result = await commandRouteParser.ParseRouteAsync(new TerminalProcessorRequest("id1", $"root1{sep}grp1{sep}cmd1{sep}\"arg1{sep}value\"{sep}{sep}{sep}{sep}{sep}"));

            result.Command.Id.Should().Be("cmd1");
            result.Command.Options.Should().BeNull();

            // Arguments
            result.Command.Arguments!.Count.Should().Be(1);

            result.Command.Arguments[0].Id.Should().Be("arg1");
            result.Command.Arguments[0].Value.Should().Be($"arg1{sep}value");
        }

        [Theory]
        [InlineData(" ")]
        [InlineData("~")]
        [InlineData("#")]
        [InlineData("sp")]
        [InlineData("öö")]
        [InlineData("माणूस")]
        [InlineData("女性")]
        public async Task Strips_Option_Separator_At_The_End(string sep)
        {
            terminalOptions.Parser.Separator = sep;
            var result = await commandRouteParser.ParseRouteAsync(new TerminalProcessorRequest("id1", $"root1{sep}grp1{sep}cmd1{sep}--opt3{sep}val3{sep}{sep}{sep}{sep}{sep}"));

            result.Command.Id.Should().Be("cmd1");
            result.Command.Arguments.Should().BeNull();

            // Options
            result.Command.Options!.Count.Should().Be(1);
            result.Command.Options["opt3"].Value.Should().Be($"val3");
        }

        [Fact]
        public async Task Unsupported_Alias_Throws()
        {
            Func<Task> act = async () => await commandRouteParser.ParseRouteAsync(new TerminalProcessorRequest("id1", "root1 grp1 cmd1 -opt8_a_invalid true"));
            await act.Should().ThrowAsync<TerminalException>().WithMessage("The command does not support option or its alias. command=cmd1 option=opt8_a_invalid");
        }

        [Fact]
        public async Task Unsupported_Option_Throws()
        {
            Func<Task> act = async () => await commandRouteParser.ParseRouteAsync(new TerminalProcessorRequest("id1", "root1 grp1 cmd1 --opt3_invalid \"  option    delimited  value3  \""));
            await act.Should().ThrowAsync<TerminalException>().WithMessage("The command does not support option or its alias. command=cmd1 option=opt3_invalid");
        }

        [Fact]
        public async Task Value_Separators_At_The_End_Are_Ignored()
        {
            terminalOptions.Parser.OptionValueSeparator = "=";

            string cmdStr = "root1 grp1 cmd1 --opt3=\"option delimited value3\"======";
            var result = await commandRouteParser.ParseRouteAsync(new TerminalProcessorRequest("id1", cmdStr));

            // Command
            result.Command.Id.Should().Be("cmd1");
            result.Command.Arguments.Should().BeNull();
            result.Command.Options.Should().NotBeNull();

            result.Command.Options!.Count.Should().Be(1);
            result.Command.Options["opt3"].Value.Should().Be("option delimited value3");
        }

        [Fact]
        public async Task Value_Separators_At_The_Start_Are_Ignored()
        {
            terminalOptions.Parser.OptionValueSeparator = "=";

            string cmdStr = "=====root1 grp1 cmd1 --opt3=\"option delimited value3\"";
            var result = await commandRouteParser.ParseRouteAsync(new TerminalProcessorRequest("id1", cmdStr));

            // Command
            result.Command.Id.Should().Be("cmd1");
            result.Command.Arguments.Should().BeNull();
            result.Command.Options.Should().NotBeNull();

            result.Command.Options!.Count.Should().Be(1);
            result.Command.Options["opt3"].Value.Should().Be("option delimited value3");
        }

        [Fact]
        public async Task Value_Separators_In_Between_Are_Ignored()
        {
            terminalOptions.Parser.OptionValueSeparator = "=";

            string cmdStr = "root1=grp1=cmd1 --opt3=\"option delimited value3\"";
            var result = await commandRouteParser.ParseRouteAsync(new TerminalProcessorRequest("id1", cmdStr));

            // Command
            result.Command.Id.Should().Be("cmd1");
            result.Command.Arguments.Should().BeNull();
            result.Command.Options.Should().NotBeNull();

            result.Command.Options!.Count.Should().Be(1);
            result.Command.Options["opt3"].Value.Should().Be("option delimited value3");
        }

        private readonly ArgumentDescriptors argumentDescriptors;
        private readonly CommandDescriptors commandDescriptors;
        private readonly ICommandRouteParser commandRouteParser;
        private readonly ITerminalCommandStore commandStore;
        private readonly ILogger<CommandRouteParser> logger;
        private readonly OptionDescriptors optionDescriptors;
        private readonly TerminalOptions terminalOptions;
        private readonly ITerminalTextHandler textHandler;
    }
}
