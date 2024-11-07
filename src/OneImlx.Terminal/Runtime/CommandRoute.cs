/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// Defines a command route with a unique identifier.
    /// </summary>
    /// <remarks>
    /// The <see cref="CommandRoute"/> implements <see cref="IEquatable{T}"/> with property <see cref="Id"/>. This helps
    /// identify each command execution uniquely, as same command can be executed multiple times.
    /// </remarks>
    public sealed class CommandRoute : IEquatable<CommandRoute?>
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="id">The command route identifier.</param>
        /// <param name="raw">The command string.</param>
        public CommandRoute(string id, string raw)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id), $"'{nameof(id)}' cannot be null or whitespace.");
            }

            if (string.IsNullOrWhiteSpace(raw))
            {
                throw new ArgumentNullException(nameof(raw), $"'{nameof(raw)}' cannot be null or whitespace.");
            }

            Id = id;
            Raw = raw;
        }

        /// <summary>
        /// The command route id.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The raw command string.
        /// </summary>
        public string Raw { get; }

        /// <inheritdoc/>
        public static bool operator !=(CommandRoute? left, CommandRoute? right)
        {
            return !Equals(left, right);
        }

        /// <inheritdoc/>
        public static bool operator ==(CommandRoute? left, CommandRoute? right)
        {
            return Equals(left, right);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return Equals(obj as CommandRoute);
        }

        /// <inheritdoc/>
        public bool Equals(CommandRoute? other)
        {
            return other is not null &&
                   Id == other.Id;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
