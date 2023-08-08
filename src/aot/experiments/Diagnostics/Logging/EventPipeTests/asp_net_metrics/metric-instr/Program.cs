using System.Diagnostics.Metrics;

class Program
{
    static Meter s_meter = new("HatCo.HatStore", "1.0.0");
    static Counter<int> s_hatsSold = s_meter.CreateCounter<int>("hats-sold");

    static void Main(string[] args)
    {
        var rand = Random.Shared;
        Console.WriteLine("Press any key to exit");
        while (!Console.KeyAvailable)
        {
            //// Simulate hat selling transactions.
            Thread.Sleep(rand.Next(100, 2500));
            s_hatsSold.Add(rand.Next(0, 1000));
        }
    }
}