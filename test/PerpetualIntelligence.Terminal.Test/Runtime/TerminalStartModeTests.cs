/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Xunit;

namespace PerpetualIntelligence.Terminal.Runtime
{
    public class TerminalStartModeTests
    {
        [Fact]
        public void StartModes_Has_Correct_EnumValues()
        {
            ((int)TerminalStartMode.Tcp).Should().Be(1);
            ((int)TerminalStartMode.Http).Should().Be(2);
            ((int)TerminalStartMode.Grpc).Should().Be(3);
            ((int)TerminalStartMode.Udp).Should().Be(4);

            ((int)TerminalStartMode.Console).Should().Be(100);

            ((int)TerminalStartMode.Custom).Should().Be(0);
        }
    }
}