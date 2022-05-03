/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System.Net.Http;

namespace PerpetualIntelligence.Cli.Configuration.Options
{
    /// <summary>
    /// The HTTP configuration options.
    /// </summary>
    public class HttpOptions
    {
        /// <summary>
        /// The <see cref="HttpClient"/> logical name.
        /// </summary>
        /// <remarks>
        /// <c>pi-cli</c> uses <see cref="IHttpClientFactory.CreateClient(string)"/> with the configured name to create
        /// an instance of <see cref="HttpClient"/>.
        /// </remarks>
        public string? HttpClientName { get; set; }
    }
}
