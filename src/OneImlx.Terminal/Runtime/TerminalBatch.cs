/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// Represents an ordered collection of commands.
    /// </summary>
    /// <remarks>The commands in a batch are executed by the router in the order they were added.</remarks>
    [JsonConverter(typeof(TerminalBatchJsonConverter))]
    public sealed class TerminalBatch : KeyedCollection<string, TerminalRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TerminalBatch"/> class.
        /// </summary>
        /// <param name="batchId">The batch identifier.</param>
        public TerminalBatch(string batchId)
        {
            BatchId = batchId ?? throw new ArgumentNullException(nameof(batchId), "The batch id cannot be null.");
        }

        /// <summary>
        /// The batch identifier.
        /// </summary>
        [JsonPropertyName("batch_id")]
        public string BatchId { get; }

        /// <summary>
        /// Adds a single command to the batch.
        /// </summary>
        /// <param name="id">The command id.</param>
        /// <param name="raw">The raw command string.</param>
        public void Add(string id, string raw)
        {
            Add(new TerminalRequest(id, raw));
        }

        /// <summary>
        /// Adds a single command to the batch.
        /// </summary>
        /// <param name="ids">The command identifiers.</param>
        /// <param name="raws">The raw command strings.</param>
        public void Add(string[] ids, string[] raws)
        {
            if (ids.Length != raws.Length)
            {
                throw new ArgumentException("The number of command ids must match the number of commands.");
            }

            for (int idx = 0; idx < ids.Length; ++idx)
            {
                Add(ids[idx], raws[idx]);
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return BatchId;
        }

        /// <summary>
        /// Returns the <see cref="TerminalRequest.Id"/> as an item key.
        /// </summary>
        /// <param name="item">The item to query.</param>
        protected override string GetKeyForItem(TerminalRequest item)
        {
            return item.Id;
        }
    }
}
