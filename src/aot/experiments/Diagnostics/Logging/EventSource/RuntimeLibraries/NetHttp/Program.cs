using System;
using System.Net.Http;
using System.Diagnostics.Tracing;

namespace MyApp // Note: actual namespace depends on the project name.
{

    // sealed class EventSourceCreatedListener : EventListener
    // {
    //     protected override void OnEventSourceCreated(EventSource eventSource)
    //     {
    //         base.OnEventSourceCreated(eventSource);
    //         Console.WriteLine($"New event source: {eventSource.Name}");
    //     }
    // }    
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // EventSourceCreatedListener el = new EventSourceCreatedListener();
            Console.WriteLine("Waiting 10 seconds to client to get the PID");
            Thread.Sleep(10*1000);
            await M1();
            Console.WriteLine("Waiting for tool to do its thing");
            Thread.Sleep(10*1000);
            Console.WriteLine("Completed");
        }

        static async Task M1()
        {
            using var httpClient = new HttpClient();
            string response = await httpClient.GetStringAsync("http://redhatloves.net/");
            Console.WriteLine($"Received response with length {response.Length}");
        }
    }
}