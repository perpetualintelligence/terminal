/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.IO;
using System.Net;
using System.Text.Json.Serialization;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// Represents a <see cref="TerminalQueue"/> item received from a remote sender to be processed by the
    /// terminal router.
    /// </summary>
    public sealed class TerminalQueueResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TerminalQueueRequest"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the command item.</param>
        /// <param name="response">The response stream.</param>
        [JsonConstructor]
        public TerminalQueueResult(string id, Stream response)
        {
            Id = id;
            Response = response;
        }

        /// <summary>
        /// Gets the unique identifier for the command item.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; }

        /// <summary>
        /// Gets the response stream.
        /// </summary>
        [JsonIgnore]
        public Stream Response { get; }
    }
}
