/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Runtime;
using System.Collections;
using System.Collections.Generic;

namespace OneImlx.Terminal.Commands
{
    /// <summary>
    /// The readonly <see cref="ArgumentDescriptor"/> keyed collection.
    /// </summary>
    public sealed class ArgumentDescriptors : IReadOnlyCollection<ArgumentDescriptor>
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ArgumentDescriptors(ITerminalTextHandler textHandler, IEnumerable<ArgumentDescriptor> arguments)
        {
            inner = new KeyAsIdCollection<ArgumentDescriptor>(textHandler);
            foreach (var argument in arguments)
            {
                inner.Add(argument);
            }

            TextHandler = textHandler;
        }

        /// <summary>
        /// The text handler.
        /// </summary>
        public ITerminalTextHandler TextHandler { get; }

        /// <summary>
        /// The argument descriptor count.
        /// </summary>
        public int Count => inner.Count;

        /// <summary>
        /// Gets the argument descriptor by its id.
        /// </summary>
        /// <param name="id">The argument descriptor identifier.</param>
        /// <returns></returns>

        public ArgumentDescriptor this[string id]
        {
            get
            {
                return inner[id];
            }
        }

        /// <summary>
        /// Gets the argument descriptor by its index.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <returns></returns>

        public ArgumentDescriptor this[int index]
        {
            get
            {
                return inner[index];
            }
        }

        /// <inheritdoc/>
        public IEnumerator<ArgumentDescriptor> GetEnumerator()
        {
            return inner.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return inner.GetEnumerator();
        }

        private readonly KeyAsIdCollection<ArgumentDescriptor> inner;
    }
}