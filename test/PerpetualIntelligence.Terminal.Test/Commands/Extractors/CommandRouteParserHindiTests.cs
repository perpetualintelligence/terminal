/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Terminal.Mocks;
using PerpetualIntelligence.Terminal.Stores;
using PerpetualIntelligence.Terminal.Stores.InMemory;
using PerpetualIntelligence.Test.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace PerpetualIntelligence.Terminal.Commands.Extractors
{
    public class CommandRouteParserHindiTests
    {
        private readonly ITextHandler _textHandler;
        private readonly ICommandStoreHandler _commandStore;
        private readonly TerminalOptions _terminalOptions;
        private readonly ILogger<CommandRouteParser> _logger;
        private readonly ICommandRouteParser _commandRouteParser;
        private CommandDescriptors _commandDescriptors;

        public CommandRouteParserHindiTests()
        {
            _logger = TestLogger.Create<CommandRouteParser>();
            _textHandler = new UnicodeTextHandler();

            var options = new OptionDescriptors(new UnicodeTextHandler(), new List<OptionDescriptor>()
            {
                new OptionDescriptor("एक", nameof(String), "पहला तर्क", OptionFlags.None, "एकहै" ),
                new OptionDescriptor("दो", nameof(Boolean), "दूसरा तर्क", OptionFlags.Required) { },
                new OptionDescriptor("तीन", nameof(String), "तीसरा तर्क", OptionFlags.None, "तीनहै" ),
                new OptionDescriptor("चार", nameof(Double), "चौथा तर्क", OptionFlags.None, "चारहै"),
            });

            _commandDescriptors = new CommandDescriptors(_textHandler, new List<CommandDescriptor>()
            {
               new CommandDescriptor("यूनिकोड", "यूनिकोड नाम", "यूनिकोड रूट कमांड", CommandType.Root, CommandFlags.None),
               new CommandDescriptor("परीक्षण", "परीक्षण नाम", "यूनिकोड समूहीकृत कमांड", CommandType.Group, CommandFlags.None, new OwnerIdCollection("यूनिकोड")),
               new CommandDescriptor("प्रिंट", "प्रिंट नाम", "प्रिंट कमांड", CommandType.SubCommand, CommandFlags.None, new OwnerIdCollection("परीक्षण"), argumentDescriptors : null, optionDescriptors : options),
               new CommandDescriptor("दूसरा", "दूसरा नाम", "दूसरा आदेश", CommandType.SubCommand, CommandFlags.None, new OwnerIdCollection("परीक्षण"), argumentDescriptors : null, optionDescriptors : options)
            });

            _commandStore = new InMemoryCommandStore(_textHandler, _commandDescriptors.Values);
            _terminalOptions = MockTerminalOptions.NewAliasOptions();
            _commandRouteParser = new CommandRouteParser(_textHandler, _commandStore, _terminalOptions, _logger);
        }

        [Fact]
        public async Task Unicode_Hindi_Root_Extracts_Correctly()
        {
            var parsedCommand = await _commandRouteParser.ParseAsync(new CommandRoute("id1", "यूनिकोड"));

            parsedCommand.Command.Descriptor.Should().NotBeNull();
            parsedCommand.Command.Descriptor.Type.Should().Be(CommandType.Root);

            parsedCommand.Should().NotBeNull();
            parsedCommand.Command.Id.Should().Be("यूनिकोड");
            parsedCommand.Command.Name.Should().Be("यूनिकोड नाम");
            parsedCommand.Command.Description.Should().Be("यूनिकोड रूट कमांड");
            parsedCommand.Command.Options.Should().BeNull();
        }

        [Fact]
        public async Task Unicode_Hindi_Group_Throws_With_Space_And_Custom_Separator()
        {
            _terminalOptions.Extractor.Separator = "एस";
            _terminalOptions.Extractor.OptionValueSeparator = "एस";

            // Here we are using a space in the command name to test that the parser will throw an error.
            // The separator is set to "एस" so the parser will not be able to find the command 'परीक्षण  '
            Func<Task> act = async () => await _commandRouteParser.ParseAsync(new CommandRoute("id1", "यूनिकोडएसपरीक्षण  "));
            await act.Should().ThrowAsync<TerminalException>().WithMessage("The command does not support any arguments. command=यूनिकोड");
        }

        [Fact]
        public async Task Unicode_Hindi_Group_With_Multiple_Separator_Extracts_Correctly()
        {
            string separator = "एस";
            _terminalOptions.Extractor.Separator = separator;
            _terminalOptions.Extractor.ParseHierarchy = true;

            var parsedCommand = await _commandRouteParser.ParseAsync(new CommandRoute("id1", $"{separator}यूनिकोड{separator}{separator}{separator}परीक्षण{separator}{separator}{separator}{separator}"));

            parsedCommand.Command.Descriptor.Should().NotBeNull();
            parsedCommand.Command.Descriptor.Type.Should().Be(CommandType.Group);

            parsedCommand.Should().NotBeNull();
            parsedCommand.Command.Id.Should().Be("परीक्षण");
            parsedCommand.Command.Name.Should().Be("परीक्षण नाम");
            parsedCommand.Command.Description.Should().Be("यूनिकोड समूहीकृत कमांड");
            parsedCommand.Command.Options.Should().BeNull();

            parsedCommand.Hierarchy.Should().NotBeNull();
            parsedCommand.Hierarchy!.LinkedCommand.Id.Should().Be("यूनिकोड");
            parsedCommand.Hierarchy.ChildSubCommand.Should().BeNull();

            parsedCommand.Hierarchy.ChildGroup.Should().NotBeNull();
            parsedCommand.Hierarchy.ChildGroup!.LinkedCommand.Id.Should().Be("परीक्षण");
            parsedCommand.Hierarchy.ChildGroup.ChildGroup.Should().BeNull();
            parsedCommand.Hierarchy.ChildGroup.ChildSubCommand.Should().BeNull();
        }

        [Fact]
        public async Task Unicode_Hindi_Group_With_Multiple_Separator_And_Spaces_Should_Error()
        {
            string separator = "एस";
            _terminalOptions.Extractor.Separator = separator;
            _terminalOptions.Extractor.OptionValueSeparator = separator;

            _terminalOptions.Extractor.ParseHierarchy = true;

            // Here space is not a separation so any space is considered part of the command name.
            // Because there is not command with name '  परीक्षण' the parser will interpret it as argument and throw.
            Func<Task> act = async () => await _commandRouteParser.ParseAsync(new CommandRoute("id1", $"{separator}यूनिकोड{separator}{separator}{separator}  परीक्षण{separator}{separator}{separator}{separator}"));
            await act.Should().ThrowAsync<TerminalException>().WithMessage("The command does not support any arguments. command=यूनिकोड");
        }

        [Fact]
        public async Task Unicode_Hindi_Group_Extracts_Correctly()
        {
            string separator = "एस";
            _terminalOptions.Extractor.Separator = separator;
            _terminalOptions.Extractor.ParseHierarchy = true;

            var parsedCommand = await _commandRouteParser.ParseAsync(new CommandRoute("id1", $"यूनिकोड{separator}परीक्षण"));

            parsedCommand.Command.Descriptor.Should().NotBeNull();
            parsedCommand.Command.Descriptor.Type.Should().Be(CommandType.Group);

            parsedCommand.Should().NotBeNull();
            parsedCommand.Command.Id.Should().Be("परीक्षण");
            parsedCommand.Command.Name.Should().Be("परीक्षण नाम");
            parsedCommand.Command.Description.Should().Be("यूनिकोड समूहीकृत कमांड");
            parsedCommand.Command.Options.Should().BeNull();

            parsedCommand.Hierarchy.Should().NotBeNull();
            parsedCommand.Hierarchy!.LinkedCommand.Id.Should().Be("यूनिकोड");
            parsedCommand.Hierarchy.ChildSubCommand.Should().BeNull();

            parsedCommand.Hierarchy.ChildGroup.Should().NotBeNull();
            parsedCommand.Hierarchy.ChildGroup!.LinkedCommand.Id.Should().Be("परीक्षण");
            parsedCommand.Hierarchy.ChildGroup.ChildGroup.Should().BeNull();
            parsedCommand.Hierarchy.ChildGroup.ChildSubCommand.Should().BeNull();
        }

        [Fact]
        public async Task Unicode_Hindi_SubCommand_Extracts_Correctly()
        {
            string separator = "एस";
            _terminalOptions.Extractor.Separator = separator;
            _terminalOptions.Extractor.ParseHierarchy = true;

            var parsedCommand = await _commandRouteParser.ParseAsync(new CommandRoute("प्रिंट", $"यूनिकोड{separator}परीक्षण{separator}प्रिंट"));

            parsedCommand.Command.Descriptor.Should().NotBeNull();
            parsedCommand.Command.Descriptor.Type.Should().Be(CommandType.SubCommand);

            parsedCommand.Should().NotBeNull();
            parsedCommand.Command.Id.Should().Be("प्रिंट");
            parsedCommand.Command.Name.Should().Be("प्रिंट नाम");
            parsedCommand.Command.Description.Should().Be("प्रिंट कमांड");
            parsedCommand.Command.Options.Should().BeNull();

            parsedCommand.Hierarchy.Should().NotBeNull();
            parsedCommand.Hierarchy!.LinkedCommand.Id.Should().Be("यूनिकोड");
            parsedCommand.Hierarchy.ChildSubCommand.Should().BeNull();

            parsedCommand.Hierarchy.ChildGroup.Should().NotBeNull();
            parsedCommand.Hierarchy.ChildGroup!.LinkedCommand.Id.Should().Be("परीक्षण");
            parsedCommand.Hierarchy.ChildGroup.ChildGroup.Should().BeNull();

            parsedCommand.Hierarchy.ChildGroup.ChildSubCommand.Should().NotBeNull();
            parsedCommand.Hierarchy.ChildGroup.ChildSubCommand!.LinkedCommand.Id.Should().Be("प्रिंट");
        }

        [Fact]
        public async Task Unicode_Hindi_SubCommand_Second_Extracts_Correctly()
        {
            string separator = "एस";
            _terminalOptions.Extractor.Separator = separator;
            _terminalOptions.Extractor.ParseHierarchy = true;

            var parsedCommand = await _commandRouteParser.ParseAsync(new CommandRoute("id1", $"यूनिकोड{separator}परीक्षण{separator}दूसरा"));

            parsedCommand.Command.Descriptor.Should().NotBeNull();
            parsedCommand.Command.Descriptor.Type.Should().Be(CommandType.SubCommand);

            parsedCommand.Should().NotBeNull();
            parsedCommand.Command.Id.Should().Be("दूसरा");
            parsedCommand.Command.Name.Should().Be("दूसरा नाम");
            parsedCommand.Command.Description.Should().Be("दूसरा आदेश");
            parsedCommand.Command.Options.Should().BeNull();

            parsedCommand.Hierarchy.Should().NotBeNull();
            parsedCommand.Hierarchy!.LinkedCommand.Id.Should().Be("यूनिकोड");
            parsedCommand.Hierarchy.ChildSubCommand.Should().BeNull();

            parsedCommand.Hierarchy.ChildGroup.Should().NotBeNull();
            parsedCommand.Hierarchy.ChildGroup!.LinkedCommand.Id.Should().Be("परीक्षण");
            parsedCommand.Hierarchy.ChildGroup.ChildGroup.Should().BeNull();

            parsedCommand.Hierarchy.ChildGroup.ChildSubCommand.Should().NotBeNull();
            parsedCommand.Hierarchy.ChildGroup.ChildSubCommand!.LinkedCommand.Id.Should().Be("दूसरा");
        }

        [Fact]
        public async Task Unicode_Hindi_SubCommand_Options_Extracts_Correctly()
        {
            string sep = "एस";
            _terminalOptions.Extractor.Separator = sep;
            _terminalOptions.Extractor.OptionValueSeparator = sep;

            string prefix = "डैश";
            _terminalOptions.Extractor.OptionPrefix = prefix;

            string alias = "एल";
            _terminalOptions.Extractor.OptionAliasPrefix = alias;

            _terminalOptions.Extractor.ParseHierarchy = true;
            var parsedCommand = await _commandRouteParser.ParseAsync(new CommandRoute("id1", $"यूनिकोड{sep}परीक्षण{sep}प्रिंट{sep}{prefix}एक{sep}{sep}{sep}प्रथम मान{sep}{sep}{sep}{prefix}दो{sep}{alias}तीनहै{sep}\"तीसरा मान\"{sep}{prefix}चार{sep}86.39"));

            parsedCommand.Command.Descriptor.Should().NotBeNull();
            parsedCommand.Command.Descriptor.Type.Should().Be(CommandType.SubCommand);

            parsedCommand.Should().NotBeNull();
            parsedCommand.Command.Id.Should().Be("प्रिंट");
            parsedCommand.Command.Name.Should().Be("प्रिंट नाम");
            parsedCommand.Command.Description.Should().Be("प्रिंट कमांड");
            parsedCommand.Command.Options.Should().NotBeNullOrEmpty();
            parsedCommand.Command.Arguments.Should().BeNull();

            parsedCommand.Hierarchy.Should().NotBeNull();
            parsedCommand.Hierarchy!.LinkedCommand.Id.Should().Be("यूनिकोड");
            parsedCommand.Hierarchy.ChildSubCommand.Should().BeNull();

            parsedCommand.Hierarchy.ChildGroup.Should().NotBeNull();
            parsedCommand.Hierarchy.ChildGroup!.LinkedCommand.Id.Should().Be("परीक्षण");
            parsedCommand.Hierarchy.ChildGroup.ChildGroup.Should().BeNull();

            parsedCommand.Hierarchy.ChildGroup.ChildSubCommand.Should().NotBeNull();
            parsedCommand.Hierarchy.ChildGroup.ChildSubCommand!.LinkedCommand.Id.Should().Be("प्रिंट");

            // 4 Options + 3 Alias
            parsedCommand.Command.Options.Should().HaveCount(7);

            parsedCommand.Command.Options!["एक"].Value.Should().Be("प्रथम मान");
            parsedCommand.Command.Options["एकहै"].Value.Should().Be("प्रथम मान");
            parsedCommand.Command.Options["एक"].Should().BeSameAs(parsedCommand.Command.Options["एकहै"]);

            parsedCommand.Command.Options["दो"].Value.Should().Be(true.ToString());

            parsedCommand.Command.Options["तीन"].Value.Should().Be("तीसरा मान");
            parsedCommand.Command.Options["तीनहै"].Value.Should().Be("तीसरा मान");
            parsedCommand.Command.Options["तीन"].Should().BeSameAs(parsedCommand.Command.Options["तीनहै"]);

            parsedCommand.Command.Options!["चार"].Value.Should().Be("86.39");
            parsedCommand.Command.Options!["चारहै"].Value.Should().Be("86.39");
            parsedCommand.Command.Options["चार"].Should().BeSameAs(parsedCommand.Command.Options["चारहै"]);
        }

        [Fact]
        public async Task Unicode_Hindi_SubCommand_With_Space_Options_Extracts_Correctly()
        {
            string sep = "एस";
            _terminalOptions.Extractor.Separator = sep;
            _terminalOptions.Extractor.OptionValueSeparator = sep;

            string prefix = "डैश";
            _terminalOptions.Extractor.OptionPrefix = prefix;

            string alias = "एल";
            _terminalOptions.Extractor.OptionAliasPrefix = alias;

            // Here space is not treated as separator so it is included in the value.
            _terminalOptions.Extractor.ParseHierarchy = true;
            var parsedCommand = await _commandRouteParser.ParseAsync(new CommandRoute("id1", $"यूनिकोड{sep}परीक्षण{sep}प्रिंट{sep}{prefix}एक{sep}  प्रथम   मान   {sep}{prefix}दो{sep}{alias}तीनहै{sep}\"तीसरा मान\"{sep}{prefix}चार{sep}86.39"));

            parsedCommand.Command.Descriptor.Should().NotBeNull();
            parsedCommand.Command.Descriptor.Type.Should().Be(CommandType.SubCommand);

            parsedCommand.Should().NotBeNull();
            parsedCommand.Command.Id.Should().Be("प्रिंट");
            parsedCommand.Command.Name.Should().Be("प्रिंट नाम");
            parsedCommand.Command.Description.Should().Be("प्रिंट कमांड");
            parsedCommand.Command.Options.Should().NotBeNullOrEmpty();
            parsedCommand.Command.Arguments.Should().BeNull();

            parsedCommand.Hierarchy.Should().NotBeNull();
            parsedCommand.Hierarchy!.LinkedCommand.Id.Should().Be("यूनिकोड");
            parsedCommand.Hierarchy.ChildSubCommand.Should().BeNull();

            parsedCommand.Hierarchy.ChildGroup.Should().NotBeNull();
            parsedCommand.Hierarchy.ChildGroup!.LinkedCommand.Id.Should().Be("परीक्षण");
            parsedCommand.Hierarchy.ChildGroup.ChildGroup.Should().BeNull();

            parsedCommand.Hierarchy.ChildGroup.ChildSubCommand.Should().NotBeNull();
            parsedCommand.Hierarchy.ChildGroup.ChildSubCommand!.LinkedCommand.Id.Should().Be("प्रिंट");

            // 4 Options + 3 Alias
            parsedCommand.Command.Options.Should().HaveCount(7);

            parsedCommand.Command.Options!["एक"].Value.Should().Be("  प्रथम   मान   ");
            parsedCommand.Command.Options["एकहै"].Value.Should().Be("  प्रथम   मान   ");
            parsedCommand.Command.Options["एक"].Should().BeSameAs(parsedCommand.Command.Options["एकहै"]);

            parsedCommand.Command.Options["दो"].Value.Should().Be(true.ToString());

            parsedCommand.Command.Options["तीन"].Value.Should().Be("तीसरा मान");
            parsedCommand.Command.Options["तीनहै"].Value.Should().Be("तीसरा मान");
            parsedCommand.Command.Options["तीन"].Should().BeSameAs(parsedCommand.Command.Options["तीनहै"]);

            parsedCommand.Command.Options!["चार"].Value.Should().Be("86.39");
            parsedCommand.Command.Options!["चारहै"].Value.Should().Be("86.39");
            parsedCommand.Command.Options["चार"].Should().BeSameAs(parsedCommand.Command.Options["चारहै"]);
        }

        [Fact]
        public async Task Unicode_Hindi_SubCommand_With_Different_Separator_With_Space_Options_Extracts_Correctly()
        {
            string cSep = "एस";
            string oSep = "साथ";
            _terminalOptions.Extractor.Separator = cSep;
            _terminalOptions.Extractor.OptionValueSeparator = oSep;

            string prefix = "डैश";
            _terminalOptions.Extractor.OptionPrefix = prefix;

            string alias = "एल";
            _terminalOptions.Extractor.OptionAliasPrefix = alias;

            // Here space is not treated as separator so it is included in the value.
            _terminalOptions.Extractor.ParseHierarchy = true;
            var parsedCommand = await _commandRouteParser.ParseAsync(new CommandRoute("id1", $"यूनिकोड{cSep}परीक्षण{cSep}प्रिंट{cSep}{prefix}एक{oSep}  प्रथम   मान   {oSep}{prefix}दो{oSep}{alias}तीनहै{oSep}\"तीसरा मान\"{oSep}{prefix}चार{oSep}86.39"));

            parsedCommand.Command.Descriptor.Should().NotBeNull();
            parsedCommand.Command.Descriptor.Type.Should().Be(CommandType.SubCommand);

            parsedCommand.Should().NotBeNull();
            parsedCommand.Command.Id.Should().Be("प्रिंट");
            parsedCommand.Command.Name.Should().Be("प्रिंट नाम");
            parsedCommand.Command.Description.Should().Be("प्रिंट कमांड");
            parsedCommand.Command.Options.Should().NotBeNullOrEmpty();
            parsedCommand.Command.Arguments.Should().BeNull();

            parsedCommand.Hierarchy.Should().NotBeNull();
            parsedCommand.Hierarchy!.LinkedCommand.Id.Should().Be("यूनिकोड");
            parsedCommand.Hierarchy.ChildSubCommand.Should().BeNull();

            parsedCommand.Hierarchy.ChildGroup.Should().NotBeNull();
            parsedCommand.Hierarchy.ChildGroup!.LinkedCommand.Id.Should().Be("परीक्षण");
            parsedCommand.Hierarchy.ChildGroup.ChildGroup.Should().BeNull();

            parsedCommand.Hierarchy.ChildGroup.ChildSubCommand.Should().NotBeNull();
            parsedCommand.Hierarchy.ChildGroup.ChildSubCommand!.LinkedCommand.Id.Should().Be("प्रिंट");

            // 4 Options + 3 Alias
            parsedCommand.Command.Options.Should().HaveCount(7);

            parsedCommand.Command.Options!["एक"].Value.Should().Be("  प्रथम   मान   ");
            parsedCommand.Command.Options["एकहै"].Value.Should().Be("  प्रथम   मान   ");
            parsedCommand.Command.Options["एक"].Should().BeSameAs(parsedCommand.Command.Options["एकहै"]);

            parsedCommand.Command.Options["दो"].Value.Should().Be(true.ToString());

            parsedCommand.Command.Options["तीन"].Value.Should().Be("तीसरा मान");
            parsedCommand.Command.Options["तीनहै"].Value.Should().Be("तीसरा मान");
            parsedCommand.Command.Options["तीन"].Should().BeSameAs(parsedCommand.Command.Options["तीनहै"]);

            parsedCommand.Command.Options!["चार"].Value.Should().Be("86.39");
            parsedCommand.Command.Options!["चारहै"].Value.Should().Be("86.39");
            parsedCommand.Command.Options["चार"].Should().BeSameAs(parsedCommand.Command.Options["चारहै"]);
        }

        [Fact]
        public async Task Unicode_Different_Separator_At_The_End_Are_Ignored()
        {
            string cSep = "एस";
            string oSep = "साथ";
            _terminalOptions.Extractor.Separator = cSep;
            _terminalOptions.Extractor.OptionValueSeparator = oSep;

            string prefix = "डैश";
            _terminalOptions.Extractor.OptionPrefix = prefix;

            string alias = "एल";
            _terminalOptions.Extractor.OptionAliasPrefix = alias;

            // Here space is not treated as separator so it is included in the value.
            _terminalOptions.Extractor.ParseHierarchy = true;
            var parsedCommand = await _commandRouteParser.ParseAsync(new CommandRoute("id1", $"यूनिकोड{cSep}परीक्षण{cSep}प्रिंट{cSep}{prefix}एक{oSep}  प्रथम   मान   {oSep}{prefix}दो{oSep}{alias}तीनहै{oSep}\"तीसरा मान\"{oSep}{prefix}चार{oSep}86.39{oSep}{oSep}{oSep}{cSep}{cSep}{cSep}"));

            parsedCommand.Command.Descriptor.Should().NotBeNull();
            parsedCommand.Command.Descriptor.Type.Should().Be(CommandType.SubCommand);

            parsedCommand.Should().NotBeNull();
            parsedCommand.Command.Id.Should().Be("प्रिंट");
            parsedCommand.Command.Name.Should().Be("प्रिंट नाम");
            parsedCommand.Command.Description.Should().Be("प्रिंट कमांड");
            parsedCommand.Command.Options.Should().NotBeNullOrEmpty();
            parsedCommand.Command.Arguments.Should().BeNull();

            parsedCommand.Hierarchy.Should().NotBeNull();
            parsedCommand.Hierarchy!.LinkedCommand.Id.Should().Be("यूनिकोड");
            parsedCommand.Hierarchy.ChildSubCommand.Should().BeNull();

            parsedCommand.Hierarchy.ChildGroup.Should().NotBeNull();
            parsedCommand.Hierarchy.ChildGroup!.LinkedCommand.Id.Should().Be("परीक्षण");
            parsedCommand.Hierarchy.ChildGroup.ChildGroup.Should().BeNull();

            parsedCommand.Hierarchy.ChildGroup.ChildSubCommand.Should().NotBeNull();
            parsedCommand.Hierarchy.ChildGroup.ChildSubCommand!.LinkedCommand.Id.Should().Be("प्रिंट");

            // 4 Options + 3 Alias
            parsedCommand.Command.Options.Should().HaveCount(7);

            parsedCommand.Command.Options!["एक"].Value.Should().Be("  प्रथम   मान   ");
            parsedCommand.Command.Options["एकहै"].Value.Should().Be("  प्रथम   मान   ");
            parsedCommand.Command.Options["एक"].Should().BeSameAs(parsedCommand.Command.Options["एकहै"]);

            parsedCommand.Command.Options["दो"].Value.Should().Be(true.ToString());

            parsedCommand.Command.Options["तीन"].Value.Should().Be("तीसरा मान");
            parsedCommand.Command.Options["तीनहै"].Value.Should().Be("तीसरा मान");
            parsedCommand.Command.Options["तीन"].Should().BeSameAs(parsedCommand.Command.Options["तीनहै"]);

            parsedCommand.Command.Options!["चार"].Value.Should().Be("86.39");
            parsedCommand.Command.Options!["चारहै"].Value.Should().Be("86.39");
            parsedCommand.Command.Options["चार"].Should().BeSameAs(parsedCommand.Command.Options["चारहै"]);
        }

        [Fact]
        public async Task Unicode_Different_Separator_At_The_Start_Are_Ignored()
        {
            string cSep = "एस";
            string oSep = "साथ";
            _terminalOptions.Extractor.Separator = cSep;
            _terminalOptions.Extractor.OptionValueSeparator = oSep;

            string prefix = "डैश";
            _terminalOptions.Extractor.OptionPrefix = prefix;

            string alias = "एल";
            _terminalOptions.Extractor.OptionAliasPrefix = alias;

            // Here space is not treated as separator so it is included in the value.
            _terminalOptions.Extractor.ParseHierarchy = true;
            var parsedCommand = await _commandRouteParser.ParseAsync(new CommandRoute("id1", $"{oSep}{oSep}{oSep}{cSep}{cSep}{cSep}यूनिकोड{cSep}परीक्षण{cSep}प्रिंट{cSep}{prefix}एक{oSep}  प्रथम   मान   {oSep}{prefix}दो{oSep}{alias}तीनहै{oSep}\"तीसरा मान\"{oSep}{prefix}चार{oSep}86.39"));

            parsedCommand.Command.Descriptor.Should().NotBeNull();
            parsedCommand.Command.Descriptor.Type.Should().Be(CommandType.SubCommand);

            parsedCommand.Should().NotBeNull();
            parsedCommand.Command.Id.Should().Be("प्रिंट");
            parsedCommand.Command.Name.Should().Be("प्रिंट नाम");
            parsedCommand.Command.Description.Should().Be("प्रिंट कमांड");
            parsedCommand.Command.Options.Should().NotBeNullOrEmpty();
            parsedCommand.Command.Arguments.Should().BeNull();

            parsedCommand.Hierarchy.Should().NotBeNull();
            parsedCommand.Hierarchy!.LinkedCommand.Id.Should().Be("यूनिकोड");
            parsedCommand.Hierarchy.ChildSubCommand.Should().BeNull();

            parsedCommand.Hierarchy.ChildGroup.Should().NotBeNull();
            parsedCommand.Hierarchy.ChildGroup!.LinkedCommand.Id.Should().Be("परीक्षण");
            parsedCommand.Hierarchy.ChildGroup.ChildGroup.Should().BeNull();

            parsedCommand.Hierarchy.ChildGroup.ChildSubCommand.Should().NotBeNull();
            parsedCommand.Hierarchy.ChildGroup.ChildSubCommand!.LinkedCommand.Id.Should().Be("प्रिंट");

            // 4 Options + 3 Alias
            parsedCommand.Command.Options.Should().HaveCount(7);

            parsedCommand.Command.Options!["एक"].Value.Should().Be("  प्रथम   मान   ");
            parsedCommand.Command.Options["एकहै"].Value.Should().Be("  प्रथम   मान   ");
            parsedCommand.Command.Options["एक"].Should().BeSameAs(parsedCommand.Command.Options["एकहै"]);

            parsedCommand.Command.Options["दो"].Value.Should().Be(true.ToString());

            parsedCommand.Command.Options["तीन"].Value.Should().Be("तीसरा मान");
            parsedCommand.Command.Options["तीनहै"].Value.Should().Be("तीसरा मान");
            parsedCommand.Command.Options["तीन"].Should().BeSameAs(parsedCommand.Command.Options["तीनहै"]);

            parsedCommand.Command.Options!["चार"].Value.Should().Be("86.39");
            parsedCommand.Command.Options!["चारहै"].Value.Should().Be("86.39");
            parsedCommand.Command.Options["चार"].Should().BeSameAs(parsedCommand.Command.Options["चारहै"]);
        }
    }
}