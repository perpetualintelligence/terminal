/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Declarative;
using OneImlx.Terminal.Commands.Handlers;
using OneImlx.Terminal.Commands.Mappers;
using OneImlx.Terminal.Commands.Parsers;
using OneImlx.Terminal.Commands.Routers;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Events;
using OneImlx.Terminal.Hosting;
using OneImlx.Terminal.Integration;
using OneImlx.Terminal.Licensing;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Stores;

namespace OneImlx.Terminal.Extensions
{
    /// <summary>
    /// The <see cref="ITerminalBuilder"/> extension methods.
    /// </summary>
    public static class ITerminalBuilderExtensions
    {
        /// <summary>
        /// Adds the <see cref="IDataTypeMapper{TValue}"/> and <see cref="IArgumentChecker"/> to the service collection.
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
        /// Adds the <see cref="ICommandParser"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <typeparam name="TCommand">The command parser type.</typeparam>
        /// <typeparam name="TParser">The command route parser type.</typeparam>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        public static ITerminalBuilder AddCommandParser<TCommand, TParser>(this ITerminalBuilder builder) where TCommand : class, ICommandParser where TParser : class, ICommandRouteParser
        {
            // Add command parser
            builder.Services.AddTransient<ICommandParser, TCommand>();

            // Add option parser
            builder.Services.AddTransient<ICommandRouteParser, TParser>();

            return builder;
        }

        /// <summary>
        /// Adds the <see cref="ICommandRouter"/> and <see cref="ICommandHandler"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        public static ITerminalBuilder AddCommandRouter<TRouter, THandler, TRuntime>(this ITerminalBuilder builder) where TRouter : class, ICommandRouter where THandler : class, ICommandHandler where TRuntime : class, ICommandRuntime
        {
            // Add command router
            builder.Services.AddTransient<ICommandRouter, TRouter>();

            // Add command handler
            builder.Services.AddTransient<ICommandHandler, THandler>();

            // Add command runtime
            builder.Services.AddTransient<ICommandRuntime, TRuntime>();

            return builder;
        }

        /// <summary>
        /// Adds the <see cref="ITerminalCommandStore"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <typeparam name="TStore">The command descriptor store type.</typeparam>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        public static ITerminalBuilder AddCommandStore<TStore>(this ITerminalBuilder builder)
            where TStore : class, ITerminalCommandStore
        {
            builder.Services.AddSingleton<ITerminalCommandStore, TStore>();

            return builder;
        }

        /// <summary>
        /// Adds the <see cref="TerminalOptions"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        public static ITerminalBuilder AddConfigurationOptions(this ITerminalBuilder builder)
        {
            // Add options.
            builder.Services.AddOptions();
            builder.Services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<TerminalOptions>>().Value);

            // Add options checker
            builder.Services.AddSingleton<IConfigurationOptionsChecker, ConfigurationOptionsChecker>();

            return builder;
        }

        /// <summary>
        /// Adds the <see cref="ITerminalConsole"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        public static ITerminalBuilder AddConsole<TConsole>(this ITerminalBuilder builder) where TConsole : class, ITerminalConsole
        {
            // Add terminal routing service.
            builder.Services.AddSingleton<ITerminalConsole, TConsole>();

            return builder;
        }

        /// <summary>
        /// Adds all the <see cref="IDeclarativeRunner"/> implementations to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="assemblyType">The type whose assembly to inspect and read all the declarative targets.</param>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        /// <remarks>
        /// The <see cref="AddDeclarativeAssembly(ITerminalBuilder, Type)"/> reads the target assembly and inspects all
        /// the declarative targets using reflection. Reflection may have a performance bottleneck. For more optimized
        /// and direct declarative target inspection, use <see cref="AddDeclarativeRunner{TDeclarativeRunner}(ITerminalBuilder)"/>.
        /// </remarks>
        public static ITerminalBuilder AddDeclarativeAssembly(this ITerminalBuilder builder, Type assemblyType)
        {
            return AddDeclarativeAssembly(builder, assemblyType.Assembly);
        }

        /// <summary>
        /// Adds all the <see cref="IDeclarativeRunner"/> implementations to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="assembly">The assembly to inspect.</param>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        /// <remarks>
        /// The <see cref="AddDeclarativeAssembly(ITerminalBuilder, Assembly)"/> reads the target assembly and inspects
        /// all the declarative targets using reflection. Reflection may have a performance bottleneck. For more
        /// optimized and direct declarative target inspection, use <see cref="AddDeclarativeRunner{TDeclarativeRunner}(ITerminalBuilder)"/>.
        /// </remarks>
        public static ITerminalBuilder AddDeclarativeAssembly(this ITerminalBuilder builder, Assembly assembly)
        {
            IEnumerable<Type> declarativeTypes = assembly.GetTypes()
                .Where(e => typeof(IDeclarativeRunner).IsAssignableFrom(e));

            foreach (Type type in declarativeTypes)
            {
                AddDeclarativeRunnerInner(builder, type);
            }

            return builder;
        }

        /// <summary>
        /// Adds all the <see cref="IDeclarativeRunner"/> implementations to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <typeparam name="TType">The type whose assembly to inspect and read all the declarative targets.</typeparam>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        /// <remarks>
        /// The <see cref="AddDeclarativeAssembly(ITerminalBuilder, Type)"/> reads the target assembly and inspects all
        /// the declarative targets using reflection. Reflection may have a performance bottleneck. For more optimized
        /// and direct declarative target inspection, use <see cref="AddDeclarativeRunner{TDeclarativeRunner}(ITerminalBuilder)"/>.
        /// </remarks>
        public static ITerminalBuilder AddDeclarativeAssembly<TType>(this ITerminalBuilder builder)
        {
            return AddDeclarativeAssembly(builder, typeof(TType));
        }

        /// <summary>
        /// Adds a <see cref="IDeclarativeRunner"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>
        /// The <see cref="AddDeclarativeRunner{TDeclarativeRunner}(ITerminalBuilder)"/> inspects the declarative target
        /// type using reflection.
        /// </returns>
        public static ITerminalBuilder AddDeclarativeRunner<TDeclarativeRunner>(this ITerminalBuilder builder) where TDeclarativeRunner : IDeclarativeRunner
        {
            return AddDeclarativeRunnerInner(builder, typeof(TDeclarativeRunner));
        }

        /// <summary>
        /// Adds the <see cref="ITerminalEventHandler"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        public static ITerminalBuilder AddEventHandler<TEventHandler>(this ITerminalBuilder builder) where TEventHandler : class, ITerminalEventHandler
        {
            builder.Services.AddSingleton<ITerminalEventHandler, TEventHandler>();
            return builder;
        }

        /// <summary>
        /// Adds the <see cref="ITerminalExceptionHandler"/> to the service collection.
        /// </summary>
        /// <typeparam name="THandler">The <see cref="ITerminalExceptionHandler"/> type.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        public static ITerminalBuilder AddExceptionHandler<THandler>(this ITerminalBuilder builder) where THandler : class, ITerminalExceptionHandler
        {
            // Add exception publisher
            builder.Services.AddTransient<ITerminalExceptionHandler, THandler>();

            return builder;
        }

        /// <summary>
        /// Adds the command <see cref="ITerminalHelpProvider"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        public static ITerminalBuilder AddHelpProvider<THelpProvider>(this ITerminalBuilder builder) where THelpProvider : class, ITerminalHelpProvider
        {
            builder.Services.AddSingleton<ITerminalHelpProvider, THelpProvider>();
            return builder;
        }

        /// <summary>
        /// Adds integration services to the terminal builder for loading commands from an external source. This method
        /// registers the specified command source, command source checker, and command source assembly loader as
        /// singletons in the service collection.
        /// </summary>
        /// <typeparam name="TContext">
        /// The type of the context used in the command source, checker, and loader. Must be a class.
        /// </typeparam>
        /// <typeparam name="TSource">The type of the terminal command source. Must be a class implementing <see cref="ITerminalCommandSource{TContext}"/>.</typeparam>
        /// <typeparam name="TChecker">
        /// The type of the terminal command source checker. Must be a class implementing <see cref="ITerminalCommandSourceChecker{TContext}"/>.
        /// </typeparam>
        /// <typeparam name="TLoader">
        /// The type of the terminal command source assembly loader. Must be a class implementing <see cref="ITerminalCommandSourceAssemblyLoader{TContext}"/>.
        /// </typeparam>
        /// <param name="builder">The terminal builder to which the integration services are added.</param>
        /// <returns>The <see cref="ITerminalBuilder"/> with the added integration services, enabling method chaining.</returns>
        public static ITerminalBuilder AddIntegration<TContext, TSource, TChecker, TLoader>(this ITerminalBuilder builder)
            where TContext : class
            where TSource : class, ITerminalCommandSource<TContext>
            where TChecker : class, ITerminalCommandSourceChecker<TContext>
            where TLoader : class, ITerminalCommandSourceAssemblyLoader<TContext>
        {
            builder.Services.AddSingleton<ITerminalCommandSource<TContext>, TSource>();
            builder.Services.AddSingleton<ITerminalCommandSourceChecker<TContext>, TChecker>();
            builder.Services.AddSingleton<ITerminalCommandSourceAssemblyLoader<TContext>, TLoader>();
            return builder;
        }

        /// <summary>
        /// Adds terminal license handler to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        public static ITerminalBuilder AddLicensing(this ITerminalBuilder builder)
        {
            // Add license debugger.
            builder.Services.AddSingleton<ILicenseDebugger, LicenseDebugger>();

            // Add license extractor as singleton
            builder.Services.AddSingleton<ILicenseExtractor, LicenseExtractor>();

            // Add license checker as singleton
            builder.Services.AddSingleton<ILicenseChecker, LicenseChecker>();

            return builder;
        }

        /// <summary>
        /// Adds the <see cref="IDataTypeMapper{TValue}"/> and <see cref="IOptionChecker"/> to the service collection.
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
        /// Adds the <see cref="ITerminalProcessor"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <typeparam name="TTerminalProcessor">The terminal processor type.</typeparam>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        public static ITerminalBuilder AddProcessor<TTerminalProcessor>(this ITerminalBuilder builder)
            where TTerminalProcessor : class, ITerminalProcessor
        {
            builder.Services.AddSingleton<ITerminalProcessor, TTerminalProcessor>();
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
        /// Adds the <see cref="ITerminalRouter{TContext}"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        public static ITerminalBuilder AddTerminalRouter<TRouter, TContext>(this ITerminalBuilder builder) where TRouter : class, ITerminalRouter<TContext> where TContext : TerminalRouterContext
        {
            // Add terminal routing service.
            builder.Services.TryAddSingleton<ITerminalRouter<TContext>, TRouter>();

            return builder;
        }

        /// <summary>
        /// Adds the <see cref="ITerminalTextHandler"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="textHandler">The text handler.</param>
        /// <typeparam name="TTextHandler">The text handler.</typeparam>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        /// <remarks>
        /// <para>
        /// <see cref="AddTextHandler{TTextHandler}(ITerminalBuilder, TTextHandler)"/> requires an instance of
        /// <typeparamref name="TTextHandler"/> instead of just its type because the terminal application is expected to
        /// operate with a single, consistent instance of <see cref="ITerminalTextHandler"/> throughout its lifetime. By
        /// passing an instance, it allows the terminal to maintain state or configuration specific to that instance,
        /// ensuring consistent text handling behavior across different parts of the application.
        /// </para>
        /// <para>
        /// This approach also facilitates more flexible initialization patterns, where the
        /// <see cref="ITerminalTextHandler"/> can be configured or initialized outside of the dependency injection
        /// container before being registered. This can be particularly useful when the text handler requires complex
        /// setup or depends on settings or services that aren't readily available within the DI context.
        /// </para>
        /// </remarks>
        public static ITerminalBuilder AddTextHandler<TTextHandler>(this ITerminalBuilder builder, TTextHandler textHandler) where TTextHandler : class, ITerminalTextHandler
        {
            builder.Services.AddSingleton<ITerminalTextHandler>(textHandler);
            return builder;
        }

        /// <summary>
        /// Starts a new <see cref="ICommandBuilder"/> definition with the default <see cref="CommandChecker"/>.
        /// Applications must call the <see cref="ICommandBuilder.Add"/> method to add the
        /// <see cref="CommandDescriptor"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="id">The command id.</param>
        /// <param name="name">The command name.</param>
        /// <param name="description">The command description.</param>
        /// <param name="commandType">The command type.</param>
        /// <param name="commandFlags">The command flags.</param>
        /// <typeparam name="TRunner">The command runner type.</typeparam>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        /// <returns>The configured <see cref="ICommandBuilder"/>.</returns>
        public static ICommandBuilder DefineCommand<TRunner>(this ITerminalBuilder builder, string id, string name, string description, CommandType commandType, CommandFlags commandFlags) where TRunner : ICommandRunner<CommandRunnerResult>
        {
            return DefineCommand(builder, id, name, description, typeof(CommandChecker), typeof(TRunner), commandType, commandFlags);
        }

        /// <exclude/>
        private static ITerminalBuilder AddDeclarativeRunnerInner(this ITerminalBuilder builder, Type declarativeRunner)
        {
            // Command descriptor The declarative runner is the command runner.
            CommandDescriptorAttribute cmdAttr = declarativeRunner.GetCustomAttribute<CommandDescriptorAttribute>(false) ?? throw new TerminalException(TerminalErrors.InvalidDeclaration, "The declarative target does not define command descriptor.");

            // Command checker, defaults to CommandChecker if not defined.
            Type checkerType = typeof(CommandChecker);
            CommandCheckerAttribute? cmdChecker = declarativeRunner.GetCustomAttribute<CommandCheckerAttribute>(false);
            if (cmdChecker != null)
            {
                checkerType = cmdChecker.Checker;
            }

            // Establish command builder Default option not set ?
            ICommandBuilder commandBuilder = builder.DefineCommand(cmdAttr.Id, cmdAttr.Name, cmdAttr.Description, checkerType, declarativeRunner, cmdAttr.CommandType, cmdAttr.CommandFlags);

            // Arguments Descriptors
            IEnumerable<ArgumentDescriptorAttribute> argAttrs = declarativeRunner.GetCustomAttributes<ArgumentDescriptorAttribute>(false);
            IEnumerable<ArgumentValidationAttribute> argVdls = declarativeRunner.GetCustomAttributes<ArgumentValidationAttribute>(false);
            foreach (ArgumentDescriptorAttribute argAttr in argAttrs)
            {
                IArgumentBuilder argBuilder = commandBuilder.DefineArgument(argAttr.Order, argAttr.Id, argAttr.DataType, argAttr.Description, argAttr.Flags);

                // Argument validation attribute
                List<ValidationAttribute>? validationAttributes = null;
                if (argVdls.Any())
                {
                    validationAttributes = [];
                    argVdls.All(e =>
                    {
                        if (e.ArgumentId.Equals(argAttr.Id))
                        {
                            argBuilder.ValidationAttribute(e.ValidationAttribute, e.ValidationParams);
                        }
                        return true;
                    });
                }

                argBuilder.Add();
            }

            // Options Descriptors
            IEnumerable<OptionDescriptorAttribute> optAttrs = declarativeRunner.GetCustomAttributes<OptionDescriptorAttribute>(false);
            IEnumerable<OptionValidationAttribute> optVdls = declarativeRunner.GetCustomAttributes<OptionValidationAttribute>(false);
            foreach (OptionDescriptorAttribute optAttr in optAttrs)
            {
                IOptionBuilder optBuilder = commandBuilder.DefineOption(optAttr.Id, optAttr.DataType, optAttr.Description, optAttr.Flags, optAttr.Alias);

                // Option validation attribute
                List<ValidationAttribute>? validationAttributes = null;
                if (optVdls.Any())
                {
                    validationAttributes = [];
                    optVdls.All(e =>
                    {
                        if (e.OptionId.Equals(optAttr.Id))
                        {
                            optBuilder.ValidationAttribute(e.ValidationAttribute, e.ValidationParams);
                        }
                        return true;
                    });
                }

                // Add an option descriptor.
                optBuilder.Add();
            }

            // Command custom properties
            IEnumerable<CommandCustomPropertyAttribute> cmdPropAttrs = declarativeRunner.GetCustomAttributes<CommandCustomPropertyAttribute>(false);
            Dictionary<string, object>? cmdCustomProps = null;
            if (cmdPropAttrs.Any())
            {
                cmdCustomProps = [];
                cmdPropAttrs.All(e =>
                {
                    commandBuilder.CustomProperty(e.Key, e.Value);
                    return true;
                });
            }

            // Tags
            CommandTagsAttribute? tagsAttr = declarativeRunner.GetCustomAttribute<CommandTagsAttribute>(false);
            if (tagsAttr != null)
            {
                commandBuilder.Tags(tagsAttr.Tags);
            }

            // Command owners
            CommandOwnersAttribute? ownersAttr = declarativeRunner.GetCustomAttribute<CommandOwnersAttribute>(false);
            if (ownersAttr != null)
            {
                commandBuilder.Owners(ownersAttr.Owners);
            }
            else if (cmdAttr.CommandType != CommandType.Root)
            {
                throw new TerminalException(TerminalErrors.InvalidDeclaration, "The declarative target does not define command owner.");
            }

            // Build and add the command descriptor to service collection
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
