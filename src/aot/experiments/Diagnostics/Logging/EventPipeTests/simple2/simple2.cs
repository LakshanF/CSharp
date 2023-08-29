// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.Tracing;
using System.Threading;
using System.Collections.Generic;

namespace Tracing.Tests.Simple2
{
    public sealed class LaksDemoEventSource : EventSource
    {
        private LaksDemoEventSource() {}
        public static LaksDemoEventSource Log = new LaksDemoEventSource();
        public void MyEvent() { WriteEvent(1, "MyEvent"); }
    }

    public class ProviderValidation2
    {
        public static int Main()
        {
            // This test (temp?) validates event behavior via dotnet-trace and perfview

            Console.WriteLine("Waiting 10 seconds to client to get the PID");
            Thread.Sleep(60*1000);

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

            Console.WriteLine("Waiting 10 seconds to EventPipe to write the data");
            Thread.Sleep(10*1000);

            return 100;
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
