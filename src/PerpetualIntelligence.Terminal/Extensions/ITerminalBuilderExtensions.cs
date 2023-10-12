/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PerpetualIntelligence.Terminal.Commands;
using PerpetualIntelligence.Terminal.Commands.Checkers;
using PerpetualIntelligence.Terminal.Commands.Declarative;
using PerpetualIntelligence.Terminal.Commands.Extractors;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Commands.Mappers;
using PerpetualIntelligence.Terminal.Commands.Providers;
using PerpetualIntelligence.Terminal.Commands.Routers;
using PerpetualIntelligence.Terminal.Commands.Runners;
using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Terminal.Events;
using PerpetualIntelligence.Terminal.Hosting;
using PerpetualIntelligence.Terminal.Licensing;
using PerpetualIntelligence.Terminal.Runtime;
using PerpetualIntelligence.Terminal.Stores;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace PerpetualIntelligence.Terminal.Extensions
{
    /// <summary>
    /// The <see cref="ITerminalBuilder"/> extension methods.
    /// </summary>
    public static class ITerminalBuilderExtensions
    {
        /// <summary>
        /// Adds the <see cref="IDataTypeMapper{TValur}"/> and <see cref="IOptionChecker"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <typeparam name="TMapper">The option mapper type.</typeparam>
        /// <typeparam name="TChecker">The option checker type.</typeparam>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        public static ITerminalBuilder AddOptionChecker<TMapper, TChecker>(this ITerminalBuilder builder) where TMapper : class, IDataTypeMapper<Option> where TChecker : class, IOptionChecker
        {
            builder.Services.AddTransient<IDataTypeMapper<Option>, TMapper>();
            builder.Services.AddTransient<IOptionChecker, TChecker>();
            return builder;
        }

        /// <summary>
        /// Adds the <see cref="IDataTypeMapper{TValur}"/> and <see cref="IArgumentChecker"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <typeparam name="TMapper">The argument mapper type.</typeparam>
        /// <typeparam name="TChecker">The argument checker type.</typeparam>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        public static ITerminalBuilder AddArgumentChecker<TMapper, TChecker>(this ITerminalBuilder builder) where TMapper : class, IDataTypeMapper<Argument> where TChecker : class, IArgumentChecker
        {
            builder.Services.AddTransient<IDataTypeMapper<Argument>, TMapper>();
            builder.Services.AddTransient<IArgumentChecker, TChecker>();
            return builder;
        }

        /// <summary>
        /// Adds the <see cref="IAsyncEventHandler"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        public static ITerminalBuilder AddEventHandler<TEventHandler>(this ITerminalBuilder builder) where TEventHandler : class, IAsyncEventHandler
        {
            builder.Services.AddTransient<IAsyncEventHandler, TEventHandler>();
            return builder;
        }

        /// <summary>
        /// Adds the <see cref="TerminalOptions"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        public static ITerminalBuilder AddTerminalOptions(this ITerminalBuilder builder)
        {
            // Add options.
            builder.Services.AddOptions();
            builder.Services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<TerminalOptions>>().Value);

            // Add options checker
            builder.Services.AddSingleton<IConfigurationOptionsChecker, ConfigurationOptionsChecker>();

            return builder;
        }

        /// <summary>
        /// Adds the <see cref="TerminalStartContext"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="terminalStartContext">The terminal start context.</param>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        public static ITerminalBuilder AddStartContext(this ITerminalBuilder builder, TerminalStartContext terminalStartContext)
        {
            builder.Services.AddSingleton(terminalStartContext);
            return builder;
        }

        /// <summary>
        /// Adds the command <see cref="IHelpProvider"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        public static ITerminalBuilder AddHelpProvider<THelpProvider>(this ITerminalBuilder builder) where THelpProvider : class, IHelpProvider
        {
            builder.Services.AddSingleton<IHelpProvider, THelpProvider>();
            return builder;
        }

        /// <summary>
        /// Adds all the <see cref="IDeclarativeTarget"/> implementations to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="assemblyType">The type whose assembly to inspect and read all the declarative targets.</param>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        /// <remarks>
        /// The <see cref="AddDeclarativeAssembly(ITerminalBuilder, Type)"/> reads the target assembly and inspects all the
        /// declarative targets using reflection. Reflection may have a performance bottleneck. For more optimized and
        /// direct declarative target inspection, use <see cref="AddDeclarativeTarget(ITerminalBuilder, Type)"/>.
        /// </remarks>
        public static ITerminalBuilder AddDeclarativeAssembly(this ITerminalBuilder builder, Type assemblyType)
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
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        /// <remarks>
        /// The <see cref="AddDeclarativeAssembly(ITerminalBuilder, Type)"/> reads the target assembly and inspects all the
        /// declarative targets using reflection. Reflection may have a performance bottleneck. For more optimized and
        /// direct declarative target inspection, use <see cref="AddDeclarativeTarget(ITerminalBuilder, Type)"/>.
        /// </remarks>
        public static ITerminalBuilder AddDeclarativeAssembly<TType>(this ITerminalBuilder builder)
        {
            return AddDeclarativeAssembly(builder, typeof(TType));
        }

        /// <summary>
        /// Adds a <see cref="IDeclarativeTarget"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>
        /// The <see cref="AddDeclarativeTarget{TDeclarativeTarget}(ITerminalBuilder)"/> inspects the declarative target type
        /// using reflection.
        /// </returns>
        public static ITerminalBuilder AddDeclarativeTarget<TDeclarativeTarget>(this ITerminalBuilder builder) where TDeclarativeTarget : IDeclarativeTarget
        {
            return AddDeclarativeTarget(builder, typeof(TDeclarativeTarget));
        }

        /// <summary>
        /// Adds the <see cref="IExceptionHandler"/> to the service collection.
        /// </summary>
        /// <typeparam name="THandler">The <see cref="IExceptionHandler"/> type.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        public static ITerminalBuilder AddExceptionHandler<THandler>(this ITerminalBuilder builder) where THandler : class, IExceptionHandler
        {
            // Add exception publisher
            builder.Services.AddTransient<IExceptionHandler, THandler>();

            return builder;
        }

        /// <summary>
        /// Adds the <see cref="ICommandExtractor"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <typeparam name="TCommand">The command extractor type.</typeparam>
        /// <typeparam name="TParser">The command route parser type.</typeparam>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        public static ITerminalBuilder AddCommandExtractor<TCommand, TParser>(this ITerminalBuilder builder) where TCommand : class, ICommandExtractor where TParser : class, ICommandRouteParser
        {
            // Add command extractor
            builder.Services.AddTransient<ICommandExtractor, TCommand>();

            // Add option extractor
            builder.Services.AddTransient<ICommandRouteParser, TParser>();

            return builder;
        }

        /// <summary>
        /// Adds  license handler to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        public static ITerminalBuilder AddLicenseHandler(this ITerminalBuilder builder)
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
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        public static ITerminalBuilder AddCommandRouter<TRouter, THandler>(this ITerminalBuilder builder) where TRouter : class, ICommandRouter where THandler : class, ICommandHandler
        {
            // Add command router
            builder.Services.AddTransient<ICommandRouter, TRouter>();

            // Add command handler
            builder.Services.AddTransient<ICommandHandler, THandler>();

            return builder;
        }

        /// <summary>
        /// Adds the <see cref="ITerminalConsole"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        public static ITerminalBuilder AddTerminalConsole<TConsole>(this ITerminalBuilder builder) where TConsole : class, ITerminalConsole
        {
            // Add terminal routing service.
            builder.Services.AddSingleton<ITerminalConsole, TConsole>();

            return builder;
        }

        /// <summary>
        /// Adds the <see cref="ITerminalRouting{TContext, TResult}"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        public static ITerminalBuilder AddTerminalRouting<TRouting, TContext, TResult>(this ITerminalBuilder builder) where TRouting : class, ITerminalRouting<TContext, TResult> where TContext : TerminalRoutingContext where TResult : TerminalRoutingResult
        {
            // Add terminal routing service.
            builder.Services.AddSingleton<ITerminalRouting<TContext, TResult>, TRouting>();

            return builder;
        }

        /// <summary>
        /// Adds the <see cref="ICommandStoreHandler"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <typeparam name="TStore">The command descriptor store type.</typeparam>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        public static ITerminalBuilder AddStoreHandler<TStore>(this ITerminalBuilder builder) where TStore : class, ICommandStoreHandler
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
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        public static ITerminalBuilder AddTextHandler<TTextHandler>(this ITerminalBuilder builder) where TTextHandler : class, ITextHandler
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
        /// <param name="description">The command description.</param>
        /// <param name="commandType">The command type.</param>
        /// <param name="commandFlags">The command flags.</param>
        /// <typeparam name="TRunner">The command runner type.</typeparam>
        /// <typeparam name="TChecker">The command checker type.</typeparam>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        /// <returns>The configured <see cref="ICommandBuilder"/>.</returns>
        public static ICommandBuilder DefineCommand<TChecker, TRunner>(this ITerminalBuilder builder, string id, string name, string description, CommandType commandType, CommandFlags commandFlags) where TChecker : ICommandChecker where TRunner : ICommandRunner<CommandRunnerResult>
        {
            return DefineCommand(builder, id, name, description, typeof(TChecker), typeof(TRunner), commandType, commandFlags);
        }

        private static ITerminalBuilder AddDeclarativeTarget(this ITerminalBuilder builder, Type declarativeTarget)
        {
            // Command descriptor
            CommandDescriptorAttribute cmdAttr = declarativeTarget.GetCustomAttribute<CommandDescriptorAttribute>(false) ?? throw new TerminalException(TerminalErrors.InvalidDeclaration, "The declarative target does not define command descriptor.");

            // Command Runner
            CommandRunnerAttribute cmdRunner = declarativeTarget.GetCustomAttribute<CommandRunnerAttribute>(false) ?? throw new TerminalException(TerminalErrors.InvalidDeclaration, "The declarative target does not define command runner.");

            // Command checker
            CommandCheckerAttribute cmdChecker = declarativeTarget.GetCustomAttribute<CommandCheckerAttribute>(false) ?? throw new TerminalException(TerminalErrors.InvalidDeclaration, "The declarative target does not define command checker.");

            // Establish command builder Default option not set ?
            ICommandBuilder commandBuilder = builder.DefineCommand(cmdAttr.Id, cmdAttr.Name, cmdAttr.Description, cmdChecker.Checker, cmdRunner.Runner, cmdAttr.CommandType, cmdAttr.CommandFlags);

            // Optional
            IEnumerable<OptionDescriptorAttribute> optAttrs = declarativeTarget.GetCustomAttributes<OptionDescriptorAttribute>(false);
            IEnumerable<ArgumentDescriptorAttribute> argAttrs = declarativeTarget.GetCustomAttributes<ArgumentDescriptorAttribute>(false);
            IEnumerable<OptionValidationAttribute> optVdls = declarativeTarget.GetCustomAttributes<OptionValidationAttribute>(false);
            IEnumerable<CommandCustomPropertyAttribute> cmdPropAttrs = declarativeTarget.GetCustomAttributes<CommandCustomPropertyAttribute>(false);

            // Arguments Descriptors
            foreach (ArgumentDescriptorAttribute argAttr in argAttrs)
            {
                commandBuilder.DefineArgument(argAttr.Order, argAttr.Id, argAttr.DataType, argAttr.Description, argAttr.Flags);
            }

            // Options Descriptors
            foreach (OptionDescriptorAttribute optAttr in optAttrs)
            {
                IOptionBuilder optBuilder = commandBuilder.DefineOption(optAttr.Id, optAttr.DataType, optAttr.Description, optAttr.Flags, optAttr.Alias);

                // Option validation attribute
                List<ValidationAttribute>? validationAttributes = null;
                if (optVdls.Any())
                {
                    validationAttributes = new List<ValidationAttribute>();
                    optVdls.All(e =>
                    {
                        if (e.OptionId.Equals(optAttr.Id))
                        {
                            optBuilder.ValidationAttribute(e.ValidationAttribute, e.ValidationArgs);
                        }
                        return true;
                    });
                }

                // Add an option descriptor.
                optBuilder.Add();
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

            // Command owners
            // Command owners
            CommandOwnersAttribute ownersAttr = declarativeTarget.GetCustomAttribute<CommandOwnersAttribute>(false);
            if (ownersAttr != null)
            {
                commandBuilder.Owners(ownersAttr.Owners);
            }
            else if (cmdAttr.CommandType != CommandType.Root)
            {
                throw new TerminalException(TerminalErrors.InvalidDeclaration, "The declarative target does not define command owner.");
            }

            return commandBuilder.Add();
        }

        private static ICommandBuilder DefineCommand(this ITerminalBuilder builder, string id, string name, string description, Type checker, Type runner, CommandType commandType, CommandFlags commandFlags)
        {
            CommandDescriptor cmd = new(id, name, description, commandType, commandFlags)
            {
                Checker = checker,
                Runner = runner,
            };

            ICommandBuilder commandBuilder = new CommandBuilder(builder);
            commandBuilder.Services.AddSingleton(cmd);
            return commandBuilder;
        }
    }
}