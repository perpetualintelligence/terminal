/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Shared.Licensing;
using System;
using System.Collections.Generic;

namespace OneImlx.Terminal.Licensing
{
    /// <summary>
    /// The license price.
    /// </summary>
    public sealed class LicensePrice
    {
        /// <summary>
        /// The currency.
        /// </summary>
        public string Currency { get; private set; }

        /// <summary>
        /// The monthly price.
        /// </summary>
        public double Monthly { get; private set; }

        /// <summary>
        /// The pricing plan.
        /// </summary>
        public string Plan { get; private set; }

        /// <summary>
        /// The yearly pricing.
        /// </summary>
        public double Yearly { get; private set; }

        /// <summary>
        /// Creates a new instance of <see cref="LicenseLimits"/> based on the specified SaaS plan.
        /// </summary>
        /// <param name="plan">The licensing plan.</param>
        /// <param name="customClaims">The custom claims.</param>
        public static LicensePrice Create(string plan, IDictionary<string, object>? customClaims = null)
        {
            if (string.IsNullOrEmpty(plan))
            {
                throw new System.ArgumentException($"'{nameof(plan)}' cannot be null or empty.", nameof(plan));
            }

            switch (plan)
            {
                case TerminalLicensePlans.Demo:
                    {
                        return new LicensePrice("USD", 0, 0, plan);
                    }
                case TerminalLicensePlans.Micro:
                    {
                        return new LicensePrice("USD", 49, 529, plan);
                    }
                case TerminalLicensePlans.SMB:
                    {
                        return new LicensePrice("USD", 299, 3229, plan);
                    }
                case TerminalLicensePlans.Enterprise:
                    {
                        return new LicensePrice("USD", 699, 7529, plan);
                    }
                case TerminalLicensePlans.OnPremise:
                    {
                        return new LicensePrice("USD", 1299, 14029, plan);
                    }
                case TerminalLicensePlans.Unlimited:
                    {
                        return new LicensePrice("USD", 3299, 35629, plan);
                    }
                case TerminalLicensePlans.Custom:
                    {
                        if (customClaims == null)
                        {
                            throw new TerminalException(TerminalErrors.InvalidLicense, "The pricing for the custom plan requires a custom claims. plan={0}", plan);
                        }

                        return new LicensePrice(customClaims["currency"].ToString(), Convert.ToDouble(customClaims["monthly_price"]), Convert.ToDouble(customClaims["yearly_price"]), plan);
                    }
                default:
                    {
                        throw new TerminalException(TerminalErrors.InvalidLicense, "The pricing for the plan is not supported. plan={0}", plan);
                    }
            }
        }

        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="currency">The currency.</param>
        /// <param name="monthly">The monthly price.</param>
        /// <param name="yearly">The yearly price.</param>
        /// <param name="plan">The pricing plan.</param>
        private LicensePrice(string currency, double monthly, double yearly, string plan)
        {
            Currency = currency;
            Monthly = monthly;
            Yearly = yearly;
            Plan = plan;
        }
    }
}