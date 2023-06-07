/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Commands;
using PerpetualIntelligence.Terminal.Commands.Checkers;
using PerpetualIntelligence.Terminal.Commands.Routers;
using PerpetualIntelligence.Terminal.Commands.Runners;
using PerpetualIntelligence.Terminal.Events;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Mocks
{
    internal class MockAsyncEventHandler : IAsyncEventHandler
    {
        public Command? PassedCommand { get; private set; }
        public CommandRouterResult? PassedRouterResult { get; private set; }
        public CommandRoute? PassedRoute { get; private set; }

        public bool AfterRouteCalled { get; private set; }
        public bool BeforeRouteCalled { get; private set; }
        public bool AfterRunCalled { get; private set; }
        public bool BeforeRunCalled { get; private set; }
        public bool AfterCheckCalled { get; private set; }
        public bool BeforeCheckCalled { get; private set; }

        public Task AfterCommandCheckAsync(Command command, CommandCheckerResult result)
        {
            AfterCheckCalled = true;
            return Task.CompletedTask;
        }

        public Task AfterCommandRouteAsync(CommandRoute route, Command? command, CommandRouterResult? result)
        {
            PassedRoute = route;
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

        public Task BeforeCommandRouteAsync(CommandRoute commandRoute)
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