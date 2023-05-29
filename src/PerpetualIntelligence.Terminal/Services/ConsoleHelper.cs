/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
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
        /// Clears the written output for the specified position.
        /// </summary>
        /// <param name="clearPosition">The console clear position.</param>
        public static void ClearOutput(ConsoleClearPosition clearPosition)
        {
            if (clearPosition.Left < 5)
            {
                throw new ArgumentException("The cursor left position cannot be less than 5.");
            }

            if (clearPosition.Top < 0)
            {
                throw new ArgumentException("The cursor top position cannot be negative.");
            }

            if (clearPosition.Length <= 0)
            {
                return;
            }

            // https://stackoverflow.com/questions/8946808/can-console-clear-be-used-to-only-clear-a-line-instead-of-whole-console/8946847
            Console.SetCursorPosition(clearPosition.Left, clearPosition.Top);
            Console.Write(new string(' ', clearPosition.Length));
            Console.SetCursorPosition(clearPosition.Left, clearPosition.Top);
        }

        /// <summary>
        /// Gets the <see cref="ConsoleClearPosition"/> for the specified output length.
        /// </summary>
        /// <param name="length">The output length to clear.</param>
        /// <returns>
        /// <see cref="ConsoleClearPosition"/> object with <see cref="Console.CursorLeft"/>,
        /// <see cref="Console.CursorTop"/> and specified length.
        /// </returns>
        public static ConsoleClearPosition GetClearPosition(int length)
        {
            return new ConsoleClearPosition() { Left = Console.CursorLeft, Top = Console.CursorTop, Length = length };
        }

        /// <summary>
        /// Writes the specified string with specified color to the standard output stream.
        /// </summary>
        /// <remarks>
        /// <see cref="WriteColor(ConsoleColor, string, object[])"/> resets the color after writing the string using
        /// <see cref="Console.ResetColor"/> method.
        /// </remarks>
        public static void WriteColor(ConsoleColor color, string value, params object[] args)
        {
            Console.ForegroundColor = color;
            Console.Write(value, args);
            Console.ResetColor();
        }

        /// <summary>
        /// Writes the specified string with specified color followed by the current line terminator to the standard
        /// output stream.
        /// </summary>
        /// <remarks>
        /// <see cref="WriteLineColor(ConsoleColor, string, object[])"/> resets the color after writing the string using
        /// <see cref="Console.ResetColor"/> method.
        /// </remarks>
        public static void WriteLineColor(ConsoleColor color, string value, params object[] args)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(value, args);
            Console.ResetColor();
        }
    }
}
