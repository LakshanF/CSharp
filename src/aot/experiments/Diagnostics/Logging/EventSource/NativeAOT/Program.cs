using System;
using System.Diagnostics.Tracing;
using System.Threading;

namespace EventSourceDemo
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Waiting 10 seconds to client to get the PID");
            Thread.Sleep(10*1000);
            DemoEventSource.Log.AppStarted("Hello World From NativeAOT!", 12);
            DemoEventSource.Log.DebugMessage("Got here From NativeAOT");
            DemoEventSource.Log.DebugMessage("finishing startup From NativeAOT");
            DemoEventSource.Log.RequestStart(3);
            DemoEventSource.Log.RequestStop(3);
            Console.WriteLine("Done!");
        }
    }

    class DemoEventSource : EventSource
    {
        public static DemoEventSource Log { get; } = new DemoEventSource();

        [Event(1, Keywords = Keywords.Startup)]
        public void AppStarted(string message, int favoriteNumber) => WriteEvent(1, message, favoriteNumber);
        [Event(2, Keywords = Keywords.Requests)]
        public void RequestStart(int requestId) => WriteEvent(2, requestId);
        [Event(3, Keywords = Keywords.Requests)]
        public void RequestStop(int requestId) => WriteEvent(3, requestId);
        [Event(4, Keywords = Keywords.Startup, Level = EventLevel.Verbose)]
        public void DebugMessage(string message) => WriteEvent(4, message);


        public class Keywords
        {
            public const EventKeywords Startup = (EventKeywords)0x0001;
            public const EventKeywords Requests = (EventKeywords)0x0002;
        }
    }
}