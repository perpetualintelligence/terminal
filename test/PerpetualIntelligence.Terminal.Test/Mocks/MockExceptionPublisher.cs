/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Commands.Handlers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Mocks
{
    public class MockExceptionPublisher : IExceptionHandler
    {
        public MockExceptionPublisher()
        {
            MultiplePublishedMessages = new List<string>();
        }

        public bool Called { get; set; }

        public string? PublishedMessage { get; set; }

        public List<string> MultiplePublishedMessages { get; set; }

        public Task HandleExceptionAsync(ExceptionHandlerContext context)
        {
            Called = true;
            PublishedMessage = context.Exception.Message;
            MultiplePublishedMessages.Add(context.Exception.Message);
            return Task.CompletedTask;
        }
    }
}