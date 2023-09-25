/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Net.Http;

namespace PerpetualIntelligence.Terminal.Configuration.Options
{
    /// <summary>
    /// The HTTP configuration options.
    /// </summary>
    public class HttpOptions
    {
        /// <summary>
        /// The logical name to create and configure <see cref="HttpClient"/> instance.
        /// </summary>
        /// <remarks>
        /// The framework uses <see cref="IHttpClientFactory.CreateClient(string)"/> and the configured name
        /// to create an instance of <see cref="HttpClient"/>.
        /// </remarks>
        public string? HttpClientName { get; set; }
    }
}