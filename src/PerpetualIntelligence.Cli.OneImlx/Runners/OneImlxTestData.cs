/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using System.Text.Json.Serialization;

namespace PerpetualIntelligence.OneImlx.Cli.Runners
{
    public sealed class OneImlxTestData
    {
        [JsonPropertyName("files")]
        public OneImlxTestFile[] Files { get; set; } = null!;

        [JsonPropertyName("files_count")]
        public int FilesCount { get; set; }

        [JsonPropertyName("tests")]
        public OneImlxTestMap[] Tests { get; set; } = null!;

        [JsonPropertyName("tests_count")]
        public int TestsCount { get; set; }
    }
}
