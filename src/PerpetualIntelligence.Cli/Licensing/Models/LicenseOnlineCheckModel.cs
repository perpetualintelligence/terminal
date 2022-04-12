/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Protocols.Licensing;
using System.Text.Json.Serialization;

namespace PerpetualIntelligence.Cli.Licensing.Models
{
    /// <summary>
    /// The <see cref="SaaSCheckModes.Online"/> check model.
    /// </summary>
    public class LicenseOnlineCheckModel
    {
        /// <summary>
        /// The Authorized party. This is also the <c>azp</c> claim.
        /// </summary>
        [JsonPropertyName("authorized_party")]
        public string AuthorizedParty { get; set; } = null!;

        /// <summary>
        /// The optional consumer object id.
        /// </summary>
        [JsonPropertyName("consumer_object_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ConsumerObjectId { get; set; }

        /// <summary>
        /// The consumer tenant id.
        /// </summary>
        [JsonPropertyName("consumer_tenant_id")]
        public string ConsumerTenantId { get; set; } = null!;

        /// <summary>
        /// The key to check.
        /// </summary>
        [JsonPropertyName("key")]
        public string Key { get; set; } = null!;

        /// <summary>
        /// The key type. <c>primary_key</c> or <c>secondary_key</c>.
        /// </summary>
        [JsonPropertyName("key_type")]
        public string KeyType { get; set; } = null!;

        /// <summary>
        /// The provider tenant id.
        /// </summary>
        [JsonPropertyName("provider_tenant_id")]
        public string ProviderTenantId { get; set; } = null!;

        /// <summary>
        /// The subject. This is also the <c>sub</c> claim.
        /// </summary>
        [JsonPropertyName("subject")]
        public string Subject { get; set; } = null!;
    }
}
