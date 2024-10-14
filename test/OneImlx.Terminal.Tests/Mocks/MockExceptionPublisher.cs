/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Runtime;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Mocks
{
    public class MockExceptionPublisher : ITerminalExceptionHandler
    {
        public MockExceptionPublisher()
        {
            MultiplePublishedMessages = [];
        }

        public bool Called { get; set; }

        public List<string> MultiplePublishedMessages { get; set; }

        public string? PublishedMessage { get; set; }

        public Task HandleExceptionAsync(TerminalExceptionHandlerContext context)
        {
            Called = true;
            PublishedMessage = context.Exception.Message;
            MultiplePublishedMessages.Add(context.Exception.Message);
            return Task.CompletedTask;
        }
    }
}
