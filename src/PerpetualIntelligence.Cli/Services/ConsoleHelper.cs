/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Shared.Attributes;
using System;

namespace PerpetualIntelligence.Cli.Services
{
    /// <summary>
    /// The <see cref="Console"/> helper methods.
    /// </summary>
    [WriteUnitTest]
    public static class ConsoleHelper
    {
        /// <summary>
        /// Clears the written value.
        /// </summary>
        /// <param name="lastValue"></param>
        public static void ClearValue(string lastValue)
        {
            if (string.IsNullOrWhiteSpace(lastValue))
            {
                throw new ArgumentException($"'{nameof(lastValue)}' cannot be null or whitespace.", nameof(lastValue));
            }

            // https://stackoverflow.com/questions/8946808/can-console-clear-be-used-to-only-clear-a-line-instead-of-whole-console/8946847
            int leftCursor = Console.CursorLeft;

            int overideLeft = leftCursor - lastValue.Length;
            if (overideLeft < 0)
            {
                throw new InvalidOperationException("The override cursor position for last value is not valid.");
            }

            Console.SetCursorPosition(overideLeft, Console.CursorTop);
            Console.Write(new string(' ', lastValue.Length));
            Console.SetCursorPosition(overideLeft, Console.CursorTop);
        }

        /// <summary>
        /// Writes the specified string with specified color to the standard output stream.
        /// </summary>
        /// <remarks>
        /// <see cref="WriteColor(string, ConsoleColor)"/> resets the color after writing the string using
        /// <see cref="Console.ResetColor"/> method.
        /// </remarks>
        public static void WriteColor(string value, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(value);
            Console.ResetColor();
        }

        /// <summary>
        /// Writes the specified string with specified color followed by the current line terminator to the standard
        /// output stream.
        /// </summary>
        /// <remarks>
        /// <see cref="WriteLineColor(string, ConsoleColor)"/> resets the color after writing the string using
        /// <see cref="Console.ResetColor"/> method.
        /// </remarks>
        public static void WriteLineColor(string value, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(value);
            Console.ResetColor();
        }
    }
}
