/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using FluentAssertions;
using OneImlx.Terminal.Commands.Runners;
using Xunit;

namespace OneImlx.Terminal.Tests.Commands.Runners
{
    public class CommandRunnerResultTests
    {
        [Fact]
        public void As_ShouldReturnValueAsSpecifiedType()
        {
            var value = "test";
            var result = new CommandRunnerResult(value);
            result.As<string>().Should().Be(value);
        }

        [Fact]
        public void As_ShouldThrowInvalidCastException_WhenValueCannotBeCast()
        {
            var value = "test";
            var result = new CommandRunnerResult(value);
            Action act = () => result.As<int>();
            act.Should().Throw<InvalidCastException>();
        }

        [Fact]
        public void As_ShouldThrowInvalidOperationException_WhenValueIsNotSet()
        {
            var result = new CommandRunnerResult();
            Action act = () => result.As<string>();
            act.Should().Throw<InvalidOperationException>().WithMessage("The value is not set.");
        }

        [Fact]
        public void Constructor_ShouldInitializeWithNullValue()
        {
            var result = new CommandRunnerResult();
            result.HasValue.Should().BeFalse();
        }

        [Fact]
        public void Constructor_ShouldInitializeWithValue()
        {
            var value = 123;
            var result = new CommandRunnerResult(value);
            result.HasValue.Should().BeTrue();
            result.Value.Should().Be(value);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenValueIsNull()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action act = static () => new CommandRunnerResult(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            act.Should().Throw<ArgumentNullException>().WithMessage("Value cannot be null. (Parameter 'value')");
        }

        [Fact]
        public void Value_ShouldThrowInvalidOperationException_WhenValueIsNotSet()
        {
            var result = new CommandRunnerResult();
            Action act = () => { var value = result.Value; };
            act.Should().Throw<InvalidOperationException>().WithMessage("The value is not set.");
        }
    }
}
