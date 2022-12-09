using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;

using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing;

namespace EP_Client;

public class Program
{
    public static void Main(string[] args)
    {
        string processName = "reproNative";
        if (args.Length > 0)
        {
            processName = args[0];
        }
        Console.WriteLine("Assumes ProcessName is <target_process>");
        var intPid = Process.GetProcessesByName("target_process").Single().Id;

        Console.WriteLine($"Starting an StartEventPipeSession for Process:<{processName}>, PID:{intPid}");

        PrintEventsLive(intPid);
    }

    public static void PrintEventsLive(int processId)
    {
        var providers = new List<EventPipeProvider>()
        {
            new EventPipeProvider("Demo",
                EventLevel.Verbose, (long)ClrTraceEventParser.Keywords.Default)
        };
        var client = new DiagnosticsClient(processId);
        using (var session = client.StartEventPipeSession(providers, false))
        {

            Task streamTask = Task.Run(() =>
            {
                var source = new EventPipeEventSource(session.EventStream);
                source.Dynamic.All += (TraceEvent obj) =>
                {
                    Console.WriteLine($"Got event, Name: {obj.EventName}, Details: {obj}");
                };
                try
                {
                    source.Process();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error encountered while processing events");
                    Console.WriteLine(e.ToString());
                }
            });

            Task inputTask = Task.Run(() =>
            {
                Console.WriteLine("Press Enter to exit");
                while (Console.ReadKey().Key != ConsoleKey.Enter)
                {
                    Thread.Sleep(100);
                }
                session.Stop();
            });

            Task.WaitAny(streamTask, inputTask);
        }
    }
}
