/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands.Runners
{
    /// <summary>
    /// Represents the result of a command runner.
    /// </summary>
    public class CommandRunnerResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandRunnerResult"/> class.
        /// </summary>
        public CommandRunnerResult()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandRunnerResult"/> class with a specified value.
        /// </summary>
        /// <param name="value">The result value of the command.</param>
        public CommandRunnerResult(object value)
        {
            this.value = value ?? throw new ArgumentNullException(nameof(value), "Value cannot be null.");
        }

        /// <summary>
        /// Indicates whether the result has a value.
        /// </summary>
        public bool HasValue => value != null;

        /// <summary>
        /// Gets the value of the result.
        /// </summary>
        public object Value
        {
            get
            {
                if (value is null)
                {
                    throw new TerminalException(TerminalErrors.ServerError, "The value is not set on the command runner result.");
                }
                return value;
            }
        }

        /// <summary>
        /// Creates an empty <see cref="CommandRunnerResult"/> with no value.
        /// </summary>
        /// <returns>A new instance of <see cref="CommandRunnerResult"/>.</returns>
        public static CommandRunnerResult Empty()
        {
            return new CommandRunnerResult();
        }

        /// <summary>
        /// Creates an empty <see cref="CommandRunnerResult"/> with no value.
        /// </summary>
        /// <returns>A task that creates a new instance of <see cref="CommandRunnerResult"/>.</returns>
        public static Task<CommandRunnerResult> EmptyAsync()
        {
            return Task.FromResult(new CommandRunnerResult());
        }

        /// <summary>
        /// Gets the value as the specified type.
        /// </summary>
        /// <typeparam name="TValue">The type to cast the value to.</typeparam>
        /// <returns>The value cast to the specified type.</returns>
        /// <exception cref="InvalidCastException">Thrown if the value cannot be cast to the specified type.</exception>
        public TValue As<TValue>()
        {
            if (value is null)
            {
                throw new InvalidOperationException("The value is not set.");
            }
            return (TValue)value;
        }

        private readonly object? value;
    }
}
