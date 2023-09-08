/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Commands.Checkers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace PerpetualIntelligence.Terminal.Commands
{
    /// <summary>
    /// The <see cref="OptionDescriptor"/> class defines the command option identity, data type, and data validation
    /// behavior. We also refer to options as command options or command flags.
    /// </summary>
    /// <seealso cref="Option"/>
    /// <seealso cref="CommandString"/>
    /// <seealso cref="CommandDescriptor"/>
    public sealed class OptionDescriptor
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="id">The option id.</param>
        /// <param name="dataType">The option data type.</param>
        /// <param name="description">The option description.</param>
        /// <param name="flags">The option flags.</param>
        /// <param name="alias">The option alias.</param>
        public OptionDescriptor(string id, string dataType, string description, OptionFlags flags, string? alias = null)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException($"'{nameof(id)}' cannot be null or whitespace.", nameof(id));
            }

            Id = id;
            DataType = dataType;
            Description = description;
            Flags = flags;
            Alias = alias;
        }

        /// <summary>
        /// The option alias.
        /// </summary>
        /// <remarks>
        /// The option alias is unique within a command. Option alias supports the legacy apps that identified a
        /// command option with an id and an alias string. For modern console apps, we recommend using just an
        /// option identifier. The core data model is optimized to work with option id. In general, an app should
        /// not identify the same option with multiple strings.
        /// </remarks>
        public string? Alias { get; }

        /// <summary>
        /// The custom properties.
        /// </summary>
        public Dictionary<string, object>? CustomProperties { get; set; }

        /// <summary>
        /// The option data type.
        /// </summary>
        public string DataType { get; }

        /// <summary>
        /// The option description.
        /// </summary>
        /// <remarks>The option id is unique across all commands.</remarks>
        public string? Description { get; }

        /// <summary>
        /// The option flags.
        /// </summary>
        public OptionFlags Flags { get; private set; }

        /// <summary>
        /// The option id.
        /// </summary>
        /// <remarks>The option id is unique within a command.</remarks>
        public string Id { get; }

        /// <summary>
        /// The option value checkers.
        /// </summary>
        public IEnumerable<IOptionValueChecker>? ValueCheckers
        {
            get
            {
                return valueCheckers;
            }

            set
            {
                valueCheckers = value;
                SetValidationRequired();
            }
        }

        /// <summary>
        /// Sets the option as required.
        /// </summary>
        private void SetValidationRequired()
        {
            if (valueCheckers != null)
            {
                if (valueCheckers.Any(e => e.GetRawType() == typeof(RequiredAttribute)))
                {
                    Flags |= OptionFlags.Required;
                }
            }
        }

        private IEnumerable<IOptionValueChecker>? valueCheckers;
    }
}