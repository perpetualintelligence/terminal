/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Publishers;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Mocks
{
    public class MockExceptionPublisher : IExceptionPublisher
    {
        public bool Called { get; set; }

        public string? PublishedMessage { get; set; }

        public Task PublishAsync(ExceptionPublisherContext context)
        {
            Called = true;
            PublishedMessage = context.Exception.Message;
            return Task.CompletedTask;
        }
    }
}
