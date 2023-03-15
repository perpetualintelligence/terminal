/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;

namespace PerpetualIntelligence.Cli.Commands.Routers
{
    /// <summary>
    /// Defines a command route with a unique identifiers for each command run.
    /// </summary>
    public sealed class CommandRoute : IEquatable<CommandRoute?>
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        public CommandRoute(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException($"'{nameof(id)}' cannot be null or whitespace.", nameof(id));
            }

            Id = id;
        }

        /// <summary>
        /// The command run id.
        /// </summary>
        public string Id { get; }

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