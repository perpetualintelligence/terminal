/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Licensing;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Handlers.Mocks
{
    public class MockLicenseCheckerInner : ILicenseChecker
    {
        public bool Called { get; set; }

        public LicenseCheckerContext? ContextCalled { get; set; }

        public Task<LicenseCheckerResult> CheckAsync(LicenseCheckerContext context)
        {
            Called = true;
            ContextCalled = context;
            return Task.FromResult(new LicenseCheckerResult(context.License));
        }
    }
}
