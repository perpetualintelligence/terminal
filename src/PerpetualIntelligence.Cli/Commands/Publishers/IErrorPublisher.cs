/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Protocols.Abstractions;
using PerpetualIntelligence.Shared.Infrastructure;

namespace PerpetualIntelligence.Cli.Commands.Publishers
{
    /// <summary>
    /// An abstraction to publish <see cref="Error"/>.
    /// </summary>
    public interface IErrorPublisher : IPublisherNoResult<ErrorPublisherContext>
    {
    }
}
