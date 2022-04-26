/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Handlers;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Mocks
{
    public class MockErrorPublisher : IErrorHandler
    {
        public bool Called { get; set; }

        public string? PublishedMessage { get; set; }

        public Task PublishAsync(ErrorHandlerContext context)
        {
            Called = true;
            PublishedMessage = context.Error.FormatDescription();
            return Task.CompletedTask;
        }
    }
}
