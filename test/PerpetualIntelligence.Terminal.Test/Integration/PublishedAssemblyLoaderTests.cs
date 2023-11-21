/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace PerpetualIntelligence.Terminal.Integration
{
    public class PublishedAssemblyLoaderTests
    {
        private readonly Mock<ILogger<PublishedAssemblyLoader>> mockLogger;
        private readonly PublishedAssemblyLoader assemblyLoader;
        private readonly PublishedCommandSourceContext commandSourceContext;
        private string testBaseDir;

        public PublishedAssemblyLoaderTests()
        {
            commandSourceContext = new PublishedCommandSourceContext();
            mockLogger = new Mock<ILogger<PublishedAssemblyLoader>>();
            assemblyLoader = new PublishedAssemblyLoader(mockLogger.Object);
            testBaseDir = AppDomain.CurrentDomain.SetupInformation.ApplicationBase!;
        }

        [Fact]
        public async Task LoadAssembly_WithValidAssemblyPath_ShouldLoadAssembly()
        {
            commandSourceContext.PublishedAssemblies.Add("PerpetualIntelligence.Shared.dll", testBaseDir);
            var loadedAssemblies = await assemblyLoader.LoadAssembliesAsync(commandSourceContext);
            loadedAssemblies.Should().NotBeNull();
            loadedAssemblies.Single().GetName().Name.Should().Be("PerpetualIntelligence.Shared");
        }

        [Fact]
        public async Task LoadAssembly_WithAlreadyLoadedAssemblyPath_ShouldReturnSameAssemblyAsync()
        {
            commandSourceContext.PublishedAssemblies.Add("FluentAssertions.dll", testBaseDir);
            commandSourceContext.PublishedAssemblies.Add("PerpetualIntelligence.Shared.dll", testBaseDir);
            var loadedAssemblies = await assemblyLoader.LoadAssembliesAsync(commandSourceContext);
            loadedAssemblies.Should().NotBeNull();
            loadedAssemblies.First().GetName().Name.Should().Be("FluentAssertions");
            loadedAssemblies.Last().GetName().Name.Should().Be("PerpetualIntelligence.Shared");
        }

        [Fact]
        public void LoadAssembly_WithInvalidAssemblyPath_ShouldThrowException()
        {
            commandSourceContext.PublishedAssemblies.Add("InvalidAssembly.dll", testBaseDir);
            Func<Task> act = () => assemblyLoader.LoadAssembliesAsync(commandSourceContext);
            act.Should().ThrowAsync<FileNotFoundException>();
        }

        [Fact]
        public async Task LoadAssembly_WithAssemblyHavingDependencies_ShouldLoadDependentAssembliesAsync()
        {
            // The structure is:
            // test/
            //   Terminal.Tests/
            //     bin/Debug/ or bin/Release/
            //   Terminal.DependentAssembly/
            //     bin/Debug/ or bin/Release/

            // Determine the current configuration (Debug/Release)
            var configuration = Environment.GetEnvironmentVariable("CONFIGURATION") ?? "Debug";
            var version = "net7.0";

            // Define the relative path to Terminal.DependentAssembly.dll from the unit test binary output directory
            var relativePathToDependentAssembly = Path.Combine("..", "..", "..", "..", "Terminal.DependentAssembly", "bin", configuration, version, "Terminal.DependentAssembly.dll");

            // Get the full path for Terminal.DependentAssembly.dll
            var dependentAssemblyPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), relativePathToDependentAssembly));

            // Ensure the assembly file exists at the source path
            if (!File.Exists(dependentAssemblyPath))
            {
                throw new FileNotFoundException($"The dependent assembly was not found at '{dependentAssemblyPath}'");
            }

            // Make sure the assembly is not loaded
            var assembliesBeforeLoad = AppDomain.CurrentDomain.GetAssemblies();
            Assembly? sharedAlreadyLoaded = assembliesBeforeLoad.FirstOrDefault(e => e.GetName().Name!.Equals("PerpetualIntelligence.Shared"));
            Assembly? externalNotLoaded = assembliesBeforeLoad.FirstOrDefault(e => e.GetName().Name!.Equals("Terminal.DependentAssembly"));
            sharedAlreadyLoaded.Should().NotBeNull();
            externalNotLoaded.Should().BeNull();

            commandSourceContext.PublishedAssemblies.Add("PerpetualIntelligence.Shared.dll", testBaseDir);
            commandSourceContext.PublishedAssemblies.Add("Terminal.DependentAssembly", dependentAssemblyPath);

            var loadedAssemblies = await assemblyLoader.LoadAssembliesAsync(commandSourceContext);
            loadedAssemblies.Should().NotBeNull();
            loadedAssemblies.First().GetName().Name.Should().Be("PerpetualIntelligence.Shared");
            loadedAssemblies.Last().GetName().Name.Should().Be("Terminal.DependentAssembly");

            // Assert both assemblies stay loaded
            var assembliesAfterLoad = AppDomain.CurrentDomain.GetAssemblies();
            sharedAlreadyLoaded = assembliesAfterLoad.FirstOrDefault(e => e.GetName().Name!.Equals("PerpetualIntelligence.Shared"));
            externalNotLoaded = assembliesAfterLoad.FirstOrDefault(e => e.GetName().Name!.Equals("Terminal.DependentAssembly"));
            sharedAlreadyLoaded.Should().NotBeNull();
            externalNotLoaded.Should().NotBeNull();
        }
    }
}