/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using System.Text.Json.Serialization;

namespace PerpetualIntelligence.OneImlx.Cli.Runners
{
    public sealed class OneImlxTestMethod
    {
        [JsonPropertyName("assembly")]
        public string Assembly { get; set; } = null!;

        [JsonPropertyName("declaring_type")]
        public string DeclaringType { get; set; } = null!;

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("namespace")]
        public string Namespace { get; set; } = null!;
    }
}
