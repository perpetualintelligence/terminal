/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using OneImlx.Terminal.Server;
using OneImlx.Test.FluentAssertions;
using Xunit;

namespace OneImlx.Terminal.Server
{
    public class AssemblyTests
    {
        [Fact]
        public void TypesLocationTest()
        {
            typeof(TerminalGrpcMapService).Assembly.Should().HaveTypesInValidLocations(
                [
                    typeof(OneimlxTerminalReflection),
                    typeof(TerminalGrpcRouterProtoInput),
                    typeof(TerminalGrpcRouterProtoOutput),
                    typeof(TerminalGrpcRouterProto),
                    typeof(TerminalGrpcRouterProto.TerminalGrpcRouterProtoBase),
                ]);
        }

        [Fact]
        public void TypesNamespaceTest()
        {
            typeof(TerminalGrpcMapService).Assembly.Should().HaveTypesInRootNamespace("OneImlx.Terminal.Server");
        }
    }
}
