using System;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace TrimAll
{
    public class IL2026
    {
        public void DoTheTango()
        {
            Type[] types = typeof(IL2026_Reflection).Assembly.GetTypes();
            Console.WriteLine($"Calling a method with RequiresUnreferencedCodeAttribute {types.Length}");
        }

    }

    class IL2026_Reflection
    {
        public string PropertySayTrim{get;}
        public string SayTrimReflection()
        {
            return "SayTrimReflection";
        }
    }

}