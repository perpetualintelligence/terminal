/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;

namespace PerpetualIntelligence.Terminal.Commands
{
    /// <summary>
    /// Defines a unique command route.
    /// </summary>
    /// <remarks>
    /// The <see cref="CommandRoute"/> implements <see cref="IEquatable{T}"/> with property <see cref="Id"/>.
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

            Id = id;
            Command = new(raw);
        }

        /// <summary>
        /// The command route id.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The command string.
        /// </summary>
        public CommandString Command { get; }

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

        /// <inheritdoc/>
        public static bool operator ==(CommandRoute? left, CommandRoute? right)
        {
            return left == right;
        }

        /// <inheritdoc/>
        public static bool operator !=(CommandRoute? left, CommandRoute? right)
        {
            return !(left == right);
        }
    }
}