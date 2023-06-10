/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Shared.Abstractions;

namespace PerpetualIntelligence.Terminal.Commands.Checkers
{
    /// <summary>
    /// An abstraction of <see cref="TerminalOptions"/> checker.
    /// </summary>
    public interface IConfigurationOptionsChecker : ICheckerNoResult<TerminalOptions>
    {
        
    }
}
