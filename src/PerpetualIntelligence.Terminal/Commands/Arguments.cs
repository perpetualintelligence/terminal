/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Commands.Handlers;
using System.Collections.ObjectModel;

namespace PerpetualIntelligence.Terminal.Commands
{
    /// <summary>
    /// The ordered <see cref="Argument"/> keyed collection.
    /// </summary>
    public sealed class Arguments : KeyedCollection<string, Argument>
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public Arguments(ITextHandler textHandler) : base(textHandler.EqualityComparer())
        {
            TextHandler = textHandler;
        }

        /// <summary>
        /// The text handler.
        /// </summary>
        public ITextHandler TextHandler { get; }

        /// <summary>
        /// Gets the option value by its id.
        /// </summary>
        /// <param name="argId">The option identifier or the alias.</param>
        public TValue GetValue<TValue>(string argId)
        {
            return (TValue)this[argId].Value;
        }

        /// <summary>
        /// Returns the key from the specified <see cref="Argument"/>.
        /// </summary>
        /// <param name="item">The <see cref="Option"/> instance.</param>
        /// <returns>The key.</returns>
        protected override string GetKeyForItem(Argument item)
        {
            return item.Id;
        }
    }
}