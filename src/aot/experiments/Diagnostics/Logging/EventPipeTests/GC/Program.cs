using System;
using System.Runtime.InteropServices;

namespace MyApp // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static void Main(string[] args)
        {
            new Program().M1();
            Console.WriteLine("Hello World!");
        }

        void M1()
        {
            // dotnet-trace collect --providers LaksDemoEventSource,Microsoft-Windows-DotNETRuntime --name GC
            // var buffer = IntPtr.Zero;
            // try
            // {
            //     buffer = Marshal.AllocHGlobal(size);
            //     GC.AddMemoryPressure(size);

            //     // ... use buffer ...
            // }
            // finally
            // {
            //     Marshal.FreeHGlobal(buffer);
            //     GC.RemoveMemoryPressure(size);
            // }            

            Console.WriteLine("Waiting 10 seconds to client to get the PID");
            Thread.Sleep(10*1000);

            GC.Collect();
            List<Foo> list = new List<Foo>();

            for (int i = 0; i < 1000; i++)
            {

                var buffer = IntPtr.Zero;
                int size = (i+1)*10000;
                try
                {
                    buffer = Marshal.AllocHGlobal(size);
                    GC.AddMemoryPressure(size);

                    // ... use buffer ...
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                    GC.RemoveMemoryPressure(size);
                }            

                list.Add(new Foo(i));
                if (i % 100 == 0)
                    Console.WriteLine($"Fired MyEvent {i:N0}/100,000 times...");
                //LaksDemoEventSource.Log.MyEvent();
            }
            int rndValue = new Random().Next(0, 1000);
            Console.WriteLine($"{list[rndValue].IValue}-{list[rndValue].SValue}");
            GC.Collect();

            Console.WriteLine("Waiting 10 seconds to EventPipe to write the data");
            Thread.Sleep(10*1000);

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