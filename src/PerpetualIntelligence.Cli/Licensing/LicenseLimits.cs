/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Protocols.Licensing;
using PerpetualIntelligence.Shared.Exceptions;

namespace PerpetualIntelligence.Cli.Licensing
{
    /// <summary>
    /// Defines the licensing limits based on the <see cref="SaaSPlans"/>.
    /// </summary>
    public sealed class LicenseLimits
    {
        /// <summary>
        /// The maximum arguments or options. Defaults to <c>null</c> or no limit.
        /// </summary>
        public long? ArgumentLimit { get; internal set; }

        /// <summary>
        /// The maximum command groups. Defaults to <c>null</c> or no limit.
        /// </summary>
        public int? CommandGroupLimit { get; internal set; }

        /// <summary>
        /// Supports the command argument data type checks. Defaults to <c>null</c> or no data type checks.
        /// </summary>
        public string[]? DataTypeChecks { get; internal set; }

        /// <summary>
        /// Supports the default command argument. Defaults to <c>false</c> or no default arguments.
        /// </summary>
        public bool DefaultArgument { get; internal set; }

        /// <summary>
        /// Supports the default argument value. Defaults to <c>false</c> or no default argument value.
        /// </summary>
        public bool DefaultArgumentValue { get; internal set; }

        /// <summary>
        /// The maximum sub commands. Defaults to <c>null</c> or no limit.
        /// </summary>
        public string[]? ErrorHandling { get; internal set; }

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
        /// The maximum sub commands. Defaults to <c>null</c> or no redistributions.
        /// </summary>
        public string[]? ServiceImplementations { get; internal set; }

        /// <summary>
        /// The maximum sub commands. Defaults to <c>null</c> or no redistributions.
        /// </summary>
        public string[]? Stores { get; internal set; }

        /// <summary>
        /// The maximum sub commands. Defaults to <c>null</c> or no limit.
        /// </summary>
        public bool StrictDataType { get; internal set; }

        /// <summary>
        /// The maximum sub commands. Defaults to <c>null</c> or no limit.
        /// </summary>
        public long? SubCommandLimit { get; internal set; }

        /// <summary>
        /// The maximum sub commands. Defaults to <c>null</c> or no limit.
        /// </summary>
        public string[]? UnicodeSupport { get; internal set; }

        /// <summary>
        /// Creates a new instance of <see cref="LicenseLimits"/> based on the specified SaaS plan.
        /// </summary>
        /// <param name="saasPlan">The SaaS plan.</param>
        public static LicenseLimits Create(string saasPlan)
        {
            if (string.IsNullOrEmpty(saasPlan))
            {
                throw new System.ArgumentException($"'{nameof(saasPlan)}' cannot be null or empty.", nameof(saasPlan));
            }

            switch (saasPlan)
            {
                case SaaSPlans.Community:
                    {
                        return ForCommunity();
                    }
                case SaaSPlans.Micro:
                    {
                        return ForMicro();
                    }
                case SaaSPlans.SMB:
                    {
                        return ForSMB();
                    }
                case SaaSPlans.Enterprise:
                    {
                        return ForEnterprise();
                    }
                case SaaSPlans.ISV:
                    {
                        return ForISV();
                    }
                case SaaSPlans.ISVU:
                    {
                        return ForISVU();
                    }
                default:
                    {
                        throw new ErrorException(Errors.InvalidLicense, "The licensing for the SaaS plan is not supported. saas_plan={0}", saasPlan);
                    }
            }
        }

        internal LicenseLimits()
        {
        }

        internal static LicenseLimits ForCommunity()
        {
            return new()
            {
                Plan = SaaSPlans.Community,
                RootCommandLimit = 1,
                CommandGroupLimit = 5,
                SubCommandLimit = 50,
                ArgumentLimit = 500,
                RedistributionLimit = 0,

                DataTypeChecks = null,
                StrictDataType = false,
                DefaultArgument = false,
                DefaultArgumentValue = false,

                UnicodeSupport = new[] { "default" },
                ErrorHandling = new[] { "default" },
                Stores = new[] { "in_memory" },
                ServiceImplementations = new[] { "default" }
            };
        }

        internal static LicenseLimits ForEnterprise()
        {
            return new()
            {
                Plan = SaaSPlans.Enterprise,
                RootCommandLimit = 3,
                CommandGroupLimit = 200,
                SubCommandLimit = 2000,
                ArgumentLimit = 20000,
                RedistributionLimit = 5000,

                DataTypeChecks = new[] { "default", "custom" },
                StrictDataType = true,
                DefaultArgument = true,
                DefaultArgumentValue = true,

                UnicodeSupport = new[] { "default" },
                ErrorHandling = new[] { "default", "custom" },
                Stores = new[] { "in_memory", "json", "custom" },
                ServiceImplementations = new[] { "default", "custom" }
            };
        }

        internal static LicenseLimits ForISV()
        {
            return new()
            {
                Plan = SaaSPlans.ISV,
                RootCommandLimit = 5,
                CommandGroupLimit = 500,
                SubCommandLimit = 5000,
                ArgumentLimit = 50000,
                RedistributionLimit = 10000,

                DataTypeChecks = new[] { "default", "custom" },
                StrictDataType = true,
                DefaultArgument = true,
                DefaultArgumentValue = true,

                UnicodeSupport = new[] { "default" },
                ErrorHandling = new[] { "default", "custom" },
                Stores = new[] { "in_memory", "json", "custom" },
                ServiceImplementations = new[] { "default", "custom" }
            };
        }

        internal static LicenseLimits ForISVU()
        {
            return new()
            {
                Plan = SaaSPlans.ISVU,
                RootCommandLimit = null,
                CommandGroupLimit = null,
                SubCommandLimit = null,
                ArgumentLimit = null,
                RedistributionLimit = null,

                DataTypeChecks = new[] { "default", "custom" },
                StrictDataType = true,
                DefaultArgument = true,
                DefaultArgumentValue = true,

                UnicodeSupport = new[] { "default" },
                ErrorHandling = new[] { "default", "custom" },
                Stores = new[] { "in_memory", "json", "custom" },
                ServiceImplementations = new[] { "default", "custom" }
            };
        }

        internal static LicenseLimits ForMicro()
        {
            return new()
            {
                Plan = SaaSPlans.Micro,
                RootCommandLimit = 1,
                CommandGroupLimit = 10,
                SubCommandLimit = 100,
                ArgumentLimit = 1000,
                RedistributionLimit = 0,

                DataTypeChecks = null,
                StrictDataType = false,
                DefaultArgument = false,
                DefaultArgumentValue = false,

                UnicodeSupport = new[] { "default" },
                ErrorHandling = new[] { "default" },
                Stores = new[] { "in_memory" },
                ServiceImplementations = new[] { "default" }
            };
        }

        internal static LicenseLimits ForSMB()
        {
            return new()
            {
                Plan = SaaSPlans.SMB,
                RootCommandLimit = 1,
                CommandGroupLimit = 50,
                SubCommandLimit = 500,
                ArgumentLimit = 5000,
                RedistributionLimit = 1000,

                DataTypeChecks = new[] { "default" },
                StrictDataType = false,
                DefaultArgument = true,
                DefaultArgumentValue = true,

                UnicodeSupport = new[] { "default" },
                ErrorHandling = new[] { "default" },
                Stores = new[] { "in_memory", "json" },
                ServiceImplementations = new[] { "default" }
            };
        }
    }
}
