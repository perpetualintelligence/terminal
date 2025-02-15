/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Text.Json.Serialization;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// Represents the input that is sent to the terminal server as an ordered collection of <see cref="TerminalRequest"/>.
    /// </summary>
    /// <remarks>
    /// The requests in the <see cref="TerminalInput"/> are executed by the router in the order they are added.
    /// </remarks>
    public sealed class TerminalInput
    {
        /// <summary>
        /// THIS METHOD IS RESERVED FOR OUR INTERNAL INFRASTRUCTURE USE ONLY. DO NOT USE IT IN YOUR APPLICATION. To
        /// create a new instance of <see cref="TerminalInput"/>, use the <see cref="Single(string, string)"/> or
        /// <see cref="Batch(string, TerminalRequest[])"/> method.
        /// </summary>
        /// <seealso cref="Single(string, string)"/>
        /// <seealso cref="Batch(string, TerminalRequest[])"/>
        /// <seealso cref="Batch(string, string[], string[])"/>
        public TerminalInput()
        {
            Requests = [];
        }

        /// <summary>
        /// The batch identifier.
        /// </summary>
        [JsonPropertyName("batch_id")]
        [JsonInclude]
        public string? BatchId { get; private set; }

        /// <summary>
        /// Gets the number of requests.
        /// </summary>
        [JsonIgnore]
        public int Count => Requests.Length;

        /// <summary>
        /// Gets a value indicating whether the input is a batch.
        /// </summary>
        [JsonIgnore]
        public bool IsBatch => !string.IsNullOrWhiteSpace(BatchId);

        /// <summary>
        /// The requests in the input.
        /// </summary>
        [JsonPropertyName("requests")]
        [JsonInclude]
        public TerminalRequest[] Requests { get; private set; }

        /// <summary>
        /// Creates a new <see cref="TerminalInput"/> for a batch of commands.
        /// </summary>
        /// <param name="batchId">The batch identifier.</param>
        /// <param name="ids">The command identifiers.</param>
        /// <param name="raws">The raw commands.</param>
        /// <returns>A new <see cref="TerminalInput"/> instance.</returns>
        /// <exception cref="ArgumentException">Thrown if the number of IDs does not match the number of raw commands.</exception>
        public static TerminalInput Batch(string batchId, string[] ids, string[] raws)
        {
            if (ids.Length != raws.Length)
            {
                throw new ArgumentException("The number of command IDs must match the number of raw commands.");
            }

            var requests = new TerminalRequest[ids.Length];
            for (int i = 0; i < ids.Length; i++)
            {
                requests[i] = new TerminalRequest(ids[i], raws[i]);
            }

            return new TerminalInput()
            {
                BatchId = batchId,
                Requests = requests,
            };
        }

        /// <summary>
        /// Creates a new <see cref="TerminalInput"/> for a batch of requests.
        /// </summary>
        /// <param name="batchId">The batch identifier.</param>
        /// <param name="requests">The array of <see cref="TerminalRequest"/> objects.</param>
        /// <returns>A new <see cref="TerminalInput"/> instance.</returns>
        /// <exception cref="ArgumentException">Thrown if the number of requests is zero.</exception>
        public static TerminalInput Batch(string batchId, TerminalRequest[] requests)
        {
            if (requests.Length == 0)
            {
                throw new ArgumentException("The number of requests must be greater than zero.");
            }

            return new TerminalInput()
            {
                BatchId = batchId,
                Requests = requests,
            };
        }

        /// <summary>
        /// Creates a new <see cref="TerminalInput"/> for a single command.
        /// </summary>
        /// <param name="id">The command identifier.</param>
        /// <param name="raw">The raw command.</param>
        /// <returns>A new <see cref="TerminalInput"/> instance.</returns>
        public static TerminalInput Single(string id, string raw)
        {
            var request = new TerminalRequest(id, raw);
            return new TerminalInput()
            {
                Requests = [request],
            };
        }

        /// <summary>
        /// Gets or sets the <see cref="TerminalRequest"/> at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        public TerminalRequest this[int index]
        {
            get => Requests[index];
            set => Requests[index] = value;
        }
    }
}
