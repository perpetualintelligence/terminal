/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Shared.Abstractions;
using PerpetualIntelligence.Shared.Infrastructure;

namespace PerpetualIntelligence.Terminal.Commands.Handlers
{
    /// <summary>
    /// An abstraction to handle <see cref="Error"/>.
    /// </summary>
    public interface IErrorHandler : IHandlerNoResult<ErrorHandlerContext>
    {
    }
}
