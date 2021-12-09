/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using System;
using System.Text.Json.Serialization;

namespace PerpetualIntelligence.OneImlx.Cli.Runners
{
    public sealed class OneImlxTestFile
    {
        [JsonPropertyName("create_stamp")]
        public DateTimeOffset CreateStamp { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("path")]
        public string Path { get; set; } = null!;

        [JsonPropertyName("update_stamp")]
        public DateTimeOffset UpdateStamp { get; set; }
    }
}
