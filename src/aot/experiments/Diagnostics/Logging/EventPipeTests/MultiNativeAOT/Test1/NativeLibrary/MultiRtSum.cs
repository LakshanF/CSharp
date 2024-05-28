﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Runtime.InteropServices;
using System.Threading;

namespace NativeLibrary
{
    public sealed class LaksDemoEventSource : EventSource
    {
        private LaksDemoEventSource() {}
        public static LaksDemoEventSource Log = new LaksDemoEventSource();
        public void MyEvent() { WriteEvent(1, "MyEvent"); }
    }

    public class MultiRtSum
    {
        [UnmanagedCallersOnly(EntryPoint = "add")]
        public static int Add(int a, int b)
        {
            Console.WriteLine($"Waiting 10 seconds to client to get the PID: Name: {Process.GetCurrentProcess().ProcessName}, Id:{Process.GetCurrentProcess().Id}");
            Thread.Sleep(10*1000);

            for (int i = 0; i < 100; i++)
            {
                LaksDemoEventSource.Log.MyEvent();
            }

            Console.WriteLine($"Waiting 10 seconds to complete the tracing (ctrl0c)");
            Thread.Sleep(10*1000);

            return a + b;
        }

        [UnmanagedCallersOnly(EntryPoint = "write_line")]
        public static int WriteLine(IntPtr pString)
        {
            // The marshalling code is typically auto-generated by a custom tool in larger projects.
            try
            {
                // UnmanagedCallersOnly methods only accept primitive arguments. The primitive arguments
                // have to be marshalled manually if necessary.
                string str = Marshal.PtrToStringAnsi(pString);

                Console.WriteLine(str);
            }
            catch
            {
                // Exceptions escaping out of UnmanagedCallersOnly methods are treated as unhandled exceptions.
                // The errors have to be marshalled manually if necessary.
                return -1;
            }
            return 0;
        }

        [UnmanagedCallersOnly(EntryPoint = "sumstring")]
        public static IntPtr sumstring(IntPtr first, IntPtr second)
        {
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