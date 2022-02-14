/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Protocols.Abstractions.Comparers;
using System.Diagnostics.CodeAnalysis;

namespace PerpetualIntelligence.Cli.Mocks
{
    public class MockStringComparer : IStringComparer
    {
        public bool Equals(string? x, string? y)
        {
            return false;
        }

        public int GetHashCode([DisallowNull] string obj)
        {
            return 0;
        }
    }
}
