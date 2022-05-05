/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Protocols.Abstractions.Comparers;
using System.Collections.ObjectModel;
using System.Linq;

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// The ordered <see cref="Argument"/> keyed collection.
    /// </summary>
    public sealed class Arguments : KeyedCollection<string, Argument>
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public Arguments(IStringComparer stringComparer) : base(stringComparer)
        {
            StringComparer = stringComparer;
        }

        /// <summary>
        /// The string comparer.
        /// </summary>
        public IStringComparer StringComparer { get; }

        /// <summary>
        /// Gets the argument value by its id.
        /// </summary>
        /// <param name="argId">The argument identifier or the alias.</param>
        public TValue GetValue<TValue>(string argId)
        {
            return (TValue)this[argId].Value;
        }

        /// <summary>
        /// Gets the argument value by its id or alias.
        /// </summary>
        /// <param name="argIdOrAlias">The argument identifier or the alias.</param>
        /// <param name="alias"><c>true</c> to find the argument by alias, <c>false</c> to find by its identifier.</param>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <remarks>
        /// We recommend to use <see cref="GetValue{TValue}(string)"/> to get an argument value. Using alias will
        /// degrade the application's performance.
        /// </remarks>
        public TValue GetValue<TValue>(string argIdOrAlias, bool? alias = false)
        {
            if (alias.GetValueOrDefault())
            {
                // if alias is true, we will still try and find with id first and then with alias
                if (Contains(argIdOrAlias))
                {
                    return (TValue)this[argIdOrAlias].Value;
                }
                else
                {
                    return (TValue)Items.First(e => e.Alias != null && StringComparer.Equals(e.Alias, argIdOrAlias)).Value;
                }
            }
            else
            {
                return (TValue)this[argIdOrAlias].Value;
            }
        }

        /// <summary>
        /// Returns the key from the specified <see cref="Argument"/>.
        /// </summary>
        /// <param name="item">The <see cref="Argument"/> instance.</param>
        /// <returns>The key.</returns>
        protected override string GetKeyForItem(Argument item)
        {
            return item.Id;
        }
    }
}
