/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace OneImlx.Terminal.Authentication
{
    /// <summary>
    /// Represents an input for interactive token acquisition in terminal applications.
    /// </summary>
    public sealed class TokenAcquisitionInteractiveInput(IEnumerable<string> scopes)
    {
        /// <summary>
        /// The authentication scopes required for the interactive login.
        /// </summary>
        public IEnumerable<string> Scopes { get; set; } = scopes;
    }
}
