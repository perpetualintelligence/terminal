/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Shared.Licensing;
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
                throw new ArgumentException($"'{nameof(plan)}' cannot be null or empty.", nameof(plan));
            }

            switch (plan)
            {
                case TerminalLicensePlans.Demo:
                    {
                        return new LicensePrice("USD", 0, 0, plan);
                    }
                case TerminalLicensePlans.Solo:
                    {
                        return new LicensePrice("USD", 50, 550, plan);
                    }
                case TerminalLicensePlans.Micro:
                    {
                        return new LicensePrice("USD", 300, 3300, plan);
                    }
                case TerminalLicensePlans.Smb:
                    {
                        return new LicensePrice("USD", 1200, 13200, plan);
                    }
                case TerminalLicensePlans.Enterprise:
                    {
                        return new LicensePrice("USD", 2400, 26400, plan);
                    }
                case TerminalLicensePlans.Corporate:
                    {
                        return new LicensePrice("USD", 4800, 52800, plan);
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