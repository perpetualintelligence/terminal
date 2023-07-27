/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Xunit;

namespace PerpetualIntelligence.Terminal
{
    public class TerminalHelperTests
    {
        [Fact]
        public void DevMode_Is_True_If_Debug_Mode()
        {
#if DEBUG
            TerminalHelper.IsDevMode().Should().BeTrue();
#endif
        }

        [Fact]
        public void DevMode_Is_False_If_Release_Mode()
        {
#if RELEASE
            TerminalHelper.IsDevMode().Should().BeFalse();
#endif
        }
    }
}