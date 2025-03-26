/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Events;
using OneImlx.Terminal.Shared;

namespace OneImlx.Terminal.Apps.TestServer
{
    internal class TestServerEventHandler : ITerminalEventHandler
    {
        public Task AfterCommandCheckAsync(Command command, CommandCheckerResult result)
        {
            throw new System.NotImplementedException();
        }

        public Task AfterCommandRouteAsync(TerminalRequest request, Command? command, CommandResult? result)
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

        public Task BeforeCommandRouteAsync(TerminalRequest request)
        {
            throw new System.NotImplementedException();
        }

        public Task BeforeCommandRunAsync(Command command)
        {
            throw new System.NotImplementedException();
        }
    }
}
