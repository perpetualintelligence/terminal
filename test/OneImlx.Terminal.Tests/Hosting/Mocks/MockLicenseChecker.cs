/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Licensing;
using OneImlx.Terminal.Mocks;
using System;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Hosting.Mocks
{
    /// <summary>
    /// </summary>
    public class MockLicenseChecker : ILicenseChecker
    {
        public ValueTuple<int, bool> CheckLicenseCalled { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task<LicenseCheckerResult> CheckLicenseAsync(License license)
        {
            CheckLicenseCalled = new(MockTerminalHostedServiceStaticCounter.Increment(), true);
            return Task.FromResult(new LicenseCheckerResult(license));
        }
    }
}
