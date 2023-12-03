/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

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
        public List<string?> Messages { get; } = new List<string?>();

        public override Encoding Encoding => Encoding.UTF8;

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