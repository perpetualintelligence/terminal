/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Commands.Checkers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace OneImlx.Terminal.Commands
{
    /// <summary>
    /// The <see cref="ArgumentDescriptor"/> class defines the command argument identity, data type, and data validation
    /// behavior. We also refer to arguments as command arguments.
    /// </summary>
    /// <seealso cref="Argument"/>
    public sealed class ArgumentDescriptor : IKeyAsId
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ArgumentDescriptor"/>.
        /// </summary>
        /// <param name="order">The argument order.</param>
        /// <param name="id">The argument identifier.</param>
        /// <param name="dataType">The argument data type.</param>
        /// <param name="description">The argument description.</param>
        /// <param name="flags">The argument flags.</param>
        public ArgumentDescriptor(int order, string id, string dataType, string description, ArgumentFlags flags)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new System.ArgumentException($"'{nameof(id)}' cannot be null or empty.", nameof(id));
            }

            if (string.IsNullOrEmpty(dataType))
            {
                throw new System.ArgumentException($"'{nameof(dataType)}' cannot be null or empty.", nameof(dataType));
            }

            if (string.IsNullOrEmpty(description))
            {
                throw new System.ArgumentException($"'{nameof(description)}' cannot be null or empty.", nameof(description));
            }

            Id = id;
            DataType = dataType;
            Description = description;
            Flags = flags;
            Order = order;
        }

        /// <summary>
        /// The argument identifier.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The argument data type.
        /// </summary>
        public string DataType { get; }

        /// <summary>
        /// The argument description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// The argument flags.
        /// </summary>
        public ArgumentFlags Flags { get; private set; }

        /// <summary>
        /// The argument order.
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// The argument value checkers.
        /// </summary>
        public IEnumerable<IValueChecker<Argument>>? ValueCheckers
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
                if (valueCheckers.Any(static e => e.GetRawType() == typeof(RequiredAttribute)))
                {
                    Flags |= ArgumentFlags.Required;
                }
            }
        }

        private IEnumerable<IValueChecker<Argument>>? valueCheckers;
    }
}