/*
    Copyright 2024 (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Routers;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Events;
using OneImlx.Terminal.Runtime;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Apps.TestServer
{
    internal class TestServerEventHandler : ITerminalEventHandler
    {
        public Task AfterCommandCheckAsync(Command command, CommandCheckerResult result)
        {
            throw new System.NotImplementedException();
        }

        public Task AfterCommandRouteAsync(TerminalProcessorRequest request, Command? command, CommandRouterResult? result)
        {
            throw new System.NotImplementedException();
        }

        public Task AfterCommandRunAsync(Command command, CommandRunnerResult result)
        {
            throw new System.NotImplementedException();
        }

        public Task BeforeCommandCheckAsync(Command command)
        {
            throw new System.NotImplementedException();
        }

        public Task BeforeCommandRouteAsync(TerminalProcessorRequest request)
        {
            throw new System.NotImplementedException();
        }

        public Task BeforeCommandRunAsync(Command command)
        {
            throw new System.NotImplementedException();
        }
    }
}
