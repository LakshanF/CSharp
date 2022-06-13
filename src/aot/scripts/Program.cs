using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net;

using HelixResults;

class Program
{
    public static void Main()
    {
        //Console.WriteLine(DateTime.UtcNow);

        // Read results from Helix
        M13();

        // All (newlibs+all) cleanup
        //M12();
        //M11();
        //M10_1();
        //M10();

        //All librries
        //M9();
        //M8();

        // Failing libs
        //M7();


        // passing libs
        //        M6();

        // Hang Test
        //M5();


        // All tests
        //M3();
        //M4();

        // Linq.Expressions
        //M1();
        //M2();
    }

    static void M13()
    {
        List<string> outList = new();

        WebClient web = new WebClient();
        //string jsonString = web.DownloadString("https://helix.dot.net/api/jobs/69e6476b-2597-447f-a833-9565d67f9a06/workitems?api-version=2019-06-17");
        //string jsonString = web.DownloadString("https://helix.dot.net/api/jobs/65b51f3b-f3ac-4996-8e54-4281c26cfada/workitems?api-version=2019-06-17");
        //string jsonString = web.DownloadString("https://helix.dot.net/api/jobs/11a4dfe1-7514-4d8f-ab60-e256a036c941/workitems?api-version=2019-06-17");
        //string jsonString = web.DownloadString("https://helix.dot.net/api/jobs/c8663be1-34e8-4e84-89d4-a65c46c35e24/workitems?api-version=2019-06-17");
        string jsonString = web.DownloadString("https://helix.dot.net/api/jobs/a6165609-1b6c-4a02-a846-39e7f7931192/workitems?api-version=2019-06-17");
        ResultDetails[] results = JsonSerializer.Deserialize<ResultDetails[]>(jsonString);
        //Console.WriteLine($"Results: {results.Length}");
        foreach(var result in results)
        {
            var jobResult = JsonSerializer.Deserialize<JobResults>(web.DownloadString(result.DetailsUrl));
            StringBuilder builder = new StringBuilder($"{jobResult.Name},{jobResult.State}");
            if (jobResult.ConsoleOutputUri != null)
            {
                string testResult = web.DownloadString(jobResult.ConsoleOutputUri);
                var helixConsoleResults = testResult.Split(new[] { '\r', '\n' });
                (int tests, int errors, int failures, int skip, string testTime, bool isCompiled) = GetTestRunInfo3(helixConsoleResults);
                builder.Append($",T,{tests},{errors},{failures}");
                //Console.WriteLine(jobResult.ConsoleOutputUri);
            }
            else
            {
                builder.Append($",F,0,-1,-1");
            }
            outList.Add(builder.ToString());
            //if (!jobResult.State.Equals("Passed"))
            //    Console.WriteLine($"{jobResult.State}");
            //if(jobResult.Files.Length>0 && jobResult.Files[0].Result_FileName!=null)
            //    Console.WriteLine($"{jobResult.State}");
            //if (jobResult.Files.Length>1 && jobResult.Files[1].Result_FileName!=null)
            //    Console.WriteLine($"{jobResult.State}");
        }
        //Console.WriteLine($"Results: {results.Length}");
        File.WriteAllLines(@"D:\work\Core\Test\NativeAot\5_29\HelixResults\test5.txt", outList);

    }

    private static (int tests, int errors, int failures, int skip, string testTime, bool isCompiled) GetTestRunInfo3(String[] results)
    {
        foreach (String line in results)
        {
            if (line.IndexOf("Tests run: ")>=0)
            {
                //   Tests run: 22226, Errors: 0, Failures: 38, Skipped: 13. Time: 1.7034817s
                string[] values = line.Split(',', StringSplitOptions.TrimEntries);
                string[] values1 = values[3].Split('.', StringSplitOptions.TrimEntries);
                return (int.Parse(values[0].Split(':', StringSplitOptions.TrimEntries)[1]),
                    int.Parse(values[1].Split(':', StringSplitOptions.TrimEntries)[1]),
                    int.Parse(values[2].Split(':', StringSplitOptions.TrimEntries)[1]),
                    int.Parse(values1[0].Split(':', StringSplitOptions.TrimEntries)[1]),
                    values1[1].Split(':', StringSplitOptions.TrimEntries)[1], true);
            }
        }
        return (-1, -1, -1, -1, "-1", false);
    }


    static void M12()
    {
        // Tests do not build
        // D:\work\Core\Test\NativeAot\5_29\Investigate\NotBuild.txt

        // D:\work\Core\Test\NativeAot\5_29\All_Test_Projs.txt
        // D:\work\Core\CurrentWork3\runtime\src\libraries\Microsoft.Diagnostics.Tracing.EventSource.Redist\tests\Microsoft.Diagnostics.Tracing.EventSource.Redist.Tests.csproj

        // <NativeAotFailingAsemblies Include="$(MSBuildThisFileDirectory)System.Diagnostics.Process\tests\System.Diagnostics.Process.Tests.csproj" />
        HashSet<string> excludeSet = File.ReadLines(@"D:\work\Core\Test\NativeAot\5_29\Investigate\temp2.txt").Select(x => x).ToHashSet();
        List<string> outList = new();
        foreach (String line in File.ReadLines(@"D:\work\Core\Test\NativeAot\5_29\All_Test_Projs.txt"))
        {
            string woProject = $"{Path.GetFileNameWithoutExtension(line)}";
            string woTest = $"{Path.GetFileNameWithoutExtension(woProject)}";
            string outLib = @$"D:\work\Core\Test\NativeAot\5_29\out\{woTest}_test.txt";
            string newLib = @$"D:\work\Core\Test\NativeAot\5_29\newlib\{woProject}_test.txt";
            string buildFailures = "Failed";
            if (File.Exists(outLib))
            {
                (int tests, int errors, int failures, int skip, string testTime, bool isCompiled) = GetTestRunInfo2(outLib!);
                buildFailures = isCompiled.ToString();
            }
            else if (File.Exists(newLib))
            {
                (int tests, int errors, int failures, int skip, string testTime, bool isCompiled) = GetTestRunInfo2(newLib!);
                buildFailures = isCompiled.ToString();
            }
            if (!buildFailures.Equals("true", StringComparison.OrdinalIgnoreCase) && !excludeSet.Contains(woTest))
            {
                outList.Add($"<NativeAotFailingAsemblies Include=\"$(MSBuildThisFileDirectory){line.Replace(@"D:\work\Core\CurrentWork3\runtime\src\libraries\", "")}\" />");
            }
            if (excludeSet.Contains(woTest))
            {
                Console.WriteLine(line);
            }
        }
        File.WriteAllLines(@"D:\work\Core\Test\NativeAot\5_29\TempTestProj2.txt", outList);
    }

    static void M10_1()
    {
        string f1 = @"D:\work\Core\Test\NativeAot\5_29\Investigate\temp.txt";
        List<string> outList = new();
        foreach (String line in File.ReadLines(f1))
        {
            outList.Add(Path.GetFileName(line));
        }
        File.WriteAllLines(@"D:\work\Core\Test\NativeAot\5_29\Investigate\temp2.txt", outList);
    }

    static void M11()
    {
        // Failing tests
        // D:\work\Core\Test\NativeAot\5_29\Investigate\Fail.txt

        // D:\work\Core\Test\NativeAot\5_29\All_Test_Projs.txt
        // D:\work\Core\CurrentWork3\runtime\src\libraries\Microsoft.Diagnostics.Tracing.EventSource.Redist\tests\Microsoft.Diagnostics.Tracing.EventSource.Redist.Tests.csproj

        // <NativeAotFailingAsemblies Include="$(MSBuildThisFileDirectory)System.Diagnostics.Process\tests\System.Diagnostics.Process.Tests.csproj" />
        HashSet<string> excludeSet = File.ReadLines(@"D:\work\Core\Test\NativeAot\5_29\Investigate\temp2.txt").Select(x=>x).ToHashSet();
        List<string> outList = new();
        foreach (String line in File.ReadLines(@"D:\work\Core\Test\NativeAot\5_29\All_Test_Projs.txt"))
        {
            string woProject = $"{Path.GetFileNameWithoutExtension(line)}";
            string woTest = $"{Path.GetFileNameWithoutExtension(woProject)}";
            string outLib = @$"D:\work\Core\Test\NativeAot\5_29\out\{woTest}_test.txt";
            string newLib = @$"D:\work\Core\Test\NativeAot\5_29\newlib\{woProject}_test.txt";
            int buildButFailures= -1;
            if (File.Exists(outLib))
            {
                (int tests, int errors, int failures, int skip, string testTime, bool isCompiled) = GetTestRunInfo2(outLib!);
                buildButFailures = failures;
            }
            else if (File.Exists(newLib))
            {
                (int tests, int errors, int failures, int skip, string testTime, bool isCompiled) = GetTestRunInfo2(newLib!);
                buildButFailures = failures;
            }
            if(buildButFailures>0 && !excludeSet.Contains(woTest))
            {
                outList.Add($"<NativeAotFailingAsemblies Include=\"$(MSBuildThisFileDirectory){line.Replace(@"D:\work\Core\CurrentWork3\runtime\src\libraries\", "")}\" />");
            }
            if(excludeSet.Contains(woTest))
            {
                Console.WriteLine(line);
            }
        }
        File.WriteAllLines(@"D:\work\Core\Test\NativeAot\5_29\TempTestProj.txt", outList);
    }

    static void M10()
    {
        // Check if all the test projects have been run
        
        // D:\work\Core\Test\NativeAot\5_29\All_Test_Projs.txt
        // D:\work\Core\CurrentWork3\runtime\src\libraries\Microsoft.Diagnostics.Tracing.EventSource.Redist\tests\Microsoft.Diagnostics.Tracing.EventSource.Redist.Tests.csproj
        
        // D:\work\Core\Test\NativeAot\5_29\out
        // D:\work\Core\Test\NativeAot\5_29\out\System.Security.Cryptography.Csp_test.txt

        // D:\work\Core\Test\NativeAot\5_29\newlib
        // D:\work\Core\Test\NativeAot\5_29\newlib\Microsoft.Extensions.Configuration.CommandLine.Tests_test.txt

        List<string> outList = new();
        outList.Add($"library,DidTestRun,TestTime,TotalTests,Errors,Failures,Skipped");
        foreach (String line in File.ReadLines(@"D:\work\Core\Test\NativeAot\5_29\All_Test_Projs.txt"))
        {
            string woProject = $"{Path.GetFileNameWithoutExtension(line)}";
            string woTest = $"{Path.GetFileNameWithoutExtension(woProject)}";
            string outLib = @$"D:\work\Core\Test\NativeAot\5_29\out\{woTest}_test.txt";
            string newLib = @$"D:\work\Core\Test\NativeAot\5_29\newlib\{woProject}_test.txt";
            if (File.Exists(outLib))
            {
                (int tests, int errors, int failures, int skip, string testTime, bool isCompiled) = GetTestRunInfo2(outLib!);
                outList.Add($"{woTest},{isCompiled},{testTime},{tests},{errors},{failures},{skip}");
            }
            else if(File.Exists(newLib))
            {
                (int tests, int errors, int failures, int skip, string testTime, bool isCompiled) = GetTestRunInfo2(newLib!);
                outList.Add($"{woTest},{isCompiled},{testTime},{tests},{errors},{failures},{skip}");
            }
            else
            {
                outList.Add($"{woTest},-1,-1,-1,-1,-1,-1");
            }
        }
        File.WriteAllLines(@"D:\work\Core\Test\NativeAot\5_29\TestsAnalysisSummary.txt", outList);
    }

    private static (int tests, int errors, int failures, int skip, string testTime, bool isCompiled) GetTestRunInfo2(string fileName)
    {
        foreach (String line in File.ReadLines(fileName))
        {
            if (line.IndexOf("  Tests run: ")>=0)
            {
                //   Tests run: 22226, Errors: 0, Failures: 38, Skipped: 13. Time: 1.7034817s
                string[] values = line.Split(',', StringSplitOptions.TrimEntries);
                string[] values1 = values[3].Split('.', StringSplitOptions.TrimEntries);
                return (int.Parse(values[0].Split(':', StringSplitOptions.TrimEntries)[1]),
                    int.Parse(values[1].Split(':', StringSplitOptions.TrimEntries)[1]),
                    int.Parse(values[2].Split(':', StringSplitOptions.TrimEntries)[1]),
                    int.Parse(values1[0].Split(':', StringSplitOptions.TrimEntries)[1]),
                    values1[1].Split(':', StringSplitOptions.TrimEntries)[1], true);
            }
        }
        return (-1, -1, -1, -1, "-1", false);
    }



    static void M9()
    {
        List<string> outList = new();
        foreach (String line in File.ReadLines(@"D:\work\Core\Test\NativeAot\5_29\libraries_new_proj.txt"))
        {
            outList.Add($"{Path.GetDirectoryName(line)} {Path.GetFileNameWithoutExtension(line)}");
        }
        File.WriteAllLines(@"D:\work\Core\Test\NativeAot\5_29\libraries_new_proj2.txt", outList);
    }

    static void M8()
    {
        HashSet<string> alreadyDone = File.ReadLines(@"D:\work\Core\Test\NativeAot\5_29\clean_libraries.txt").Select(x => x).ToHashSet();
        HashSet<string> needsToBeDone = File.ReadLines(@"D:\work\Core\Test\NativeAot\5_29\All_Test_Projs.txt").Select(x => x).ToHashSet();
        List<string> outList = new();
        foreach (String ad in alreadyDone)
        {
            // lazy
            bool foundLib = false;
            string lineToRemove = null;
            foreach(string nd in needsToBeDone)
            {
                if(nd.IndexOf(ad)>=0)
                {
                    foundLib = true;
                    lineToRemove=nd;
                    break;
                }
            }
            if (foundLib)
            {
                needsToBeDone.Remove(lineToRemove);
            }
            else
            {
                outList.Add(ad);
            }
            //outList.Add($"<NativeAotFailingAsemblies Include=\"$(MSBuildThisFileDirectory){line}\\tests\\{line}.Tests.csproj\" />");
        }
        File.WriteAllLines(@"D:\work\Core\Test\NativeAot\5_29\libraries_new_proj.txt", needsToBeDone);
        File.WriteAllLines(@"D:\work\Core\Test\NativeAot\5_29\libraries_new_missing.txt", outList);
    }


    static void M7()
    {
        //        <NativeAotFailingAsemblies Include="$(MSBuildThisFileDirectory)Microsoft.Win32.Primitives\tests\Microsoft.Win32.Primitives.Tests.csproj" />
        // D:\work\Core\Test\NativeAot\5_29\libraries_zero_fail.txt
        List<string> outList = new();
        foreach (String line in File.ReadLines(@"D:\work\Core\Test\NativeAot\5_29\libraries_skip.txt"))
        {
            outList.Add($"<NativeAotFailingAsemblies Include=\"$(MSBuildThisFileDirectory){line}\\tests\\{line}.Tests.csproj\" />");
        }
        File.WriteAllLines(@"D:\work\Core\Test\NativeAot\5_29\libraries_skip_tests_proj.txt", outList);
    }

    static void M6()
    {
        //    <ProjectExclusions Remove="$(MSBuildThisFileDirectory)System.Globalization.Calendars\tests\System.Globalization.Calendars.Tests.csproj" />
        // D:\work\Core\Test\NativeAot\5_29\libraries_zero_fail.txt
        List<string> outList = new();
        foreach (String line in File.ReadLines(@"D:\work\Core\Test\NativeAot\5_29\libraries_zero_fail.txt"))
        {
            // System.Diagnostics.Contracts
            outList.Add($"<ProjectExclusions Remove=\"$(MSBuildThisFileDirectory){line}\\tests\\{line}.Tests.csproj\" />");
        }
        File.WriteAllLines(@"D:\work\Core\Test\NativeAot\5_29\libraries_zero_fail_tests_proj.txt", outList);
    }

    static void M5()
    {
        List<string> outList = new();
        Dictionary<string, DateTime> map = new Dictionary<string, DateTime>();
        //[START] System.Collections.Concurrent.Tests.ConcurrentStackTests.IEnumerable_Generic_Enumerator_Current_ReturnsSameValueOnRepeatedCalls(count: 75):5/30/2022 12:41:50 AM
        //[END] System.Collections.Concurrent.Tests.ConcurrentStackTests.IEnumerable_Generic_Enumerator_Current_ReturnsSameValueOnRepeatedCalls(count: 75):5/30/2022 12:41:50 AM
        foreach (String line in File.ReadLines(@"D:\work\Core\Test\NativeAot\5_29\investigate\System.Collections.Concurrent_test.txt"))
        {
            string[] values = line.Split("###", StringSplitOptions.TrimEntries|StringSplitOptions.RemoveEmptyEntries);
            if(values.Length > 0)
            {
                if (values[0].Equals("[START]"))
                {
                    outList.Add(values[1]);

                    int mapCount = 0;
                    while(!map.TryGetValue($"{values[1]}{mapCount++}", out DateTime dt))
                    {
                        map.Add($"{values[1]}{--mapCount}", DateTime.Parse(values[2]));
                    }

                }
                if (values[0].Equals("[END]"))
                {
                    outList.Remove(values[1]);
                    int mapCount = 0;
                    while (map.TryGetValue($"{values[1]}{mapCount++}", out DateTime dt))
                    {
                        map[$"{values[1]}{--mapCount}"] = DateTime.Now.Add(DateTime.Parse(values[2]).Subtract(dt));
                        break;
//                        map.Add($"{values[1]}{--mapCount}", DateTime.Parse(values[2]));
                    }
                }
            }
        }

        File.WriteAllLines(@"D:\work\Core\Test\NativeAot\5_29\investigate\test_Cases.txt", outList);
        File.WriteAllLines(@"D:\work\Core\Test\NativeAot\5_29\investigate\test_Cases_time.txt", map.OrderByDescending(x=>x.Value)
            .Take(5).Select(x=>String.Join(",", x.Key, (x.Value.Subtract(DateTime.Now).TotalSeconds))));

    }

    static void M4()
    {
        List<string> outList = new();
        string libName=null;
        TimeSpan startTime=default;
        //START: System.Collections Sun 05/29/2022  4:05:59.44 
        //END: System.Collections Sun 05/29/2022  4:07:04.75 
        outList.Add($"library,DidTestRun,TotalTime,TestTime,TotalTests,Errors,Failures,Skipped");
        foreach (String line in File.ReadLines(@"D:\work\Core\Test\NativeAot\5_29\Full_Summary.txt"))
        {
            string[] values = line.Split(' ', StringSplitOptions.TrimEntries|StringSplitOptions.RemoveEmptyEntries);
            if (values[0].Equals("START:"))
            {
                libName = values[1];
                startTime = TimeSpan.Parse(values[4]);
            }
            if (values[0].Equals("END:"))
            {
                (int tests, int errors, int failures, int skip, string testTime, bool isCompiled) = GetTestRunInfo(libName!);
                outList.Add($"{libName},{isCompiled},{TimeSpan.Parse(values[4]).Subtract(startTime).TotalSeconds},{testTime},{tests},{errors},{failures},{skip}");
            }
        }
        File.WriteAllLines(@"D:\work\Core\Test\NativeAot\5_29\lib_summary.txt", outList);
    }

    private static (int tests, int errors, int failures, int skip, string testTime, bool isCompiled) GetTestRunInfo(string libName)
    {
        foreach (String line in File.ReadLines($@"D:\work\Core\Test\NativeAot\5_29\out\{libName}_test.txt"))
        {
            if(line.IndexOf("  Tests run: ")>=0)
            {
                //   Tests run: 22226, Errors: 0, Failures: 38, Skipped: 13. Time: 1.7034817s
                string[] values = line.Split(',', StringSplitOptions.TrimEntries);
                string[] values1 = values[3].Split('.', StringSplitOptions.TrimEntries);
                return (int.Parse(values[0].Split(':', StringSplitOptions.TrimEntries)[1]), 
                    int.Parse(values[1].Split(':', StringSplitOptions.TrimEntries)[1]),
                    int.Parse(values[2].Split(':', StringSplitOptions.TrimEntries)[1]),
                    int.Parse(values1[0].Split(':', StringSplitOptions.TrimEntries)[1]),
                    values1[1].Split(':', StringSplitOptions.TrimEntries)[1], true);
            }
        }
        return (-1, -1, -1, -1, "-1", false);
    }

    static void M3()
    {
        List<string> outList = new();
        HashSet<string> set = File.ReadLines(@"D:\work\Core\Test\NativeAot\5_29\list.txt").Select(x => Path.GetFileNameWithoutExtension(x)).ToHashSet();
        foreach (String line in File.ReadLines(@"D:\work\Core\Test\NativeAot\5_29\dir_libraries.txt"))
        {
            Console.WriteLine(line);
            if (set.Contains(line))
            {
                outList.Add(line);
            }
        }
        File.WriteAllLines(@"D:\work\Core\Test\NativeAot\5_29\clean_libraries.txt", outList);
    }

    static void M1()
    {
        //List<string> outLine = new();
        Dictionary<string, int> map = new Dictionary<string, int>();
        foreach (String line in File.ReadLines(@"D:\work\Core\Test\NativeAot\5_19\tests\aot_test_linq3.txt"))
        {
            if (line.IndexOf("System.Reflection.MissingMetadataException : ")>=0)
            {
                int index=line.IndexOf("'");
                var method = line.Substring(index+1, (line.LastIndexOf("'")-index-1));
                if (!map.TryGetValue(method, out int value))
                {
                    map.Add(method, value = 0);
                }
                map[method]++;
                //outLine.Add(method);
            }
        }
        File.WriteAllLines(@"D:\work\Core\Test\NativeAot\5_19\tests\aot_error_9.txt", map.OrderByDescending(x => x.Value).Select(x => String.Join(",", x.Key, x.Value)));
        File.WriteAllLines(@"D:\work\Core\Test\NativeAot\5_19\tests\aot_error_10.txt", map.OrderByDescending(x => x.Value).Select(x => x.Key));
    }
    static void M2()
    {
        /**
         
System.Runtime.CompilerServices.CallSite<System.Func<System.Runtime.CompilerServices.CallSite,System.Int32,System.Int32,System.Object>>
<Type Name="System.Runtime.CompilerServices.CallSite`1[[System.Func`4[[System.Runtime.CompilerServices.CallSite],[System.Int32,System.Private.CoreLib],[System.Int32,System.Private.CoreLib],[System.Int32,System.Private.CoreLib]],System.Private.CoreLib]]" Dynamic="Required All" />



         **/
        List<string> outLines = new();
        foreach (String line1 in File.ReadLines(@"D:\work\Core\Test\NativeAot\5_19\tests\aot_error_8.txt"))//@"D:\work\Core\Test\NativeAot\5_19\tests\aot_test_linq2.txt"))
        {
            StringBuilder builder = new StringBuilder("<Type Name=\"");

            string line = line1;
            int index = line.IndexOf('<');
            var type = line.Substring(0, index);
            line = line.Substring(index+1);
            //System.Runtime.CompilerServices.CallSite`1
            builder.Append($"{type}`1[[");

            index = line.IndexOf('<');
            type = line.Substring(0, index);
            line = line.Substring(index+1);
            int num = 0;
            while((index=line.IndexOf(',', index+1))>=0)
            {
                num++;
            }
            builder.Append($"{type}`{num}[");

            while ((index=line.IndexOf(','))>=0)
            {
                type = line.Substring(0, index);
                line = line.Substring(index+1);
                builder.Append($"[{type}");
                if (!type.Equals("System.Runtime.CompilerServices.CallSite"))
                {
                    builder.Append($",System.Private.CoreLib");
                }
                builder.Append("]");
                if(line.IndexOf(',')>=0)
                    builder.Append(",");
            }
            builder.Append("],System.Private.CoreLib]]\" Dynamic=\"Required All\" />");
            outLines.Add(builder.ToString());

        }
        File.WriteAllLines(@"D:\work\Core\Test\NativeAot\5_19\tests\aot_error_21.txt", outLines);
    }
}

