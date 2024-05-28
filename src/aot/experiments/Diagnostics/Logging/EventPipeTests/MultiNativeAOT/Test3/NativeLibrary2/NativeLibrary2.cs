using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace NativeLibrary2;

public sealed class LaksNativeLib2EventSource : EventSource
{
    private LaksNativeLib2EventSource() {}
    public static LaksNativeLib2EventSource Log = new LaksNativeLib2EventSource();
    public void MyEvent() { WriteEvent(1, "MyEvent from lib"); }
}

public class MultiRtSum
{
    [UnmanagedCallersOnly(EntryPoint = "addInLib2")]
    public static int AddInLib2(int a, int b)
    {
        Console.WriteLine($"NativeLibrary2:  Waiting 10 seconds to client to get the PID: Name: {Process.GetCurrentProcess().ProcessName}, Id:{Process.GetCurrentProcess().Id}");
        Thread.Sleep(10*1000);

        GC.Collect();

        for (int i = 0; i < 100000; i++)
        {
            if (i % 10000 == 0)
                Console.WriteLine($"Fired NativeLib2 MyEvent {i:N0}/100,000 times...");
            LaksNativeLib2EventSource.Log.MyEvent();
        }
        GC.Collect();

        Console.WriteLine("NativeLibrary2: Waiting 10 seconds to EventPipe to write the data");
        Thread.Sleep(10*1000);

        return a + b;
    }
}
