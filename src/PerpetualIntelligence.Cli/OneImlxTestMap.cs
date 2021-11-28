/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using System.Text.Json.Serialization;

namespace PerpetualIntelligence.Cli
{
    public sealed class OneImlxTestMap
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("source")]
        public OneImlxTestMethod Source { get; set; } = null!;

        [JsonPropertyName("target")]
        public OneImlxTestMethod? Target { get; set; } = null!;
    }
}
