/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Text.Json;
using FluentAssertions;
using OneImlx.Test.FluentAssertions;
using Xunit;

namespace OneImlx.Terminal
{
    public class AssemblyTests
    {
        [Fact]
        public void JsonTest()
        {
            // Test Json serilzation and deserialization of null value
            object? oldValue = null;
            var json = JsonSerializer.SerializeToUtf8Bytes(oldValue);
            var newValue = JsonSerializer.Deserialize<object>(json);
            newValue.Should().Be(oldValue);

            oldValue = "   ";
            json = JsonSerializer.SerializeToUtf8Bytes(oldValue);
            newValue = JsonSerializer.Deserialize<object>(json);

            oldValue = "";
            json = JsonSerializer.SerializeToUtf8Bytes(oldValue);
            newValue = JsonSerializer.Deserialize<object>(json);

            oldValue = "test";
            json = JsonSerializer.SerializeToUtf8Bytes(oldValue);
            newValue = JsonSerializer.Deserialize<object>(json);

            oldValue = 1;
            json = JsonSerializer.SerializeToUtf8Bytes(oldValue);
            newValue = JsonSerializer.Deserialize<object>(json);

            oldValue = 1.0;
            json = JsonSerializer.SerializeToUtf8Bytes(oldValue);
            newValue = JsonSerializer.Deserialize<object>(json);

            oldValue = true;
            json = JsonSerializer.SerializeToUtf8Bytes(oldValue);
            newValue = JsonSerializer.Deserialize<object>(json);
        }

        [Fact]
        public void TypesLocationTest()
        {
            typeof(TerminalErrors).Assembly.Should().HaveTypesInValidLocations();
        }

        [Fact]
        public void TypesNamespaceTest()
        {
            typeof(TerminalErrors).Assembly.Should().HaveTypesInRootNamespace("OneImlx.Terminal");
        }
    }
}
