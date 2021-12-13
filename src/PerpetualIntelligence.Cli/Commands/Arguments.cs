/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using System.Collections.Generic;

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// The <see cref="Argument"/> collection.
    /// </summary>
    public sealed class Arguments : List<Argument>
    {
        /// <summary>
        /// Returns a dictionary of argument name and <see cref="Argument"/>.
        /// </summary>
        /// <returns><see cref="Dictionary{TKey, TValue}"/></returns>
        public Dictionary<string, Argument> ToNameArgumentDisctionary()
        {
            Dictionary<string, Argument> args = new();
            foreach (var key in this)
            {
                args.Add(key.Id, key);
            }
            return args;
        }

        /// <summary>
        /// Returns a dictionary of argument name and value.
        /// </summary>
        /// <returns><see cref="Dictionary{TKey, TValue}"/></returns>
        public Dictionary<string, object> ToNameValueDictionary()
        {
            Dictionary<string, object> args = new();
            foreach (var key in this)
            {
                args.Add(key.Id, key.Value);
            }
            return args;
        }
    }
}
