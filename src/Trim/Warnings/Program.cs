using System;

namespace TrimAll
{
    class Trim
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Top Trim Warning Hits");

            foreach(var type in typeof(Trim).Assembly.GetTypes())
            {
                //All class names for the warning have the ILNNNN format. This check should be sufficient
                if(type.Name.StartsWith("IL") && type.Name.Length==6)
                {
                    Console.WriteLine($"Calling {type.Name} driver method");
                    Object o = Activator.CreateInstance(type);
                    foreach(var m in type.GetMethods())
                    {
                        if(m.Name.Equals("DoTheTango"))
                        {
                            m.Invoke(o, null);
                        }
                    }
                    Console.WriteLine();
                }
            }

        }        
    }
}
