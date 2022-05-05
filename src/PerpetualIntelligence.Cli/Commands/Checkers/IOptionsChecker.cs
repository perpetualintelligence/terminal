/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Protocols.Abstractions;

namespace PerpetualIntelligence.Cli.Commands.Checkers
{
    /// <summary>
    /// An abstraction of <see cref="CliOptions"/> checker.
    /// </summary>
    public interface IOptionsChecker : ICheckerNoResult<CliOptions>
    {
        
    }
}
