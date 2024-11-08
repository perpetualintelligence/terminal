/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using System;
using Xunit;

namespace OneImlx.Terminal.Runtime
{
    public class TerminalProcessorRequestTests
    {
        [Fact]
        public void Constructor_ShouldThrowException_WhenIdIsNullOrWhitespace()
        {
            Action createWithEmptyString = () => new TerminalProcessorRequest("", "test");
            Action createWithWhitespace = () => new TerminalProcessorRequest(" ", "test");
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action createWithNull = () => new TerminalProcessorRequest(null, "test");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            createWithEmptyString.Should().Throw<ArgumentNullException>().WithMessage($"*{nameof(TerminalProcessorRequest.Id)}*");
            createWithWhitespace.Should().Throw<ArgumentNullException>().WithMessage($"*{nameof(TerminalProcessorRequest.Id)}*");
            createWithNull.Should().Throw<ArgumentNullException>().WithMessage($"*{nameof(TerminalProcessorRequest.Id)}*");
        }

        [Fact]
        public void Equals_ShouldReturnTrue_ForIdenticalObjects()
        {
            var route1 = new TerminalProcessorRequest("id1", "test");
            var route2 = new TerminalProcessorRequest("id1", "test");

            route1.Equals(route2).Should().BeTrue();
        }

        [Fact]
        public void Equals_ShouldReturnFalse_ForDifferentObjects()
        {
            var route1 = new TerminalProcessorRequest("id1", "test");
            var route2 = new TerminalProcessorRequest("id2", "test");

            route1.Equals(route2).Should().BeFalse();
        }

        [Fact]
        public void Equals_ShouldReturnFalse_WhenComparingObjectToNull()
        {
            var request = new TerminalProcessorRequest("id1", "test");
            request.Equals(null).Should().BeFalse();
        }

        [Fact]
        public void OperatorEqual_ShouldReturnTrue_WhenComparingIdenticalObjects()
        {
            var route1 = new TerminalProcessorRequest("id1", "test");
            var route2 = new TerminalProcessorRequest("id1", "test");

            (route1 == route2).Should().BeTrue();
        }

        [Fact]
        public void OperatorNotEqual_ShouldReturnTrue_WhenComparingDifferentObjects()
        {
            var route1 = new TerminalProcessorRequest("id1", "test");
            var route2 = new TerminalProcessorRequest("id2", "test");

            (route1 != route2).Should().BeTrue();
        }

        [Fact]
        public void OperatorEqual_ShouldReturnTrue_WhenComparingNullToNull()
        {
            TerminalProcessorRequest? left = null;
            TerminalProcessorRequest? right = null;

            (left == right).Should().BeTrue();
        }

        [Fact]
        public void OperatorNotEqual_ShouldReturnFalse_WhenComparingNullToNull()
        {
            TerminalProcessorRequest? left = null;
            TerminalProcessorRequest? right = null;

            (left != right).Should().BeFalse();
        }

        [Fact]
        public void Equals_ShouldReturnTrue_ForObjectsWithSameIdButDifferentCommands()
        {
            var route1 = new TerminalProcessorRequest("id1", "test1");
            var route2 = new TerminalProcessorRequest("id1", "test2");

            route1.Equals(route2).Should().BeTrue();
        }

        [Fact]
        public void OperatorEqual_ShouldReturnTrue_ForObjectsWithSameIdButDifferentCommands()
        {
            var route1 = new TerminalProcessorRequest("id1", "test1");
            var route2 = new TerminalProcessorRequest("id1", "test2");

            (route1 == route2).Should().BeTrue();
        }

        [Fact]
        public void OperatorNotEqual_ShouldReturnFalse_ForObjectsWithSameIdButDifferentCommands()
        {
            var route1 = new TerminalProcessorRequest("id1", "test1");
            var route2 = new TerminalProcessorRequest("id1", "test2");

            (route1 != route2).Should().BeFalse();
        }
    }
}