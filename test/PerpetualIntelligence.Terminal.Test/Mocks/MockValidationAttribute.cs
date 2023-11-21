/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.ComponentModel.DataAnnotations;

namespace PerpetualIntelligence.Terminal.Mocks
{
    internal class MockValidationAttribute : ValidationAttribute
    {
        public string Property1 { get; private set; }

        public int Property2 { get; private set; }

        public MockValidationAttribute()
        {
            Property1 = "Default";
            Property2 = 0;
        }

        public MockValidationAttribute(string property1, int property2)
        {
            Property1 = property1;
            Property2 = property2;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            return ValidationResult.Success;
        }
    }
}