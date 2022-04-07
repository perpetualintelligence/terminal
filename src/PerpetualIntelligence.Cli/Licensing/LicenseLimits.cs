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
        public long? ArgumentLimit { get; private set; }

        /// <summary>
        /// The maximum command groups. Defaults to <c>null</c> or no limit.
        /// </summary>
        public int? CommandGroupLimit { get; private set; }

        /// <summary>
        /// Supports the command argument data type checks. Defaults to <c>false</c> or no data type checks.
        /// </summary>
        public bool DataTypeChecks { get; private set; }

        /// <summary>
        /// Supports the default arguments and default argument value. Defaults to <c>false</c> or no default arguments.
        /// </summary>
        public bool DefaultArguments { get; private set; }

        /// <summary>
        /// The maximum sub commands. Defaults to <c>null</c> or no limit.
        /// </summary>
        public string[]? ErrorHandling { get; private set; }

        /// <summary>
        /// The maximum sub commands. Defaults to <c>null</c> or no redistribution limit.
        /// </summary>
        public long? RedistributionLimit { get; private set; }

        /// <summary>
        /// The maximum root commands. Defaults to <c>null</c> or no limit.
        /// </summary>
        public int? RootCommandLimit { get; private set; }

        /// <summary>
        /// The maximum sub commands. Defaults to <c>null</c> or no redistributions.
        /// </summary>
        public string[]? Stores { get; private set; }

        /// <summary>
        /// The maximum sub commands. Defaults to <c>null</c> or no redistributions.
        /// </summary>
        public string[]? ServiceImplementations { get; private set; }

        /// <summary>
        /// The maximum sub commands. Defaults to <c>null</c> or no limit.
        /// </summary>
        public bool StrictDataType { get; private set; }

        /// <summary>
        /// The maximum sub commands. Defaults to <c>null</c> or no limit.
        /// </summary>
        public long? SubCommandLimit { get; private set; }

        /// <summary>
        /// The maximum sub commands. Defaults to <c>null</c> or no limit.
        /// </summary>
        public string[]? UnicodeSupport { get; private set; }

        /// <summary>
        /// Creates a new instance of <see cref="LicenseLimits"/> based on the specified SaaS plan.
        /// </summary>
        /// <param name="saasPlan">The SaaS plan.</param>
        public static LicenseLimits Create(string saasPlan)
        {
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

        private LicenseLimits()
        {
        }

        private static LicenseLimits ForCommunity()
        {
            return new()
            {
                RootCommandLimit = 1,
                CommandGroupLimit = 5,
                SubCommandLimit = 50,
                ArgumentLimit = 500,
                RedistributionLimit = 0,

                DataTypeChecks = false,
                DefaultArguments = false,
                StrictDataType = false,

                UnicodeSupport = new[] { "standard" },
                ErrorHandling = new[] { "standard" },
                Stores = new[] { "in_memory" },
                ServiceImplementations = new[] { "standard" }
            };
        }

        private static LicenseLimits ForEnterprise()
        {
            return new()
            {
                RootCommandLimit = 3,
                CommandGroupLimit = 200,
                SubCommandLimit = 2000,
                ArgumentLimit = 20000,
                RedistributionLimit = 5000,

                DataTypeChecks = true,
                DefaultArguments = true,
                StrictDataType = true,

                UnicodeSupport = new[] { "standard" },
                ErrorHandling = new[] { "standard", "custom" },
                Stores = new[] { "in_memory", "json", "custom" },
                ServiceImplementations = new[] { "standard", "custom" }
            };
        }

        private static LicenseLimits ForISV()
        {
            return new()
            {
                RootCommandLimit = 5,
                CommandGroupLimit = 500,
                SubCommandLimit = 5000,
                ArgumentLimit = 50000,
                RedistributionLimit = 10000,

                DataTypeChecks = true,
                DefaultArguments = true,
                StrictDataType = true,

                UnicodeSupport = new[] { "standard" },
                ErrorHandling = new[] { "standard", "custom" },
                Stores = new[] { "in_memory", "json", "custom" },
                ServiceImplementations = new[] { "standard", "custom" }
            };
        }



        private static LicenseLimits ForISVU()
        {
            return new()
            {
                RootCommandLimit = null,
                CommandGroupLimit = null,
                SubCommandLimit = null,
                ArgumentLimit = null,
                RedistributionLimit = null,

                DataTypeChecks = true,
                DefaultArguments = true,
                StrictDataType = true,

                UnicodeSupport = new[] { "standard" },
                ErrorHandling = new[] { "standard", "custom" },
                Stores = new[] { "in_memory", "json", "custom" },
                ServiceImplementations = new[] { "standard", "custom" }
            };
        }

        private static LicenseLimits ForMicro()
        {
            return new()
            {
                RootCommandLimit = 1,
                CommandGroupLimit = 10,
                SubCommandLimit = 100,
                ArgumentLimit = 1000,
                RedistributionLimit = 0,

                DataTypeChecks = false,
                DefaultArguments = false,
                StrictDataType = false,

                UnicodeSupport = new[] { "standard" },
                ErrorHandling = new[] { "standard" },
                Stores = new[] { "in_memory" },
                ServiceImplementations = new[] { "standard" }
            };
        }

        private static LicenseLimits ForSMB()
        {
            return new()
            {
                RootCommandLimit = 1,
                CommandGroupLimit = 50,
                SubCommandLimit = 500,
                ArgumentLimit = 5000,
                RedistributionLimit = 1000,

                DataTypeChecks = true,
                DefaultArguments = true,
                StrictDataType = true,

                UnicodeSupport = new[] { "standard" },
                ErrorHandling = new[] { "standard" },
                Stores = new[] { "in_memory", "json" },
                ServiceImplementations = new[] { "standard" }
            };
        }
    }
}
