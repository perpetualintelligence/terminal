/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using OneImlx.Terminal.Runtime;

namespace OneImlx.Terminal.Client.Extensions
{
    /// <summary>
    /// Provides extension methods for the <see cref="HttpClient"/> to interact with terminal servers via HTTP.
    /// </summary>
    /// <remarks>
    /// The terminal HTTP router expects the command to be sent as a JSON object with the following format to the
    /// following endpoint <c>oneimlx/terminal/httprouter</c>.
    /// </remarks>
    public static class HttpClientExtensions
    {
        /// <summary>
        /// Sends multiple command strings to a terminal server in a single batch HTTP POST request.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/> instance used to send the request.</param>
        /// <param name="batch">An array of command strings to send as a batch.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while awaiting completion.</param>
        /// <returns>
        /// A <see cref="HttpResponseMessage"/> representing the HTTP response. This response does not indicate the
        /// execution result of the commands on the terminal.
        /// </returns>
        /// <remarks>
        /// The command strings are concatenated and delimited to form a single batch message. This method improves
        /// efficiency by reducing the number of HTTP requests when multiple commands need to be sent. Delimiters are
        /// mandatory for batch requests.
        /// </remarks>
        public static Task<HttpResponseMessage> SendBatchAsync(this HttpClient httpClient, TerminalBatch batch, CancellationToken cancellationToken)
        {
            return httpClient.PostAsJsonAsync("oneimlx/terminal/httprouter", batch, cancellationToken);
        }

        /// <summary>
        /// Sends a single command string to a terminal server via an HTTP POST request using specified delimiters.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/> instance used to send the request.</param>
        /// <param name="request">The command request to send.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while awaiting completion.</param>
        /// <returns>
        /// A <see cref="HttpResponseMessage"/> representing the HTTP response. This response does not indicate the
        /// execution result of the command on the terminal.
        /// </returns>
        /// <remarks>
        /// The command string is formatted using the provided delimiters before being sent. These delimiters separate
        /// different parts of the command for correct parsing by the terminal server.
        /// </remarks>
        public static Task<HttpResponseMessage> SendSingleAsync(this HttpClient httpClient, TerminalRequest request, CancellationToken cancellationToken)
        {
            return httpClient.PostAsJsonAsync("oneimlx/terminal/httprouter", request, cancellationToken);
        }
    }
}
