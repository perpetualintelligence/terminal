/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// Terminals, also known as command lines, consoles, or CLI applications, allow organizations and users to
    /// accomplish and automate tasks on a computer without using a graphical user interface. If a CLI terminal supports
    /// user interaction, the UX is the terminal.
    /// </summary>
    public sealed class Terminal
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="id">The terminal identifier.</param>
        public Terminal(string id)
        {
            Id = id;
        }

        /// <summary>
        /// The terminal identifier.
        /// </summary>
        public string Id { get; }
    }
}