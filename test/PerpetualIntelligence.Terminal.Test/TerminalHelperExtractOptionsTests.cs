/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Terminal.Commands;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Terminal.Mocks;
using System;
using System.Text;
using Xunit;

namespace PerpetualIntelligence.Terminal
{
    public class TerminalHelperExtractOptionsTests
    {
        public TerminalHelperExtractOptionsTests()
        {
            terminalOptions = MockTerminalOptions.NewAliasOptions();
            textHandler = new UnicodeTextHandler();
        }

        [Fact]
        public void Counter_More_Than_50_Throws()
        {
            StringBuilder sb50 = new ();
            for (int idx = 0; idx < 50; ++idx)
            {
                sb50.Append($"--opt{idx} val{idx}");
                sb50.Append(' ');
            }

            OptionStrings optionStrings = TerminalHelper.ExtractOptionStrings(sb50.ToString(), terminalOptions, textHandler);
            optionStrings.Count.Should().Be(50);

            StringBuilder sb51 = new ();
            for (int idx = 0; idx < 51; ++idx)
            {
                sb51.Append($"--opt{idx} val{idx}");
                sb51.Append(' ');
            }

            Action act = () => TerminalHelper.ExtractOptionStrings(sb51.ToString(), terminalOptions, textHandler);
            act.Should().Throw<ErrorException>().WithMessage("Too many iteration while extracting options. max=50 current=51");
        }

        [Fact]
        public void Extracts_With_Separator_In_Between()
        {
            OptionStrings optionStrings = TerminalHelper.ExtractOptionStrings("   --key1=Test  space    message   --key2=nospacemessage -key6    -key10=Again with  space      ", terminalOptions, textHandler);

            optionStrings[0].AliasPrefix.Should().BeFalse();
            optionStrings[0].Position.Should().Be(0);
            optionStrings[0].Raw.Should().Be(" --key1=Test  space    message  ");

            optionStrings[32].AliasPrefix.Should().BeFalse();
            optionStrings[32].Position.Should().Be(32);
            optionStrings[32].Raw.Should().Be(" --key2=nospacemessage");

            optionStrings[54].AliasPrefix.Should().BeTrue();
            optionStrings[54].Position.Should().Be(54);
            optionStrings[54].Raw.Should().Be(" -key6   ");

            optionStrings[63].AliasPrefix.Should().BeTrue();
            optionStrings[63].Position.Should().Be(63);
            optionStrings[63].Raw.Should().Be(" -key10=Again with  space      ");
        }

        [Fact]
        public void Extracts_ValueValueDelimiter_With_Options_In_Value()
        {
            OptionStrings optionStrings = TerminalHelper.ExtractOptionStrings(" --opt1 val1 -opt2 \"run -o1 v1 --o2 v2\" --opt3 --opt4 \"run2 -o4.1 v1 --o4.2 v2\" -opt5", terminalOptions, textHandler);

            optionStrings.Count.Should().Be(5);

            optionStrings[0].AliasPrefix.Should().BeFalse();
            optionStrings[0].Position.Should().Be(0);
            optionStrings[0].Raw.Should().Be(" --opt1 val1");

            optionStrings[12].AliasPrefix.Should().BeTrue();
            optionStrings[12].Position.Should().Be(12);
            optionStrings[12].Raw.Should().Be(" -opt2 \"run -o1 v1 --o2 v2\"");

            optionStrings[39].AliasPrefix.Should().BeFalse();
            optionStrings[39].Position.Should().Be(39);
            optionStrings[39].Raw.Should().Be(" --opt3");

            optionStrings[46].AliasPrefix.Should().BeFalse();
            optionStrings[46].Position.Should().Be(46);
            optionStrings[46].Raw.Should().Be(" --opt4 \"run2 -o4.1 v1 --o4.2 v2\"");

            optionStrings[79].AliasPrefix.Should().BeTrue();
            optionStrings[79].Position.Should().Be(79);
            optionStrings[79].Raw.Should().Be(" -opt5");
        }

        [Fact]
        public void Extracts_ValueValueDelimiter_With_Options_In_Value_Single()
        {
            OptionStrings optionStrings = TerminalHelper.ExtractOptionStrings(" --opt1 \"run -o1 v1 --o2 v2\"", terminalOptions, textHandler);

            optionStrings[0].AliasPrefix.Should().BeFalse();
            optionStrings[0].Position.Should().Be(0);
            optionStrings[0].Raw.Should().Be(" --opt1 \"run -o1 v1 --o2 v2\"");
        }

        [Fact]
        public void Extracts_ValueValueDelimiter_Single()
        {
            OptionStrings optionStrings = TerminalHelper.ExtractOptionStrings(" --opt1 \"val1\"", terminalOptions, textHandler);

            optionStrings[0].AliasPrefix.Should().BeFalse();
            optionStrings[0].Position.Should().Be(0);
            optionStrings[0].Raw.Should().Be(" --opt1 \"val1\"");
        }

        [Fact]
        public void Extracts_Single()
        {
            OptionStrings optionStrings = TerminalHelper.ExtractOptionStrings(" --opt1 val1", terminalOptions, textHandler);

            optionStrings[0].AliasPrefix.Should().BeFalse();
            optionStrings[0].Position.Should().Be(0);
            optionStrings[0].Raw.Should().Be(" --opt1 val1");
        }

        [Fact]
        public void Extracts_Single_Multiple_Spaces()
        {
            OptionStrings optionStrings = TerminalHelper.ExtractOptionStrings("    --opt1 val1", terminalOptions, textHandler);

            optionStrings[0].AliasPrefix.Should().BeFalse();
            optionStrings[0].Position.Should().Be(0);
            optionStrings[0].Raw.Should().Be(" --opt1 val1");
        }

        [Fact]
        public void Extracts_Single_Switch()
        {
            OptionStrings optionStrings = TerminalHelper.ExtractOptionStrings(" --opt1", terminalOptions, textHandler);

            optionStrings[0].AliasPrefix.Should().BeFalse();
            optionStrings[0].Position.Should().Be(0);
            optionStrings[0].Raw.Should().Be(" --opt1");
        }

        [Fact]
        public void Extracts_Alias_Single_Switch()
        {
            OptionStrings optionStrings = TerminalHelper.ExtractOptionStrings(" -opt1", terminalOptions, textHandler);

            optionStrings[0].AliasPrefix.Should().BeTrue();
            optionStrings[0].Position.Should().Be(0);
            optionStrings[0].Raw.Should().Be(" -opt1");
        }

        [Fact]
        public void Extracts_Alias_Single()
        {
            OptionStrings optionStrings = TerminalHelper.ExtractOptionStrings(" -opt1 val1", terminalOptions, textHandler);

            optionStrings[0].AliasPrefix.Should().BeTrue();
            optionStrings[0].Position.Should().Be(0);
            optionStrings[0].Raw.Should().Be(" -opt1 val1");
        }

        [Fact]
        public void Extracts_Alias_Single_Multiple_Spaces()
        {
            OptionStrings optionStrings = TerminalHelper.ExtractOptionStrings("      -opt1 val1", terminalOptions, textHandler);

            optionStrings[0].AliasPrefix.Should().BeTrue();
            optionStrings[0].Position.Should().Be(0);
            optionStrings[0].Raw.Should().Be(" -opt1 val1");
        }

        [Fact]
        public void Extracts_ValueValueDelimiter_With_Alias_Multiple_Options()
        {
            OptionStrings optionStrings = TerminalHelper.ExtractOptionStrings(" --opt1 \"val1\" -opt2 val2 --opt3 -opt4 -opt5 \"val5\" -opt6", terminalOptions, textHandler);

            optionStrings[0].AliasPrefix.Should().BeFalse();
            optionStrings[0].Position.Should().Be(0);
            optionStrings[0].Raw.Should().Be(" --opt1 \"val1\"");

            optionStrings[14].AliasPrefix.Should().BeTrue();
            optionStrings[14].Position.Should().Be(14);
            optionStrings[14].Raw.Should().Be(" -opt2 val2");

            optionStrings[25].AliasPrefix.Should().BeFalse();
            optionStrings[25].Position.Should().Be(25);
            optionStrings[25].Raw.Should().Be(" --opt3");

            optionStrings[32].AliasPrefix.Should().BeTrue();
            optionStrings[32].Position.Should().Be(32);
            optionStrings[32].Raw.Should().Be(" -opt4");

            optionStrings[38].AliasPrefix.Should().BeTrue();
            optionStrings[38].Position.Should().Be(38);
            optionStrings[38].Raw.Should().Be(" -opt5 \"val5\"");

            optionStrings[51].AliasPrefix.Should().BeTrue();
            optionStrings[51].Position.Should().Be(51);
            optionStrings[51].Raw.Should().Be(" -opt6");
        }

        [Fact]
        public void Extracts_With_Alias_Multiple_Options()
        {
            OptionStrings optionStrings = TerminalHelper.ExtractOptionStrings(" --opt1 val1 -opt2 val2 --opt3 -opt4 --opt5 val5 -opt6", terminalOptions, textHandler);

            optionStrings[0].AliasPrefix.Should().BeFalse();
            optionStrings[0].Position.Should().Be(0);
            optionStrings[0].Raw.Should().Be(" --opt1 val1");

            optionStrings[12].AliasPrefix.Should().BeTrue();
            optionStrings[12].Position.Should().Be(12);
            optionStrings[12].Raw.Should().Be(" -opt2 val2");

            optionStrings[23].AliasPrefix.Should().BeFalse();
            optionStrings[23].Position.Should().Be(23);
            optionStrings[23].Raw.Should().Be(" --opt3");

            optionStrings[30].AliasPrefix.Should().BeTrue();
            optionStrings[30].Position.Should().Be(30);
            optionStrings[30].Raw.Should().Be(" -opt4");

            optionStrings[36].AliasPrefix.Should().BeFalse();
            optionStrings[36].Position.Should().Be(36);
            optionStrings[36].Raw.Should().Be(" --opt5 val5");

            optionStrings[48].AliasPrefix.Should().BeTrue();
            optionStrings[48].Position.Should().Be(48);
            optionStrings[48].Raw.Should().Be(" -opt6");
        }

        [Fact]
        public void Extracts_With_Alias_Multiple_Options_With_AliasPrefix_In_OptionId()
        {
            OptionStrings optionStrings = TerminalHelper.ExtractOptionStrings(" --opt-dash1 val1 -opt2 val2 --opt3-dash3 -opt4 --opt5-dash5 val5 -opt6", terminalOptions, textHandler);

            optionStrings[0].AliasPrefix.Should().BeFalse();
            optionStrings[0].Position.Should().Be(0);
            optionStrings[0].Raw.Should().Be(" --opt-dash1 val1");

            optionStrings[17].AliasPrefix.Should().BeTrue();
            optionStrings[17].Position.Should().Be(17);
            optionStrings[17].Raw.Should().Be(" -opt2 val2");

            optionStrings[28].AliasPrefix.Should().BeFalse();
            optionStrings[28].Position.Should().Be(28);
            optionStrings[28].Raw.Should().Be(" --opt3-dash3");

            optionStrings[41].AliasPrefix.Should().BeTrue();
            optionStrings[41].Position.Should().Be(41);
            optionStrings[41].Raw.Should().Be(" -opt4");

            optionStrings[47].AliasPrefix.Should().BeFalse();
            optionStrings[47].Position.Should().Be(47);
            optionStrings[47].Raw.Should().Be(" --opt5-dash5 val5");

            optionStrings[65].AliasPrefix.Should().BeTrue();
            optionStrings[65].Position.Should().Be(65);
            optionStrings[65].Raw.Should().Be(" -opt6");
        }

        [Fact]
        public void Extracts_With_Multiple_Options()
        {
            OptionStrings optionStrings = TerminalHelper.ExtractOptionStrings(" --opt1 val1 --opt2 val2 --opt3", terminalOptions, textHandler);

            optionStrings[0].AliasPrefix.Should().BeFalse();
            optionStrings[0].Position.Should().Be(0);
            optionStrings[0].Raw.Should().Be(" --opt1 val1");

            optionStrings[12].AliasPrefix.Should().BeFalse();
            optionStrings[12].Position.Should().Be(12);
            optionStrings[12].Raw.Should().Be(" --opt2 val2");

            optionStrings[24].AliasPrefix.Should().BeFalse();
            optionStrings[24].Position.Should().Be(24);
            optionStrings[24].Raw.Should().Be(" --opt3");
        }

        [Theory]
        [InlineData(" asdasdas")]
        [InlineData(" 123123")]
        [InlineData(" asd768ads")]
        [InlineData(" $#$^@HJA(**")]
        public void Garbage_Raw_String_Does_Not_Error(string raw)
        {
            OptionStrings optionStrings = TerminalHelper.ExtractOptionStrings(raw, terminalOptions, textHandler);
            optionStrings.Count.Should().Be(1);

            OptionString optionString = optionStrings[0];
            optionString.Position.Should().Be(0);
            optionString.Raw.Should().Be(raw);
            optionString.AliasPrefix.Should().BeTrue();
        }

        private readonly TerminalOptions terminalOptions;
        private readonly ITextHandler textHandler;
    }
}