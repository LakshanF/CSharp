using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Linq;
using EcoSystemDriver;
using System.Runtime;

namespace ProjectCreator
{
    class Program
    {
        static object _lock = new object();
        static async Task Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: Driver.exe <Kusto csv file> <dir to write results> <file to write results>");
                return;
            }
            //CreateAndPublishProjects(args[0], args[1], args[2]);
            await CreateAndPublishProjects2(args[0], args[1], args[2]);
        }

        static NugetPkg2[] RemoveSamePkgHash(NugetPkg2[] packages)
        {
            HashSet<string> hashes = new HashSet<string>();
            List<NugetPkg2> result = new List<NugetPkg2>();
            foreach (NugetPkg2 pkg in packages)
            {
                if (!hashes.Contains(pkg.PkgHash))
                {
                    hashes.Add(pkg.PkgHash);
                    result.Add(pkg);
                }
            }
            return result.ToArray();
        }


        /// <summary>
        /// Parallel version of CreateAndPublishProjects
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="v3"></param>
        /// <returns></returns>
        private static async Task CreateAndPublishProjects2(string kustoFileName, string resultDir, string outputFileName)
        {

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

            string[] lines = File.ReadLines(kustoFileName).Skip(1).ToArray();

            // Create an array of DirInfo objects by using the constructor
            NugetPkg2[] packages = Array.ConvertAll(lines, line => new NugetPkg2(line));
            packages = RemoveSamePkgHash(packages);

            // Create a cancellation token source
            var cts = new CancellationTokenSource();

            // Handle the Ctrl+C event to cancel the operation
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            File.WriteAllText(outputFileName, $"PackageHAsh###Id###Version###AssemblyName###AssemblySize###TimeTaken###PkgHashAlreadyExists###AssemblyNameMatchesId###NoOfTrimWarnings###TrimSuccess{Environment.NewLine}");

            try
            {
                // Use Parallel.ForEach to run a delegate for each DirInfo object in parallel
                await Task.Run(() =>
                {
                    Parallel.ForEach(packages, new ParallelOptions { CancellationToken = cts.Token }, package =>
                    {
                        Console.Write(".");

                        Stopwatch sw = new Stopwatch();
                        sw.Restart();

                        // Get the directory name from the object
                        string dirName = package.PkgHash;

                        // Create the directory if it does not exist
                        Directory.CreateDirectory(dirName);

                        // Create test project file
                        StringBuilder builder = new StringBuilder(projectPrefix);
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

                        string dirToWriteOutputFile = Path.Combine(resultDir, package.PkgHash);
                        bool pkgHashAlreadyExists = PrepareToPublish2(dirToWriteOutputFile);
                        Debug.Assert(!pkgHashAlreadyExists, "Problems with hashes");
                        // @TODO - we are ignoring multiple assemblies with the same PkgHash for now. Use BinHash
                        string resultFile = PublishProject2(dirToWriteOutputFile, dirName);
                        sw.Stop();

                        // We should be able to find the assembly file in bin\Release\net8.0\win-x64 directory
                        // If trimmed successfully, we should be able to find the assembly file in bin\Release\net8.0\win-x64\publish as well obj\Release\net8.0\win-x64\linked directories
                        long assemblySize = -1;
                        try
                        {
                            string assemblyFile = Path.Combine(dirName, @"bin\Release\net8.0\win-x64", Path.GetFileName(package.ContainerPath));
                            assemblySize = new FileInfo(assemblyFile).Length;
                        }
                        catch { }

                        // We assume that AnalyseResultFile2 is thread safe
                        string result = $"{AnalyseResultFile2(resultFile, package, sw.ElapsedMilliseconds, pkgHashAlreadyExists, assemblySize)}{Environment.NewLine}";
                        lock (_lock)
                        {
                            File.AppendAllText(outputFileName, result);
                        }

                        // We delete the directory since we can detect duplicate package hashes from the results directory
                        Directory.Delete(dirName, true);

                    });
                });

                // Print a message when done
                Console.WriteLine("All processes completed.");
            }
            catch (OperationCanceledException)
            {
                // Print a message when cancelled
                Console.WriteLine("Operation cancelled.");
            }
        }

        private static bool PrepareToPublish2(string dirToWriteOutputFile)
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

        private static string AnalyseResultFile2(string resultFile, NugetPkg2 pkg, long elapsedMilliseconds, bool pkgAlreadyExists, long assemblySize)
        {
            const string FieldSeparator = "###";
            // Check if the pkg.Id and ContainerPath are the same
            // Check if there are any trim warnings
            // check if the publish was successful

            StringBuilder builder = new StringBuilder($"{pkg.PkgHash}{FieldSeparator}{pkg.Id}{FieldSeparator}{pkg.Version}{FieldSeparator}{Path.GetFileNameWithoutExtension(pkg.ContainerPath)}{FieldSeparator}{assemblySize}{FieldSeparator}{elapsedMilliseconds}");
            builder.Append(pkgAlreadyExists ? $"{FieldSeparator}Y" : $"{FieldSeparator}N");
            builder.Append(pkg.Id.Equals(Path.GetFileNameWithoutExtension(pkg.ContainerPath)) ? $"{FieldSeparator}Y" : $"{FieldSeparator}N");

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

                string dirToWriteOutputFile = Path.Combine(resultDir, pkg.PkgHash);
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
            builder.Append(pkg.Id.Equals(Path.GetFileNameWithoutExtension(pkg.ContainerPath)) ? $"{FieldSeparator}Y" : $"{FieldSeparator}N");

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
            string publishOutput = publishProcess.StandardOutput.ReadToEnd();
            publishProcess.WaitForExit();

            string resultFile = Path.Combine(dir, "publish_output.txt");
            File.WriteAllText(resultFile, publishOutput); // Write the output to a text file
            return resultFile;
        }

        static string PublishProject2(string resultDir, string projectDir)
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
            string publishOutput = publishProcess.StandardOutput.ReadToEnd();
            publishProcess.WaitForExit();

            string resultFile = Path.Combine(resultDir, "publish_output.txt");
            File.WriteAllText(resultFile, publishOutput); // Write the output to a text file
            return resultFile;
        }


    }
}
