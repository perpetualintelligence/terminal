/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using OneImlx.Shared.Licensing;

namespace OneImlx.Terminal.Licensing
{
    /// <summary>
    /// Defines the licensing limits based on the <see cref="TerminalLicensePlans"/>.
    /// </summary>
    public sealed class LicenseLimits
    {
        /// <summary>
        /// The terminal authentication. Defaults to <c>null</c> or no limit.
        /// </summary>
        public bool? Authentication { get; internal set; }

        /// <summary>
        /// The maximum commands. Defaults to <c>null</c> or no limit.
        /// </summary>
        public int? CommandLimit { get; internal set; }

        /// <summary>
        /// The maximum arguments and options combined. Defaults to <c>null</c> for no limit.
        /// </summary>
        public int? InputLimit { get; internal set; }

        /// <summary>
        /// The license plan.
        /// </summary>
        public string? Plan { get; internal set; }

        /// <summary>
        /// The maximum redistributions. Defaults to <c>null</c> or no redistribution limit.
        /// </summary>
        public long? RedistributionLimit { get; internal set; }

        /// <summary>
        /// The string date type. Defaults to <c>null</c> or no limit.
        /// </summary>
        public bool StrictDataType { get; internal set; }

        /// <summary>
        /// The maximum terminals. Defaults to <c>null</c> or no limit.
        /// </summary>
        public int? TerminalLimit { get; internal set; }

        /// <summary>
        /// Creates a new instance of <see cref="LicenseLimits"/> based on the specified SaaS plan.
        /// </summary>
        /// <param name="licensePlan">The license plan.</param>
        /// <param name="customClaims">The custom claims. Only used if SaaS plan is custom.</param>
        public static LicenseLimits Create(string licensePlan, IDictionary<string, object>? customClaims = null)
        {
            switch (licensePlan)
            {
                case TerminalLicensePlans.Demo:
                    {
                        return ForDemo();
                    }
                case TerminalLicensePlans.Solo:
                    {
                        return ForSolo();
                    }
                case TerminalLicensePlans.Micro:
                    {
                        return ForMicro();
                    }
                case TerminalLicensePlans.Smb:
                    {
                        return ForSmb();
                    }
                case TerminalLicensePlans.Enterprise:
                    {
                        return ForEnterprise();
                    }
                case TerminalLicensePlans.Corporate:
                    {
                        return ForCorporate();
                    }
                case TerminalLicensePlans.Custom:
                    {
                        if (customClaims == null)
                        {
                            throw new TerminalException(TerminalErrors.InvalidLicense, "The licensing for the custom plan requires a custom claims. plan={0}", licensePlan);
                        }

                        return ForCustom(customClaims);
                    }
                default:
                    {
                        throw new TerminalException(TerminalErrors.InvalidLicense, "The license for the plan is not supported. plan={0}", licensePlan);
                    }
            }
        }

        private static LicenseLimits ForCorporate()
        {
            return new()
            {
                Plan = TerminalLicensePlans.Corporate,

                TerminalLimit = 15,
                CommandLimit = null,
                InputLimit = null,
                RedistributionLimit = null,

                StrictDataType = true,
                Authentication = true
            };
        }

        private static LicenseLimits ForCustom(IDictionary<string, object> customClaims)
        {
            LicenseLimits limits = new()
            {
                Plan = TerminalLicensePlans.Custom,

                TerminalLimit = Convert.ToInt32(customClaims["terminal_limit"]),
                CommandLimit = Convert.ToInt32(customClaims["command_limit"]),
                InputLimit = Convert.ToInt32(customClaims["input_limit"]),
                RedistributionLimit = Convert.ToInt32(customClaims["redistribution_limit"]),

                StrictDataType = Convert.ToBoolean(customClaims["strict_data_type"]),
                Authentication = Convert.ToBoolean(customClaims["authentication"]),
            };

            return limits;
        }

        private static LicenseLimits ForDemo()
        {
            return new()
            {
                Plan = TerminalLicensePlans.Demo,

                TerminalLimit = 1,
                CommandLimit = 25,
                InputLimit = 250,
                RedistributionLimit = 0,

                StrictDataType = true,
                Authentication = true
            };
        }

        private static LicenseLimits ForEnterprise()
        {
            return new()
            {
                Plan = TerminalLicensePlans.Enterprise,

                TerminalLimit = 10,
                CommandLimit = 300,
                InputLimit = 6000,
                RedistributionLimit = 15000,

                StrictDataType = true,
                Authentication = true
            };
        }

        private static LicenseLimits ForMicro()
        {
            return new()
            {
                Plan = TerminalLicensePlans.Micro,

                TerminalLimit = 3,
                CommandLimit = 50,
                InputLimit = 500,
                RedistributionLimit = 1000,

                StrictDataType = true,
                Authentication = true
            };
        }

        private static LicenseLimits ForSmb()
        {
            return new()
            {
                Plan = TerminalLicensePlans.Smb,

                TerminalLimit = 5,
                CommandLimit = 100,
                InputLimit = 2000,
                RedistributionLimit = 5000,

                StrictDataType = true,
                Authentication = true
            };
        }

        private static LicenseLimits ForSolo()
        {
            return new()
            {
                Plan = TerminalLicensePlans.Solo,

                TerminalLimit = 1,
                CommandLimit = 25,
                InputLimit = 250,
                RedistributionLimit = 0,

                StrictDataType = false,
                Authentication = false
            };
        }
    }
}
