/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Commands.Handlers;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Mocks
{
    public class MockErrorPublisher : IErrorHandler
    {
        public bool Called { get; set; }

        public string? PublishedMessage { get; set; }

        public Task HandleAsync(ErrorHandlerContext context)
        {
            Called = true;
            PublishedMessage = context.Error.FormatDescription();
            return Task.CompletedTask;
        }
    }
}
