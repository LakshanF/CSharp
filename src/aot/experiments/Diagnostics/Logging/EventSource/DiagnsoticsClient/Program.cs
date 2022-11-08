using System.Diagnostics.Tracing;

using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Please enter the program id:");
        int intPid = Convert.ToInt32(Console.ReadLine());
        
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