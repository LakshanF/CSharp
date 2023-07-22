// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.Tracing;
using System.Runtime.InteropServices;
using System.Threading;

namespace NativeLibrary
{
    public sealed class LaksDemoEventSource2 : EventSource
    {
        private LaksDemoEventSource2() {}
        public static LaksDemoEventSource2 Log = new LaksDemoEventSource2();
        public void MyEvent() { WriteEvent(1, "MyEvent2"); }
    }

    public class MultiRtSum
    {

        [UnmanagedCallersOnly(EntryPoint = "sumstring")]
        public static IntPtr sumstring(IntPtr first, IntPtr second)
        {
            Console.WriteLine("Lib2: Waiting 10 seconds to client to get the PID");
            Thread.Sleep(10*1000);

            for (int i = 0; i < 100; i++)
            {
                LaksDemoEventSource2.Log.MyEvent();
            }

            // Parse strings from the passed pointers 
            string my1String = Marshal.PtrToStringAnsi(first);
            string my2String = Marshal.PtrToStringAnsi(second);

            // Concatenate strings 
            string sum = my1String + my2String;

            // Assign pointer of the concatenated string to sumPointer
            IntPtr sumPointer = Marshal.StringToHGlobalAnsi(sum);

            // Return pointer
            return sumPointer;
        }
    }
}