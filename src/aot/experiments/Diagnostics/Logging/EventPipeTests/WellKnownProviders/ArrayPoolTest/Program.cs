using System;
using System.Buffers;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Wait for 10 seconds");
        Thread.Sleep(10*1000);
        const int ARRAYSIZE = 1000;
        int[] buffer = ArrayPool<int>.Shared.Rent(ARRAYSIZE);
        try
        {
            FillTheArray(buffer);
            UseTheArray(buffer);
        }
        finally
        {
            ArrayPool<int>.Shared.Return(buffer);
        }
    }

    private static void FillTheArray(int[] arr)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = i;
        }
    }

    private static void UseTheArray(int[] arr)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            Console.WriteLine(arr[i]);
        }
    }
}
