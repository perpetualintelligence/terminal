/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using OneImlx.Terminal.Commands;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// The <see cref="TerminalBatch"/> Json converter.
    /// </summary>
    public class TerminalBatchJsonConverter : JsonConverter<TerminalBatch>
    {
        /// <inheritdoc/>
        public override TerminalBatch Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Invalid JSON format: Expected StartObject.");
            }

            string? batchId = null;
            TerminalCommand[]? commands = null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    break;
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string? propertyName = reader.GetString();
                    reader.Read(); // Move to property value

                    switch (propertyName)
                    {
                        case "batch_id":
                            batchId = reader.GetString();
                            break;

                        case "commands":
                            if (reader.TokenType == JsonTokenType.StartArray)
                            {
                                commands = JsonSerializer.Deserialize<TerminalCommand[]>(ref reader, options);
                            }
                            else
                            {
                                throw new JsonException("Expected StartArray for 'commands'.");
                            }
                            break;

                        default:
                            {
                                throw new JsonException($"Unexpected property '{propertyName}' encountered in JSON.");
                            }
                    }
                }
            }

            if (batchId == null)
            {
                throw new JsonException("Missing 'batch_id' property.");
            }

            var batch = new TerminalBatch(batchId);
            if (commands != null)
            {
                foreach (var command in commands)
                {
                    batch.Add(command);
                }
            }

            return batch;
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, TerminalBatch value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value), "TerminalBatch cannot be null for serialization.");
            }

            writer.WriteStartObject();
            writer.WriteString("batch_id", value.BatchId);

            writer.WritePropertyName("commands");
            writer.WriteStartArray();
            foreach (var command in value)
            {
                // Direct serialization for optimal performance
                JsonSerializer.Serialize(writer, command, options);
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }
}
