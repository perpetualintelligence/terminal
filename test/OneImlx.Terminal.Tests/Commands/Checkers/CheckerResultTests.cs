/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using System;
using Xunit;

namespace OneImlx.Terminal.Commands.Checkers
{
    public class CheckerResultTests
    {
        [Fact]
        public void OptionCheckerResultNullMappedTypeShouldThrow()
        {
#pragma warning disable CA1806 // Do not ignore method results
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action act = () => new OptionCheckerResult(null);
            act.Should().Throw<ArgumentNullException>();
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CA1806 // Do not ignore method results
        }

        [Fact]
        public void ArgumentCheckerResultNullMappedTypeShouldThrow()
        {
#pragma warning disable CA1806 // Do not ignore method results
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action act = () => new ArgumentCheckerResult(null);
            act.Should().Throw<ArgumentNullException>();
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CA1806 // Do not ignore method results
        }
    }
}