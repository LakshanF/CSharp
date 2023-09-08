using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NativeLibrary1;

public class MultiRtSum
{
    [UnmanagedCallersOnly(EntryPoint = "addInLib1")]
    public static int AddInLib1(int a, int b)
    {
        Console.WriteLine("Waiting 10 seconds to client to get the PID");
        Thread.Sleep(10*1000);

        // for (int i = 0; i < 100; i++)
        // {
        //     LaksDemoEventSource.Log.MyEvent();
        // }

        return a + b;
    }
}
