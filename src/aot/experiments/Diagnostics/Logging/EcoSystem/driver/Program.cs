using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Linq;
using EcoSystemDriver;
using System.Runtime;

namespace ProjectCreator;

class Program
{
    static object _lock = new object();
    static object _errorLock = new object();
    static object _cancelLock = new object();
    static async Task Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: Driver.exe <Kusto csv file> <dir to write results> <file to write results>");
            return;
        }
        //CreateAndPublishProjects(args[0], args[1], args[2]);
        await CreateAndPublishProjectsAsync(args[0], args[1], args[2]);
    }

    /// <summary>
    /// Ensure that there is only 1 pkg hash per assembly by matching assembly name with the package id
    /// </summary>
    /// <param name="packages"></param>
    /// <returns></returns>
    static NugetPkg2[] RemoveSamePkgHash(NugetPkg2[] packages)
    {
        // First, lets get the hashes that we need to include (unique and ID and assembly name match)
        // Second, we will iterate and remove any that contains file extensions like ".resources" or the grandparent directory name is not "lib"
        // Next, exclude problematic packages - from Vitek: Syncfusion.Maui.*, Vintasoft.Imaging.*
        // We keep track of the package hashes that we added since we do post additions after the first pass
        HashSet<string> allHashes = new HashSet<string>();
        List<NugetPkg2> result = new List<NugetPkg2>();
        foreach (NugetPkg2 pkg in packages)
        {
            if (!allHashes.Contains(pkg.PkgHash!)
                && pkg.Id!.Equals(Path.GetFileNameWithoutExtension(pkg.ContainerPath), StringComparison.InvariantCultureIgnoreCase)
                && Path.GetDirectoryName(Path.GetDirectoryName(pkg.ContainerPath!))!.Equals("lib", StringComparison.InvariantCultureIgnoreCase)
                && !Path.GetFileNameWithoutExtension(pkg.ContainerPath!).EndsWith(".resources", StringComparison.InvariantCultureIgnoreCase)
                && !pkg.Id.StartsWith("Syncfusion.Maui.", StringComparison.InvariantCultureIgnoreCase)
                && !pkg.Id.StartsWith("Vintasoft.Imaging.", StringComparison.InvariantCultureIgnoreCase)
                )
            {
                allHashes.Add(pkg.PkgHash!);
                result.Add(pkg);
            }
        }

        // Include packages that doesn't meet the criteria where the assembly name matches the package id
        foreach (NugetPkg2 pkg in packages)
        {
            if (!allHashes.Contains(pkg.PkgHash!)
                && Path.GetDirectoryName(Path.GetDirectoryName(pkg.ContainerPath!))!.Equals("lib", StringComparison.InvariantCultureIgnoreCase)
                && !Path.GetFileNameWithoutExtension(pkg.ContainerPath!).EndsWith(".resources", StringComparison.InvariantCultureIgnoreCase)
                && !pkg.Id.StartsWith("Syncfusion.Maui.", StringComparison.InvariantCultureIgnoreCase)
                && !pkg.Id.StartsWith("Vintasoft.Imaging.", StringComparison.InvariantCultureIgnoreCase)
                )
            {
                allHashes.Add(pkg.PkgHash!);
                result.Add(pkg);
            }
        }

        // Since we have messed up the download ordering, we will sort it again
        return result.OrderByDescending(x => x.DownloadCount).ToArray();
    }

    /// <summary>
    /// Parallel version of CreateAndPublishProjects
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <param name="v3"></param>
    /// <returns></returns>
    private static async Task CreateAndPublishProjectsAsync(string kustoFileName, string resultDir, string? outputFileName)
    {
        const int CHUNK_SIZE = 1000;

        string projectPrefix = """
            <Project Sdk="Microsoft.NET.Sdk">

            <PropertyGroup>
                <OutputType>Exe</OutputType>
                <TargetFramework>net8.0</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
                <PublishTrimmed>true</PublishTrimmed>
                <WarningsAsErrors>false</WarningsAsErrors>
                <NoWarn>NU1605;NU1603;NU1701</NoWarn>
        """;


        File.WriteAllText(outputFileName!, $"PackageHAsh###Id###Version###AssemblyName###AssemblySize###TimeTaken###PkgHashAlreadyExists###AssemblyNameMatchesId###NoOfTrimWarnings###TrimSuccess###DownLoadCount###SingleWarn{Environment.NewLine}");

        string[] lines = File.ReadLines(kustoFileName).Skip(1).ToArray();
        // Create an array of DirInfo objects by using the constructor
        NugetPkg2[] packages = Array.ConvertAll(lines, line => new NugetPkg2(line));
        packages = RemoveSamePkgHash(packages);
        string errorFileName = Path.Combine(Path.GetDirectoryName(outputFileName!), "error.txt");

        // we want to parallelize but given that the kustoFileName is ordered (from the Kusto query), we want to preserve the order in some way and will use ArraySegement to do that
        ArraySegment<NugetPkg2> segment;
        bool userCancelled = false;
        for (int i = 0; i < packages.Length && !userCancelled; i += CHUNK_SIZE)
        {
            segment = new ArraySegment<NugetPkg2>(packages, i, CHUNK_SIZE);

            try
            {
                // Create a cancellation token source
                var cts = new CancellationTokenSource();

                // Handle the Ctrl+C event to cancel the operation
                Console.CancelKeyPress += (s, e) =>
                {
                    e.Cancel = true;
                    cts.Cancel();
                };

                // Use Parallel.ForEach to run a delegate for each DirInfo object in parallel
                await Task.Run(() =>
                {
                    Parallel.ForEach(segment, new ParallelOptions { CancellationToken = cts.Token }, package =>
                    {
                        Console.Write(".");

                        Stopwatch sw = new Stopwatch();
                        sw.Restart();

                        // Get the directory name from the object
                        string dirName = package.PkgHash!;

                        // Create the directory if it does not exist
                        Directory.CreateDirectory(dirName);

                        // Create test project file
                        StringBuilder builder = new StringBuilder(projectPrefix);

                        builder.AppendLine("\t\t<_ExtraTrimmerArgs>--singlewarn- "  + Path.GetFileNameWithoutExtension(package.ContainerPath) + "</_ExtraTrimmerArgs>");
                        builder.AppendLine("    </PropertyGroup>");
                        builder.AppendLine();
                        builder.AppendLine("    <ItemGroup>");

                        builder.AppendLine("    <TrimmerRootAssembly Include=\"" + Path.GetFileNameWithoutExtension(package.ContainerPath) + "\" />");
                        builder.AppendLine("  </ItemGroup>");
                        builder.AppendLine();
                        builder.AppendLine("<ItemGroup>");
                        builder.AppendLine("    <PackageReference Include=\"" + package.Id + "\" Version=\"" + package.Version + "\" />");
                        builder.AppendLine("</ItemGroup>");
                        builder.AppendLine();
                        builder.AppendLine("</Project>");

                        File.WriteAllText(Path.Combine(dirName, "SimpleApp.csproj"), builder.ToString());
                        // We need to copy the HW program that is on the current directory to the new directory as well
                        File.Copy("Program.cs", Path.Combine(dirName, "Program.cs"), true);

                        string dirToWriteOutputFile = Path.Combine(resultDir, package.PkgHash!);
                        bool pkgHashAlreadyExists = PrepareToPublishAsync(dirToWriteOutputFile);
                        Debug.Assert(!pkgHashAlreadyExists, "Problems with hashes");
                        // @TODO - we are ignoring multiple assemblies with the same PkgHash for now. Use BinHash
                        string resultFile = PublishProjectAsync(dirToWriteOutputFile, dirName);
                        sw.Stop();

                        // We should be able to find the assembly file in bin\Release\net8.0\win-x64 directory
                        // If trimmed successfully, we should be able to find the assembly file in bin\Release\net8.0\win-x64\publish as well obj\Release\net8.0\win-x64\linked directories
                        long assemblySize = -1;
                        try
                        {
                            string assemblyFile = Path.Combine(dirName, @"bin\Release\net8.0\win-x64", Path.GetFileName(package.ContainerPath!));
                            assemblySize = new FileInfo(assemblyFile).Length;
                        }
                        catch { }

                        // We assume that AnalyseResultFileAsync is thread safe
                        string result = $"{AnalyseResultFileAsync(resultFile, package, sw.ElapsedMilliseconds, pkgHashAlreadyExists, assemblySize)}{Environment.NewLine}";
                        lock (_lock)
                        {
                            File.AppendAllText(outputFileName!, result);
                        }

                        // delete the directory, this operation can fail, and we will retry and do full cleanup later
                        int retryDelete = 0;
                        const int RetryMax = 10;
                        while (retryDelete < RetryMax)
                        {
                            try
                            {
                                Directory.Delete(dirName, true);
                                break;
                            }
                            catch
                            {
                                Thread.Sleep(100);
                                retryDelete++;
                                if(retryDelete >= RetryMax)
                                {
                                    lock(_errorLock)
                                    {
                                        File.AppendAllText(errorFileName, $"{dirName}{Environment.NewLine}");
                                    }
                                }
                            }
                        }
                    });
                });

            }
            catch (OperationCanceledException)
            {
                // Its ok for this to be a bottleneck. We absolutely need to make sure that no more processing happens after this point
                lock (_cancelLock)
                {
                    userCancelled = true;
                }
                // Print a message when cancelled
                Console.WriteLine("Operation cancelled.");
                // @TODO - delete any directories that were missed being deleted
            }
            // Print a message when done
            Console.WriteLine($"Chunk completed");
        }

    }

    private static bool PrepareToPublishAsync(string dirToWriteOutputFile)
    {
        if (!Directory.Exists(dirToWriteOutputFile))
        {
            Directory.CreateDirectory(dirToWriteOutputFile);
            return false;
        }
        else
        {
            return true;
        }
    }

    private static string AnalyseResultFileAsync(string resultFile, NugetPkg2 pkg, long elapsedMilliseconds, bool pkgAlreadyExists, long assemblySize)
    {
        const string FieldSeparator = "###";
        // Check if the pkg.Id and ContainerPath are the same
        // Check if there are any trim warnings
        // check if the publish was successful

        StringBuilder builder = new StringBuilder($"{pkg.PkgHash}{FieldSeparator}{pkg.Id}{FieldSeparator}{pkg.Version}{FieldSeparator}{Path.GetFileNameWithoutExtension(pkg.ContainerPath)}{FieldSeparator}{assemblySize}{FieldSeparator}{elapsedMilliseconds}");
        builder.Append(pkgAlreadyExists ? $"{FieldSeparator}Y" : $"{FieldSeparator}N");
        builder.Append(pkg.Id!.Equals(Path.GetFileNameWithoutExtension(pkg.ContainerPath)) ? $"{FieldSeparator}Y" : $"{FieldSeparator}N");

        // Look at the publish output file
        int trimCount = 0;
        bool foundSuccessfullPublish = false, singleAssemblyWarningFound = false;
        foreach (string line in File.ReadLines(resultFile))
        {
            if (line.Contains("Trim analysis warning"))
            {
                trimCount++;
            }
            //SimpleApp -> C:\work\core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\EcoSystem\TrimClient\bin\Release\net8.0\win-x64\publish\
            if (line.Trim().StartsWith("SimpleApp -> ") && line.Trim().EndsWith(@"bin\Release\net8.0\win-x64\publish\"))
            {
                foundSuccessfullPublish = true;
            }
            if (line.Contains(" warning IL2104: Assembly "))
            {
                singleAssemblyWarningFound=true;
            }
        }
        builder.Append($"{FieldSeparator}{trimCount}");
        builder.Append(foundSuccessfullPublish ? $"{FieldSeparator}Y" : $"{FieldSeparator}N");

        // We will write the download size and singleAssemblyWarningFound at the end
        builder.Append($"{FieldSeparator}{pkg.DownloadCount}");
        builder.Append(singleAssemblyWarningFound ? $"{FieldSeparator}Y" : $"{FieldSeparator}N");

        return builder.ToString();
    }

    /// <summary>
    /// Read the csv file, update the csproj file with package Id, version, and the assembly name
    /// </summary>
    static void CreateAndPublishProjects(string kustoFileName, string resultDir, string outputFileName)
    {
        Stopwatch sw = new Stopwatch();
        string projectPrefix = """
            <Project Sdk="Microsoft.NET.Sdk">

            <PropertyGroup>
                <OutputType>Exe</OutputType>
                <TargetFramework>net8.0</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
                <PublishTrimmed>true</PublishTrimmed>
                <TrimmerSingleWarn>false</TrimmerSingleWarn>
            </PropertyGroup>

            <ItemGroup>
        """;

        PkgManager manager = new PkgManager(kustoFileName);
        File.WriteAllText(outputFileName, $"PackageHAsh###Id###Version###AssemblyName###TimeTaken###PkgHashAlreadyExists###AssemblyNameMatchesId###NoOfTrimWarnings###TrimSuccess{Environment.NewLine}");
        foreach (NugetPkg pkg in manager)
        {
            Console.Write(".");
            sw.Restart();

            // Create test project file
            StringBuilder builder = new StringBuilder(projectPrefix);
            builder.AppendLine("    <TrimmerRootAssembly Include=\"" + Path.GetFileNameWithoutExtension(pkg.ContainerPath) + "\" />");
            builder.AppendLine("  </ItemGroup>");
            builder.AppendLine();
            builder.AppendLine("<ItemGroup>");
            builder.AppendLine("    <PackageReference Include=\"" + pkg.Id + "\" Version=\"" + pkg.Version + "\" />");
            builder.AppendLine("</ItemGroup>");
            builder.AppendLine();
            builder.AppendLine("</Project>");

            File.WriteAllText("SimpleApp.csproj", builder.ToString());

            string dirToWriteOutputFile = Path.Combine(resultDir, pkg.PkgHash!);
            bool pkgHashAlreadyExists = PrepareToPublish(dirToWriteOutputFile);
            string resultFile = PublishProject(dirToWriteOutputFile);
            sw.Stop();

            File.AppendAllText(outputFileName, $"{AnalyseResultFile(resultFile, pkg, sw.ElapsedMilliseconds, pkgHashAlreadyExists)}{Environment.NewLine}");
        }
        Console.WriteLine();
    }

    private static string AnalyseResultFile(string resultFile, NugetPkg pkg, long elapsedMilliseconds, bool publishSuccess)
    {
        const string FieldSeparator = "###";
        // Check if the pkg.Id and ContainerPath are the same
        // Check if there are any trim warnings
        // check if the publish was successful

        StringBuilder builder = new StringBuilder($"{pkg.PkgHash}{FieldSeparator}{pkg.Id}{FieldSeparator}{pkg.Version}{FieldSeparator}{Path.GetFileNameWithoutExtension(pkg.ContainerPath)}{FieldSeparator}{elapsedMilliseconds}");
        builder.Append(publishSuccess ? $"{FieldSeparator}Y" : $"{FieldSeparator}N");
        builder.Append(pkg.Id!.Equals(Path.GetFileNameWithoutExtension(pkg.ContainerPath)) ? $"{FieldSeparator}Y" : $"{FieldSeparator}N");

        // Look at the publish output file
        int trimCount = 0;
        bool foundSuccessfullPublish = false;
        foreach (string line in File.ReadLines(resultFile))
        {
            if (line.Contains("Trim analysis warning"))
            {
                trimCount++;
            }
            //SimpleApp -> C:\work\core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\EcoSystem\TrimClient\bin\Release\net8.0\win-x64\publish\
            if (line.Trim().StartsWith("SimpleApp -> ") && line.Trim().EndsWith(@"bin\Release\net8.0\win-x64\publish\"))
            {
                foundSuccessfullPublish = true;
            }
        }
        builder.Append($"{FieldSeparator}{trimCount}");
        builder.Append(foundSuccessfullPublish ? $"{FieldSeparator}Y" : $"{FieldSeparator}N");

        return builder.ToString();
    }

    private static bool PrepareToPublish(string resultDir)
    {
        //clean up
        if (Directory.Exists("bin"))
            Directory.Delete("bin", true);
        if (Directory.Exists("obj"))
            Directory.Delete("obj", true);
        if (!Directory.Exists(resultDir))
        {
            Directory.CreateDirectory(resultDir);
            return false;
        }
        else
        {
            return true;
        }
    }

    static string PublishProject(string dir)
    {
        // Publish the created project
        var publishProcess = Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "publish",
            UseShellExecute = false, // Do not use OS shell
            RedirectStandardOutput = true // Redirect the output stream of the process
        });

        // To avoid deadlocks, always read the output stream first and then wait.  
        string publishOutput = publishProcess!.StandardOutput.ReadToEnd();
        publishProcess.WaitForExit();

        string resultFile = Path.Combine(dir, "publish_output.txt");
        File.WriteAllText(resultFile, publishOutput); // Write the output to a text file
        return resultFile;
    }

    static string PublishProjectAsync(string resultDir, string projectDir)
    {
        // Publish the created project
        var publishProcess = Process.Start(new ProcessStartInfo
        {
            WorkingDirectory = projectDir,
            FileName = "dotnet",
            Arguments = "publish",
            UseShellExecute = false, // Do not use OS shell
            RedirectStandardOutput = true // Redirect the output stream of the process
        });

        // To avoid deadlocks, always read the output stream first and then wait.  
        string publishOutput = publishProcess!.StandardOutput.ReadToEnd();
        publishProcess.WaitForExit();

        string resultFile = Path.Combine(resultDir, "publish_output.txt");
        File.WriteAllText(resultFile, publishOutput); // Write the output to a text file
        return resultFile;
    }
}
