/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Shared.Attributes;
using System.Diagnostics;

namespace PerpetualIntelligence.Terminal
{
    /// <summary>
    /// The helper methods.
    /// </summary>
    [WriteUnitTest]
    public static class TerminalHelper
    {
        /// <summary>
        /// Determines if we are in a <c>dev-mode</c>. We assume <c>dev-mode</c> during debugging if the consumer deploys on-premise,
        /// use any source code editor or an IDE such as Visual Studio, Visual Studio Code, NotePad, Eclipse etc., use DEBUG or any other custom configuration.
        /// It is a violation of licensing terms to disable <c>dev-mode</c> with IL Weaving, Reflection or any other methods.
        /// </summary>
        /// <returns></returns>
        public static bool IsDevMode()
        {
            if (Debugger.IsAttached)
            {
                return true;
            }

#if RELEASE
            return false;
#else
            return true;
#endif
        }
    }
}