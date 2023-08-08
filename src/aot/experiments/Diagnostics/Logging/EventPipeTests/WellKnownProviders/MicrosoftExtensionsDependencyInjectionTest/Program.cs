using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Wait for 10 seconds");
        Thread.Sleep(10*1000);

        var serviceProvider = new ServiceCollection()
            .AddSingleton<IMyService, MyService>()
            .BuildServiceProvider();

        var myService = serviceProvider.GetService<IMyService>();
        myService.DoSomething();
    }
}

public interface IMyService
{
    void DoSomething();
}

public class MyService : IMyService
{
    public void DoSomething()
    {
        Console.WriteLine("Hello from MyService!");
    }
}
