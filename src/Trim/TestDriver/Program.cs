using System.Reflection;
internal class Program
{
    static void Main(string[] args)
    {
        TrimmerTest test = new TrimmerTest();
        test.TestMethod("SomeKnownAssemblyFile");
    }
}

public class TrimmerTest
{
    public void TestMethod(string assemblyName)
    {
        Console.WriteLine(Assembly.LoadFrom(assemblyName));
    }
}
