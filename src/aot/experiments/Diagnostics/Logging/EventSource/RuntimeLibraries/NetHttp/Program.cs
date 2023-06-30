using System;
using System.Net.Http;
using System.Diagnostics.Tracing;

namespace MyApp // Note: actual namespace depends on the project name.
{

    public sealed class LaksDemoEventSource : EventSource
    {
        private LaksDemoEventSource() {}
        public static LaksDemoEventSource Log = new LaksDemoEventSource();
        public void MyEvent() { WriteEvent(1, "MyEvent"); }
    }


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
            M2();
            Console.WriteLine("Waiting for tool to do its thing");
            Thread.Sleep(10*1000);
            Console.WriteLine("Completed");
        }

        static void M2()
        {
            GC.Collect();
            List<Foo> list = new List<Foo>();

            for (int i = 0; i < 100000; i++)
            {
                list.Add(new Foo(i));
                if (i % 10000 == 0)
                    Console.WriteLine($"Fired MyEvent {i:N0}/100,000 times...");
                LaksDemoEventSource.Log.MyEvent();
            }
            int rndValue = new Random().Next(0, 100000);
            Console.WriteLine($"{list[rndValue].IValue}-{list[rndValue].SValue}");
            GC.Collect();
        }

        static async Task M1()
        {
            using var httpClient = new HttpClient();
            string response = await httpClient.GetStringAsync("http://redhatloves.net/");
            Console.WriteLine($"Received response with length {response.Length}");
        }
    }
    public class Foo
    {
        public int IValue{get;set;}
        public string SValue{get;set;}
        public Foo(int value)
        {
            IValue = value;
            SValue = value.ToString();
        }
    }

}