/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System.Text.Json.Serialization;

namespace PerpetualIntelligence.Cli.Runtime
{
    /// <summary>
    /// Terminals, also known as command lines, consoles, or CLI applications, allow organizations and users to
    /// accomplish and automate tasks on a computer without using a graphical user interface. If a CLI terminal supports
    /// user interaction, the UX is the terminal.
    /// </summary>
    public sealed class Terminal
    {
        /// <summary>
        /// The authorized application id for this terminal.
        /// </summary>
        [JsonPropertyName("authorized_application_id")]
        public string AuthorizedApplicationId { get; set; } = null!;
    }
}