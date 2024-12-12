/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands
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
        Task<CommandRouterResult> RouteCommandAsync(CommandRouterContext context);
    }
}
