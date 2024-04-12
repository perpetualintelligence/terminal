/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Xunit;

namespace OneImlx.Terminal.Commands.Parsers
{
    //example of a unit test for the Root class
    public class RootTests
    {
        [Fact]
        public void Default_CreatesRoot_ReturnsDefault()
        {
            var root1 = Root.Default();
            root1.IsDefault.Should().BeTrue();
            root1.LinkedCommand.Should().NotBeNull();
            root1.LinkedCommand.Descriptor.Id.Should().Be("default");
            root1.LinkedCommand.Descriptor.Name.Should().Be("default");
            root1.LinkedCommand.Descriptor.Description.Should().Be("Default root command.");
            root1.ChildSubCommand.Should().BeNull();

            var cmd = new Command(new CommandDescriptor("cmd", "cmd", "just a command", CommandType.SubCommand, CommandFlags.None));
            var subCommand = new SubCommand(cmd);
            var root2 = Root.Default(subCommand);
            root2.IsDefault.Should().BeTrue();
            root2.LinkedCommand.Should().NotBeNull();
            root2.LinkedCommand.Descriptor.Id.Should().Be("default");
            root2.LinkedCommand.Descriptor.Name.Should().Be("default");
            root2.LinkedCommand.Descriptor.Description.Should().Be("Default root command.");
            root2.ChildSubCommand.Should().Be(subCommand);

            root1.Should().NotBe(root2);
        }
    }
}