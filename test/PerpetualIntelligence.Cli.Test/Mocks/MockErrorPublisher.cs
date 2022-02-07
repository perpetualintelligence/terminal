/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Publishers;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Mocks
{
    public class MockErrorPublisher : IErrorPublisher
    {
        public bool Called { get; set; }

        public string PublishedMessage { get; set; }

        public Task PublishAsync(ErrorPublisherContext context)
        {
            Called = true;
            PublishedMessage = context.Error.FormatDescription();
            return Task.CompletedTask;
        }
    }
}
