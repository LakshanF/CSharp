using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Diagnostics.Tracing;

namespace NativeLibrary1;

public sealed class LaksNativeLib1EventSource : EventSource
{
    private LaksNativeLib1EventSource() {}
    public static LaksNativeLib1EventSource Log = new LaksNativeLib1EventSource();
    public void MyEvent() { WriteEvent(1, "MyEvent from lib"); }
}

public class MultiRtSum
{
    [UnmanagedCallersOnly(EntryPoint = "addInLib1")]
    public static int AddInLib1(int a, int b)
    {
        Console.WriteLine("NativeLibrary1: Waiting 10 seconds to client to get the PID");
        Thread.Sleep(10*1000);

        GC.Collect();

        for (int i = 0; i < 100000; i++)
        {
            if (i % 10000 == 0)
                Console.WriteLine($"Fired MyEvent {i:N0}/100,000 times...");
            LaksNativeLib1EventSource.Log.MyEvent();
        }
        GC.Collect();

        Console.WriteLine("NativeLibrary1: Waiting 10 seconds to EventPipe to write the data");
        Thread.Sleep(10*1000);

        return a + b;
    }
}
