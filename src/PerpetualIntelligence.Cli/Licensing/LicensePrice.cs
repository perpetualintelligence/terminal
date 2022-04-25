﻿/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Protocols.Licensing;
using PerpetualIntelligence.Shared.Exceptions;

namespace PerpetualIntelligence.Cli.Licensing
{
    /// <summary>
    /// The <c>pi-cli</c> license price.
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
        /// <param name="saasPlan">The SaaS plan.</param>
        public static LicensePrice Create(string saasPlan)
        {
            if (string.IsNullOrEmpty(saasPlan))
            {
                throw new System.ArgumentException($"'{nameof(saasPlan)}' cannot be null or empty.", nameof(saasPlan));
            }

            switch (saasPlan)
            {
                case SaaSPlans.Community:
                    {
                        return new LicensePrice("USD", 0, 0, saasPlan);
                    }
                case SaaSPlans.Micro:
                    {
                        return new LicensePrice("USD", 19, 199, saasPlan);
                    }
                case SaaSPlans.SMB:
                    {
                        return new LicensePrice("USD", 119, 1209, saasPlan);
                    }
                case SaaSPlans.Enterprise:
                    {
                        return new LicensePrice("USD", 219, 2309, saasPlan);
                    }
                case SaaSPlans.ISV:
                    {
                        return new LicensePrice("USD", 619, 6609, saasPlan);
                    }
                case SaaSPlans.ISVU:
                    {
                        return new LicensePrice("USD", 1219, 13109, saasPlan);
                    }
                default:
                    {
                        throw new ErrorException(Errors.InvalidLicense, "The pricing for the SaaS plan is not supported. saas_plan={0}", saasPlan);
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