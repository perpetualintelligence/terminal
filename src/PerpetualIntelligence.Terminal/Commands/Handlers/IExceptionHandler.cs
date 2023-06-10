/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Shared.Abstractions;
using System;

namespace PerpetualIntelligence.Terminal.Commands.Handlers
{
    /// <summary>
    /// An abstraction to handle <see cref="Exception"/>.
    /// </summary>
    public interface IExceptionHandler : IHandlerNoResult<ExceptionHandlerContext>
    {
    }
}
