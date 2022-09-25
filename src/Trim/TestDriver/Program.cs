using System.Reflection;
internal class Program
{
    static void Main(string[] args)
    {
        AotTest test = new AotTest();
        test.TestMethod();
    }
}

public class AotTest
{
    public void TestMethod()
    {
        Type ex = typeof(ClassWithGenericMethod);
        Object o1 = Activator.CreateInstance(ex, null);
        MethodInfo mi = ex.GetMethod("Generic");
        MethodInfo miConstructed = mi.MakeGenericMethod(typeof(SomeClass));
        miConstructed.Invoke(o1, null);
    }
}

public class ClassWithGenericMethod 
{ 
    public void Generic<T>()=>Console.WriteLine(typeof(T));
}

public class SomeClass { }

