/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Commands.Handlers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PerpetualIntelligence.Terminal.Commands
{
    /// <summary>
    /// The ordered <see cref="ArgumentDescriptors"/> collection.
    /// </summary>
    public sealed class ArgumentDescriptors : KeyedCollection<string, ArgumentDescriptor>
    {
        /// <summary>
        /// Initializes a new instance with the specified argument descriptors.
        /// </summary>
        /// <param name="textHandler">The text handler.</param>
        /// <param name="collection">The argument descriptors.</param>
        public ArgumentDescriptors(ITextHandler textHandler, IEnumerable<ArgumentDescriptor> collection) : this(textHandler)
        {
            foreach (ArgumentDescriptor argumentDescriptor in collection)
            {
                Add(argumentDescriptor);
            }
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ArgumentDescriptors(ITextHandler textHandler) : base(textHandler.EqualityComparer())
        {
            TextHandler = textHandler ?? throw new ArgumentNullException(nameof(textHandler));
        }

        /// <summary>
        /// The text handler.
        /// </summary>
        public ITextHandler TextHandler { get; }

        /// <summary>
        /// Returns the key from the specified <see cref="ArgumentDescriptor"/>.
        /// </summary>
        /// <param name="item">The <see cref="ArgumentDescriptor"/> instance.</param>
        /// <returns>The key.</returns>
        protected override string GetKeyForItem(ArgumentDescriptor item)
        {
            return item.Id;
        }
    }
}