/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands;
using PerpetualIntelligence.Cli.Commands.Checkers;
using PerpetualIntelligence.Cli.Commands.Runners;
using PerpetualIntelligence.Cli.Events;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Mocks
{
    internal class MockAsyncEventHandler : IAsyncCliEventHandler
    {
        public bool AfterRunCalled { get; private set; }
        public bool BeforeRunCalled { get; private set; }
        public bool AfterCheckCalled { get; private set; }
        public bool BeforeCheckCalled { get; private set; }

        public Task AfterCommandCheckAsync(CommandDescriptor commandDescriptor, Command command, CommandCheckerResult result)
        {
            AfterCheckCalled = true;
            return Task.CompletedTask;
        }

        public Task AfterCommandRunAsync(Command command, CommandRunnerResult result)
        {
            AfterRunCalled = true;
            return Task.CompletedTask;
        }

        public Task BeforeCommandCheckAsync(CommandDescriptor commandDescriptor, Command command)
        {
            BeforeCheckCalled = true;
            return Task.CompletedTask;
        }

        public Task BeforeCommandRunAsync(Command command)
        {
            BeforeRunCalled = true;
            return Task.CompletedTask;
        }
    }
}