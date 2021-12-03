/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Protocols.Abstractions;

namespace PerpetualIntelligence.Cli.Commands.Checkers
{
    /// <summary>
    /// An abstraction to check a <see cref="Command"/>.
    /// </summary>
    public interface ICommandChecker : IChecker<CommandCheckerContext, CommandCheckerResult>
    {
    }
}
