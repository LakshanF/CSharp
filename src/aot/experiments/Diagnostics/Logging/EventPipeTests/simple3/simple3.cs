// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.Tracing;
using System.Threading;

namespace Tracing.Tests.Simple3
{
    public sealed class LaksDemoEventSource : EventSource
    {
        private LaksDemoEventSource() {}
        public static LaksDemoEventSource Log = new LaksDemoEventSource();
        public void MyEvent() { WriteEvent(1, "MyEvent"); }
    }

    public class ProviderValidation3
    {
        public static int Main()
        {
            // This test (temp?) validates event behavior via dotnet-trace and perfview

            Console.WriteLine("Waiting 10 seconds to client to get the PID");
            Thread.Sleep(10*1000);

            GC.Collect();
            for (int i = 0; i < 100; i++)
            {                
                if (i % 10 == 0)
                    Console.WriteLine($"Fired MyEvent {i:N0}/100,000 times...");
                LaksDemoEventSource.Log.MyEvent();
            }
            GC.Collect();

            return 100;
        }
    }
}
