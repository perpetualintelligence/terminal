/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

namespace PerpetualIntelligence.Tools.TestMapper
{
    /// <summary>
    /// Represents a command.
    /// </summary>
    public struct Command
    {
        /// <summary>
        /// Command arguments.
        /// </summary>
        public Dictionary<string, string> Arguments;

        /// <summary>
        /// Command name.
        /// </summary>
        public string Name;
    }
}
