// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using Xunit.ConsoleClient;

#nullable enable
public class SingleFileTestRunner : XunitTestFramework
{
    private SingleFileTestRunner(IMessageSink messageSink)
    : base(messageSink) { }

    public static int Main(string[] args)
    {
            // Validate the command line and parse out the relevant pieces.
        if (!TryParseCommandLine(args, out string testAssemblyPath, out string runtimeAssembliesPath, out string outputPath, out Xunit.ConsoleClient.CommandLine? xunitCommandLine))
        {
            return -1;
        }

        var asm = typeof(SingleFileTestRunner).Assembly;
        Console.WriteLine("Running assembly:" + asm.FullName);

        var diagnosticSink = new ConsoleDiagnosticMessageSink();
        var testsFinished = new TaskCompletionSource();
        var testSink = new TestMessageSink();
        var summarySink = new DelegatingExecutionSummarySink(testSink,
            () => false,
            (completed, summary) => Console.WriteLine($"Tests run: {summary.Total}, Errors: {summary.Errors}, Failures: {summary.Failed}, Skipped: {summary.Skipped}. Time: {TimeSpan.FromSeconds((double)summary.Time).TotalSeconds}s"));
        var resultsXmlAssembly = new XElement("assembly");
        var resultsSink = new DelegatingXmlCreationSink(summarySink, resultsXmlAssembly);

        testSink.Execution.TestSkippedEvent += args => { Console.WriteLine($"[SKIP] {args.Message.Test.DisplayName}"); };
        testSink.Execution.TestFailedEvent += args => { Console.WriteLine($"[FAIL] {args.Message.Test.DisplayName}{Environment.NewLine}{Xunit.ExceptionUtility.CombineMessages(args.Message)}{Environment.NewLine}{Xunit.ExceptionUtility.CombineStackTraces(args.Message)}"); };

        testSink.Execution.TestAssemblyFinishedEvent += args =>
        {
            Console.WriteLine($"Finished {args.Message.TestAssembly.Assembly}{Environment.NewLine}");
            testsFinished.SetResult();
        };

        var xunitTestFx = new SingleFileTestRunner(diagnosticSink);
        var asmInfo = Reflector.Wrap(asm);
        var asmName = asm.GetName();

        var discoverySink = new TestDiscoverySink();
        var discoverer = xunitTestFx.CreateDiscoverer(asmInfo);
        discoverer.Find(false, discoverySink, TestFrameworkOptions.ForDiscovery());
        discoverySink.Finished.WaitOne();
        XunitFilters filters = new XunitFilters();
        

        XunitFilters xunitFilters = xunitCommandLine!.Project.Filters;
        var ex1 = xunitFilters.ExcludedTraits;
        Console.WriteLine("-------------EXCLUDE-----------");
        foreach(var ek in ex1.Keys)
        {
            foreach(var ev1 in ex1[ek])
            {
                Console.WriteLine($"<{ek}><{ev1}>");
            }
        }
        Console.WriteLine("-------------EXCLUDE-----------");
        var en1 = xunitFilters.IncludedTraits;
        Console.WriteLine("-------------ICNLUDE-----------");
        foreach(var ek in en1.Keys)
        {
            foreach(var ev1 in en1[ek])
            {
                Console.WriteLine($"<{ek}><{ev1}>");
            }
        }
        Console.WriteLine("-------------ICNLUDE-----------");

        // CI systems will likely timeout on outer loop tests        
        var excludeTraits = new List<string> { "failing" };
        if(args.Where(arg => arg.Contains("IgnoreForCI", StringComparison.OrdinalIgnoreCase)).Any())
            excludeTraits.Add("OuterLoop");
        filters.ExcludedTraits.Add("category", excludeTraits);
        var filteredTestCases = discoverySink.TestCases.Where(filters.Filter).ToList();
        var executor = xunitTestFx.CreateExecutor(asmName);
        executor.RunTests(filteredTestCases, resultsSink, TestFrameworkOptions.ForExecution());

        resultsSink.Finished.WaitOne();

        var failed = resultsSink.ExecutionSummary.Failed > 0 || resultsSink.ExecutionSummary.Errors > 0;
        return failed ? 1 : 0;
    }

        private static readonly string[] s_defaultXunitOptions = new string[]
        {
            "-notrait", "category=nonnetcoreapptests",
            "-notrait", "category=nonwindowstests",
            "-notrait", "category=IgnoreForCI",
            "-notrait", "category=failing",
            "-notrait", "category=OuterLoop"
        };


        /// <summary>Parse the command-line.</summary>
        /// <param name="args">The arguments passed to Main.</param>
        /// <param name="testAssemblyPath">The location of the xunit test assembly to be analyzed. The resulting .cs file will call into this assembly.</param>
        /// <param name="runtimeAssembliesPath">The directory containing all of the helper assemblies needed, e.g. xunit's assemblies, .NET Core utility assemblies, etc.</param>
        /// <param name="outputPath">The directory into which the resulting project should be written.</param>
        /// <param name="xunitCommandLine">The xunit command-line object to pass to xunit test discovery.</param>
        /// <returns></returns>
        private static bool TryParseCommandLine(
            string[] args,
            out string testAssemblyPath,
            out string runtimeAssembliesPath,
            out string outputPath,
            out Xunit.ConsoleClient.CommandLine? xunitCommandLine)
        {
            if (args.Length >= 3)
            {
                static string EnsureEndsWithSeparator(string path) =>
                    !path.EndsWith(Path.DirectorySeparatorChar) && !path.EndsWith(Path.AltDirectorySeparatorChar) ?
                    path + Path.DirectorySeparatorChar :
                    path;

                runtimeAssembliesPath = EnsureEndsWithSeparator(args[1]);
                testAssemblyPath = Path.GetFullPath(args[2]);
                outputPath = EnsureEndsWithSeparator(Path.Combine(args[0], Path.GetFileNameWithoutExtension(testAssemblyPath)));

                // Gather arguments for xunit.
                var argsForXunit = new List<string>();
                argsForXunit.Add(testAssemblyPath); // first argument is the test assembly
                foreach (string extraArg in args.Skip(3))
                {
                    // If an argument is a response file, load its contents and add that instead.
                    if (extraArg.StartsWith("@"))
                    {
                        argsForXunit.AddRange(from line in File.ReadAllLines(extraArg.Substring(1))
                                              where line.Length > 0 && line[0] != '#'
                                              from part in line.Split(' ')
                                              select part);
                    }
                    else
                    {
                        // Otherwise, add the argument as-is.
                        argsForXunit.Add(extraArg);
                    }
                }

                // If the only argument added was the test assembly path, use default arguments.
                if (argsForXunit.Count == 1)
                {
                    argsForXunit.AddRange(s_defaultXunitOptions);
                }

                // Finally, hand off these arguments to xunit.
                xunitCommandLine = Xunit.ConsoleClient.CommandLine.Parse(argsForXunit.ToArray());

                // Log($"Test assembly path    : {testAssemblyPath}");
                // Log($"Helper assemblies path: {runtimeAssembliesPath}");
                // Log($"Output path           : {outputPath}");
                // Log($"Xunit arguments       : {string.Join(" ", argsForXunit)}");
                // Log("");
                return true;
            }

            // Invalid command line arguments.
            Console.WriteLine("Usage: <output_directory> <helper_assemblies_directory> <test_assembly_path> <xunit_console_options>");
            Console.WriteLine("    Example:");
            Console.WriteLine(@"   dotnet run d:\tmpoutput d:\repos\runtime\artifacts\bin\testhost\net7.0-windows-Debug-x64\shared\Microsoft.NETCore.App\7.0.0\ d:\repos\runtime\artifacts\bin\System.Runtime.Tests\net7.0-windows-Debug\System.Runtime.Tests.dll");
            testAssemblyPath = string.Empty;
            runtimeAssembliesPath = string.Empty;
            outputPath = string.Empty;
            xunitCommandLine = null;
            return false;
        }

}

internal class ConsoleDiagnosticMessageSink : IMessageSink
{
    public bool OnMessage(IMessageSinkMessage message)
    {
        if (message is IDiagnosticMessage diagnosticMessage)
        {
            return true;
        }
        return false;
    }
}
#nullable disable