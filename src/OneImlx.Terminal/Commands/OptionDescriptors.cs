/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Commands.Handlers;
using System.Collections;
using System.Collections.Generic;

namespace OneImlx.Terminal.Commands
{
    /// <summary>
    /// The <see cref="OptionDescriptor"/> collection.
    /// </summary>
    public sealed class OptionDescriptors : IReadOnlyDictionary<string, OptionDescriptor>
    {
        /// <summary>
        /// Initializes a new instance with the specified option descriptors.
        /// </summary>
        /// <param name="textHandler">The text handler.</param>
        /// <param name="collection">The option descriptors.</param>
        public OptionDescriptors(ITextHandler textHandler, IEnumerable<OptionDescriptor>? collection = null)
        {
            TextHandler = textHandler;
            inner = new Dictionary<string, OptionDescriptor>(textHandler.EqualityComparer());
            if (collection != null)
            {
                foreach (OptionDescriptor optionDescriptor in collection)
                {
                    AddOption(optionDescriptor);
                }
            }
        }

        /// <summary>
        /// Registers the help option.
        /// </summary>
        public void RegisterHelp(OptionDescriptor helpDescriptor)
        {
            AddOption(helpDescriptor);
        }

        /// <summary>
        /// The text handler.
        /// </summary>
        public ITextHandler TextHandler { get; }

        /// <inheritdoc/>
        public int Count => inner.Count;

        /// <inheritdoc/>
        public IEnumerable<string> Keys => inner.Keys;

        /// <inheritdoc/>
        public IEnumerable<OptionDescriptor> Values => inner.Values;

        /// <summary>
        /// Gets an <see cref="OptionDescriptor"/> instance with the specified id or alias.
        /// </summary>
        /// <param name="idOrAlias">The option id or its alias.</param>
        /// <returns><see cref="OptionDescriptor"/> instance if found.</returns>
        /// <exception cref="TerminalException">
        /// If <see cref="OptionDescriptor"/> instance with specified id is not found.
        /// </exception>
        public OptionDescriptor this[string idOrAlias]
        {
            get
            {
                try
                {
                    return inner[idOrAlias];
                }
                catch
                {
                    throw new TerminalException(TerminalErrors.UnsupportedOption, "The option is not supported. option={0}", idOrAlias);
                }
            }
        }

        /// <inheritdoc/>
        public bool ContainsKey(string key)
        {
            return inner.ContainsKey(key);
        }

        /// <inheritdoc/>
        public bool TryGetValue(string key, out OptionDescriptor value)
        {
            return inner.TryGetValue(key, out value);
        }

        /// <inheritdoc/>
        IEnumerator<KeyValuePair<string, OptionDescriptor>> IEnumerable<KeyValuePair<string, OptionDescriptor>>.GetEnumerator()
        {
            return inner.GetEnumerator();
        }

        /// <inheritdoc/>
        public IEnumerator GetEnumerator()
        {
            return inner.GetEnumerator();
        }

        private void AddOption(OptionDescriptor optionDescriptor)
        {
            if (optionDescriptor.Alias != null)
            {
                inner.Add(optionDescriptor.Alias, optionDescriptor);
            }

            inner.Add(optionDescriptor.Id, optionDescriptor);
        }

        private readonly Dictionary<string, OptionDescriptor> inner;
    }
}