using System;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace TrimAll
{
    public class IL2072
    {

        //[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type GetTrimType()
        {          
            return typeof(IL2072_Reflection);
        }

        public void DoTheTango()
        {
            Type ct1 = GetTrimType();
            var c1 = Activator.CreateInstance(ct1);
            Console.WriteLine($"DynamicallyAccessedMembers ctor mismatch: {c1!=null}");
        }
    }

    class IL2072_Reflection
    {
        public string SayTrim()
        {
            return "SayTrim";
        }

    }
}