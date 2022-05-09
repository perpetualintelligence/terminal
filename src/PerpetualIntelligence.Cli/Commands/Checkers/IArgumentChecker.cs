/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Protocols.Abstractions;

namespace PerpetualIntelligence.Cli.Commands.Checkers
{
    /// <summary>
    /// An abstraction to check an <see cref="Argument"/>.
    /// </summary>
    public interface IArgumentChecker : IChecker<ArgumentCheckerContext, ArgumentCheckerResult>
    {
    }
}
