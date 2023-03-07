/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System.ComponentModel.DataAnnotations;

namespace PerpetualIntelligence.Cli.Commands.Checkers
{
    /// <summary>
    /// The default <see cref="IArgumentValueChecker"/> that uses <see cref="ValidationAttribute"/> to check an argument value.
    /// </summary>
    public class DataValidationArgumentValueChecker : IArgumentValueChecker
    {
        private readonly ValidationAttribute validationAttribute;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="validationAttribute">The validation attribute.</param>
        public DataValidationArgumentValueChecker(ValidationAttribute validationAttribute)
        {
            this.validationAttribute = validationAttribute;
        }
    }
}