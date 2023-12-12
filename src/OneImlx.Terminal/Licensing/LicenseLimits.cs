/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Shared.Extensions;
using OneImlx.Shared.Licensing;
using System;
using System.Collections.Generic;

namespace OneImlx.Terminal.Licensing
{
    /// <summary>
    /// Defines the licensing limits based on the <see cref="TerminalLicensePlans"/>.
    /// </summary>
    public sealed class LicenseLimits
    {
        /// <summary>
        /// The maximum options or options. Defaults to <c>null</c> or no limit.
        /// </summary>
        public long? OptionLimit { get; internal set; }

        /// <summary>
        /// The maximum grouped commands. Defaults to <c>null</c> or no limit.
        /// </summary>
        public int? GroupedCommandLimit { get; internal set; }

        /// <summary>
        /// The maximum sub commands. Defaults to <c>null</c> or no redistributions.
        /// </summary>
        public string[]? LicenseHandlers { get; internal set; }

        /// <summary>
        /// The SaaS plan.
        /// </summary>
        public string? Plan { get; internal set; }

        /// <summary>
        /// The maximum sub commands. Defaults to <c>null</c> or no redistribution limit.
        /// </summary>
        public long? RedistributionLimit { get; internal set; }

        /// <summary>
        /// The maximum root commands. Defaults to <c>null</c> or no limit.
        /// </summary>
        public int? RootCommandLimit { get; internal set; }

        /// <summary>
        /// The service implantations.
        /// </summary>
        public string[]? ServiceHandlers { get; internal set; }

        /// <summary>
        /// The maximum sub commands. Defaults to <c>null</c> or no redistributions.
        /// </summary>
        public string[]? StoreHandlers { get; internal set; }

        /// <summary>
        /// The maximum sub commands. Defaults to <c>null</c> or no limit.
        /// </summary>
        public bool StrictDataType { get; internal set; }

        /// <summary>
        /// The maximum sub commands. Defaults to <c>null</c> or no limit.
        /// </summary>
        public long? SubCommandLimit { get; internal set; }

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
            if (string.IsNullOrEmpty(licensePlan))
            {
                throw new System.ArgumentException($"'{nameof(licensePlan)}' cannot be null or empty.", nameof(licensePlan));
            }

            switch (licensePlan)
            {
                case TerminalLicensePlans.Demo:
                    {
                        return ForDemo();
                    }
                case TerminalLicensePlans.Micro:
                    {
                        return ForMicro();
                    }
                case TerminalLicensePlans.SMB:
                    {
                        return ForSMB();
                    }
                case TerminalLicensePlans.Enterprise:
                    {
                        return ForEnterprise();
                    }
                case TerminalLicensePlans.OnPremise:
                    {
                        return ForOnPremise();
                    }
                case TerminalLicensePlans.Unlimited:
                    {
                        return ForUnlimited();
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

        internal static LicenseLimits ForDemo()
        {
            return new()
            {
                Plan = TerminalLicensePlans.Demo,
                TerminalLimit = 1,
                RedistributionLimit = 0,
                RootCommandLimit = 1,
                GroupedCommandLimit = 5,
                SubCommandLimit = 25,
                OptionLimit = 500,

                StrictDataType = true,

                StoreHandlers = new[] { TerminalHandlers.InMemoryHandler },
                ServiceHandlers = new[] { TerminalHandlers.DefaultHandler },
                LicenseHandlers = new[] { TerminalHandlers.OnlineLicenseHandler }
            };
        }

        internal static LicenseLimits ForCustom(IDictionary<string, object> customClaims)
        {
            LicenseLimits limits = new()
            {
                Plan = TerminalLicensePlans.Custom,

                TerminalLimit = Convert.ToInt16(customClaims["terminal_limit"]),
                RedistributionLimit = Convert.ToInt16(customClaims["redistribution_limit"]),
                RootCommandLimit = Convert.ToInt16(customClaims["root_command_limit"]),
                GroupedCommandLimit = Convert.ToInt16(customClaims["grouped_command_limit"]),
                SubCommandLimit = Convert.ToInt16(customClaims["sub_command_limit"]),
                OptionLimit = Convert.ToInt16(customClaims["option_limit"]),

                StrictDataType = Convert.ToBoolean(customClaims["strict_data_type"]),

                StoreHandlers = customClaims["store_handlers"].ToString().SplitBySpace(),
                ServiceHandlers = customClaims["service_handlers"].ToString().SplitBySpace(),
                LicenseHandlers = customClaims["license_handlers"].ToString().SplitBySpace()
            };

            return limits;
        }

        internal static LicenseLimits ForEnterprise()
        {
            return new()
            {
                Plan = TerminalLicensePlans.Enterprise,
                TerminalLimit = 5,
                RedistributionLimit = 5000,
                RootCommandLimit = 3,
                GroupedCommandLimit = 20,
                SubCommandLimit = 100,
                OptionLimit = 2000,

                StrictDataType = true,

                StoreHandlers = new[] { TerminalHandlers.InMemoryHandler, TerminalHandlers.JsonHandler, TerminalHandlers.CustomHandler },
                ServiceHandlers = new[] { TerminalHandlers.DefaultHandler, TerminalHandlers.CustomHandler },
                LicenseHandlers = new[] { TerminalHandlers.OnlineLicenseHandler, TerminalHandlers.OfflineLicenseHandler }
            };
        }

        internal static LicenseLimits ForOnPremise()
        {
            return new()
            {
                Plan = TerminalLicensePlans.OnPremise,
                TerminalLimit = 25,
                RedistributionLimit = 10000,
                RootCommandLimit = 5,
                GroupedCommandLimit = 50,
                SubCommandLimit = 250,
                OptionLimit = 5000,

                StrictDataType = true,

                StoreHandlers = new[] { TerminalHandlers.InMemoryHandler, TerminalHandlers.JsonHandler, TerminalHandlers.CustomHandler },
                ServiceHandlers = new[] { TerminalHandlers.DefaultHandler, TerminalHandlers.CustomHandler },
                LicenseHandlers = new[] { TerminalHandlers.OnlineLicenseHandler, TerminalHandlers.OfflineLicenseHandler, TerminalHandlers.OnPremiseLicenseHandler }
            };
        }

        internal static LicenseLimits ForUnlimited()
        {
            return new()
            {
                Plan = TerminalLicensePlans.Unlimited,
                TerminalLimit = null,
                RedistributionLimit = null,
                RootCommandLimit = null,
                GroupedCommandLimit = null,
                SubCommandLimit = null,
                OptionLimit = null,

                StrictDataType = true,

                StoreHandlers = new[] { TerminalHandlers.InMemoryHandler, TerminalHandlers.JsonHandler, TerminalHandlers.CustomHandler },
                ServiceHandlers = new[] { TerminalHandlers.DefaultHandler, TerminalHandlers.CustomHandler },
                LicenseHandlers = new[] { TerminalHandlers.OnlineLicenseHandler, TerminalHandlers.OfflineLicenseHandler, TerminalHandlers.OnPremiseLicenseHandler }
            };
        }

        internal static LicenseLimits ForMicro()
        {
            return new()
            {
                Plan = TerminalLicensePlans.Micro,
                TerminalLimit = 1,
                RedistributionLimit = 0,
                RootCommandLimit = 1,
                GroupedCommandLimit = 5,
                SubCommandLimit = 25,
                OptionLimit = 500,

                StrictDataType = false,

                StoreHandlers = new[] { TerminalHandlers.InMemoryHandler },
                ServiceHandlers = new[] { TerminalHandlers.DefaultHandler },
                LicenseHandlers = new[] { TerminalHandlers.OnlineLicenseHandler }
            };
        }

        internal static LicenseLimits ForSMB()
        {
            return new()
            {
                Plan = TerminalLicensePlans.SMB,
                TerminalLimit = 3,
                RedistributionLimit = 1000,
                RootCommandLimit = 1,
                GroupedCommandLimit = 10,
                SubCommandLimit = 50,
                OptionLimit = 1000,

                StrictDataType = true,

                StoreHandlers = new[] { TerminalHandlers.InMemoryHandler, TerminalHandlers.JsonHandler },
                ServiceHandlers = new[] { TerminalHandlers.DefaultHandler },
                LicenseHandlers = new[] { TerminalHandlers.OnlineLicenseHandler }
            };
        }
    }
}