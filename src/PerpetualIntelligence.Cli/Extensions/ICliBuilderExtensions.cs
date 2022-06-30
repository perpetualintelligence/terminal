/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PerpetualIntelligence.Cli.Commands;
using PerpetualIntelligence.Cli.Commands.Checkers;
using PerpetualIntelligence.Cli.Commands.Declarative;
using PerpetualIntelligence.Cli.Commands.Extractors;
using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Cli.Commands.Mappers;
using PerpetualIntelligence.Cli.Commands.Providers;
using PerpetualIntelligence.Cli.Commands.Routers;
using PerpetualIntelligence.Cli.Commands.Runners;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Integration;
using PerpetualIntelligence.Cli.Licensing;
using PerpetualIntelligence.Cli.Stores;
using PerpetualIntelligence.Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

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

            // Add options checker
            builder.Services.AddSingleton<IOptionsChecker, OptionsChecker>();

            return builder;
        }

        /// <summary>
        /// Adds all the <see cref="IDeclarativeTarget"/> implementations to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="assemblyType">The type whose assembly to inspect and read all the declarative targets.</param>
        /// <returns>The configured <see cref="ICliBuilder"/>.</returns>
        /// <remarks>
        /// The <see cref="AddDeclarativeAssembly(ICliBuilder, Type)"/> reads the target assembly and inspects all the
        /// declarative targets using reflection. Reflection may have a performance bottleneck. For more optimized and
        /// direct declarative target inspection, use <see cref="AddDeclarativeTarget(ICliBuilder, Type)"/>.
        /// </remarks>
        public static ICliBuilder AddDeclarativeAssembly(this ICliBuilder builder, Type assemblyType)
        {
            IEnumerable<Type> declarativeTypes = assemblyType.Assembly.GetTypes()
                .Where(e => e.IsAssignableFrom(typeof(IDeclarativeTarget)) && e.IsClass);

            foreach (Type type in declarativeTypes)
            {
                AddDeclarativeTarget(builder, type);
            }

            return builder;
        }

        /// <summary>
        /// Adds a <see cref="IDeclarativeTarget"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>
        /// The <see cref="AddDeclarativeTarget{TDeclarativeTarget}(ICliBuilder)"/> inspects the declarative target type
        /// using reflection.
        /// </returns>
        public static ICliBuilder AddDeclarativeTarget<TDeclarativeTarget>(this ICliBuilder builder) where TDeclarativeTarget : IDeclarativeTarget
        {
            return AddDeclarativeTarget(builder, typeof(TDeclarativeTarget));
        }

        /// <summary>
        /// Adds the <see cref="CommandDescriptor"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="commandDescriptor">The command descriptor.</param>
        /// <param name="isGroup"><c>true</c> if the descriptor represents a grouped command; otherwise, <c>false</c>.</param>
        /// <param name="isRoot"><c>true</c> if the descriptor represents a root command; otherwise, <c>false</c>.</param>
        /// <param name="isProtected"><c>true</c> if the descriptor represents a protected command; otherwise, <c>false</c>.</param>
        /// <typeparam name="TRunner">The command runner type.</typeparam>
        /// <typeparam name="TChecker">The command checker type.</typeparam>
        /// <returns>The configured <see cref="ICliBuilder"/>.</returns>
        public static ICliBuilder AddDescriptor<TRunner, TChecker>(this ICliBuilder builder, CommandDescriptor commandDescriptor, bool isGroup = false, bool isRoot = false, bool isProtected = false) where TRunner : class, ICommandRunner where TChecker : class, ICommandChecker
        {
            return AddDescriptor(builder, typeof(TRunner), typeof(TChecker), commandDescriptor, isGroup, isRoot, isProtected);
        }

        /// <summary>
        /// Adds the <see cref="IErrorHandler"/> and <see cref="IExceptionHandler"/> to the service collection.
        /// </summary>
        /// <typeparam name="TError">The <see cref="IErrorHandler"/> type.</typeparam>
        /// <typeparam name="TException">The <see cref="IExceptionHandler"/> type.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <returns>The configured <see cref="ICliBuilder"/>.</returns>
        public static ICliBuilder AddErrorHandler<TError, TException>(this ICliBuilder builder) where TError : class, IErrorHandler where TException : class, IExceptionHandler
        {
            // Add error publisher
            builder.Services.AddTransient<IErrorHandler, TError>();

            // Add exception publisher
            builder.Services.AddTransient<IExceptionHandler, TException>();

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
        /// Adds <c>pi-cli</c> license handler to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The configured <see cref="ICliBuilder"/>.</returns>
        public static ICliBuilder AddLicenseHandler(this ICliBuilder builder)
        {
            // Add license extractor as singleton
            builder.Services.AddSingleton<ILicenseExtractor, LicenseExtractor>();

            // Add license checker as singleton
            builder.Services.AddSingleton<ILicenseChecker, LicenseChecker>();

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
        /// Adds the <see cref="ICommandStoreHandler"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <typeparam name="TStore">The command descriptor store type.</typeparam>
        /// <returns>The configured <see cref="ICliBuilder"/>.</returns>
        public static ICliBuilder AddStoreHandler<TStore>(this ICliBuilder builder) where TStore : class, ICommandStoreHandler
        {
            // Add command extractor
            builder.Services.AddTransient<ICommandStoreHandler, TStore>();

            return builder;
        }

        /// <summary>
        /// Adds the <see cref="ITextHandler"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <typeparam name="TTextHandler">The text handler.</typeparam>
        /// <returns>The configured <see cref="ICliBuilder"/>.</returns>
        public static ICliBuilder AddTextHandler<TTextHandler>(this ICliBuilder builder) where TTextHandler : class, ITextHandler
        {
            builder.Services.AddTransient<ITextHandler, TTextHandler>();
            return builder;
        }

        private static ICliBuilder AddDeclarativeTarget(this ICliBuilder builder, Type declarativeTarget)
        {
            // Command descriptor
            CommandDescriptorAttribute cmdAttr = declarativeTarget.GetCustomAttribute<CommandDescriptorAttribute>(false);
            if (cmdAttr == null)
            {
                throw new ErrorException(Errors.InvalidDeclaration, "The declarative target does not define command descriptor.");
            }

            // Command Runner
            CommandRunnerAttribute cmdRunner = declarativeTarget.GetCustomAttribute<CommandRunnerAttribute>(false);
            if (cmdRunner == null)
            {
                throw new ErrorException(Errors.InvalidDeclaration, "The declarative target does not define command runner.");
            }

            // Command checker
            CommandCheckerAttribute cmdChecker = declarativeTarget.GetCustomAttribute<CommandCheckerAttribute>(false);
            if (cmdChecker == null)
            {
                throw new ErrorException(Errors.InvalidDeclaration, "The declarative target does not define command checker.");
            }

            // Text handler
            TextHandlerAttribute textHandlerAttribute = declarativeTarget.GetCustomAttribute<TextHandlerAttribute>(false);
            if (textHandlerAttribute == null)
            {
                throw new ErrorException(Errors.InvalidDeclaration, "The declarative target does not define text handler.");
            }
            ITextHandler textHandler = (ITextHandler)Activator.CreateInstance(textHandlerAttribute.TestHandler);

            // Optional
            IEnumerable<ArgumentDescriptorAttribute> argAttrs = declarativeTarget.GetCustomAttributes<ArgumentDescriptorAttribute>(false);
            IEnumerable<ArgumentValidationAttribute> argVdls = declarativeTarget.GetCustomAttributes<ArgumentValidationAttribute>(false);
            IEnumerable<CommandCustomPropertyAttribute> cmdPropAttrs = declarativeTarget.GetCustomAttributes<CommandCustomPropertyAttribute>(false);
            IEnumerable<ArgumentCustomPropertyAttribute> argPropAttrs = declarativeTarget.GetCustomAttributes<ArgumentCustomPropertyAttribute>(false);

            // Arguments Descriptors
            List<ArgumentDescriptor> argDescs = new();
            foreach (ArgumentDescriptorAttribute argAttr in argAttrs)
            {
                ArgumentDescriptor desc;
                if (argAttr.DataType == DataType.Custom)
                {
                    desc = new ArgumentDescriptor(argAttr.Id, DataType.Custom, argAttr.Required, argAttr.Description, defaultValue: argAttr.DefaultValue);
                }
                else
                {
                    desc = new ArgumentDescriptor(argAttr.Id, argAttr.DataType, argAttr.Required, argAttr.Description, defaultValue: argAttr.DefaultValue);
                }

                // Argument validation attribute
                List<ValidationAttribute>? validationAttributes = null;
                if (argVdls.Any())
                {
                    validationAttributes = new List<ValidationAttribute>();
                    argVdls.All(e =>
                    {
                        if (e.ArgId.Equals(argAttr.Id))
                        {
                            ValidationAttribute validationAttr = (ValidationAttribute)Activator.CreateInstance(e.ValidationAttribute, e.ValidationArgs);
                            validationAttributes.Add(validationAttr);
                        }
                        return true;
                    });
                    desc.ValidationAttributes = validationAttributes.Count == 0 ? null : validationAttributes;
                }

                // Argument custom properties
                Dictionary<string, object>? argCustomProps = null;
                if (argPropAttrs.Any())
                {
                    argCustomProps = new Dictionary<string, object>();
                    argPropAttrs.All(e =>
                    {
                        if (e.ArgId.Equals(argAttr.Id))
                        {
                            argCustomProps.Add(e.Key, e.Value);
                        }
                        return true;
                    });
                }
                desc.CustomProperties = argCustomProps;

                desc.Obsolete = argAttr.Obsolete ? argAttr.Obsolete : null;
                desc.Disabled = argAttr.Disabled ? argAttr.Disabled : null;
                desc.Alias = argAttr.Alias;
                desc.CustomDataType = argAttr.CustomDataType;

                argDescs.Add(desc);
            }
            ArgumentDescriptors argumentDescriptors = new(textHandler, argDescs);

            // Command custom properties
            Dictionary<string, object>? cmdCustomProps = null;
            if (cmdPropAttrs.Any())
            {
                cmdCustomProps = new Dictionary<string, object>();
                cmdPropAttrs.All(e =>
                {
                    cmdCustomProps.Add(e.Key, e.Value);
                    return true;
                });
            }

            // Tags
            CommandTagsAttribute tagsAttr = declarativeTarget.GetCustomAttribute<CommandTagsAttribute>(false);

            // Command descriptor
            CommandDescriptor cmdDesc = new(cmdAttr.Id, cmdAttr.Name, cmdAttr.Prefix, cmdAttr.Description, argumentDescriptors, cmdCustomProps, cmdAttr.DefaultArgument, tagsAttr?.Tags);

            // Add descriptor to service collection.
            return AddDescriptor(builder, cmdRunner.Runner, cmdChecker.Checker, cmdDesc, cmdAttr.IsGroup, cmdAttr.IsRoot, cmdAttr.IsProtected);
        }

        private static ICliBuilder AddDescriptor(ICliBuilder builder, Type runner, Type checker, CommandDescriptor commandDescriptor, bool isGroup, bool isRoot, bool isProtected)
        {
            if (isRoot && !isGroup)
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The root command must also be a grouped command. command_id={0} command_name={1}", commandDescriptor.Id, commandDescriptor.Name);
            }

            if (commandDescriptor.Runner != null || commandDescriptor.Checker != null)
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The command descriptor is already configured and added to the service collection. command_id={0} command_name={1}", commandDescriptor.Id, commandDescriptor.Name);
            }

            // Add the command descriptor as a singleton. Set the runner and checker as transient. These are internal fields.
            commandDescriptor.Runner = runner;
            commandDescriptor.Checker = checker;
            builder.Services.AddSingleton(commandDescriptor);

            // Special annotations
            commandDescriptor.IsRoot = isRoot;
            commandDescriptor.IsGroup = isGroup;
            commandDescriptor.IsProtected = isProtected;

            // Add command runner
            builder.Services.AddTransient(runner);

            // Add command checker
            builder.Services.AddTransient(checker);

            return builder;
        }
    }
}
