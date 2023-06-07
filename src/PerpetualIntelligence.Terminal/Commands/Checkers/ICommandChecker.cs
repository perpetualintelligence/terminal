/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Shared.Abstractions;

namespace PerpetualIntelligence.Terminal.Commands.Checkers
{
    /// <summary>
    /// An abstraction to check a <see cref="Command"/>.
    /// </summary>
    public interface ICommandChecker : IChecker<CommandCheckerContext, CommandCheckerResult>
    {
    }
}
