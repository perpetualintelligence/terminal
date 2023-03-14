/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Shared.Abstractions;

namespace PerpetualIntelligence.Cli.Commands.Checkers
{
    /// <summary>
    /// An abstraction to check an <see cref="Option"/>.
    /// </summary>
    public interface IOptionChecker : IChecker<OptionCheckerContext, OptionCheckerResult>
    {
    }
}
