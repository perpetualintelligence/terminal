/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Providers
{
    /// <summary>
    /// An abstraction to provide help for commands.
    /// </summary>
    public interface IHelpProvider
    {
        /// <summary>
        /// Provides help asynchronously.
        /// </summary>
        /// <returns></returns>
        public Task ProvideHelpAsync();
    }
}