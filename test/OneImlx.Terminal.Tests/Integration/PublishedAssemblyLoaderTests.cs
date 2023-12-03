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

namespace OneImlx.Terminal.Integration
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
        public async Task LoadAssembly_WithInvalidAssemblyPath_ShouldThrowExceptionAsync()
        {
            commandSourceContext.PublishedAssemblies.Add("InvalidAssembly.dll", testBaseDir);
            Func<Task> act = () => assemblyLoader.LoadAssembliesAsync(commandSourceContext);
            await act.Should().ThrowAsync<TerminalException>().WithMessage($"The published command source assembly does not exist. path={Path.Combine(testBaseDir, "InvalidAssembly.dll")}");
        }

        [Fact]
        public async Task LoadAssembly_WithAssemblyHavingDependencies_ShouldLoadDependentAssembliesAsync()
        {
            // The structure is:
            // test/
            //   OneImlx.Terminal.Tests/
            //     bin/Debug/ or bin/Release/
            //   OneImlx.Terminal.DependentAssembly/
            //     bin/Debug/ or bin/Release/

            // Determine the current configuration (Debug/Release)
            var configuration = "Release";
#if DEBUG
            configuration = "Debug";
#endif
            var version = "net7.0";

            // Define the relative path to Terminal.DependentAssembly.dll from the unit test binary output directory
            var relativePathToDependentAssembly = Path.Combine("..", "..", "..", "..", "OneImlx.Terminal.DependentAssembly", "bin", configuration, version);

            // Get the full path for Terminal.DependentAssembly.dll
            var dependentAssemblyPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), relativePathToDependentAssembly));

            // Ensure the assembly file exists at the source path
            if (!Directory.Exists(dependentAssemblyPath))
            {
                throw new FileNotFoundException($"The dependent assembly location was not found at '{dependentAssemblyPath}'");
            }

            // Make sure the assembly is not loaded
            var assembliesBeforeLoad = AppDomain.CurrentDomain.GetAssemblies();
            Assembly? sharedAlreadyLoaded = assembliesBeforeLoad.FirstOrDefault(e => e.GetName().Name!.Equals("PerpetualIntelligence.Shared"));
            Assembly? externalNotLoaded = assembliesBeforeLoad.FirstOrDefault(e => e.GetName().Name!.Equals("OneImlx.Terminal.DependentAssembly"));
            sharedAlreadyLoaded.Should().NotBeNull();
            externalNotLoaded.Should().BeNull();

            commandSourceContext.PublishedAssemblies.Add("PerpetualIntelligence.Shared.dll", testBaseDir);
            commandSourceContext.PublishedAssemblies.Add("OneImlx.Terminal.DependentAssembly.dll", dependentAssemblyPath);

            var loadedAssemblies = await assemblyLoader.LoadAssembliesAsync(commandSourceContext);
            loadedAssemblies.Should().NotBeNull();
            loadedAssemblies.First().GetName().Name.Should().Be("PerpetualIntelligence.Shared");
            loadedAssemblies.Last().GetName().Name.Should().Be("OneImlx.Terminal.DependentAssembly");

            // Assert both assemblies stay loaded
            var assembliesAfterLoad = AppDomain.CurrentDomain.GetAssemblies();
            sharedAlreadyLoaded = assembliesAfterLoad.FirstOrDefault(e => e.GetName().Name!.Equals("PerpetualIntelligence.Shared"));
            externalNotLoaded = assembliesAfterLoad.FirstOrDefault(e => e.GetName().Name!.Equals("OneImlx.Terminal.DependentAssembly"));
            sharedAlreadyLoaded.Should().NotBeNull();
            externalNotLoaded.Should().NotBeNull();
        }
    }
}