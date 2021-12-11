/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
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
