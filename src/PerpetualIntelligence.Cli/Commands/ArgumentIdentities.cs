/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Shared.Exceptions;
using System.Collections.Generic;
using System.Linq;

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// The <see cref="ArgumentIdentity"/> collection.
    /// </summary>
    public sealed class ArgumentIdentities : List<ArgumentIdentity>
    {
        /// <summary>
        /// Attempts to find an argument by name.
        /// </summary>
        /// <param name="name">The argument name</param>
        public ArgumentIdentity? FindByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new System.ArgumentException($"'{nameof(name)}' cannot be null or whitespace.", nameof(name));
            }

            try
            {
                return this.SingleOrDefault(e => e.Id.Equals(name, System.StringComparison.Ordinal));
            }
            catch
            {
                throw new NotUniqueException();
            }
        }
    }
}
