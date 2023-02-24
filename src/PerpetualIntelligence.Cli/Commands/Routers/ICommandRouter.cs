/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Routers
{
    /// <summary>
    /// An abstraction of a command router.
    /// </summary>
    public interface ICommandRouter
    {
        /// <summary>
        /// Routes the request asynchronously.
        /// </summary>
        /// <param name="context">The router context.</param>
        /// <returns>The router result.</returns>
        Task<CommandRouterResult> RouteAsync(CommandRouterContext context);
    }
}