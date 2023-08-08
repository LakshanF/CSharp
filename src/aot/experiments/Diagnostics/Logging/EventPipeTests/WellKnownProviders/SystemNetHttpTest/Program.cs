using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Wait for 10 seconds");
        Thread.Sleep(10*1000);

        using var client = new HttpClient();

        var response = await client.GetAsync("https://jsonplaceholder.typicode.com/todos/1");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine(content);
        }
    }
}
