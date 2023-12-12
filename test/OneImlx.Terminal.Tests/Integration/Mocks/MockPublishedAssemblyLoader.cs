/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Shared.Attributes.Validation;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Declarative;
using OneImlx.Terminal.Integration;
using OneImlx.Terminal.Mocks;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace OneImlx.Terminals.Integration.Mocks
{
    internal class MockPublishedAssemblyLoader : ITerminalCommandSourceAssemblyLoader<PublishedCommandSourceContext>
    {
        public bool Called { get; private set; }

        public PublishedCommandSourceContext PassedContext { get; private set; } = null!;

        private Assembly CreateDynamicAssembly(string assembly)
        {
            // Create a new assembly name
            AssemblyName assemblyName = new(assembly);
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MockModule");
            CreateDeclarativeType(moduleBuilder, $"{assembly}MockClass1");
            CreateDeclarativeType(moduleBuilder, $"{assembly}MockClass2");
            CreateDeclarativeType(moduleBuilder, $"{assembly}MockClass3");

            // Return the assembly
            return assemblyBuilder;
        }

        private static void CreateDeclarativeType(ModuleBuilder moduleBuilder, string className)
        {
            // Define a public class in the module
            // If this is called multiple times for multiple assemblies then className is unique for each assembly.
            TypeBuilder typeBuilder = moduleBuilder.DefineType(className, TypeAttributes.Public);

            // Implement the IDeclarativeTarget interface
            typeBuilder.AddInterfaceImplementation(typeof(IDeclarativeTarget));

            // Add custom attributes to the class
            // Ensure the commandId is unique across multiple assemblies so we just use className
            AddCustomAttribute(typeBuilder, typeof(CommandOwnersAttribute), new object[] { new string[] { "oid1", "oid2" } });
            AddCustomAttribute(typeBuilder, typeof(CommandDescriptorAttribute), new object[] { className, "name1", "description", CommandType.SubCommand, CommandFlags.None });
            AddCustomAttribute(typeBuilder, typeof(CommandRunnerAttribute), new object[] { typeof(MockCommandRunner) });
            AddCustomAttribute(typeBuilder, typeof(CommandCheckerAttribute), new object[] { typeof(MockCommandChecker) });
            AddCustomAttribute(typeBuilder, typeof(CommandTagsAttribute), new object[] { new string[] { "tag1", "tag2", "tag3" } });
            AddCustomAttribute(typeBuilder, typeof(CommandCustomPropertyAttribute), new object[] { "key1", "value1" });
            AddCustomAttribute(typeBuilder, typeof(CommandCustomPropertyAttribute), new object[] { "key2", "value2" });
            AddCustomAttribute(typeBuilder, typeof(CommandCustomPropertyAttribute), new object[] { "key3", "value3" });
            AddCustomAttribute(typeBuilder, typeof(OptionDescriptorAttribute), new object[] { "opt1", nameof(String), "test opt desc1", OptionFlags.None, null! });
            AddCustomAttribute(typeBuilder, typeof(OptionDescriptorAttribute), new object[] { "opt2", nameof(String), "test opt desc2", OptionFlags.Disabled, "opt2_alias" });
            AddCustomAttribute(typeBuilder, typeof(OptionValidationAttribute), new object[] { "opt2", typeof(RequiredAttribute), Array.Empty<object>() });
            AddCustomAttribute(typeBuilder, typeof(OptionValidationAttribute), new object[] { "opt2", typeof(OneOfAttribute), new object[] { "test1", "test2", "test3" } });
            AddCustomAttribute(typeBuilder, typeof(OptionDescriptorAttribute), new object[] { "opt3", nameof(Double), "test opt desc3", OptionFlags.Required | OptionFlags.Obsolete, null! });
            AddCustomAttribute(typeBuilder, typeof(OptionValidationAttribute), new object[] { "opt3", typeof(RangeAttribute), new object[] { 25.34, 40.56 } });
            AddCustomAttribute(typeBuilder, typeof(ArgumentDescriptorAttribute), new object[] { 1, "arg1", nameof(String), "test arg desc1", ArgumentFlags.None });
            AddCustomAttribute(typeBuilder, typeof(ArgumentDescriptorAttribute), new object[] { 2, "arg2", nameof(String), "test arg desc2", ArgumentFlags.Disabled });
            AddCustomAttribute(typeBuilder, typeof(ArgumentValidationAttribute), new object[] { "arg2", typeof(RequiredAttribute), Array.Empty<object>() });
            AddCustomAttribute(typeBuilder, typeof(ArgumentValidationAttribute), new object[] { "arg2", typeof(OneOfAttribute), new object[] { "test1", "test2", "test3" } });
            AddCustomAttribute(typeBuilder, typeof(ArgumentDescriptorAttribute), new object[] { 3, "arg3", nameof(Double), "test arg desc3", ArgumentFlags.Required | ArgumentFlags.Obsolete });
            AddCustomAttribute(typeBuilder, typeof(ArgumentValidationAttribute), new object[] { "arg3", typeof(RangeAttribute), new object[] { 25.34, 40.56 } });

            // Create the type
            Type mockDeclarativeTarget1Type = typeBuilder.CreateType();
        }

        private static void AddCustomAttribute(TypeBuilder typeBuilder, Type attributeType, params object[] constructorArgs)
        {
            // Get the single constructor for the attribute type
            ConstructorInfo constructor = attributeType.GetConstructors().Single();

            // Create the custom attribute builder with the provided arguments
            CustomAttributeBuilder customAttributeBuilder = new(constructor, constructorArgs);

            // Apply the custom attribute to the type
            typeBuilder.SetCustomAttribute(customAttributeBuilder);
        }

        public Task<IEnumerable<Assembly>> LoadAssembliesAsync(PublishedCommandSourceContext context)
        {
            Called = true;
            PassedContext = context;

            List<Assembly> assemblies = new List<Assembly>();

            foreach (var kvp in context.PublishedAssemblies)
            {
                string assemblyName = Path.GetFileNameWithoutExtension(kvp.Key);
                assemblies.Add(CreateDynamicAssembly(assemblyName));
            }

            return Task.FromResult(assemblies.AsEnumerable());
        }
    }
}