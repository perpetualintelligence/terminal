/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Protocols.Abstractions.Comparers;

using PerpetualIntelligence.Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// The ordered <see cref="ArgumentDescriptor"/> collection.
    /// </summary>
    public sealed class ArgumentDescriptors : KeyedCollection<string, ArgumentDescriptor>
    {
        /// <summary>
        /// Initializes a new instance with the specified argument descriptors.
        /// </summary>
        /// <param name="stringComparer">The string comparer.</param>
        /// <param name="collection">The argument descriptors.</param>
        public ArgumentDescriptors(IStringComparer stringComparer, IEnumerable<ArgumentDescriptor> collection) : this(stringComparer)
        {
            foreach (ArgumentDescriptor argumentDescriptor in collection)
            {
                Add(argumentDescriptor);
            }
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ArgumentDescriptors(IStringComparer stringComparer) : base(stringComparer)
        {
            StringComparer = stringComparer ?? throw new ArgumentNullException(nameof(stringComparer));
        }

        /// <summary>
        /// The string comparer.
        /// </summary>
        public IStringComparer StringComparer { get; }

        /// <summary>
        /// Gets or sets an <see cref="ArgumentDescriptor"/> instance at the specified index.
        /// </summary>
        /// <param name="index">The zero based index.</param>
        public new ArgumentDescriptor this[int index]
        {
            get
            {
                try
                {
                    return base[index];
                }
                catch
                {
                    throw new ErrorException(Errors.UnsupportedArgument, "The argument is not supported. argument={0}", index);
                }
            }

            set
            {
                base[index] = value;
            }
        }

        /// <summary>
        /// Gets an <see cref="ArgumentDescriptor"/> instance with the specified id.
        /// </summary>
        /// <param name="id">The argument id.</param>
        /// <returns><see cref="ArgumentDescriptor"/> instance if found.</returns>
        /// <exception cref="ErrorException">
        /// If <see cref="ArgumentDescriptor"/> instance with specified id is not found.
        /// </exception>
        public new ArgumentDescriptor this[string id]
        {
            get
            {
                try
                {
                    return base[id];
                }
                catch
                {
                    throw new ErrorException(Errors.UnsupportedArgument, "The argument is not supported. argument={0}", id);
                }
            }
        }

        /// <summary>
        /// Gets an <see cref="ArgumentDescriptor"/> instance with the specified id.
        /// </summary>
        /// <param name="idOrAlias">The argument id or its alias.</param>
        /// <param name="alias"><c>true</c> to find the argument by its alias, <c>false</c> to find by its identifier.</param>
        /// <param name="ifNotThen">
        /// <c>true</c> to find the argument by its id. If not found, then attempt to find it by its alias. <c>false</c>
        /// to find by id or alias but not both.
        /// </param>
        /// <returns><see cref="ArgumentDescriptor"/> instance if found.</returns>
        /// <exception cref="ErrorException">
        /// If <see cref="ArgumentDescriptor"/> instance with specified id is not found.
        /// </exception>
        /// <remarks>
        /// We recommend to use <see cref="this[string]"/> to get an argument. Using alias will degrade the
        /// application's performance.
        /// </remarks>
        public ArgumentDescriptor this[string idOrAlias, bool? alias = null, bool? ifNotThen = null]
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
                            return this.First(e => e.Alias != null && StringComparer.Equals(e.Alias, idOrAlias));
                        }
                    }
                    else
                    {
                        // Id or alias
                        if (alias.GetValueOrDefault())
                        {
                            return this.First(e => e.Alias != null && StringComparer.Equals(e.Alias, idOrAlias));
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
                        throw new ErrorException(Errors.UnsupportedArgument, "The argument or its alias is not supported. argument={0}", idOrAlias);
                    }
                    else if (idError)
                    {
                        throw new ErrorException(Errors.UnsupportedArgument, "The argument is not supported. argument={0}", idOrAlias);
                    }
                    else
                    {
                        throw new ErrorException(Errors.UnsupportedArgument, "The argument alias is not supported. argument={0}", idOrAlias);
                    }
                }
            }
        }

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
