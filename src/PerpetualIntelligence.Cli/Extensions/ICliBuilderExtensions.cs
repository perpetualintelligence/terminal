/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
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
using PerpetualIntelligence.Cli.Events;
using PerpetualIntelligence.Cli.Hosting;
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
        /// Adds the <see cref="IOptionDataTypeMapper"/> and <see cref="IOptionChecker"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <typeparam name="TMapper">The option mapper type.</typeparam>
        /// <typeparam name="TChecker">The option checker type.</typeparam>
        /// <returns>The configured <see cref="ICliBuilder"/>.</returns>
        public static ICliBuilder AddArgumentChecker<TMapper, TChecker>(this ICliBuilder builder) where TMapper : class, IOptionDataTypeMapper where TChecker : class, IOptionChecker
        {
            builder.Services.AddTransient<IOptionDataTypeMapper, TMapper>();
            builder.Services.AddTransient<IOptionChecker, TChecker>();
            return builder;
        }

        /// <summary>
        /// Adds the <see cref="IAsyncEventHandler"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The configured <see cref="ICliBuilder"/>.</returns>
        public static ICliBuilder AddEventHandler<TEventHandler>(this ICliBuilder builder) where TEventHandler : class, IAsyncEventHandler
        {
            builder.Services.AddTransient<IAsyncEventHandler, TEventHandler>();
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
            builder.Services.AddSingleton<IConfigurationOptionsChecker, ConfigurationOptionsChecker>();

            return builder;
        }

        /// <summary>
        /// Adds the command <see cref="IHelpProvider"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The configured <see cref="ICliBuilder"/>.</returns>
        public static ICliBuilder AddHelpProvider<THelpProvider>(this ICliBuilder builder) where THelpProvider : class, IHelpProvider
        {
            builder.Services.AddSingleton<IHelpProvider, THelpProvider>();
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
                .Where(e => typeof(IDeclarativeTarget).IsAssignableFrom(e));

            foreach (Type type in declarativeTypes)
            {
                AddDeclarativeTarget(builder, type);
            }

            return builder;
        }

        /// <summary>
        /// Adds all the <see cref="IDeclarativeTarget"/> implementations to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <typeparam name="TType">The type whose assembly to inspect and read all the declarative targets.</typeparam>
        /// <returns>The configured <see cref="ICliBuilder"/>.</returns>
        /// <remarks>
        /// The <see cref="AddDeclarativeAssembly(ICliBuilder, Type)"/> reads the target assembly and inspects all the
        /// declarative targets using reflection. Reflection may have a performance bottleneck. For more optimized and
        /// direct declarative target inspection, use <see cref="AddDeclarativeTarget(ICliBuilder, Type)"/>.
        /// </remarks>
        public static ICliBuilder AddDeclarativeAssembly<TType>(this ICliBuilder builder)
        {
            return AddDeclarativeAssembly(builder, typeof(TType));
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
        /// Adds the <see cref="ICommandExtractor"/> and <see cref="IOptionExtractor"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <typeparam name="TCommand">The command extractor type.</typeparam>
        /// <typeparam name="TArgument">The option extractor type.</typeparam>
        /// <returns>The configured <see cref="ICliBuilder"/>.</returns>
        public static ICliBuilder AddExtractor<TCommand, TArgument>(this ICliBuilder builder) where TCommand : class, ICommandExtractor where TArgument : class, IOptionExtractor
        {
            // Add command extractor
            builder.Services.AddTransient<ICommandExtractor, TCommand>();

            // Add option extractor
            builder.Services.AddTransient<IOptionExtractor, TArgument>();

            return builder;
        }

        /// <summary>
        /// Adds the <see cref="ICommandExtractor"/>, <see cref="IOptionExtractor"/> and
        /// <see cref="IDefaultOptionValueProvider"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <typeparam name="TCommand">The command extractor type.</typeparam>
        /// <typeparam name="TArgument">The option extractor type.</typeparam>
        /// <typeparam name="TDefaultArgumentValue">The option default value provider type.</typeparam>
        /// <returns>The configured <see cref="ICliBuilder"/>.</returns>
        public static ICliBuilder AddExtractor<TCommand, TArgument, TDefaultArgumentValue>(this ICliBuilder builder) where TCommand : class, ICommandExtractor where TArgument : class, IOptionExtractor where TDefaultArgumentValue : class, IDefaultOptionValueProvider
        {
            // Add command extractor
            builder.Services.AddTransient<ICommandExtractor, TCommand>();

            // Add option extractor
            builder.Services.AddTransient<IOptionExtractor, TArgument>();

            // Add default option value provider
            builder.Services.AddTransient<IDefaultOptionValueProvider, TDefaultArgumentValue>();

            return builder;
        }

        /// <summary>
        /// Adds the <see cref="ICommandExtractor"/>, <see cref="IOptionExtractor"/>,
        /// <see cref="IDefaultOptionProvider"/> and <see cref="IDefaultOptionValueProvider"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <typeparam name="TCommand">The command extractor type.</typeparam>
        /// <typeparam name="TArgument">The option extractor type.</typeparam>
        /// <typeparam name="TDefaultArgument">The default option provider type.</typeparam>
        /// <typeparam name="TDefaultArgumentValue">The default option value provider type.</typeparam>
        /// <returns>The configured <see cref="ICliBuilder"/>.</returns>
        public static ICliBuilder AddExtractor<TCommand, TArgument, TDefaultArgument, TDefaultArgumentValue>(this ICliBuilder builder) where TCommand : class, ICommandExtractor where TArgument : class, IOptionExtractor where TDefaultArgument : class, IDefaultOptionProvider where TDefaultArgumentValue : class, IDefaultOptionValueProvider
        {
            // Add command extractor
            builder.Services.AddTransient<ICommandExtractor, TCommand>();

            // Add option extractor
            builder.Services.AddTransient<IOptionExtractor, TArgument>();

            // Add default option provider
            builder.Services.AddTransient<IDefaultOptionProvider, TDefaultArgument>();

            // Add default option value provider
            builder.Services.AddTransient<IDefaultOptionValueProvider, TDefaultArgumentValue>();

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
        /// Adds the <see cref="IRoutingService"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The configured <see cref="ICliBuilder"/>.</returns>
        public static ICliBuilder AddRoutingService<TRService>(this ICliBuilder builder) where TRService : class, IRoutingService
        {
            // Add command routing service.
            builder.Services.AddTransient<IRoutingService, TRService>();

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

        /// <summary>
        /// Starts a new <see cref="ICommandBuilder"/> definition. Applications must call the
        /// <see cref="ICommandBuilder.Add"/> method to add the <see cref="CommandDescriptor"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="id">The command id.</param>
        /// <param name="name">The command name.</param>
        /// <param name="prefix">The command string prefix.</param>
        /// <param name="description">The command description.</param>
        /// <param name="isGroup"><c>true</c> if the descriptor represents a grouped command; otherwise, <c>false</c>.</param>
        /// <param name="isRoot"><c>true</c> if the descriptor represents a root command; otherwise, <c>false</c>.</param>
        /// <param name="isProtected"><c>true</c> if the descriptor represents a protected command; otherwise, <c>false</c>.</param>
        /// <param name="defaultArgument">The default option.</param>
        /// <typeparam name="TRunner">The command runner type.</typeparam>
        /// <typeparam name="TChecker">The command checker type.</typeparam>
        /// <returns>The configured <see cref="ICliBuilder"/>.</returns>
        /// <returns>The configured <see cref="ICommandBuilder"/>.</returns>
        public static ICommandBuilder DefineCommand<TChecker, TRunner>(this ICliBuilder builder, string id, string name, string prefix, string description, bool isGroup = false, bool isRoot = false, bool isProtected = false, string? defaultArgument = null) where TChecker : ICommandChecker where TRunner : ICommandRunner<CommandRunnerResult>
        {
            return DefineCommand(builder, id, name, prefix, description, typeof(TChecker), typeof(TRunner), isGroup, isRoot, isProtected, defaultArgument);
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

            // Establish command builder Default option not set ?
            ICommandBuilder commandBuilder = builder.DefineCommand(cmdAttr.Id, cmdAttr.Name, cmdAttr.Prefix, cmdAttr.Description, cmdChecker.Checker, cmdRunner.Runner, cmdAttr.IsGroup, cmdAttr.IsRoot, cmdAttr.IsProtected);

            // Optional
            IEnumerable<OptionDescriptorAttribute> argAttrs = declarativeTarget.GetCustomAttributes<OptionDescriptorAttribute>(false);
            IEnumerable<OptionValidationAttribute> argVdls = declarativeTarget.GetCustomAttributes<OptionValidationAttribute>(false);
            IEnumerable<CommandCustomPropertyAttribute> cmdPropAttrs = declarativeTarget.GetCustomAttributes<CommandCustomPropertyAttribute>(false);
            IEnumerable<OptionCustomPropertyAttribute> argPropAttrs = declarativeTarget.GetCustomAttributes<OptionCustomPropertyAttribute>(false);

            // Options Descriptors
            foreach (OptionDescriptorAttribute argAttr in argAttrs)
            {
                IOptionBuilder argumentBuilder;
                if (argAttr.CustomDataType != null)
                {
                    argumentBuilder = commandBuilder.DefineArgument(argAttr.Id, argAttr.CustomDataType, argAttr.Description, argAttr.Alias, argAttr.DefaultValue, argAttr.Required, argAttr.Disabled, argAttr.Obsolete);
                }
                else
                {
                    argumentBuilder = commandBuilder.DefineArgument(argAttr.Id, argAttr.DataType, argAttr.Description, argAttr.Alias, argAttr.DefaultValue, argAttr.Required, argAttr.Disabled, argAttr.Obsolete);
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
                            argumentBuilder.ValidationAttribute(e.ValidationAttribute, e.ValidationArgs);
                        }
                        return true;
                    });
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
                            argumentBuilder.CustomProperty(e.Key, e.Value);
                        }
                        return true;
                    });
                }

                // Add an option descriptor.
                argumentBuilder.Add();
            }

            // Command custom properties
            Dictionary<string, object>? cmdCustomProps = null;
            if (cmdPropAttrs.Any())
            {
                cmdCustomProps = new Dictionary<string, object>();
                cmdPropAttrs.All(e =>
                {
                    commandBuilder.CustomProperty(e.Key, e.Value);
                    return true;
                });
            }

            // Tags
            CommandTagsAttribute tagsAttr = declarativeTarget.GetCustomAttribute<CommandTagsAttribute>(false);
            if (tagsAttr != null)
            {
                commandBuilder.Tags(tagsAttr.Tags);
            }

            return commandBuilder.Add();
        }

        private static ICommandBuilder DefineCommand(this ICliBuilder builder, string id, string name, string prefix, string description, Type checker, Type runner, bool isGroup = false, bool isRoot = false, bool isProtected = false, string? defaultArgument = null)
        {
            if (isRoot && !isGroup)
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The root command must also be a grouped command. command_id={0} command_name={1}", id, name);
            }

            CommandDescriptor cmd = new(id, name, prefix, description, defaultArgument: defaultArgument)
            {
                Checker = checker,
                Runner = runner,
                IsGroup = isGroup,
                IsProtected = isProtected,
                IsRoot = isRoot,
            };

            ICommandBuilder commandBuilder = new CommandBuilder(builder);
            commandBuilder.Services.AddSingleton(cmd);
            return commandBuilder;
        }
    }
}