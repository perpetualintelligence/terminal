/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands;
using PerpetualIntelligence.Shared.Attributes;
using PerpetualIntelligence.Shared.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Extensions
{
    /// <summary>
    /// The <see cref="Console"/> extension methods.
    /// </summary>
    public static class CommandExtensions
    {
        /// <summary>
        /// Prints the question to the console and reads an answer from the standard input stream.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="question">The question to print. The <c>?</c> will be appended at the end.</param>
        /// <param name="answers">
        /// The allowed answers or <c>null</c> if all answers are allowed. It is recommended to keep the answers short
        /// for readability. If specified this method will print the answers with question in the format <c>{question} {answer1}/{answer2}/{answer3}?</c>
        /// </param>
        /// <returns>The answer for the question or <c>null</c> if canceled.</returns>
        [WriteUnitTest]
        public static Task<string?> ReadAnswerAsync(this Command command, string question, params string[]? answers)
        {
            // Print the question
            if (answers != null)
            {
                Console.Write($"{question} ({string.Join("/", answers)})? ");
            }
            else
            {
                Console.Write($"{question}? ");
            }

            // Check answer
            string? answer = Console.ReadLine();
            if (answers != null)
            {
                if (answers.Contains(answer))
                {
                    return Task.FromResult<string?>(answer);
                }
                else
                {
                    Console.WriteLine($"The answer is not valid. answers={answers.JoinBySpace()}");
                    return ReadAnswerAsync(command, question, answers);
                }
            }
            else
            {
                return Task.FromResult<string?>(answer);
            }
        }
    }
}
