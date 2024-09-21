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
    /// The readonly <see cref="Argument"/> keyed collection.
    /// </summary>
    public sealed class Arguments : IReadOnlyCollection<Argument>
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public Arguments(ITerminalTextHandler textHandler, IEnumerable<Argument> arguments)
        {
            inner = new KeyAsIdCollection<Argument>(textHandler);
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
        /// The argument count.
        /// </summary>
        public int Count => inner.Count;

        /// <summary>
        /// Gets the argument by its id.
        /// </summary>
        /// <param name="id">The argument identifier.</param>
        /// <returns></returns>

        public Argument this[string id]
        {
            get
            {
                return inner[id];
            }
        }

        /// <summary>
        /// Gets the argument by its index.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <returns></returns>

        public Argument this[int index]
        {
            get
            {
                return inner[index];
            }
        }

        /// <inheritdoc/>
        public IEnumerator<Argument> GetEnumerator()
        {
            return inner.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return inner.GetEnumerator();
        }

        private readonly KeyAsIdCollection<Argument> inner;
    }
}