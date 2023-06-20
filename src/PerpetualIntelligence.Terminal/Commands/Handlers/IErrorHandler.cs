/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Shared.Infrastructure;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Handlers
{
    /// <summary>
    /// An abstraction to handle <see cref="Error"/>.
    /// </summary>
    public interface IErrorHandler
    {
        /// <summary>
        /// Handles <see cref="Error"/> asynchronously.
        /// </summary>
        /// <param name="context">The error context.</param>
        public Task HandleAsync(ErrorHandlerContext context);
    }
}