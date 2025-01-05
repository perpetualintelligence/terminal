/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Licensing;

namespace OneImlx.Terminal.Mocks
{
    internal class MockLicenseDebugger : ILicenseDebugger
    {
        public MockLicenseDebugger(bool isDebuggerAttached)
        {
            this.isDebuggerAttached = isDebuggerAttached;
        }

        public bool IsDebuggerAttached()
        {
            return isDebuggerAttached;
        }

        internal void SetDebuggerAttached(bool isAttached)
        {
            isDebuggerAttached = isAttached;
        }

        private bool isDebuggerAttached;
    }
}
