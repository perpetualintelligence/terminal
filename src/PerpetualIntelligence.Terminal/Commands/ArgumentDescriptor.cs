/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace PerpetualIntelligence.Terminal.Commands
{
    /// <summary>
    /// The <see cref="ArgumentDescriptor"/> class defines the command argument identity, data type, and data validation
    /// behavior. We also refer to arguments as command arguments.
    /// </summary>
    public sealed class ArgumentDescriptor
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ArgumentDescriptor"/>.
        /// </summary>
        /// <param name="id">The argument identifier.</param>
        /// <param name="dataType">The argument data type.</param>
        /// <param name="description">The argument description.</param>
        public ArgumentDescriptor(string id, string dataType, string description)
        {
            Id = id;
            DataType = dataType;
            Description = description;
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
    }
}