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
        var intPid = Process.GetProcessesByName(processName).Single().Id;

        Console.WriteLine($"Starting an StartEventPipeSession for Process:<{processName}>, PID:{intPid}");

        var retCode = PrintEventsLive(intPid);

        Console.WriteLine($"retCode:{retCode} - ");
    }

    public static int PrintEventsLive(int processId)
    {
        int retCode = -1;
        bool managedEvent = false, nativeEvent = false;
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
                source.Dynamic.All += (TraceEvent data) =>
                {
                    Console.WriteLine($"Got event, Name: {data.EventName}, Details: {data}");
                    if(data.ProviderName=="Demo" && 
                        data.EventName == "AppStarted" &&
                        data.PayloadNames.Length == 2 &&
                        (string)data.PayloadByName(data.PayloadNames[0])=="Hello World From .NET!" &&
                        (int)data.PayloadByName(data.PayloadNames[1])==12)
                    {
                        managedEvent=true;
                    }
                    if (data.ProviderName=="Microsoft-DotNETCore-EventPipe" && data.EventName == "ProcessInfo")
                    {
                        nativeEvent = true;
                    }
                };
                try
                {
                   source.Process();
                }
                catch (Exception e)
                {
                    retCode = 1;
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
        if (managedEvent && nativeEvent)
            retCode = 100;
        return retCode;
    }
}
