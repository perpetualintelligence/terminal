/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli;
using PerpetualIntelligence.Shared.Extensions;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Xunit;

Activity.DefaultIdFormat = ActivityIdFormat.W3C;
Console.Title = "pi.cli();";

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Error)
    .MinimumLevel.Override("System", LogEventLevel.Error)
    .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Error)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:w4}] {Message:lj}{NewLine}{Exception}", theme: AnsiConsoleTheme.Code)
    .CreateLogger();

try
{
    Console.WriteLine("---------------------------------------------------------------------------------------------");
    Console.WriteLine("Copyright (c). All Rights Reserved. Perpetual Intelligence L.L.C.");
    Console.WriteLine("https://perpetualintelligence.com");
    Console.WriteLine("https://api.perpetualintelligence.com");
    Console.WriteLine("https://oneimlx.com");
    Console.WriteLine("---------------------------------------------------------------------------------------------");

    Log.Information("Starting {0} server...", "pi.cli();");
    Log.Information("Version={0}", typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "<none>");

    Thread.Sleep(500);

    // Check if we have any command line args
    if (args != null && args.Length > 0)
    {
        Console.WriteLine();
        Log.Warning("Found {0} in command line arguments...", args[0]);

        Thread.Sleep(1000);

        await ProcessCommandLineArgumentsAsync(args);

        Console.WriteLine();
        Log.Warning("Starting command execution loop...");
        Thread.Sleep(1000);
    }
    else
    {
        Console.WriteLine();
    }

    // Regular command execution loop
    while (true)
    {
        try
        {
            // Get the command to execute from user.
            Command command = await ReadCommandAsync();

            // Check and process the command
            await ProcessCommandAsync(command);
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message);
        }

        Console.WriteLine();
    };
}
catch (Exception ex)
{
    Log.Fatal(ex, "Server terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}

Task<Command> ReadCommandAsync()
{
    // Read a command
    Console.Write("cmd > ");
    string? command = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(command))
    {
        // Parse the command
        return ParseCommand(command);
    }
    else
    {
        // Wait till we get a command.
        return ReadCommandAsync();
    }
}

Task<string?> ReadAnswerAsync(string question, params string[]? answers)
{
    // Print the question
    if (answers != null)
    {
        Console.Write($"{question} ({string.Join('/', answers)}): ");
    }
    else
    {
        Console.Write($"{question}: ");
    }

    // Check answer
    string? answer = Console.ReadLine();
    if (answers != null)
    {
        // Special command
        if (answer == "exit")
        {
            return Task.FromResult<string?>(null);
        }

        if (answers.Contains(answer))
        {
            return Task.FromResult(answer);
        }
        else
        {
            Log.Error("The answer is not valid. answers={0}", answers.JoinSpace());
            return ReadAnswerAsync(question, answers);
        }
    }
    else
    {
        return Task.FromResult(answer);
    }
}

async Task ProcessCommandAsync(Command command)
{
    Log.Information("Begin command {0}", command.Name);
    if (command.Name == "map")
    {
        await ExecuteMapCommandAsync(command);
    }
    else if (command.Name == "exit")
    {
        await ExecuteExitCommandAsync(command);
    }
    else
    {
        Log.Error("The command {0} is not valid.", command.Name);
    }
    Log.Information("End command {0}", command.Name);
}

async Task ExecuteMapCommandAsync(Command command)
{
    // Root Directory
    string testRoot = CheckArgument(command, "r");

    // Projects
    string projectStr = CheckArgument(command, "p");
    string[] projects = projectStr.Split(',');

    // Configuration (Debug/Release)
    string config = CheckArgument(command, "c");

    // .NET Framework
    string framework = CheckArgument(command, "f");

    // Input Json mapping file (optional)
    string iMapFile = CheckArgument(command, "i");

    // Output Json mapping file
    string oMapFile = CheckArgument(command, "o");

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

async Task ExecuteExitCommandAsync(Command command)
{
    string? answer = await ReadAnswerAsync("Are you sure ?", new[] { "y", "n" });
    if (answer == null || answer == "y")
    {
        WriteLineRed("Shutting down server. Please wait...");
        Thread.Sleep(1000);
        Console.WriteLine($"Shut down is complete on {DateTimeOffset.UtcNow}.");
        Environment.Exit(-1);
    }
}

Task<string?> CheckFileOrFolder(string root, string fileOrfolder)
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

Task<Command> ParseCommand(string command)
{
    // First Split by space
    string[] args = command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

    Command commandObj = new Command();

    // First one is the command
    commandObj.Name = args[0];
    commandObj.Arguments = new Dictionary<string, string>();

    // Now process the args
    for (int idx = 1; idx < args.Length; ++idx)
    {
        var kvp = ParseArgument(args[idx]);
        commandObj.Arguments.Add(kvp.Key, kvp.Value);
    }

    return Task.FromResult(commandObj);
}

KeyValuePair<string, string> ParseArgument(string arg)
{
    // Split by space
    string[] argSplit = arg.Split(new char[] { '=' });
    if (argSplit.Length != 2)
    {
        throw new ArgumentException("Command arguments must be in format -arg=value");
    }

    return new KeyValuePair<string, string>(argSplit[0].Trim(new char[] { '-', ' ' }), argSplit[1].Trim());
}

void WriteLineRed(string message)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(message);
    Console.ResetColor();
}

string CheckArgument(Command command, string key, string? value = null)
{
    if (!command.Arguments.ContainsKey(key))
    {
        throw new ArgumentException($"Command '{command.Name}' must have argument '-{key}'");
    }

    return command.Arguments[key];
}

bool IsFile(string path)
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

Assembly? LoadAssembly(string path)
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

async Task<Assembly?> FindAndLoadAssemblyAsync(string dir, string config, string framework, string projectName)
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

Task<List<OneImlxTestMap>> InspectTestAssembly(Assembly testLibAssembly)
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

Task<List<OneImlxTestFile>> InspectTestFiles(string projectDir)
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

bool IsXUnitTestFile(string arg)
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

async Task ProcessCommandLineArgumentsAsync(string[] args)
{
    // arg[0] => command arg[idx] => command arg (-arg={value})
    Command argCommand = await ParseCommand(args[0]);
    for (int idx = 1; idx < args.Length; ++idx)
    {
        var kvp = ParseArgument(args[idx]);
        argCommand.Arguments.Add(kvp.Key, kvp.Value);
    }

    // Check and process the command
    await ProcessCommandAsync(argCommand);
}
