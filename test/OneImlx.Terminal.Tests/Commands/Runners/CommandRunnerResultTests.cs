/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using OneImlx.Terminal.Shared;
using OneImlx.Test.FluentAssertions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Commands.Runners
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
        public async Task Empty_Returns_New_Instance()
        {
            var result1 = CommandRunnerResult.Empty();
            var result2 = CommandRunnerResult.Empty();
            result1.Should().NotBeSameAs(result2);

            var result3 = await CommandRunnerResult.EmptyAsync();
            var result4 = await CommandRunnerResult.EmptyAsync();
            result3.Should().NotBeSameAs(result4);

            result1.Should().NotBeSameAs(result3);
        }

        [Fact]
        public Task EmptyAysnc_Returns_Task()
        {
            var result = CommandRunnerResult.EmptyAsync();
            result.Should().BeOfType<Task<CommandRunnerResult>>();
            return result;
        }

        [Fact]
        public void Value_ShouldThrowInvalidOperationException_WhenValueIsNotSet()
        {
            var result = new CommandRunnerResult();
            Action act = () => { var value = result.Value; };
            act.Should().Throw<TerminalException>()
               .WithErrorCode(TerminalErrors.ServerError)
               .WithErrorDescription("The value is not set on the command runner result.");
        }
    }
}
