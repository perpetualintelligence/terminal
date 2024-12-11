/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands.Handlers
{
    /// <summary>
    /// An abstraction to handle a command request routed from a <see cref="ICommandRouter"/>.
    /// </summary>
    public interface ICommandHandler
    {
        /// <summary>
        /// Handles the command request.
        /// </summary>
        /// <param name="context">The handler context.</param>
        /// <returns>The handler result.</returns>
        Task HandleCommandAsync(CommandRouterContext context);
    }
}
