/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Protocols.Abstractions;
using System;

namespace PerpetualIntelligence.Cli.Commands.Handlers
{
    /// <summary>
    /// An abstraction to publish <see cref="Exception"/>.
    /// </summary>
    public interface IExceptionHandler : IPublisherNoResult<ExceptionHandlerContext>
    {
    }
}
