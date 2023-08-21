/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Shared.Extensions;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Configuration.Options;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Extractors
{
    /// <summary>
    /// The <see cref="CommandRouteParser"/> class parses a command string into a <see cref="Root"/> object.
    /// </summary>
    public class CommandRouteParser : ICommandRouteParser
    {
        private readonly ITextHandler textHandler;
        private readonly CommandDescriptors commandDescriptors;
        private readonly TerminalOptions terminalOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandRouteParser"/> class.
        /// </summary>
        /// <param name="textHandler">The text handler.</param>
        /// <param name="commandDescriptors">The command descriptors.</param>
        /// <param name="terminalOptions">The terminal configuration options.</param>
        public CommandRouteParser(ITextHandler textHandler, CommandDescriptors commandDescriptors, TerminalOptions terminalOptions)
        {
            this.textHandler = textHandler;
            this.commandDescriptors = commandDescriptors;
            this.terminalOptions = terminalOptions;
        }

        /// <summary>
        /// Parses the command string into a <see cref="Root"/> object.
        /// </summary>
        /// <param name="commandRoute"></param>
        /// <returns></returns>
        /// <exception cref="ErrorException"></exception>
        public Task<Root> ParseAsync(CommandRoute commandRoute)
        {
            MatchCollection matches = Regex.Matches(commandRoute.Command.Raw, textHandler.ExtractionRegex(terminalOptions));

            foreach (Match match in matches)
            {
                Debug.WriteLine("Match: " + match.Value);
            }

            if (matches.Count <= 0)
            {
                throw new ErrorException(TerminalErrors.InvalidRequest, "The command string is invalid.");
            }

            Root root = ExtractRoot(matches, commandRoute);

            return Task.FromResult(root);
        }

        private Root ExtractRoot(MatchCollection matches, CommandRoute commandRoute)
        {
            string rootId = matches[0].Value;
            Command command = GetCommandOrThrow(commandRoute, rootId);

            Root root;
            if (command.Descriptor.IsRoot)
            {
                root = new Root(linkedCommand: command);
            }
            else if (command.Descriptor.IsSubCommand)
            {
                // First element is a sub-command, so we create a dummy root for execution.
                SubCommand subCommand = new(command);
                root = Root.Default(subCommand);
            }
            else
            {
                throw new ErrorException(TerminalErrors.InvalidCommand, $"The command is not a root or a sub-command. command={0}", rootId);
            }

            return root;
        }

        private CommandDescriptor GetCommandDescriptorOrThrow(string commandId)
        {
            if (!commandDescriptors.ContainsKey(commandId))
            {
                throw new ErrorException(TerminalErrors.InvalidCommand, $"The command is invalid. command={0}", commandId);
            }

            return commandDescriptors[commandId];
        }

        private Root ThrowIfNotRoot(Command rootCommand)
        {
            if (!rootCommand.Descriptor.IsRoot)
            {
                throw new ErrorException(TerminalErrors.InvalidCommand, $"The command is not a root. command={0}", rootCommand.Id);
            }

            return new Root(linkedCommand: rootCommand);
        }

        private void ThrowIfNotGroup(Command grpCommand)
        {
            if (!grpCommand.Descriptor.IsGroup)
            {
                throw new ErrorException(TerminalErrors.InvalidCommand, $"The command is not a group. command={0}", grpCommand.Id);
            }
        }

        private Group? ExtractNestedGroup(Match match, CommandRoute commandRoute, Root root)
        {
            string groupIds = match.Groups[2].Value.Trim();
            string[] groupArr = groupIds.Split(new[] { terminalOptions.Extractor.Separator }, StringSplitOptions.RemoveEmptyEntries);

            // Make sure each groups are distinct
            if (groupArr.Distinct().Count() != groupArr.Length)
            {
                throw new ErrorException(TerminalErrors.InvalidRequest, "The command string contains groups that are not distinct. command_string={0}", groupIds);
            }

            Group? group = null;
            foreach (string groupId in groupArr)
            {
                Command groupCommand = GetCommandOrThrow(commandRoute, groupId);
                ThrowIfNotGroup(groupCommand);
                Group currentGroup = new(groupCommand);

                if (group == null)
                {
                    root.ChildGroup = currentGroup;
                }
                else
                {
                    group.ChildGroup = currentGroup;
                }

                group = currentGroup;
            }

            // Return the last leaf group.
            return group;
        }

        private Command GetCommandOrThrow(CommandRoute commandRoute, string commandId)
        {
            CommandDescriptor commandDescriptor = GetCommandDescriptorOrThrow(commandId);
            return new Command(commandDescriptor);
        }

        private Task<Command> ExtractCommandAsync(Match match, CommandRoute commandRoute, Root? root, Group? group)
        {
            return Task.Run(() =>
            {
                string commandId = match.Groups[3].Value.Trim();
                Command command = GetCommandOrThrow(commandRoute, commandId);

                if (group != null)
                {
                    if (command.Equals(group.ChildSubCommand))
                    {
                        throw new ErrorException(TerminalErrors.InvalidCommand, "The command does not belong to the group. command={0} group={1}", commandId, group.LinkedCommand.Id);
                    }
                }
                else if (root != null)
                {
                    if (command.Equals(root.ChildSubCommand))
                    {
                        throw new ErrorException(TerminalErrors.InvalidCommand, "The command does not belong to the root. command={0} root={1}", commandId, root.LinkedCommand.Id);
                    }
                }

                ExtractArguments(match, command);
                ExtractOptions(match, command);

                return command;
            });
        }

        private Arguments? ExtractArguments(Match match, Command command)
        {
            string argumentsString = match.Groups[4].Value.Trim();
            string[] argumentValues = argumentsString.Split(new[] { terminalOptions.Extractor.Separator }, StringSplitOptions.RemoveEmptyEntries);

            // Command may not support if arguments
            if (command.Descriptor.ArgumentDescriptors == null)
            {
                if (argumentValues.Length > 0)
                {
                    throw new ErrorException(TerminalErrors.UnsupportedArgument, "The command does not support any arguments. command={0}", command.Id);
                }

                // Nothing to process.
                return null;
            }

            // Arguments are always specified in order
            if (argumentValues.Length > command.Descriptor.ArgumentDescriptors.Count)
            {
                throw new ErrorException(TerminalErrors.UnsupportedArgument, "The command string contains more arguments than the command supports. command_string={0}", argumentsString);
            }

            Arguments arguments = new(textHandler);
            int index = 0;
            foreach (string argumentVal in argumentValues)
            {
                ArgumentDescriptor argumentDescriptor = command.Descriptor.ArgumentDescriptors[index];
                arguments.Add(new Argument(argumentDescriptor, argumentVal));
                index++;
            }

            return arguments;
        }

        private Options? ExtractOptions(Match match, Command command)
        {
            string optionsString = match.Groups[5].Value.Trim();
            string[] optionsKvp = optionsString.Split(new[] { terminalOptions.Extractor.Separator }, StringSplitOptions.RemoveEmptyEntries);

            // Command may not support if options
            if (command.Descriptor.OptionDescriptors == null)
            {
                if (optionsKvp.Length > 0)
                {
                    throw new ErrorException(TerminalErrors.UnsupportedOption, "The command does not support any options. command={0}", command.Id);
                }

                // Nothing to process.
                return null;
            }

            // Options are always specified in order
            if (optionsKvp.Length > command.Descriptor.OptionDescriptors.Count)
            {
                throw new ErrorException(TerminalErrors.UnsupportedArgument, "The command string contains more options than the command supports. command_string={0}", optionsKvp);
            }

            Options options = new(textHandler);
            foreach (string opt in optionsKvp)
            {
                Option option = CreateOption(opt, command.Descriptor);
                options.Add(option);
            }

            return options;
        }

        private Option CreateOption(string optionKvp, CommandDescriptor commandDescriptor)
        {
            string[] keyValue = optionKvp.Split(new[] { terminalOptions.Extractor.OptionValueSeparator }, StringSplitOptions.RemoveEmptyEntries);
            string key = keyValue[0].TrimStart(terminalOptions.Extractor.OptionPrefix, textHandler.Comparison);
            key = key.TrimStart(terminalOptions.Extractor.OptionAliasPrefix, textHandler.Comparison);

            object value = keyValue.Length > 1 ? keyValue[1] : true;

            OptionDescriptor optionDescriptor = commandDescriptor.OptionDescriptors![key];
            return new Option(optionDescriptor, value);
        }
    }
}