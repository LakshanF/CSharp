using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Linq;
using EcoSystemDriver;
using System.Runtime;
using System.Xml.Linq;

namespace ProjectCreator;

class Program
{
    const string PROCESS_NOT_COMPLETED = "PROCESS_NOT_COMPLETED";
    static object _lock = new object();
    static object _errorLock = new object();
    static object _cancelLock = new object();
    static int RT_VERSION = Environment.Version.Major;

    static async Task Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: Driver.exe <Kusto csv file> <dir to write results> <file to write results>");
            return;
        }

        string inputDataFile = args[0];
        string outputDir = args[1];
        String outputFileName = args[2];

        if(File.Exists(outputFileName))
            File.Delete(outputFileName);
        if (Directory.Exists(outputDir))
        {
            Directory.Delete(outputDir, true);
        }
        if (!Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);
        
        await CreateAndPublishProjectsAsync(inputDataFile, outputDir, outputFileName);
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
                <TargetFramework>net9.0</TargetFramework>
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
        packages = RemoveProblematicPackages(packages);
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
                        // We need a skeleton HW as the app
                        File.WriteAllText(Path.Combine(dirName, "Program.cs"), $"Console.WriteLine(\"Hello, World!\");{Environment.NewLine}");
                        //File.Copy("Program.cs", Path.Combine(dirName, "Program.cs"), true);

                        string dirToWriteOutputFile = Path.Combine(resultDir, package.PkgHash!);
                        bool pkgHashAlreadyExists = PrepareToPublishAsync(dirToWriteOutputFile);
                        Debug.Assert(!pkgHashAlreadyExists, "Problems with hashes");
                        // @TODO - we are ignoring multiple assemblies with the same PkgHash for now. Use BinHash
                        var resultFile = PublishProjectAsync(dirToWriteOutputFile, dirName).Result;
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
            catch (AggregateException ex)
            {
                Console.WriteLine("AggregateException thrown...");
                foreach (var innerEx in ex.InnerExceptions)
                {
                    Console.WriteLine($"Message: {innerEx.Message}");
                    // Optionally log the stack trace or other details
                }
            }

            // Print a message when done
            Console.WriteLine($"Chunk completed");
        }

    }

    /// <summary>
    /// Ensure that there is only 1 pkg hash per assembly by matching assembly name with the package id
    /// </summary>
    /// <param name="packages"></param>
    /// <returns></returns>
    static NugetPkg2[] RemoveProblematicPackages(NugetPkg2[] packages)
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
                && !IsAKnownProblematicPackage(pkg.Id)
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
                && !IsAKnownProblematicPackage(pkg.Id)
                )
            {
                allHashes.Add(pkg.PkgHash!);
                result.Add(pkg);
            }
        }

        // Since we have messed up the download ordering, we will sort it again
        return result.OrderByDescending(x => x.DownloadCount).ToArray();
    }

    private static bool IsAKnownProblematicPackage(string pkgName)
    {
        List<string> badPackages = new List<string>();
        badPackages.Add("Syncfusion.Maui.");
        badPackages.Add("Vintasoft.Imaging.");
        //badPackages.Add("Syncfusion.");
        //badPackages.Add("Vintasoft.Imaging.");
        //badPackages.Add("ImageGlue.");
        //badPackages.Add("Aspose.");
        //badPackages.Add("FenixAlliance.");
        //badPackages.Add("Mutagen.");
        //badPackages.Add("unofficial.");
        //badPackages.Add("AssetRipper.");
        //badPackages.Add("Mutagen.");

        foreach (string badPackage in badPackages)
        {
            if (pkgName.StartsWith(badPackage, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
        }
        return false;
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
        if (resultFile != PROCESS_NOT_COMPLETED)
        {
            foreach (string line in File.ReadLines(resultFile))
            {
                if (line.Contains("Trim analysis warning"))
                {
                    trimCount++;
                }
                //SimpleApp -> C:\work\core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\EcoSystem\TrimClient\bin\Release\net9.0\win-x64\publish\
                if (line.Trim().StartsWith("SimpleApp -> ") && line.Trim().ToLowerInvariant().Contains($@"bin\Release\net{RT_VERSION}.0\win-x64\publish\".ToLowerInvariant()))
                {
                    foundSuccessfullPublish = true;
                }
                if (line.Contains(" warning IL2104: Assembly "))
                {
                    singleAssemblyWarningFound=true;
                }
            }
        }
        builder.Append($"{FieldSeparator}{trimCount}");
        builder.Append(foundSuccessfullPublish ? $"{FieldSeparator}Y" : $"{FieldSeparator}N");

        // We will write the download size and singleAssemblyWarningFound at the end
        builder.Append($"{FieldSeparator}{pkg.DownloadCount}");
        builder.Append(singleAssemblyWarningFound ? $"{FieldSeparator}Y" : $"{FieldSeparator}N");

        return builder.ToString();
    }

    static async Task<string> PublishProjectAsync(string resultDir, string projectDir)
    {
        var publishProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                WorkingDirectory = projectDir,
                FileName = "dotnet",
                Arguments = "publish",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        publishProcess.OutputDataReceived += (sender, args) => outputBuilder.AppendLine(args.Data);
        publishProcess.ErrorDataReceived += (sender, args) => errorBuilder.AppendLine(args.Data);

        publishProcess.Start();
        publishProcess.BeginOutputReadLine();
        publishProcess.BeginErrorReadLine();

        bool result = await Task.Run(() => publishProcess.WaitForExit(10 * 60 * 1000)); // 10 minutes timeout

        string resultFile = PROCESS_NOT_COMPLETED;
        if (result)
        {
            resultFile = Path.Combine(resultDir, "publish_output.txt");
            await File.WriteAllTextAsync(resultFile, outputBuilder.ToString()); // Write the output to a text file
        }
        else
        {
            // Handle the case where the process did not exit in time
            await File.WriteAllTextAsync(resultFile, $"Process did not complete in the expected time. Errors: {errorBuilder}");
        }

        return resultFile;
    }
}
