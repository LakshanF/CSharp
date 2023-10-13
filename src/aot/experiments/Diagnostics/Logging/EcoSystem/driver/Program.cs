using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Linq;
using EcoSystemDriver;

namespace ProjectCreator
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length < 3)
            {
                Console.WriteLine("Usage: Driver.exe <Kusto csv file> <dir to write results> <file to write results>");
                return;
            }
            CreateAndPublishProjects(args[0], args[1], args[2]);
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
            }else
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

    }
}
