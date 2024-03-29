﻿/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

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
            MultiplePublishedMessages = new List<string>();
        }

        public bool Called { get; set; }

        public string? PublishedMessage { get; set; }

        public List<string> MultiplePublishedMessages { get; set; }

        public Task HandleExceptionAsync(TerminalExceptionHandlerContext context)
        {
            Called = true;
            PublishedMessage = context.Exception.Message;
            MultiplePublishedMessages.Add(context.Exception.Message);
            return Task.CompletedTask;
        }
    }
}