using System;

namespace Classes
{
    class Program
    {
        static void Main(string[] args)
        {
            ClassTest();
            //FunctionalTest();
        }

        static void ConsoleWrite(double data)
        {
            Console.WriteLine(data);
        }

        private static void ClassTest()
        {
            Buffer<double> buffer = new Buffer<double>();


            GetInput(buffer);
            //Printer<double> p = new Printer<double>(ConsoleWrite);
            //buffer.Dump1(p);
            buffer.Dump1(ConsoleWrite);

            buffer.Dump2(d => Console.WriteLine(d));


            var results = buffer.AsEnumerable<double, int>();
            foreach (var value in results)
            {
                Console.WriteLine(value);
            }

            var asDates = buffer.Map(d => new DateTime(2020, 11, 28).AddDays(d));
            foreach (var value in asDates)
            {
                Console.WriteLine(value);
            }

            double sum = ProcessInput(buffer);
            Console.WriteLine(sum);

            Console.WriteLine();
            Console.WriteLine();

            var buffer2 = new CircularBuffer<double>(capacity: 2);
            buffer2.ItemsDiscarded += Buffer2_ItemsDiscarded;
            buffer2.Write(2.3);
            buffer2.Write(4.6);
            buffer2.Write(6.9);
        }

        private static void Buffer2_ItemsDiscarded(object sender, ItemDiscardedEventArgs<double> e)
        {
            Console.WriteLine($"Discarded Info - Discarded:{e.ItemDiscarded}, New: {e.NewItem}");
        }

        private static void FunctionalTest()
        {
            Functional.Run();
        }

        private static double ProcessInput(Buffer<double> buffer)
        {
            double sum = 0.0;
            while(!buffer.IsEmpty)
            {
                sum += buffer.Read();
            }
            return sum;
        }

        private static void GetInput(Buffer<double> values)
        {
            String s;

            Console.WriteLine("Enter a double or 'q'");

            while ((s=Console.ReadLine()) != "q")
            {
                values.Write(Double.Parse(s));
            }
        }
    }
}
