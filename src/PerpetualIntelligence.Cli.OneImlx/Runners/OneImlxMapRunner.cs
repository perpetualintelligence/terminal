/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Commands;
using PerpetualIntelligence.Cli.Commands.Runners;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.OneImlx.Cli;
using Serilog;
using System.Reflection;
using System.Text.Json;
using Xunit;

namespace PerpetualIntelligence.OneImlx.Cli.Runners
{
    /// <summary>
    /// The <c>map</c> command runner.
    /// </summary>
    public class OneImlxMapRunner : CommandRunner
    {
        /// <summary>
        /// </summary>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public OneImlxMapRunner(CliOptions options, ILogger<CommandRunner> logger) : base(options, logger)
        {
        }

        /// <inheritdoc/>
        public override async Task<CommandRunnerResult> RunAsync(CommandRunnerContext context)
        {
            await ExecuteMapCommandAsync(context.Command);
            return new CommandRunnerResult();
        }

        private static Task<string?> CheckFileOrFolder(string root, string fileOrfolder)
        {
            Console.Write($"Looking for {fileOrfolder}: ");
            string checkPath = Path.Combine(root, fileOrfolder);

            if (IsFile(checkPath))
            {
                if (!File.Exists(checkPath))
                {
                    WriteLineRed("Not Found");
                    return Task.FromResult<string?>(null);
                }
            }
            else
            {
                if (!Directory.Exists(checkPath))
                {
                    WriteLineRed("Not Found");
                    return Task.FromResult<string?>(null);
                }
            }

            Console.WriteLine("OK");
            return Task.FromResult<string?>(checkPath);
        }

        private static async Task ExecuteMapCommandAsync(Command command)
        {
            Dictionary<string, object> args = command.Arguments.ToNameValueCollection();

            // Root Directory
            string testRoot = (string)args["r"];

            // Projects
            string projectStr = (string)args["p"];
            string[] projects = projectStr.Split(',');

            // Configuration (Debug/Release)
            string config = (string)args["c"];

            // .NET Framework
            string framework = (string)args["f"];

            // Input Json mapping file (optional)
            string iMapFile = (string)args["i"];

            // Output Json mapping file
            string oMapFile = (string)args["o"];

            // Make sure the directory exist
            if (!Directory.Exists(testRoot))
            {
                Log.Error("The root directory is not valid. dir={0}", testRoot);
                return;
            }

            Console.WriteLine("Parsing root directory...");

            // Check for test folder
            string? testDir = await CheckFileOrFolder(testRoot, "test");
            if (testDir == null)
            {
                return;
            }

            // Init the containers
            List<OneImlxTestMap> testMaps = new();
            List<OneImlxTestFile> testFiles = new();

            // Enumerate all the projects and inspect the types and files
            foreach (string project in projects)
            {
                Console.WriteLine();

                // Find and load {project}.dll
                Assembly? projectAssembly = await FindAndLoadAssemblyAsync(testDir, config, framework, project);
                if (projectAssembly == null)
                {
                    return;
                }

                // Inspect and load test mapping
                testMaps.AddRange(await InspectTestAssembly(projectAssembly));

                // Inspect and load test files
                testFiles.AddRange(await InspectTestFiles(Path.Combine(testDir, project)));
            }

            // Serialize to oMapFile
            OneImlxTestData testData = new()
            {
                Tests = testMaps.ToArray(),
                TestsCount = testMaps.Count,
                Files = testFiles.ToArray(),
                FilesCount = testFiles.Count
            };
            await File.WriteAllBytesAsync(oMapFile, JsonSerializer.SerializeToUtf8Bytes(testData, new JsonSerializerOptions() { WriteIndented = true }));
        }

        private static async Task<Assembly?> FindAndLoadAssemblyAsync(string dir, string config, string framework, string projectName)
        {
            string assemblyNameDll = projectName + ".dll";
            Log.Information("Finding test assembly {0}", assemblyNameDll);

            // Check for test/{assemblyName}
            string? testLibDir = await CheckFileOrFolder(dir, projectName);
            if (testLibDir == null)
            {
                return null;
            }

            // Check for test/{assemblyName}/bin
            string? binDir = await CheckFileOrFolder(testLibDir, "bin");
            if (binDir == null)
            {
                return null;
            }

            // Check for test/{assemblyName}/{config}
            string? configDir = await CheckFileOrFolder(binDir, config);
            if (configDir == null)
            {
                return null;
            }

            // Check for test/{assemblyName}/{config}/{framework}
            string? frameworkDir = await CheckFileOrFolder(configDir, framework);
            if (frameworkDir == null)
            {
                return null;
            }

            // Check for test/{assemblyName}/{config}/{framework}/{assemblyName}.dll
            string? testLibDll = await CheckFileOrFolder(frameworkDir, assemblyNameDll);
            if (testLibDll == null)
            {
                return null;
            }

            // Load the assembly in the App Domain
            Assembly? testLibAssembly = LoadAssembly(testLibDll);
            if (testLibAssembly == null)
            {
                return null;
            }

            return testLibAssembly;
        }

        private static Task<List<OneImlxTestMap>> InspectTestAssembly(Assembly testLibAssembly)
        {
            Type[] types = testLibAssembly.GetTypes();
            List<OneImlxTestMap> testMethods = new();
            foreach (Type type in types)
            {
                var methods = type.GetMethods().Where(e => e.IsDefined(typeof(FactAttribute)) || e.IsDefined(typeof(TheoryAttribute)));

                foreach (MethodInfo method in methods)
                {
                    if (method.DeclaringType == null)
                    {
                        continue;
                    }

                    OneImlxTestMap testMethod = new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Source = new()
                        {
                            Assembly = method.DeclaringType.Assembly.GetName().FullName,
                            DeclaringType = method.DeclaringType.FullName ?? "undefined",
                            Name = method.Name,
                            Namespace = method.DeclaringType.Namespace ?? "undefined",
                        },
                        Target = new()
                        {
                            Assembly = "undefined",
                            DeclaringType = "undefined",
                            Name = "undefined",
                            Namespace = "undefined"
                        }
                    };

                    testMethods.Add(testMethod);
                }
            }

            return Task.FromResult(testMethods);
        }

        private static Task<List<OneImlxTestFile>> InspectTestFiles(string projectDir)
        {
            IEnumerable<string> srcFiles = Directory.EnumerateFiles(projectDir, "*.cs", SearchOption.AllDirectories);
            var testFiles = srcFiles.Where(IsXUnitTestFile);

            List<OneImlxTestFile> files = new();
            foreach (string testFile in testFiles)
            {
                OneImlxTestFile file = new()
                {
                    CreateStamp = File.GetCreationTimeUtc(testFile),
                    Name = Path.GetFileName(testFile),
                    Path = testFile,
                    UpdateStamp = File.GetLastWriteTimeUtc(testFile)
                };

                files.Add(file);
            }

            return Task.FromResult(files);
        }

        private static bool IsFile(string path)
        {
            if (!File.Exists(path))
            {
                return false;
            }

            FileAttributes attr = File.GetAttributes(path);

            if (attr.HasFlag(FileAttributes.Directory))
                return false;
            else
                return true;
        }

        private static bool IsXUnitTestFile(string arg)
        {
            // Check for Fact or Theory attribute
            string[] allLines = File.ReadAllLines(arg);

            // Any line that has [Fact] or [Theory] is a test file.
            if (allLines.Any(l => l.Contains("[Fact]") || l.Contains("[Theory]")))
            {
                return true;
            }

            return false;
        }

        private static Assembly? LoadAssembly(string path)
        {
            try
            {
                Console.Write($"Loading assembly {Path.GetFileName(path)}: ");

                // Use the file name to load the assembly into the current application domain.
                Assembly a = Assembly.LoadFrom(path);
                Console.WriteLine("OK");
                return a;
            }
            catch (Exception ex)
            {
                WriteLineRed("Failed");
                Console.Write($" ({ex.Message})");
                return null;
            }
        }

        private static void WriteLineRed(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
