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
    public sealed class Arguments : HashSet<Argument>
    {
        /// <summary>
        /// Returns a dictionary of argument name and value.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> ToDictionary()
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
