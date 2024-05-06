/*
    Copyright 2024 (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands.Handlers
{
    /// <summary>
    /// An abstraction for handling authentication and authorization logic for terminal commands.
    /// </summary>
    public interface ICommandAuthenticator
    {
        /// <summary>
        /// Asynchronously authenticates the principal to run a command.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AuthenticateAsync();

        /// <summary>
        /// Asynchronously authorizes the authenticated principal to run a command.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AuthorizeAsync();

        /// <summary>
        /// Asynchronously checks if the principal is authenticated.
        /// </summary>
        /// <returns>
        /// A task representing the asynchronous operation that returns true if the user is authenticated, otherwise false.
        /// </returns>
        Task<bool> IsAuthenticatedAsync();
    }
}
