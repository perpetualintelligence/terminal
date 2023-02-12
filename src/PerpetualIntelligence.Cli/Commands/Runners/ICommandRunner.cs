/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Protocols.Abstractions;

namespace PerpetualIntelligence.Cli.Commands.Runners
{
    /// <summary>
    /// An abstraction of a command runner.
    /// </summary>
    public interface ICommandRunner<TContext, TResult> : IRunner<TContext, TResult> where TContext : CommandRunnerContext where TResult : CommandRunnerResult
    {
    }
}