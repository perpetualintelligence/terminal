/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using System.Collections.Generic;

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// The command arguments.
    /// </summary>
    public sealed class Arguments : List<Argument>
    {
        /// <summary>
        /// Returns a collection of argument name and argument itself.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, Argument> ToNameArgumentCollection()
        {
            Dictionary<string, Argument> args = new Dictionary<string, Argument>();
            foreach (var key in this)
            {
                args.Add(key.Name, key);
            }
            return args;
        }

        /// <summary>
        /// Returns a collection of argument name and value.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> ToNameValueCollection()
        {
            Dictionary<string, object> args = new Dictionary<string, object>();
            foreach (var key in this)
            {
                args.Add(key.Name, key.Value);
            }
            return args;
        }
    }
}
