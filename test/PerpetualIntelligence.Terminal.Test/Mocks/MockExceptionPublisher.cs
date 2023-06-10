/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Commands.Handlers;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Mocks
{
    public class MockExceptionPublisher : IExceptionHandler
    {
        public bool Called { get; set; }

        public string? PublishedMessage { get; set; }

        public Task HandleAsync(ExceptionHandlerContext context)
        {
            Called = true;
            PublishedMessage = context.Exception.Message;
            return Task.CompletedTask;
        }
    }
}
