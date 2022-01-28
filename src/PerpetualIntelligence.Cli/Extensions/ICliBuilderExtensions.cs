/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PerpetualIntelligence.Cli.Commands;
using PerpetualIntelligence.Cli.Commands.Checkers;
using PerpetualIntelligence.Cli.Commands.Extractors;
using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Cli.Commands.Mappers;
using PerpetualIntelligence.Cli.Commands.Providers;
using PerpetualIntelligence.Cli.Commands.Routers;
using PerpetualIntelligence.Cli.Commands.Runners;
using PerpetualIntelligence.Cli.Commands.Stores;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Integration;
using System;

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
        /// <typeparam name="TRunner">The command runner type.</typeparam>
        /// <typeparam name="TChecker">The command checker type.</typeparam>
        /// <returns>The configured <see cref="ICliBuilder"/>.</returns>
        public static ICliBuilder AddCommandIdentity<TRunner, TChecker>(this ICliBuilder builder, CommandDescriptor commandDescriptor) where TRunner : class, ICommandRunner where TChecker : class, ICommandChecker
        {
            if (commandDescriptor.Runner != null && commandDescriptor.Checker != null)
            {
                throw new InvalidOperationException("The command descriptor cannot specify runner or checker during explicit configuration.");
            }

            // Add the command descriptor as a singleton. Set the runner and checker as transient. These are internal fields.
            commandDescriptor.Runner = typeof(TRunner);
            commandDescriptor.Checker = typeof(TChecker);
            builder.Services.AddSingleton(commandDescriptor);

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
        public static ICliBuilder AddCommandIdentityStore<TStore>(this ICliBuilder builder) where TStore : class, ICommandDescriptorStore
        {
            // Add command extractor
            builder.Services.AddTransient<ICommandDescriptorStore, TStore>();

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
        /// <see cref="IArgumentDefaultValueProvider"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <typeparam name="TCommand">The command extractor type.</typeparam>
        /// <typeparam name="TArgument">The argument extractor type.</typeparam>
        /// <typeparam name="TDefaultProvider">The argument default value provider type.</typeparam>
        /// <returns>The configured <see cref="ICliBuilder"/>.</returns>
        public static ICliBuilder AddExtractor<TCommand, TArgument, TDefaultProvider>(this ICliBuilder builder) where TCommand : class, ICommandExtractor where TArgument : class, IArgumentExtractor where TDefaultProvider : class, IArgumentDefaultValueProvider
        {
            // Add command extractor
            builder.Services.AddTransient<ICommandExtractor, TCommand>();

            // Add argument extractor
            builder.Services.AddTransient<IArgumentExtractor, TArgument>();

            // Add argument default value provider
            builder.Services.AddTransient<IArgumentDefaultValueProvider, TDefaultProvider>();

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
    }
}
