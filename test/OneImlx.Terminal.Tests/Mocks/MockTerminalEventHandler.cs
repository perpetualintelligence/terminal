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
using OneImlx.Terminal.Runtime;

namespace OneImlx.Terminal.Mocks
{
    internal class MockTerminalEventHandler : ITerminalEventHandler
    {
        public bool AfterCheckCalled { get; private set; }

        public bool AfterRouteCalled { get; private set; }

        public bool AfterRunCalled { get; private set; }

        public bool BeforeCheckCalled { get; private set; }

        public bool BeforeRouteCalled { get; private set; }

        public bool BeforeRunCalled { get; private set; }

        public Command? PassedCommand { get; private set; }

        public TerminalRequest? PassedRoute { get; private set; }

        public CommandRouterResult? PassedRouterResult { get; private set; }

        public Task AfterCommandCheckAsync(Command command, CommandCheckerResult result)
        {
            AfterCheckCalled = true;
            return Task.CompletedTask;
        }

        public Task AfterCommandRouteAsync(TerminalRequest request, Command? command, CommandRouterResult? result)
        {
            PassedRoute = request;
            PassedCommand = command;
            PassedRouterResult = result;

            AfterRouteCalled = true;
            return Task.CompletedTask;
        }

        public Task AfterCommandRunAsync(Command command, CommandRunnerResult result)
        {
            AfterRunCalled = true;
            return Task.CompletedTask;
        }

        public Task BeforeCommandCheckAsync(Command command)
        {
            BeforeCheckCalled = true;
            return Task.CompletedTask;
        }

        public Task BeforeCommandRouteAsync(TerminalRequest request)
        {
            BeforeRouteCalled = true;
            return Task.CompletedTask;
        }

        public Task BeforeCommandRunAsync(Command command)
        {
            BeforeRunCalled = true;
            return Task.CompletedTask;
        }
    }
}
