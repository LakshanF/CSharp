﻿using System.Diagnostics.Tracing;

namespace EventSourceDemo
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Please enter to continue:");
            Console.ReadLine();

            DemoEventSource.Log.AppStarted("Hello World From .NET!", 12);
            DemoEventSource.Log.DebugMessage("Got here From .NET!");
            DemoEventSource.Log.DebugMessage("finishing startup From .NET!");
            DemoEventSource.Log.RequestStart(3);
            DemoEventSource.Log.RequestStop(3);

            Console.WriteLine("Done done!");
        }
    }

    [EventSource(Name = "Demo")]
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