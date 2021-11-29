/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// Represents a non existent command group.
    /// </summary>
    /// <remarks>The command group has unique id and name.</remarks>
    /// <seealso cref="CommandGroup"/>
    public class NoCommandGroup : CommandGroup
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        public NoCommandGroup()
        {
            Id = "None";
        }
    }
}
