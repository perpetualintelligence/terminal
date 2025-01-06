/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Grpc.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Server
{
    public class MockServerCallContext : ServerCallContext
    {
        public MockServerCallContext(string peer)
        {
            this.peer = peer;
        }

        protected override AuthContext AuthContextCore => new("TestContext", []);

        protected override CancellationToken CancellationTokenCore => CancellationToken.None;

        protected override DateTime DeadlineCore => DateTime.UtcNow.AddMinutes(30);

        protected override string HostCore => "localhost";

        // Override required properties and methods to create a minimal working version for testing
        protected override string MethodCore => "TestMethod";

        protected override string PeerCore => peer;

        protected override Metadata RequestHeadersCore => [];

        protected override Metadata ResponseTrailersCore => [];

        protected override Status StatusCore { get; set; }

        protected override WriteOptions WriteOptionsCore { get; set; }

        protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions options) => null;

        protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders)
        {
            return Task.CompletedTask;
        }

        private readonly string peer;
    }
}
