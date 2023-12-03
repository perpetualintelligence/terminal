/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Commands.Handlers;
using System.Collections;
using System.Collections.Generic;

namespace PerpetualIntelligence.Terminal.Commands
{
    /// <summary>
    /// The ordered <see cref="Option"/> keyed collection.
    /// </summary>
    public sealed class Options : IReadOnlyDictionary<string, Option>
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="textHandler">The text handler.</param>
        /// <param name="options">The options.</param>
        public Options(ITextHandler textHandler, IEnumerable<Option>? options = null)
        {
            TextHandler = textHandler;

            inner = new Dictionary<string, Option>(textHandler.EqualityComparer());
            if (options != null)
            {
                inner = new Dictionary<string, Option>(textHandler.EqualityComparer());
                foreach (Option option in options)
                {
                    AddOption(option);
                }
            }
        }

        private void AddOption(Option option)
        {
            if (option.Descriptor.Alias != null)
            {
                inner.Add(option.Descriptor.Alias, option);
            }

            inner.Add(option.Descriptor.Id, option);
        }

        /// <inheritdoc/>
        public Option this[string key] => inner[key];

        /// <summary>
        /// The text handler.
        /// </summary>
        public ITextHandler TextHandler { get; }

        /// <inheritdoc/>
        public IEnumerable<string> Keys => inner.Keys;

        /// <inheritdoc/>
        public IEnumerable<Option> Values => inner.Values;

        /// <inheritdoc/>
        public int Count => inner.Count;

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<string, Option>> GetEnumerator()
        {
            return inner.GetEnumerator();
        }

        /// <summary>
        /// Gets the option value by its id.
        /// </summary>
        /// <param name="optId">The option identifier or the alias.</param>
        public TValue GetOptionValue<TValue>(string optId)
        {
            return (TValue)this[optId].Value;
        }

        /// <inheritdoc/>
        public bool TryGetValue(string key, out Option value)
        {
            return inner.TryGetValue(key, out value);
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return inner.GetEnumerator();
        }

        /// <inheritdoc/>
        public bool ContainsKey(string key)
        {
            return inner.ContainsKey(key);
        }

        private readonly Dictionary<string, Option> inner;
    }
}