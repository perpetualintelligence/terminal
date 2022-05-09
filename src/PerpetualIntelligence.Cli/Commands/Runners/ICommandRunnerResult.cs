/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Protocols.Abstractions;
using System;

namespace PerpetualIntelligence.Cli.Commands.Runners
{
    /// <summary>
    /// An abstraction of command runner result.
    /// </summary>
    public interface ICommandRunnerResult : IProcessorNoResult<CommandRunnerResultProcessorContext>, IAsyncDisposable
    {
    }
}
