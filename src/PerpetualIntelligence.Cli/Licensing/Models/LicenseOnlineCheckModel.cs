/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System.Text.Json.Serialization;

namespace PerpetualIntelligence.Cli.Licensing.Models
{
    /// <summary>
    /// The <see cref="LicenseCheckMode.Online"/> check model.
    /// </summary>
    public class LicenseOnlineCheckModel
    {
        /// <summary>
        /// The Authorized party. This is also the <c>azp</c> claim.
        /// </summary>
        [JsonPropertyName("authorized_party")]
        public string? AuthorizedParty { get; set; }

        /// <summary>
        /// The consumer tenant id.
        /// </summary>
        [JsonPropertyName("consumer_tenant_id")]
        public string? ConsumerTenantId { get; set; }

        /// <summary>
        /// The primary <c>jwt</c> license key.
        /// </summary>
        [JsonPropertyName("key")]
        public string? Key { get; set; }

        /// <summary>
        /// The provider tenant id.
        /// </summary>
        [JsonPropertyName("provider_tenant_id")]
        public string? ProviderTenantId { get; set; }

        /// <summary>
        /// The subject. This is also the <c>sub</c> claim.
        /// </summary>
        [JsonPropertyName("subject")]
        public string? Subject { get; set; }
    }
}
