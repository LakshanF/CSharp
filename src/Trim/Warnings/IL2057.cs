using System;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace TrimAll
{
    public class IL2057
    {
        string ReadName()
        {
            return "AnyName";
        }

        public void DoTheTango()
        {
            string typeName = ReadName();
            Type.GetType(typeName);
            Console.WriteLine($"Type.GetType(typeName) where typeName is an arbitary string");
        }

    }

}