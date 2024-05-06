/*
    Copyright 2024 (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Handlers;
using OneImlx.Terminal.Commands.Handlers.Mocks;
using OneImlx.Terminal.Commands.Runners;

namespace OneImlx.Terminal.Mocks
{
    internal class MockCommandRuntime : ICommandRuntime
    {
        public bool ResolveAuthenticatorCalled { get; private set; }

        public bool ResolveCheckerCalled { get; private set; }

        public bool ResolveRunnerCalled { get; private set; }

        public ICommandChecker? ReturnedChecker { get; private set; }

        public IDelegateCommandRunner? ReturnedRunner { get; private set; }

        public ICommandChecker? ReturnThisChecker { get; set; }

        public IDelegateCommandRunner? ReturnThisRunner { get; set; }

        public ICommandChecker ResolveCommandChecker(CommandDescriptor commandDescriptor)
        {
            ResolveCheckerCalled = true;

            if (ReturnThisChecker != null)
            {
                ReturnedChecker = ReturnThisChecker;
            }
            else
            {
                ReturnedChecker = new MockCommandCheckerInner();
            }
            return ReturnedChecker;
        }

        public IDelegateCommandRunner ResolveCommandRunner(CommandDescriptor commandDescriptor)
        {
            ResolveRunnerCalled = true;

            if (ReturnThisRunner != null)
            {
                ReturnedRunner = ReturnThisRunner;
            }
            else
            {
                ReturnedRunner = new MockCommandRunnerInner();
            }

            return ReturnedRunner;
        }
    }
}
