/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Protocols.Abstractions;

namespace PerpetualIntelligence.Cli.Commands.Runners
{
    /// <summary>
    /// An abstraction of a command runner.
    /// </summary>
    public interface ICommandRunner : IRunner<CommandRunnerContext, CommandRunnerResult>
    {
    }
}
