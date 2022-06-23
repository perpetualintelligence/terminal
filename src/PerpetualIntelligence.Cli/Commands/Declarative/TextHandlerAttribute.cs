/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System;

namespace PerpetualIntelligence.Cli.Commands.Declarative
{
    /// <summary>
    /// Declares the text handler.
    /// </summary>
    public class TextHandlerAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="testHandler">The text handler.</param>
        public TextHandlerAttribute(Type testHandler)
        {
            TestHandler = testHandler;
        }

        /// <summary>
        /// </summary>
        public Type TestHandler { get; set; }
    }
}
