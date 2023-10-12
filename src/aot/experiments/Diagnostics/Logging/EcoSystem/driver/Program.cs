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
            CreateAndPublishProject();
        }

        /// <summary>
        /// Read the csv file, update the csproj file with package Id, version, and the assembly name
        /// </summary>
        static void CreateAndPublishProject()
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

            PkgManager manager = new PkgManager(@"C:\work\core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\EcoSystem\TrimClient\2023_10_11_Results_500K.csv");
            foreach (NugetPkg pkg in manager)
            {
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

                string dirToWriteOutputFile = PrepareToPublish(pkg.PkgHash!);
                PublishProject(dirToWriteOutputFile);
                sw.Stop();
                Console.WriteLine($"Time taken to publish {pkg.Id} {pkg.Version} is {sw.ElapsedMilliseconds} ms");
            }

        }

        private static string PrepareToPublish(string pkgHash)
        {
            //clean up
            if (Directory.Exists("bin"))
                Directory.Delete("bin", true);
            if (Directory.Exists("obj"))
                Directory.Delete("obj", true);
            string dir = Path.Combine(@"C:\work\core\LakshanF\CSharp\src\tmp\Telemetry\Results", pkgHash);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }else
            {
                Console.WriteLine($"Directory {dir} already exists");
            }

            return dir;

        }

        static void PublishProject(string dir)
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

            File.WriteAllText(Path.Combine(dir, "publish_output.txt"), publishOutput); // Write the output to a text file
        }
    }
}
