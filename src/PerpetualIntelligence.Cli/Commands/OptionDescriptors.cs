/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// The ordered <see cref="OptionDescriptor"/> collection.
    /// </summary>
    public sealed class OptionDescriptors : KeyedCollection<string, OptionDescriptor>
    {
        /// <summary>
        /// Initializes a new instance with the specified option descriptors.
        /// </summary>
        /// <param name="textHandler">The text handler.</param>
        /// <param name="collection">The option descriptors.</param>
        public OptionDescriptors(ITextHandler textHandler, IEnumerable<OptionDescriptor> collection) : this(textHandler)
        {
            foreach (OptionDescriptor argumentDescriptor in collection)
            {
                Add(argumentDescriptor);
            }
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public OptionDescriptors(ITextHandler textHandler) : base(textHandler.EqualityComparer())
        {
            TextHandler = textHandler ?? throw new ArgumentNullException(nameof(textHandler));
        }

        /// <summary>
        /// The text handler.
        /// </summary>
        public ITextHandler TextHandler { get; }

        /// <summary>
        /// Gets or sets an <see cref="OptionDescriptor"/> instance at the specified index.
        /// </summary>
        /// <param name="index">The zero based index.</param>
        public new OptionDescriptor this[int index]
        {
            get
            {
                try
                {
                    return base[index];
                }
                catch
                {
                    throw new ErrorException(Errors.UnsupportedOption, "The option is not supported. option={0}", index);
                }
            }

            set
            {
                base[index] = value;
            }
        }

        /// <summary>
        /// Gets an <see cref="OptionDescriptor"/> instance with the specified id.
        /// </summary>
        /// <param name="id">The option id.</param>
        /// <returns><see cref="OptionDescriptor"/> instance if found.</returns>
        /// <exception cref="ErrorException">
        /// If <see cref="OptionDescriptor"/> instance with specified id is not found.
        /// </exception>
        public new OptionDescriptor this[string id]
        {
            get
            {
                try
                {
                    return base[id];
                }
                catch
                {
                    throw new ErrorException(Errors.UnsupportedOption, "The option is not supported. option={0}", id);
                }
            }
        }

        /// <summary>
        /// Gets an <see cref="OptionDescriptor"/> instance with the specified id.
        /// </summary>
        /// <param name="idOrAlias">The option id or its alias.</param>
        /// <param name="alias"><c>true</c> to find the option by its alias, <c>false</c> to find by its identifier.</param>
        /// <param name="ifNotThen">
        /// <c>true</c> to find the option by its id. If not found, then attempt to find it by its alias. <c>false</c>
        /// to find by id or alias but not both.
        /// </param>
        /// <returns><see cref="OptionDescriptor"/> instance if found.</returns>
        /// <exception cref="ErrorException">
        /// If <see cref="OptionDescriptor"/> instance with specified id is not found.
        /// </exception>
        /// <remarks>
        /// We recommend to use <see cref="this[string]"/> to get an option. Using alias will degrade the
        /// application's performance.
        /// </remarks>
        public OptionDescriptor this[string idOrAlias, bool? alias = null, bool? ifNotThen = null]
        {
            get
            {
                bool idError = false;
                bool isOrAliasError = false;
                try
                {
                    // Try with id then alias
                    if (ifNotThen.GetValueOrDefault() && alias.GetValueOrDefault())
                    {
                        // This will help us determine more accurate error message.
                        isOrAliasError = true;

                        // First find by id, check if id exists otherwise this[idOrAlias] will throw exception
                        if (Contains(idOrAlias))
                        {
                            return this[idOrAlias];
                        }
                        else
                        {
                            // Then by alias, if this throws we are ok. Nothing to check further.
                            return this.First(e => e.Alias != null && TextHandler.TextEquals(e.Alias, idOrAlias));
                        }
                    }
                    else
                    {
                        // Id or alias
                        if (alias.GetValueOrDefault())
                        {
                            return this.First(e => e.Alias != null && TextHandler.TextEquals(e.Alias, idOrAlias));
                        }
                        else
                        {
                            // This will help us determine more accurate error message.
                            idError = true;
                            return this[idOrAlias];
                        }
                    }
                }
                catch
                {
                    if (isOrAliasError)
                    {
                        throw new ErrorException(Errors.UnsupportedOption, "The option or its alias is not supported. option={0}", idOrAlias);
                    }
                    else if (idError)
                    {
                        throw new ErrorException(Errors.UnsupportedOption, "The option is not supported. option={0}", idOrAlias);
                    }
                    else
                    {
                        throw new ErrorException(Errors.UnsupportedOption, "The option alias is not supported. option={0}", idOrAlias);
                    }
                }
            }
        }

        /// <summary>
        /// Returns the key from the specified <see cref="OptionDescriptor"/>.
        /// </summary>
        /// <param name="item">The <see cref="OptionDescriptor"/> instance.</param>
        /// <returns>The key.</returns>
        protected override string GetKeyForItem(OptionDescriptor item)
        {
            return item.Id;
        }
    }
}
