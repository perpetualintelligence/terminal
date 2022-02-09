/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// The argument prefix location.
    /// </summary>
    public class ArgumentPrefixLocation
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="argumentString"></param>
        /// <param name="isAlias"></param>
        /// <param name="location"></param>
        public ArgumentPrefixLocation(string argumentString, bool isAlias, int location)
        {
            ArgumentString = argumentString;
            IsAlias = isAlias;
            Location = location;
        }

        /// <summary>
        /// The argument string.
        /// </summary>
        public string ArgumentString { get; set; }

        /// <summary>
        /// Determines if the <see cref="ArgumentString"/> is identified by an alias prefix.
        /// </summary>
        public bool IsAlias { get; set; }

        /// <summary>
        /// The location.
        /// </summary>
        public int Location { get; set; }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Location} - {IsAlias} - {ArgumentString}";
        }
    }
}
