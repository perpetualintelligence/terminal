/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace OneImlx.Terminal.Configuration.Options
{
    /// <summary>
    /// The debug options for the <c>OneImlx.Terminal</c> framework.
    /// </summary>
    public sealed class DebugOptions
    {
        /// <summary>
        /// If enabled, the terminal framework returns the message queue items that was routed by the HTTP or gRPC routers.
        /// </summary>
        public bool? ReturnRouted { get; set; }
    }
}
