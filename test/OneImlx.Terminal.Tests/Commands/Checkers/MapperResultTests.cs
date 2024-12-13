/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using System;
using Xunit;

namespace OneImlx.Terminal.Commands.Checkers
{
    public class MapperResultTests
    {
        [Fact]
        public void DataTypeMapperResultNullMappedTypeShouldThrow()
        {
#pragma warning disable CA1806 // Do not ignore method results
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action act = static () => new DataTypeMapperResult(null);
            act.Should().Throw<ArgumentNullException>();
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CA1806 // Do not ignore method results
        }
    }
}
