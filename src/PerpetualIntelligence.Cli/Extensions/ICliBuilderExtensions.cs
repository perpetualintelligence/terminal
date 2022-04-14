/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using PerpetualIntelligence.Cli.Authentication;
using PerpetualIntelligence.Cli.Commands;
using PerpetualIntelligence.Cli.Commands.Checkers;
using PerpetualIntelligence.Cli.Commands.Comparers;
using PerpetualIntelligence.Cli.Commands.Extractors;
using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Cli.Commands.Mappers;
using PerpetualIntelligence.Cli.Commands.Providers;
using PerpetualIntelligence.Cli.Commands.Publishers;
using PerpetualIntelligence.Cli.Commands.Routers;
using PerpetualIntelligence.Cli.Commands.Runners;
using PerpetualIntelligence.Cli.Commands.Stores;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Integration;
using PerpetualIntelligence.Cli.Licensing;
using PerpetualIntelligence.Protocols.Abstractions.Comparers;
using PerpetualIntelligence.Protocols.Licensing;
using PerpetualIntelligence.Shared.Exceptions;
using System;
using System.Net.Http;
using System.Threading;

namespace PerpetualIntelligence.Cli.Extensions
{
    /// <summary>
    /// The <see cref="ICliBuilder"/> extension methods.
    /// </summary>
    public static class ICliBuilderExtensions
    {
        /// <summary>
        /// Adds the <see cref="IArgumentDataTypeMapper"/> and <see cref="IArgumentChecker"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <typeparam name="TMapper">The argument mapper type.</typeparam>
        /// <typeparam name="TChecker">The argument checker type.</typeparam>
        /// <returns>The configured <see cref="ICliBuilder"/>.</returns>
        public static ICliBuilder AddArgumentChecker<TMapper, TChecker>(this ICliBuilder builder) where TMapper : class, IArgumentDataTypeMapper where TChecker : class, IArgumentChecker
        {
            builder.Services.AddTransient<IArgumentDataTypeMapper, TMapper>();
            builder.Services.AddTransient<IArgumentChecker, TChecker>();
            return builder;
        }

        /// <summary>
        /// Adds authentication to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="name">The HTTP client name.</param>
        /// <param name="baseAddress">The HTTP base address. Specify <c>null</c> for no specific well known base address.</param>
        /// <param name="timeout">
        /// The HTTP request timeout in milliseconds. Defaults to <c>2</c> minutes or <c>120000</c> milliseconds. We
        /// recommend a timeout of at least a few minutes, to take into account cases where the user is prompted to
        /// change password or perform 2FA.
        /// </param>
        /// <typeparam name="TProvider">The authentication provider.</typeparam>
        /// <typeparam name="TAppFactory">The authentication application factory.</typeparam>
        /// <typeparam name="TAppCache">The authentication application cache.</typeparam>
        /// <typeparam name="TDelegateHandler">The authentication application delegate handler.</typeparam>
        /// <returns>The configured <see cref="ICliBuilder"/>.</returns>
        /// <remarks>
        /// Use <see cref="ClientCrossPlatformNoTokenCache"/> if your application does not require token caching.
        /// </remarks>
        public static ICliBuilder AddAuthentication<TProvider, TAppFactory, TAppCache, TDelegateHandler>(this ICliBuilder builder, string name, string? baseAddress = null, int? timeout = 120000) where TProvider : class, IAuthenticationProvider where TAppFactory : class, IMsalPublicClientApplicationFactory where TAppCache : class, IClientCrossPlatformTokenCache where TDelegateHandler : DelegatingHandler
        {
            builder.Services.AddSingleton<IAuthenticationProvider, TProvider>();

            builder.Services.AddSingleton<IMsalPublicClientApplicationFactory, TAppFactory>();

            builder.Services.AddSingleton<IClientCrossPlatformTokenCache, TAppCache>();

            builder.Services.AddSingleton<TDelegateHandler>();

            // Configure to call the authority
            builder.Services.AddHttpClient(name, client =>
          {
              client.BaseAddress = (baseAddress != null) ? new Uri(baseAddress) : null;
              client.Timeout = (timeout == null) ? Timeout.InfiniteTimeSpan : TimeSpan.FromMilliseconds(timeout.GetValueOrDefault());
          }).AddHttpMessageHandler<TDelegateHandler>();

            return builder;
        }

        /// <summary>
        /// Adds the <see cref="CliOptions"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The configured <see cref="ICliBuilder"/>.</returns>
        public static ICliBuilder AddCliOptions(this ICliBuilder builder)
        {
            // Add options.
            builder.Services.AddOptions();
            builder.Services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<CliOptions>>().Value);
            return builder;
        }

        /// <summary>
        /// Adds the <see cref="CommandDescriptor"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="commandDescriptor">The command descriptor.</param>
        /// <param name="isGroup"><c>true</c> if the descriptor represents a command group; otherwise, <c>false</c>.</param>
        /// <param name="isRoot"><c>true</c> if the descriptor represents a command root; otherwise, <c>false</c>.</param>
        /// <param name="isProtected"><c>true</c> if the descriptor represents a protected command; otherwise, <c>false</c>.</param>
        /// <typeparam name="TRunner">The command runner type.</typeparam>
        /// <typeparam name="TChecker">The command checker type.</typeparam>
        /// <returns>The configured <see cref="ICliBuilder"/>.</returns>
        public static ICliBuilder AddDescriptor<TRunner, TChecker>(this ICliBuilder builder, CommandDescriptor commandDescriptor, bool isGroup = false, bool isRoot = false, bool isProtected = false) where TRunner : class, ICommandRunner where TChecker : class, ICommandChecker
        {
            if (isRoot && !isGroup)
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The root command must also be a command group. command_id={0} command_name={1}", commandDescriptor.Id, commandDescriptor.Name);
            }

            if (commandDescriptor._runner != null || commandDescriptor._checker != null)
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The command descriptor is already configured and added to the service collection.");
            }

            // Add the command descriptor as a singleton. Set the runner and checker as transient. These are internal fields.
            commandDescriptor._runner = typeof(TRunner);
            commandDescriptor._checker = typeof(TChecker);
            builder.Services.AddSingleton(commandDescriptor);

            // Special annotations
            commandDescriptor._isRoot = isRoot;
            commandDescriptor._isGroup = isGroup;
            commandDescriptor._isProtected = isProtected;

            // Add command runner
            builder.Services.AddTransient<TRunner>();

            // Add command checker
            builder.Services.AddTransient<TChecker>();

            return builder;
        }

        /// <summary>
        /// Adds the <see cref="ICommandDescriptorStore"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <typeparam name="TStore">The command descriptor store type.</typeparam>
        /// <returns>The configured <see cref="ICliBuilder"/>.</returns>
        public static ICliBuilder AddDescriptorStore<TStore>(this ICliBuilder builder) where TStore : class, ICommandDescriptorStore
        {
            // Add command extractor
            builder.Services.AddTransient<ICommandDescriptorStore, TStore>();

            return builder;
        }

        /// <summary>
        /// Adds the <see cref="IErrorPublisher"/> and <see cref="IExceptionPublisher"/> to the service collection.
        /// </summary>
        /// <typeparam name="TError">The <see cref="IErrorPublisher"/> type.</typeparam>
        /// <typeparam name="TException">The <see cref="IExceptionPublisher"/> type.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <returns>The configured <see cref="ICliBuilder"/>.</returns>
        public static ICliBuilder AddErrorPublisher<TError, TException>(this ICliBuilder builder) where TError : class, IErrorPublisher where TException : class, IExceptionPublisher
        {
            // Add error publisher
            builder.Services.AddTransient<IErrorPublisher, TError>();

            // Add exception publisher
            builder.Services.AddTransient<IExceptionPublisher, TException>();

            return builder;
        }

        /// <summary>
        /// Adds the <see cref="ICommandExtractor"/> and <see cref="IArgumentExtractor"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <typeparam name="TCommand">The command extractor type.</typeparam>
        /// <typeparam name="TArgument">The argument extractor type.</typeparam>
        /// <returns>The configured <see cref="ICliBuilder"/>.</returns>
        public static ICliBuilder AddExtractor<TCommand, TArgument>(this ICliBuilder builder) where TCommand : class, ICommandExtractor where TArgument : class, IArgumentExtractor
        {
            // Add command extractor
            builder.Services.AddTransient<ICommandExtractor, TCommand>();

            // Add argument extractor
            builder.Services.AddTransient<IArgumentExtractor, TArgument>();

            return builder;
        }

        /// <summary>
        /// Adds the <see cref="ICommandExtractor"/>, <see cref="IArgumentExtractor"/> and
        /// <see cref="IDefaultArgumentValueProvider"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <typeparam name="TCommand">The command extractor type.</typeparam>
        /// <typeparam name="TArgument">The argument extractor type.</typeparam>
        /// <typeparam name="TDefaultArgumentValue">The argument default value provider type.</typeparam>
        /// <returns>The configured <see cref="ICliBuilder"/>.</returns>
        public static ICliBuilder AddExtractor<TCommand, TArgument, TDefaultArgumentValue>(this ICliBuilder builder) where TCommand : class, ICommandExtractor where TArgument : class, IArgumentExtractor where TDefaultArgumentValue : class, IDefaultArgumentValueProvider
        {
            // Add command extractor
            builder.Services.AddTransient<ICommandExtractor, TCommand>();

            // Add argument extractor
            builder.Services.AddTransient<IArgumentExtractor, TArgument>();

            // Add default argument value provider
            builder.Services.AddTransient<IDefaultArgumentValueProvider, TDefaultArgumentValue>();

            return builder;
        }

        /// <summary>
        /// Adds the <see cref="ICommandExtractor"/>, <see cref="IArgumentExtractor"/>,
        /// <see cref="IDefaultArgumentProvider"/> and <see cref="IDefaultArgumentValueProvider"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <typeparam name="TCommand">The command extractor type.</typeparam>
        /// <typeparam name="TArgument">The argument extractor type.</typeparam>
        /// <typeparam name="TDefaultArgument">The default argument provider type.</typeparam>
        /// <typeparam name="TDefaultArgumentValue">The default argument value provider type.</typeparam>
        /// <returns>The configured <see cref="ICliBuilder"/>.</returns>
        public static ICliBuilder AddExtractor<TCommand, TArgument, TDefaultArgument, TDefaultArgumentValue>(this ICliBuilder builder) where TCommand : class, ICommandExtractor where TArgument : class, IArgumentExtractor where TDefaultArgument : class, IDefaultArgumentProvider where TDefaultArgumentValue : class, IDefaultArgumentValueProvider
        {
            // Add command extractor
            builder.Services.AddTransient<ICommandExtractor, TCommand>();

            // Add argument extractor
            builder.Services.AddTransient<IArgumentExtractor, TArgument>();

            // Add default argument provider
            builder.Services.AddTransient<IDefaultArgumentProvider, TDefaultArgument>();

            // Add default argument value provider
            builder.Services.AddTransient<IDefaultArgumentValueProvider, TDefaultArgumentValue>();

            return builder;
        }

        /// <summary>
        /// Adds <c>pi-cli</c> licensing to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The configured <see cref="ICliBuilder"/>.</returns>
        public static ICliBuilder AddLicensing(this ICliBuilder builder)
        {
            // Add license extractor as singleton
            builder.Services.AddSingleton<ILicenseExtractor, LicenseExtractor>();

            // Add license checker as singleton
            builder.Services.AddSingleton<ILicenseChecker, LicenseChecker>();

            // Add license provider resolver as singleton
            builder.Services.AddSingleton<ILicenseProviderResolver, LicenseProviderResolver>();

            return builder;
        }

        /// <summary>
        /// Adds a <see cref="IHttpClientFactory"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="name">The HTTP client name.</param>
        /// <param name="timeout">The HTTP client timeout.</param>
        /// <returns>The configured <see cref="ICliBuilder"/>.</returns>
        /// <remarks>
        /// The <see cref="AddLicensingClient(ICliBuilder, string, TimeSpan)"/> is required if you are using
        /// <see cref="SaaSCheckModes.Online"/> check. Please set the <see cref="LicensingOptions.HttpClientName"/> to
        /// match the name used to register this service.
        /// </remarks>
        public static ICliBuilder AddLicensingClient(this ICliBuilder builder, string name, TimeSpan timeout)
        {
            // Configure to call the security API
            builder.Services.AddHttpClient(name, client =>
            {
                client.BaseAddress = new Uri("https://api.perpetualintelligence.com/security/");
                client.Timeout = timeout;
            });

            return builder;
        }

        /// <summary>
        /// Adds the <see cref="ICommandRouter"/> and <see cref="ICommandHandler"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The configured <see cref="ICliBuilder"/>.</returns>
        public static ICliBuilder AddRouter<TRouter, THandler>(this ICliBuilder builder) where TRouter : class, ICommandRouter where THandler : class, ICommandHandler
        {
            // Add command router
            builder.Services.AddTransient<ICommandRouter, TRouter>();

            // Add command handler
            builder.Services.AddTransient<ICommandHandler, THandler>();

            return builder;
        }

        /// <summary>
        /// Add the <see cref="IStringComparer"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="stringComparison">The string comparison to use.</param>
        /// <returns>The configured <see cref="ICliBuilder"/>.</returns>
        public static ICliBuilder AddStringComparer(this ICliBuilder builder, StringComparison stringComparison)
        {
            // Add string comparer
            IServiceCollection serviceCollection = builder.Services.AddTransient<IStringComparer>(resolver => new StringComparisonComparer(stringComparison));
            return builder;
        }
    }
}
