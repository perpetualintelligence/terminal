/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Licensing;

namespace OneImlx.Terminal.Mocks
{
    internal class MockLicenseDebugger : ILicenseDebugger
    {
        private readonly bool isDebuggerAttached;

        public MockLicenseDebugger(bool isDebuggerAttached)
        {
            this.isDebuggerAttached = isDebuggerAttached;
        }

        public bool IsDebuggerAttached()
        {
            return isDebuggerAttached;
        }
    }
}