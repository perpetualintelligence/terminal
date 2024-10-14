/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OneImlx.Terminal.Mocks
{
    public class MockListWriter : TextWriter
    {
        public override Encoding Encoding => Encoding.UTF8;

        public List<string?> Messages { get; } = [];

        public override void Write(string? value)
        {
            Messages.Add(value);
        }

        public override void WriteLine()
        {
            Messages.Add("");
        }

        public override void WriteLine(string? value)
        {
            Messages.Add(value);
        }
    }
}
