namespace ES_Test1;
public static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Waiting 10 seconds to client to get the PID");
        Thread.Sleep(10 * 1000);
        Console.WriteLine("Done Waiting");

        TargetStartLogging();

        Console.WriteLine("Done done!");
    }

    private static void TargetStartLogging()
    {
        Test1EventSource.Log.AppStarted("Hello World From .NET!", 12);
        Test1EventSource.Log.DebugMessage("Got here From .NET!");
        Test1EventSource.Log.DebugMessage("finishing startup From .NET!");
        Test1EventSource.Log.AppInfo(new ClassTest(), new StructTest());
        Test1EventSource.Log.RequestStart(3);
        Test1EventSource.Log.RequestStop(3);
    }
}
