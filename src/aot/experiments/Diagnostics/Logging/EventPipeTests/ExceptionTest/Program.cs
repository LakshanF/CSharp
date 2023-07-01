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
            for (int i = 0; i < 1000; i++)
            {
                if (i % 100 == 0)
                    Console.WriteLine($"Thrown an exception {i} times...");
                try
                {
                    throw new ArgumentNullException("Throw ArgumentNullException");
                }
                catch (Exception e)
                {
                    //Do nothing
                }
            }
        }
    }

}