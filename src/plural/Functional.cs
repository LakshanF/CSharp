using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Net;

namespace Plural_CSharp
{
    static class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start");

            void CallToString()=>Console.WriteLine("From inside the function");

            CallToString();

            TimeKeeper keeper = new TimeKeeper();
            var result = keeper.Measure(()=>{
                foreach(var number in GetRandomNumbers().Find(IsPrime).Take(2))
                {
                    Console.WriteLine(number);
                }
            }
            );
            Console.WriteLine(result);

            var client = new WebClient();
            Func<string, string> download = url=> client.DownloadString(url);

            Func<string, Func<string>> downloadCurry = download.Curry();

            var data = download.Partial("http://www.microsoft.com").WithRetry();
            Console.WriteLine($"Client return data lenght: {data?.Length}");

            var data2 = downloadCurry("http://www.microsoft.com").WithRetry();
            Console.WriteLine($"Client return data lenght: {data2?.Length}");



            Console.WriteLine("End");


        }

        private static IEnumerable<int> GetRandomNumbers()
        {
            Random random = new Random();
            while(true)
            {
                yield return random.Next(1000);
            }
        }

        private static IEnumerable<int> Find(this IEnumerable<int> values, Func<int, bool> test)
        {
            foreach(int value in values)
            {
                Console.WriteLine($"Testing ... {value}");
                if(test(value))
                {
                    yield return value;
                }
            }
        } 

        private static bool IsPrime(int number)
        {
            bool result = true;
            for(long i=2; i<number; i++)
            {
                if(number%i==0)
                {
                    result=false;
                    break;
                }
            }
            return result;
        }

        private static bool IsOdd(int number)
        {
            return (number%2)!=0;
        }
        private static bool IsEven(int number)
        {
            return (number%2)==0;
        }
    }

    public class TimeKeeper
    {
        public TimeSpan Measure(Action action)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            action();
            watch.Stop();
            return watch.Elapsed;
        }
    }

    public static class Extensions
    {
        public static T WithRetry<T>(this Func<T> action)
        {
            T result = default(T);
            int retryCount=0;
            bool successfull = false;
            do
            {
                try
                {
                    result = action();
                    successfull=true;
                }catch(WebException)
                {
                    retryCount++;
                }

            }while(retryCount<3 && !successfull);
            return result;
        }

        public static Func<TResult> Partial<TParm1, TResult>(this Func<TParm1, TResult> func, TParm1 param)
        {
            return () => func(param);
        }

        public static Func<TParm1, Func<TResult>> Curry<TParm1, TResult>(this Func<TParm1, TResult> func)
        {
            return paramter => () => func(paramter);
        }

    }
}
