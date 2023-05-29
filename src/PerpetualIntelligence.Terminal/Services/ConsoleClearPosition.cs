/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace PerpetualIntelligence.Cli.Services
{
    /// <summary>
    /// Defines the positioning data to clear the console output.
    /// </summary>
    public struct ConsoleClearPosition
    {
        /// <summary>
        /// The left cursor clear position.
        /// </summary>
        public int Left;

        /// <summary>
        /// The length of output to clear.
        /// </summary>
        public int Length;

        /// <summary>
        /// The top cursor clear position.
        /// </summary>
        public int Top;
    }
}
