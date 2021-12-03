/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

namespace PerpetualIntelligence.Cli.Commands.Extractors
{
    /// <summary>
    /// The <c>onimlx</c> argument syntax checker context.
    /// </summary>
    public class ArgumentExtractorContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="argumentString">The argument string.</param>
        /// <param name="commandIdentity">The command identity.</param>
        public ArgumentExtractorContext(string argumentString, CommandIdentity commandIdentity)
        {
            ArgumentString = argumentString;
            CommandIdentity = commandIdentity;
        }

        /// <summary>
        /// The argument string to check.
        /// </summary>
        public string ArgumentString { get; set; }

        /// <summary>
        /// The command identity.
        /// </summary>
        public CommandIdentity CommandIdentity { get; set; }
    }
}
